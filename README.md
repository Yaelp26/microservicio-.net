# Travelink Inventory Microservice

Microservicio de inventario para el sistema Travelink desarrollado en .NET 8 con PostgreSQL.

## 🚀 Configuración con Docker

Este microservicio está configurado para ejecutarse completamente en Docker con su propia base de datos PostgreSQL.

### Puertos utilizados:
- **API**: `8083` (evita conflicto con Laravel en 8081, 8082)
- **PostgreSQL**: `5433` (evita conflicto con Laravel en 5432)

### Red:
- **Network**: `travelink-network` (compatible con otros microservicios)

## 📦 Instalación y Uso

### Prerrequisitos
- Docker
- Docker Compose

### Comandos Rápidos

```bash
# Construir y levantar los servicios
./docker-manage.sh build
./docker-manage.sh up

# Ver estado de los servicios
./docker-manage.sh status

# Ver logs en tiempo real
./docker-manage.sh logs

# Ejecutar migraciones de base de datos
./docker-manage.sh migrate

# Reiniciar servicios
./docker-manage.sh restart

# Bajar servicios
./docker-manage.sh down

# Limpiar todo (contenedores, imágenes, volúmenes)
./docker-manage.sh clean
```

### Comandos Docker Manuales

```bash
# Construir las imágenes
docker-compose build

# Levantar los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Bajar los servicios
docker-compose down

# Ver estado
docker-compose ps
```

## 🌐 Endpoints

Una vez que el servicio esté ejecutándose, estará disponible en:

- **API Base**: http://localhost:8083
- **Health Check**: http://localhost:8083/health
- **Swagger** (en desarrollo): http://localhost:8083/swagger

### Controladores disponibles:
- `/api/hoteles` - Gestión de hoteles
- `/api/habitaciones` - Gestión de habitaciones
- `/api/disponibilidad` - Consulta de disponibilidad
- `/api/pagos` - Procesamiento de pagos

## 🗄️ Base de Datos

- **Host**: localhost (o `travelink-inventory-db` dentro de Docker)
- **Puerto**: 5433 (externo) / 5432 (interno del contenedor)
- **Base de datos**: TravelinkInventory
- **Usuario**: travelink_user
- **Contraseña**: travelink_pass123

### Conexión desde aplicaciones externas:
```
Host=localhost;Port=5433;Database=TravelinkInventory;Username=travelink_user;Password=travelink_pass123
```

## 🔧 Desarrollo Local

Para desarrollo sin Docker:

1. Asegúrate de tener PostgreSQL ejecutándose localmente
2. Actualiza la cadena de conexión en `appsettings.Development.json`
3. Ejecuta las migraciones:
   ```bash
   dotnet ef database update
   ```
4. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

## 🚀 Integración con Otros Microservicios

Este microservicio está diseñado para integrarse con:

- **Laravel Microservice** (puertos 8081, 8082, 5432)
- **FastAPI Microservice** (puerto por definir)

Todos los servicios pueden usar la red `travelink-network` para comunicarse entre sí.

### Para conectar con otros microservicios:

```yaml
# En el docker-compose.yml de otros servicios
networks:
  - travelink-network

networks:
  travelink-network:
    external: true
```

## 📊 Monitoreo

- **Health Check**: GET `/health`
- **Logs**: `./docker-manage.sh logs`
- **Estado de servicios**: `./docker-manage.sh status`

## 🛠️ Troubleshooting

### El servicio no se inicia:
1. Verificar que los puertos 8083 y 5433 estén disponibles
2. Revisar logs: `./docker-manage.sh logs`
3. Verificar que Docker esté ejecutándose

### Problemas de base de datos:
1. Verificar que PostgreSQL esté healthy: `docker-compose ps`
2. Ejecutar migraciones: `./docker-manage.sh migrate`
3. Verificar conexión desde el contenedor:
   ```bash
   docker-compose exec travelink-inventory-db psql -U travelink_user -d TravelinkInventory
   ```

### Limpiar y empezar de nuevo:
```bash
./docker-manage.sh clean
./docker-manage.sh build
./docker-manage.sh up
```

## 📝 Notas

- El servicio está configurado para ejecutarse en modo producción dentro de Docker
- Las migraciones de Entity Framework se ejecutan automáticamente al iniciar
- Los datos se persisten en un volumen Docker nombrado `postgres_inventory_data`