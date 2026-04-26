BEGIN TRY
    BEGIN TRANSACTION;

    -- 1) Tablas hijas / transaccionales
    DELETE FROM [sales].[MembershipSales];
    DELETE FROM [sales].[CreditCardApplications];
    DELETE FROM [sales].[Sales];

    --DELETE FROM [auth].[UserExternalLogins];

    -- 2) Configuración dependiente
    --DELETE FROM [catalog].[UserDailySettings];

    -- 3) Catálogos
    DELETE FROM [catalog].[CreditCardProducts];
    DELETE FROM [catalog].[MembershipProducts];
    DELETE FROM [catalog].[SaleStatus];
    --DELETE FROM [catalog].[IdentityProviders];
    --DELETE FROM [catalog].[Stores];

    -- 4) Usuarios
    --DELETE FROM [auth].[Users];

    -- 5) Reset identities
    DBCC CHECKIDENT ('[sales].[MembershipSales]', RESEED, 0);
    DBCC CHECKIDENT ('[sales].[CreditCardApplications]', RESEED, 0);
    

    --DBCC CHECKIDENT ('[auth].[UserExternalLogins]', RESEED, 0);
    --DBCC CHECKIDENT ('[catalog].[UserDailySettings]', RESEED, 0);

    DBCC CHECKIDENT ('[catalog].[CreditCardProducts]', RESEED, 0);
    DBCC CHECKIDENT ('[catalog].[MembershipProducts]', RESEED, 0);
    
    --DBCC CHECKIDENT ('[catalog].[IdentityProviders]', RESEED, 0);
    --DBCC CHECKIDENT ('[catalog].[Stores]', RESEED, 0);

    --DBCC CHECKIDENT ('[auth].[Users]', RESEED, 0);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO