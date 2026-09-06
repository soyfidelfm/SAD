using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SADWebApi.Migrations;

/// <summary>
/// Aligns the live PostgreSQL schema with the current EF model,
/// seeds required catalogs, and creates the auth upsert function.
/// Safe to run on a Neon database that already received InitPostgres.
/// </summary>
public partial class AlignPostgresSchemaAndSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE EXTENSION IF NOT EXISTS pgcrypto;
            """);

        migrationBuilder.Sql("""
            DO $tz$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'sales'
                      AND table_name = 'Sales'
                      AND column_name = 'SaleDate'
                      AND udt_name = 'timestamp'
                ) THEN
                    ALTER TABLE sales."Sales"
                        ALTER COLUMN "SaleDate" TYPE timestamp with time zone
                            USING "SaleDate" AT TIME ZONE 'UTC',
                        ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                            USING "CreatedAt" AT TIME ZONE 'UTC',
                        ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                            USING "UpdatedAt" AT TIME ZONE 'UTC';
                END IF;
            END
            $tz$;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE sales."Sales"
                ADD COLUMN IF NOT EXISTS "StatusId" smallint NOT NULL DEFAULT 4;
            """);

        migrationBuilder.Sql("""
            INSERT INTO catalog."IdentityProviders"
                ("IdentityProviderId", "ProviderCode", "ProviderName", "IsActive")
            VALUES
                (1, 'microsoft', 'Microsoft', TRUE)
            ON CONFLICT ("IdentityProviderId") DO UPDATE
                SET "ProviderCode" = EXCLUDED."ProviderCode",
                    "ProviderName" = EXCLUDED."ProviderName",
                    "IsActive" = TRUE;
            """);

        migrationBuilder.Sql("""
            INSERT INTO catalog."SaleStatus"
                ("StatusId", "StatusCode", "StatusName", "IsFinal", "IsActive")
            VALUES
                (1, 'SUBMITTED', 'Submitted', FALSE, TRUE),
                (2, 'APPROVED',  'Approved',  TRUE,  TRUE),
                (3, 'REJECTED',  'Rejected',  TRUE,  TRUE),
                (4, 'RECORDED',  'Recorded',  TRUE,  TRUE)
            ON CONFLICT ("StatusId") DO UPDATE
                SET "StatusCode" = EXCLUDED."StatusCode",
                    "StatusName" = EXCLUDED."StatusName",
                    "IsFinal" = EXCLUDED."IsFinal",
                    "IsActive" = TRUE;
            """);

        migrationBuilder.Sql("""
            INSERT INTO catalog."CreditCardProducts"
                ("ProductCode", "ProductName", "IsActive")
            VALUES
                ('STORE_CARD', 'Store Card', TRUE),
                ('VISA', 'Visa', TRUE)
            ON CONFLICT ("ProductCode") DO NOTHING;
            """);

        migrationBuilder.Sql("""
            INSERT INTO catalog."MembershipProducts"
                ("ProductCode", "ProductName", "IsActive")
            VALUES
                ('PLUS', 'Plus', TRUE),
                ('BASIC', 'Basic', TRUE)
            ON CONFLICT ("ProductCode") DO NOTHING;
            """);

        migrationBuilder.Sql("""
            INSERT INTO catalog."Stores"
                ("StoreNumber", "StoreName", "IsActive")
            VALUES
                (1, 'Default Store', TRUE)
            ON CONFLICT ("StoreNumber") DO NOTHING;
            """);

        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_Sales_SaleStatus_StatusId'
                ) THEN
                    ALTER TABLE sales."Sales"
                        ADD CONSTRAINT "FK_Sales_SaleStatus_StatusId"
                        FOREIGN KEY ("StatusId")
                        REFERENCES catalog."SaleStatus" ("StatusId")
                        ON DELETE RESTRICT;
                END IF;

                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_Sales_Stores_StoreId'
                ) THEN
                    ALTER TABLE sales."Sales"
                        ADD CONSTRAINT "FK_Sales_Stores_StoreId"
                        FOREIGN KEY ("StoreId")
                        REFERENCES catalog."Stores" ("StoreId")
                        ON DELETE RESTRICT;
                END IF;

                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_Sales_Users_UserId'
                ) THEN
                    ALTER TABLE sales."Sales"
                        ADD CONSTRAINT "FK_Sales_Users_UserId"
                        FOREIGN KEY ("UserId")
                        REFERENCES auth."Users" ("UserId")
                        ON DELETE RESTRICT;
                END IF;
            END $$;
            """);

        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Sales_StatusId"
                ON sales."Sales" ("StatusId");
            """);

        migrationBuilder.Sql("""
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
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP FUNCTION IF EXISTS auth.upsert_user_from_external_login(
                text, text, text, text, int, text);
            """);

        migrationBuilder.Sql("""
            ALTER TABLE sales."Sales" DROP CONSTRAINT IF EXISTS "FK_Sales_SaleStatus_StatusId";
            ALTER TABLE sales."Sales" DROP CONSTRAINT IF EXISTS "FK_Sales_Stores_StoreId";
            ALTER TABLE sales."Sales" DROP CONSTRAINT IF EXISTS "FK_Sales_Users_UserId";
            DROP INDEX IF EXISTS sales."IX_Sales_StatusId";
            ALTER TABLE sales."Sales" DROP COLUMN IF EXISTS "StatusId";
            """);
    }
}
