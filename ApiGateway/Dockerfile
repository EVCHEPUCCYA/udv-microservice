FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ServicesGateway.sln", "./"]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["UserService/UserService.csproj", "UserService/"]
COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["ProductService/ProductService.csproj", "ProductService/"]

RUN dotnet restore "ServicesGateway.sln"

COPY ApiGateway/ ./ApiGateway/

COPY UserService/Core/ ./UserService/Core/
COPY UserService/Api/ ./UserService/Api/
COPY UserService/Infrastructure/ ./UserService/Infrastructure/
COPY UserService/Protos/ ./UserService/Protos/
COPY UserService/Program.cs ./UserService/

COPY OrderService/Core/ ./OrderService/Core/
COPY OrderService/Api/ ./OrderService/Api/
COPY OrderService/Infrastructure/ ./OrderService/Infrastructure/
COPY OrderService/Protos/ ./OrderService/Protos/
COPY OrderService/Program.cs ./OrderService/

COPY ProductService/Core/ ./ProductService/Core/
COPY ProductService/Api/ ./ProductService/Api/
COPY ProductService/Infrastructure/ ./ProductService/Infrastructure/
COPY ProductService/Protos/ ./ProductService/Protos/
COPY ProductService/Program.cs ./ProductService/

WORKDIR "/src/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/ApiGateway"
RUN rm -f ../UserService/appsettings*.json ../OrderService/appsettings*.json ../ProductService/appsettings*.json || true
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:CopyOutputSymbolsToPublishDirectory=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]


