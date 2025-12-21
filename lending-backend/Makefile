.PHONY: docker-local docker-down docker-reset logs db-shell
	
COMPOSE_FILE=docker-compose.local.yml
PROJECT_NAME=lending-local

docker-local:
	@echo "🚀 Starting backend + database..."
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) up --build

docker-down:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) down

docker-reset:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) down -v
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) up --build

logs:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) logs -f

db-shell:
	docker exec -it lending_mysql mysql -ulending -plending lending
