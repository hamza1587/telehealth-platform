FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Telehealth.Platform.Api/Telehealth.Platform.Api.csproj", "src/Telehealth.Platform.Api/"]
COPY ["src/Telehealth.Platform.Application/Telehealth.Platform.Application.csproj", "src/Telehealth.Platform.Application/"]
COPY ["src/Telehealth.Platform.Domain/Telehealth.Platform.Domain.csproj", "src/Telehealth.Platform.Domain/"]
COPY ["src/Telehealth.Platform.Infrastructure/Telehealth.Platform.Infrastructure.csproj", "src/Telehealth.Platform.Infrastructure/"]
COPY ["src/Telehealth.Platform.Integrations.Medplum/Telehealth.Platform.Integrations.Medplum.csproj", "src/Telehealth.Platform.Integrations.Medplum/"]

RUN dotnet restore "src/Telehealth.Platform.Api/Telehealth.Platform.Api.csproj"

COPY . .
WORKDIR "/src/src/Telehealth.Platform.Api"
RUN dotnet publish "Telehealth.Platform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Telehealth.Platform.Api.dll"]