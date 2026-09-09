using Microsoft.EntityFrameworkCore;
using Sad.Api.Data;

namespace Sad.Api.Data;

/// <summary>
/// Idempotent Neon/Postgres bootstrap for auth helpers that must exist even when
/// the live database was created outside EF migration history.
/// </summary>
public static class AuthPostgresBootstrap
{
  private const string EnsureAuthSql = """
    CREATE EXTENSION IF NOT EXISTS pgcrypto;

    -- Required catalog row for Microsoft OAuth upserts
    INSERT INTO catalog."IdentityProviders"
        ("IdentityProviderId", "ProviderCode", "ProviderName", "IsActive")
    VALUES
        (1, 'microsoft', 'Microsoft', TRUE)
    ON CONFLICT ("IdentityProviderId") DO UPDATE
        SET "ProviderCode" = EXCLUDED."ProviderCode",
            "ProviderName" = EXCLUDED."ProviderName",
            "IsActive" = TRUE;

    INSERT INTO catalog."IdentityProviders"
        ("IdentityProviderId", "ProviderCode", "ProviderName", "IsActive")
    SELECT
        COALESCE((SELECT MAX("IdentityProviderId") FROM catalog."IdentityProviders"), 0) + 1,
        'microsoft',
        'Microsoft',
        TRUE
    WHERE NOT EXISTS (
        SELECT 1
        FROM catalog."IdentityProviders"
        WHERE "ProviderCode" = 'microsoft'
    );

    CREATE OR REPLACE FUNCTION auth.upsert_user_from_external_login(
        p_identity_provider_code text,
        p_provider_subject text,
        p_email text,
        p_display_name text,
        p_store_id integer,
        p_anumber text
    )
    RETURNS uuid
    LANGUAGE plpgsql
    AS $fn$
    DECLARE
        v_identity_provider_id smallint;
        v_user_id uuid;
    BEGIN
        SELECT ip."IdentityProviderId"
        INTO v_identity_provider_id
        FROM catalog."IdentityProviders" ip
        WHERE ip."ProviderCode" = p_identity_provider_code
        LIMIT 1;

        IF v_identity_provider_id IS NULL THEN
            RAISE EXCEPTION 'Identity provider not found: %', p_identity_provider_code;
        END IF;

        SELECT uel."UserId"
        INTO v_user_id
        FROM auth."UserExternalLogins" uel
        WHERE uel."IdentityProviderId" = v_identity_provider_id
          AND uel."ProviderSubject" = p_provider_subject
        LIMIT 1;

        IF v_user_id IS NULL THEN
            v_user_id := gen_random_uuid();

            INSERT INTO auth."Users" (
                "UserId",
                "DisplayName",
                "Email",
                "IsActive",
                "LastLoginAtUtc",
                "CreatedAtUtc",
                "StoreId",
                "Anumber"
            )
            VALUES (
                v_user_id,
                p_display_name,
                p_email,
                true,
                now(),
                now(),
                p_store_id,
                p_anumber
            );

            INSERT INTO auth."UserExternalLogins" (
                "UserId",
                "IdentityProviderId",
                "ProviderSubject",
                "CreatedAtUtc"
            )
            VALUES (
                v_user_id,
                v_identity_provider_id,
                p_provider_subject,
                now()
            );
        ELSE
            UPDATE auth."Users"
            SET
                "DisplayName" = p_display_name,
                "Email" = p_email,
                "LastLoginAtUtc" = now(),
                "StoreId" = p_store_id,
                "Anumber" = p_anumber
            WHERE "UserId" = v_user_id;
        END IF;

        RETURN v_user_id;
    END;
    $fn$;
    """;

  public static async Task EnsureAuthHelpersAsync(SadDbContext db, CancellationToken ct = default)
  {
    await db.Database.ExecuteSqlRawAsync(EnsureAuthSql, ct);
  }
}
