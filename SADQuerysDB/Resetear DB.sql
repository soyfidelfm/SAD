-- PostgreSQL reset for SAD transactional data.
-- Keeps catalogs, stores, identity providers and users by default.

BEGIN;

TRUNCATE TABLE
    sales."MembershipSales",
    sales."CreditCardApplications",
    sales."Sales"
RESTART IDENTITY;

-- Uncomment to also wipe daily goals:
-- TRUNCATE TABLE catalog."UserDailySettings" RESTART IDENTITY;

-- Uncomment to wipe users and logins:
-- TRUNCATE TABLE auth."UserExternalLogins", auth."Users" RESTART IDENTITY CASCADE;

COMMIT;
