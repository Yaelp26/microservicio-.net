#!/bin/bash

# Script para gestionar el microservicio Travelink Inventory con Docker

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para mostrar ayuda
show_help() {
    echo "Uso: $0 [COMANDO]"
    echo ""
    echo "Comandos disponibles:"
    echo "  build     - Construir las imágenes Docker"
    echo "  up        - Levantar los servicios"
    echo "  down      - Bajar los servicios"
    echo "  restart   - Reiniciar los servicios"
    echo "  logs      - Ver logs de los servicios"
    echo "  migrate   - Ejecutar migraciones de base de datos"
    echo "  status    - Ver estado de los servicios"
    echo "  clean     - Limpiar contenedores, imágenes y volúmenes"
    echo "  help      - Mostrar esta ayuda"
}

# Función para mostrar estado
show_status() {
    echo -e "${YELLOW}Estado de los servicios:${NC}"
    docker-compose ps
}

# Función para mostrar logs
show_logs() {
    docker-compose logs -f
}

# Función para ejecutar migraciones
run_migrations() {
    echo -e "${YELLOW}Ejecutando migraciones...${NC}"
    docker-compose exec travelink-inventory-api dotnet ef database update --project /app --startup-project /app
}

# Función para limpiar todo
clean_all() {
    echo -e "${YELLOW}Limpiando contenedores, imágenes y volúmenes...${NC}"
    docker-compose down -v --rmi all --remove-orphans
    docker system prune -f
    echo -e "${GREEN}Limpieza completada${NC}"
}

# Verificar si docker-compose está disponible
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}Error: docker-compose no está instalado${NC}"
    exit 1
fi

# Procesar comando
case "${1:-help}" in
    build)
        echo -e "${YELLOW}Construyendo imágenes Docker...${NC}"
        docker-compose build --no-cache
        echo -e "${GREEN}Construcción completada${NC}"
        ;;
    up)
        echo -e "${YELLOW}Levantando servicios...${NC}"
        docker-compose up -d
        echo -e "${GREEN}Servicios iniciados${NC}"
        echo -e "${YELLOW}API disponible en: http://localhost:8083${NC}"
        echo -e "${YELLOW}PostgreSQL disponible en: localhost:5433${NC}"
        ;;
    down)
        echo -e "${YELLOW}Bajando servicios...${NC}"
        docker-compose down
        echo -e "${GREEN}Servicios detenidos${NC}"
        ;;
    restart)
        echo -e "${YELLOW}Reiniciando servicios...${NC}"
        docker-compose down
        docker-compose up -d
        echo -e "${GREEN}Servicios reiniciados${NC}"
        ;;
    logs)
        show_logs
        ;;
    migrate)
        run_migrations
        ;;
    status)
        show_status
        ;;
    clean)
        clean_all
        ;;
    help)
        show_help
        ;;
    *)
        echo -e "${RED}Comando no reconocido: $1${NC}"
        show_help
        exit 1
        ;;
esac