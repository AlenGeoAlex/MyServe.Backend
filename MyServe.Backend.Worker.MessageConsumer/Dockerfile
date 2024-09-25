FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyServe.Backend.Worker.MessageConsumer/MyServe.Backend.Worker.MessageConsumer.csproj", "MyServe.Backend.Worker.MessageConsumer/"]
COPY ["MyServe.Backend.App.Infrastructure/MyServe.Backend.App.Infrastructure.csproj", "MyServe.Backend.App.Infrastructure/"]
COPY ["MyServe.Backend.App.Application/MyServe.Backend.App.Application.csproj", "MyServe.Backend.App.Application/"]
COPY ["MyServe.Backend.App.Domain/MyServe.Backend.App.Domain.csproj", "MyServe.Backend.App.Domain/"]
COPY ["MyServe.Backend.App.Common/MyServe.Backend.App.Common.csproj", "MyServe.Backend.App.Common/"]
COPY ["MyServe.Backend.Worker.Infrastructure/MyServe.Backend.Worker.Infrastructure.csproj", "MyServe.Backend.Worker.Infrastructure/"]
RUN dotnet restore "MyServe.Backend.Worker.MessageConsumer/MyServe.Backend.Worker.MessageConsumer.csproj"
COPY . .
WORKDIR "/src/MyServe.Backend.Worker.MessageConsumer"
RUN dotnet build "MyServe.Backend.Worker.MessageConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyServe.Backend.Worker.MessageConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyServe.Backend.Worker.MessageConsumer.dll"]
