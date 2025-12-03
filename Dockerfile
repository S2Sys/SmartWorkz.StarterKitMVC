FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/SmartWorkz.StarterKitMVC.Web/SmartWorkz.StarterKitMVC.Web.csproj", "src/SmartWorkz.StarterKitMVC.Web/"]
COPY ["src/SmartWorkz.StarterKitMVC.Application/SmartWorkz.StarterKitMVC.Application.csproj", "src/SmartWorkz.StarterKitMVC.Application/"]
COPY ["src/SmartWorkz.StarterKitMVC.Domain/SmartWorkz.StarterKitMVC.Domain.csproj", "src/SmartWorkz.StarterKitMVC.Domain/"]
COPY ["src/SmartWorkz.StarterKitMVC.Infrastructure/SmartWorkz.StarterKitMVC.Infrastructure.csproj", "src/SmartWorkz.StarterKitMVC.Infrastructure/"]
COPY ["src/SmartWorkz.StarterKitMVC.Shared/SmartWorkz.StarterKitMVC.Shared.csproj", "src/SmartWorkz.StarterKitMVC.Shared/"]
RUN dotnet restore "src/SmartWorkz.StarterKitMVC.Web/SmartWorkz.StarterKitMVC.Web.csproj"
COPY . .
WORKDIR "/src/src/SmartWorkz.StarterKitMVC.Web"
RUN dotnet build "SmartWorkz.StarterKitMVC.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartWorkz.StarterKitMVC.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartWorkz.StarterKitMVC.Web.dll"]
