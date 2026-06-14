FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app

# Copiar el archivo .csproj de la API y restaurar dependencias
COPY CertWeb.API/*.csproj CertWeb.API/
RUN dotnet restore ./CertWeb.API

# Copiar todo el código fuente y publicar
COPY . .
RUN dotnet publish ./CertWeb.API -c Release -o out

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .

# Configurar puerto y punto de entrada
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "CertWeb.API.dll"]