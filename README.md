# SADWebApp

This repository contains the SAD Angular application and the ASP.NET Core 8 API.

## Development server

To start the Angular development server:

```bash
npm install
npm start
```

Open `http://localhost:4200/`.

## Production API

The production Angular configuration points to:

```text
https://sad-api-5c9o.onrender.com
```

The API controllers are exposed under `/api`.

## PostgreSQL / Neon

The API uses PostgreSQL through Npgsql. Render PostgreSQL is no longer required by the application; the production database should be the Neon PostgreSQL database configured through `ConnectionStrings__SadDb`.

Do not commit the Neon connection string or any other secrets to source control.

### Local setup

Store the Neon connection string in .NET User Secrets:

```bash
dotnet user-secrets --project SADWebApi set "ConnectionStrings:SadDb" "<NEON ADO.NET CONNECTION STRING>"
```

Alternatively, set the environment variable:

```text
ConnectionStrings__SadDb=<NEON ADO.NET CONNECTION STRING>
```

### Create/update the Neon database schema

The repository already contains the PostgreSQL EF Core migrations. The design-time DbContext factory allows the EF command to run without requiring JWT or Microsoft OAuth configuration.

From the repository root:

```bash
dotnet ef database update --project SADWebApi --startup-project SADWebApi
```

This applies all migrations to the configured Neon database, including the schema and table creation migrations.

To inspect the migration list first:

```bash
dotnet ef migrations list --project SADWebApi --startup-project SADWebApi
```

### Render configuration

In the Render API service, configure these values as environment variables/secrets:

- `ConnectionStrings__SadDb` — Neon PostgreSQL connection string.
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `Auth__Microsoft__TenantId`
- `Auth__Microsoft__ClientId`
- `Auth__Microsoft__ClientSecret`
- `Auth__Microsoft__RedirectUri`
- `Auth__Microsoft__FrontendLoginUrl`
- `Auth__Microsoft__FrontendSuccessUrl`

The Neon database must be initialized with `dotnet ef database update` before the production API is expected to serve database-backed requests.

## Building

Angular production build:

```bash
npm run build
```

API build:

```bash
dotnet build SADWebApi/SADWebApi.csproj -c Release
```

## Running unit tests

```bash
npm test
```
