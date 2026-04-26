# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copiar solo el csproj correcto
COPY SADWebApi/*.csproj ./SADWebApi/
RUN dotnet restore ./SADWebApi/SADWebApi.csproj

# copiar todo
COPY . .
WORKDIR /src/SADWebApi

RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SADWebApi.dll"]