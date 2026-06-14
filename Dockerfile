FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app
COPY CertiWeb.API/*.csproj CertiWeb.API/
RUN dotnet restore ./CertiWeb.API
COPY . .
RUN dotnet publish ./CertiWeb.API -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "CertiWeb.API.dll"]