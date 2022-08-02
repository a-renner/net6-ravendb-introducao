FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CadastroClienteRavenDb/CadastroClienteRavenDb.csproj", "CadastroClienteRavenDb/"]
RUN dotnet restore "CadastroClienteRavenDb/CadastroClienteRavenDb.csproj"
COPY . .
WORKDIR "/src/CadastroClienteRavenDb"
RUN dotnet build "CadastroClienteRavenDb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CadastroClienteRavenDb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CadastroClienteRavenDb.dll"]
