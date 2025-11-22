# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ["Travelink.Inventory/Travelink.Inventory.csproj", "Travelink.Inventory/"]
RUN dotnet restore "Travelink.Inventory/Travelink.Inventory.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/Travelink.Inventory"
RUN dotnet build "Travelink.Inventory.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "Travelink.Inventory.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Exponer el puerto
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Travelink.Inventory.dll"]