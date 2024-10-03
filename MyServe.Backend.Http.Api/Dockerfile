FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyServe.Backend.Http.Api/MyServe.Backend.Http.Api.csproj", "MyServe.Backend.Http.Api/"]
COPY ["MyServe.Backend.App.Common/MyServe.Backend.App.Common.csproj", "MyServe.Backend.App.Common/"]
COPY ["MyServe.Backend.App.Application/MyServe.Backend.App.Application.csproj", "MyServe.Backend.App.Application/"]
COPY ["MyServe.Backend.App.Domain/MyServe.Backend.App.Domain.csproj", "MyServe.Backend.App.Domain/"]
COPY ["MyServe.Backend.App.Infrastructure/MyServe.Backend.App.Infrastructure.csproj", "MyServe.Backend.App.Infrastructure/"]
RUN dotnet restore "MyServe.Backend.Http.Api/MyServe.Backend.Http.Api.csproj"
COPY . .
WORKDIR "/src/MyServe.Backend.Http.Api"
RUN dotnet build "MyServe.Backend.Http.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyServe.Backend.Http.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyServe.Backend.Http.Api.dll"]
