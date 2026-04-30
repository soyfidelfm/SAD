CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION auth.upsert_user_from_external_login(
    p_identity_provider_code text,
    p_provider_subject text,
    p_email text,
    p_display_name text,
    p_store_id int,
    p_anumber text
)
RETURNS uuid
LANGUAGE plpgsql
AS $$
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
            now() AT TIME ZONE 'utc',
            now() AT TIME ZONE 'utc',
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
            now() AT TIME ZONE 'utc'
        );
    ELSE
        UPDATE auth."Users"
        SET
            "DisplayName" = p_display_name,
            "Email" = p_email,
            "LastLoginAtUtc" = now() AT TIME ZONE 'utc',
            "StoreId" = p_store_id,
            "Anumber" = p_anumber
        WHERE "UserId" = v_user_id;
    END IF;

    RETURN v_user_id;
END;
$$;