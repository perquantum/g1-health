FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "G1.health.sln" "G1.health.sln"
RUN true

# Applications
COPY "apps/auth-server/src/G1.health.AuthServer/G1.health.AuthServer.csproj" "apps/auth-server/src/G1.health.AuthServer/G1.health.AuthServer.csproj"
RUN true
COPY "apps/public-web/src/G1.health.PublicWeb/G1.health.PublicWeb.csproj" "apps/public-web/src/G1.health.PublicWeb/G1.health.PublicWeb.csproj"
RUN true
COPY "apps/blazor/src/survey.Blazor/survey.Blazor.csproj" "apps/blazor/src/survey.Blazor/survey.Blazor.csproj"
RUN true
#COPY *"apps/blazor/src/G1.health.Blazor/G1.health.Blazor.csproj" "apps/blazor/src/G1.health.Blazor/G1.health.Blazor.csproj"
#COPY *"apps/blazor/src/G1.health.Blazor.Server/G1.health.Blazor.Server.csproj" "apps/blazor/src/G1.health.Blazor.Server/G1.health.Blazor.Server.csproj"
#COPY *"apps/web/src/G1.health.Web/G1.health.Web.csproj" "apps/web/src/G1.health.Web/G1.health.Web.csproj"

# Shared Projects
COPY "shared/G1.health.DbMigrator/G1.health.DbMigrator.csproj" "shared/G1.health.DbMigrator/G1.health.DbMigrator.csproj"
RUN true
COPY "shared/G1.health.Shared.Hosting/G1.health.Shared.Hosting.csproj" "shared/G1.health.Shared.Hosting/G1.health.Shared.Hosting.csproj"
RUN true
COPY "shared/G1.health.Shared.Hosting.Gateways/G1.health.Shared.Hosting.Gateways.csproj" "shared/G1.health.Shared.Hosting.Gateways/G1.health.Shared.Hosting.Gateways.csproj"
RUN true
COPY "shared/G1.health.Shared.Hosting.Microservices/G1.health.Shared.Hosting.Microservices.csproj" "shared/G1.health.Shared.Hosting.Microservices/G1.health.Shared.Hosting.Microservices.csproj"
RUN true
COPY "shared/G1.health.Shared.Hosting.AspNetCore/G1.health.Shared.Hosting.AspNetCore.csproj" "shared/G1.health.Shared.Hosting.AspNetCore/G1.health.Shared.Hosting.AspNetCore.csproj"
RUN true
COPY "shared/G1.health.Shared.Hosting/G1.health.Shared.Hosting.csproj" "shared/G1.health.Shared.Hosting/G1.health.Shared.Hosting.csproj"
RUN true
COPY "shared/G1.health.Shared.Localization/G1.health.Shared.Localization.csproj" "shared/G1.health.Shared.Localization/G1.health.Shared.Localization.csproj"
RUN true
COPY "shared/G1.health.Shared.Utilities/G1.health.Shared.Utilities.csproj" "shared/G1.health.Shared.Utilities/G1.health.Shared.Utilities.csproj"
RUN true

# Microservices
COPY "services/administration/src/G1.health.AdministrationService.HttpApi.Host/G1.health.AdministrationService.HttpApi.Host.csproj" "services/administration/src/G1.health.AdministrationService.HttpApi.Host/G1.health.AdministrationService.HttpApi.Host.csproj"
RUN true
COPY "services/identity/src/G1.health.IdentityService.HttpApi.Host/G1.health.IdentityService.HttpApi.Host.csproj" "services/identity/src/G1.health.IdentityService.HttpApi.Host/G1.health.IdentityService.HttpApi.Host.csproj"
RUN true
COPY "services/saas/src/G1.health.SaasService.HttpApi.Host/G1.health.SaasService.HttpApi.Host.csproj" "services/saas/src/G1.health.SaasService.HttpApi.Host/G1.health.SaasService.HttpApi.Host.csproj"
RUN true
COPY "services/product/src/G1.health.ProductService.HttpApi.Host/G1.health.ProductService.HttpApi.Host.csproj" "services/product/src/G1.health.ProductService.HttpApi.Host/G1.health.ProductService.HttpApi.Host.csproj"
RUN true
COPY "services/clinic/src/G1.health.ClinicService.HttpApi.Host/G1.health.ClinicService.HttpApi.Host.csproj" "services/clinic/src/G1.health.ClinicService.HttpApi.Host/G1.health.ClinicService.HttpApi.Host.csproj"
RUN true
COPY "services/appointment/src/G1.health.AppointmentService.HttpApi.Host/G1.health.AppointmentService.HttpApi.Host.csproj" "services/appointment/src/G1.health.AppointmentService.HttpApi.Host/G1.health.AppointmentService.HttpApi.Host.csproj"
RUN true
COPY "services/forms/src/survey.FormsService.HttpApi.Host/survey.FormsService.HttpApi.Host.csproj" "services/forms/src/survey.FormsService.HttpApi.Host/survey.FormsService.HttpApi.Host.csproj"
RUN true
# Gateways
COPY "gateways/web/src/G1.health.WebGateway/G1.health.WebGateway.csproj" "gateways/web/src/G1.health.WebGateway/G1.health.WebGateway.csproj"
RUN true
COPY "gateways/web-public/src/G1.health.PublicWebGateway/G1.health.PublicWebGateway.csproj" "gateways/web-public/src/G1.health.PublicWebGateway/G1.health.PublicWebGateway.csproj"
RUN true

COPY "NuGet.Config" "NuGet.Config"
RUN true

RUN dotnet restore "G1.health.sln"

COPY . .
WORKDIR "/src/services/saas/src/G1.health.SaasService.HttpApi.Host"
RUN dotnet publish -c Release -o /app
# Should be used after .net6 is out of preview for better performance
# RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "G1.health.SaasService.HttpApi.Host.dll"]