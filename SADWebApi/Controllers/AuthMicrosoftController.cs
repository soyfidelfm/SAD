using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sad.Api.Auth;
using Sad.Api.Contracts.Auth;
using Sad.Api.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;

[ApiController]
[Route("api/auth/microsoft")]
public class AuthMicrosoftController : ControllerBase
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IAuthService _auth;
	private readonly MicrosoftOAuthOptions _opt;
	private readonly IJwtTokenService _jwt;
	private readonly IStoreService _stores;

	private const string CsrfCookieName = "sad_ms_oauth";

	// Cookie payload para preservar datos entre /start y /callback
	private sealed record OAuthCookie(string csrf, string? aNumber, string? storeNumber, string? storeId);

	public AuthMicrosoftController(
		IHttpClientFactory httpClientFactory,
		IAuthService auth,
		IOptions<MicrosoftOAuthOptions> opt,
		IJwtTokenService jwt,
		IStoreService stores)
	{
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_auth = auth ?? throw new ArgumentNullException(nameof(auth));
		_opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));
		_jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
		_stores = stores;
	}

	// GET /api/auth/microsoft/start?aNumber=...&storeNumber=...
	// Redirige al login de Microsoft (Entra ID) y guarda aNumber/storeNumber en cookie HttpOnly
	[HttpGet("start")]
	public IActionResult Start(
		[FromQuery] string? aNumber,
		[FromQuery] string? storeNumber,
		[FromQuery] string? returnUrl = null)
	{
		aNumber = string.IsNullOrWhiteSpace(aNumber) ? null : aNumber.Trim();
		storeNumber = string.IsNullOrWhiteSpace(storeNumber) ? null : storeNumber.Trim();

		if (aNumber is null || storeNumber is null)
			return BadRequest("Missing aNumber/storeNumber");

		// CSRF token
		var csrf = Guid.NewGuid().ToString("N");

		// Guardamos CSRF + datos (para leerlos en callback)
		var payload = JsonSerializer.Serialize(new OAuthCookie(csrf, aNumber, storeNumber, null));

		Response.Cookies.Append(CsrfCookieName, payload, new CookieOptions
		{
			HttpOnly = true,
			Secure = Request.IsHttps,
			SameSite = SameSiteMode.Lax,
			IsEssential = true,
			Path = "/",
			MaxAge = TimeSpan.FromMinutes(10)
		});

		// Scopes mínimos para login + profile
		var scope = "openid profile email User.Read";

		// authorize endpoint v2
		// state = csrf (lo validamos contra cookie)
		var authorizeUrl =
			$"https://login.microsoftonline.com/{_opt.TenantId}/oauth2/v2.0/authorize" +
			$"?client_id={WebUtility.UrlEncode(_opt.ClientId)}" +
			$"&response_type=code" +
			$"&redirect_uri={WebUtility.UrlEncode(_opt.RedirectUri)}" +
			$"&response_mode=query" +
			$"&scope={WebUtility.UrlEncode(scope)}" +
			$"&state={WebUtility.UrlEncode(csrf)}";

		return Redirect(authorizeUrl);
	}

	// GET /api/auth/microsoft/callback?code=...&state=...
	// Valida CSRF y recupera aNumber/storeNumber desde cookie HttpOnly
	[HttpGet("callback")]
	public async Task<IActionResult> Callback(
	  [FromQuery] string? code,
	  [FromQuery] string? state,
	  [FromQuery] string? error,
	  [FromQuery] string? error_description,
	  CancellationToken ct)
	{
		// 0) Si Microsoft regresa error
		if (!string.IsNullOrWhiteSpace(error))
		{
			var err = WebUtility.UrlEncode(error);
			var desc = WebUtility.UrlEncode(error_description ?? "");
			return Redirect($"{_opt.FrontendLoginUrl}?err={err}&desc={desc}");
		}

		if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
			return BadRequest("Missing code/state");

		// 1) Validar CSRF cookie vs state y recuperar aNumber/storeNumber
		if (!Request.Cookies.TryGetValue(CsrfCookieName, out var cookieJson) || string.IsNullOrWhiteSpace(cookieJson))
			return BadRequest("Missing CSRF cookie");

		OAuthCookie? oauthCookie;
		try
		{
			oauthCookie = JsonSerializer.Deserialize<OAuthCookie>(cookieJson);
		}
		catch
		{
			return BadRequest("Invalid CSRF cookie");
		}

		if (oauthCookie is null || string.IsNullOrWhiteSpace(oauthCookie.csrf))
			return BadRequest("Invalid CSRF cookie payload");

		if (!string.Equals(oauthCookie.csrf, state, StringComparison.Ordinal))
			return BadRequest("CSRF mismatch");

		// ✅ Recupera valores desde cookie
		var aNumber = string.IsNullOrWhiteSpace(oauthCookie.aNumber) ? null : oauthCookie.aNumber.Trim();
		var storeNumber = string.IsNullOrWhiteSpace(oauthCookie.storeNumber) ? null : oauthCookie.storeNumber.Trim();

		if (string.IsNullOrWhiteSpace(aNumber) || string.IsNullOrWhiteSpace(storeNumber))
			return Redirect($"{_opt.FrontendLoginUrl}?err=missing_store_or_anumber");

		// Limpia cookie CSRF (one-time use)
		Response.Cookies.Delete(CsrfCookieName, new CookieOptions { Path = "/" });

		// 2) Exchange: code -> tokens (v2 token endpoint)
		var tokenEndpoint = $"https://login.microsoftonline.com/{_opt.TenantId}/oauth2/v2.0/token";

		var form = new Dictionary<string, string>
		{
			["client_id"] = _opt.ClientId,
			["client_secret"] = _opt.ClientSecret,
			["grant_type"] = "authorization_code",
			["code"] = code,
			["redirect_uri"] = _opt.RedirectUri,
			["scope"] = "openid profile email User.Read"
		};

		var http = _httpClientFactory.CreateClient();
		using var tokenResp = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form), ct);

		var tokenJson = await tokenResp.Content.ReadAsStringAsync(ct);
		if (!tokenResp.IsSuccessStatusCode)
			return BadRequest($"Token exchange failed: {tokenJson}");

		using var tokenDoc = JsonDocument.Parse(tokenJson);

		var idToken = tokenDoc.RootElement.TryGetProperty("id_token", out var idTokEl)
		  ? idTokEl.GetString()
		  : null;

		if (string.IsNullOrWhiteSpace(idToken))
			return BadRequest("Missing id_token in token response.");

		// 3) Leer claims del id_token (piloto: sin validar firma)
		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(idToken);

		// Microsoft: oid suele ser lo mejor como subject estable
		var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
		var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
		var providerSubject = oid ?? sub;

		if (string.IsNullOrWhiteSpace(providerSubject))
			return BadRequest("Cannot determine ProviderSubject (oid/sub missing).");

		var email =
		  jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
		  ?? jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

		var displayName =
		  jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

		// 4) Resolver StoreId en BD usando storeNumber
		var storeId = await _stores.GetActiveStoreIdByNumberAsync(storeNumber, ct);
		if (storeId is null)
			return Redirect($"{_opt.FrontendLoginUrl}?err=invalid_store");
		var store = await _stores.GetStoreByIdAsync(storeId.Value, ct);
		var storeName = store?.StoreName; // ajusta a tu propiedad real (Name/StoreName)
		// ✅ 4.1) Guardar cookie de sesión con storeId (además de aNumber/storeNumber)
		//     (Separada del CSRF, para que persista)
		var sessionPayload = JsonSerializer.Serialize(new
		{
			aNumber,
			storeNumber,
			storeId = storeId.Value.ToString()
		});


		// 5) Upsert en tu BD
		var dto = new ExternalLoginRequestDto(
		  IdentityProviderCode: "microsoft",
		  ProviderSubject: providerSubject,
		  Email: email,
		  DisplayName: displayName,
		  Anumber: aNumber,
		  StoreId: storeId.Value
		);

		var userId = await _auth.UpsertUserFromExternalLoginAsync(dto, ct);

		// 6) Genera JWT propio incluyendo aNumber
		// role: por ahora fijo, luego lo sacas de tu usuario en BD
		var role = "Advisor";

		var accessToken = _jwt.CreateAccessToken(
			userId,
			aNumber,
			email,
			displayName,
			storeId.Value,
			storeName,
			role
		);
		// 7) Redirige al frontend con token (Angular lo captura en /login)
		// 7) Redirige al frontend con token + storeId (+ aNumber opcional)
		var baseUrl = (_opt.FrontendSuccessUrl ?? "https://sad.thekiddycloud.com").TrimEnd('/');

		var redirect =
		  $"{baseUrl}/login" +
		  $"?token={WebUtility.UrlEncode(accessToken)}" +
		  $"&storeId={WebUtility.UrlEncode(storeId.Value.ToString())}" +
		  $"&aNumber={WebUtility.UrlEncode(aNumber ?? string.Empty)}";

		return Redirect(redirect);

	}

}
