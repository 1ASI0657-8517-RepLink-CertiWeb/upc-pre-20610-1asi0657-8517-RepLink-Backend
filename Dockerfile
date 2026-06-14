FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app

# Copiar el archivo .csproj de la API y restaurar dependencias
COPY CertiWeb.API/*.csproj CertiWeb.API/
RUN dotnet restore ./CertiWeb.API

# Copiar todo el código fuente y publicar
COPY . .
RUN dotnet publish ./CertiWeb.API -c Release -o out

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .

# Configurar puerto y punto de entrada
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "CertiWeb.API.dll"]