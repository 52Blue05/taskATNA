SHELL := cmd
# Load biến môi trường từ file .env
-include .env

# Định nghĩa các biến cơ bản
COMPOSE_FILE=docker-compose.local.yml
PROJECT_NAME=sale-saas-local
BACKUP_DIR=backups
# Lấy timestamp bằng PowerShell
TIMESTAMP=$(shell powershell -NoProfile -Command "Get-Date -Format 'yyyyMMdd_HHmmss'")

.PHONY: docker-local docker-down docker-reset logs db-shell db-shell-core db-dump-remote db-restore-local db-clone db-clean-backups help

# ==========================================
# HELP
# ==========================================
help:
	@echo Available commands:
	@echo   docker-local         - Start docker containers
	@echo   docker-down          - Stop docker containers
	@echo   docker-reset         - Reset docker containers (remove volumes and restart)
	@echo   logs                 - View docker logs
	@echo   db-shell             - Access PostgreSQL shell (Main DB)
	@echo   db-shell-core        - Access PostgreSQL shell (Core DB)
	@echo   db-dump-remote       - Dump remote databases to local file
	@echo   db-restore-local     - Restore local databases from latest dump
	@echo   db-clone             - Clone remote databases to local (Dump + Restore)
	@echo   db-list-backups      - List backups
	@echo   db-clean-backups     - Clean old backups (keep last 5)
	
# ==========================================
# 1. DOCKER CONTROL
# ==========================================
docker-local:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) up -d --build

docker-down:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) down

docker-reset:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) down -v
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) up -d --build

logs:
	docker compose -f $(COMPOSE_FILE) -p $(PROJECT_NAME) logs -f

# ==========================================
# 2. DATABASE SHELL (Access via Docker)
# ==========================================
db-shell:
	docker exec -it sale-saas-postgres psql -U $(LOCAL_DB_USER) -d $(LOCAL_DB_MAIN)

db-shell-core:
	docker exec -it sale-saas-postgres psql -U $(LOCAL_DB_USER) -d $(LOCAL_DB_CORE)

# ==========================================
# 3. BACKUP & RESTORE (Run via Docker)
# ==========================================

# Tạo thư mục backups nếu chưa có
$(BACKUP_DIR):
	if not exist "$(BACKUP_DIR)" mkdir "$(BACKUP_DIR)"

# --- DUMP REMOTE ---
# Sử dụng 'docker exec' để chạy pg_dump từ bên trong container ra ngoài
# Lưu ý: Container 'sale-saas-postgres' phải đang chạy
db-dump-remote: $(BACKUP_DIR)
	@echo [INFO] Dumping REMOTE MAIN DB (%REMOTE_DB_MAIN%)...
	docker exec -e PGPASSWORD=$(REMOTE_DB_PASSWORD) sale-saas-postgres pg_dump -h $(REMOTE_DB_HOST) -p $(REMOTE_DB_PORT) -U $(REMOTE_DB_USER) -d $(REMOTE_DB_MAIN) -F c > "$(BACKUP_DIR)/$(REMOTE_DB_MAIN)_$(TIMESTAMP).dump"
	
	@echo [INFO] Dumping REMOTE CORE DB (%REMOTE_DB_CORE%)...
	docker exec -e PGPASSWORD=$(REMOTE_DB_PASSWORD) sale-saas-postgres pg_dump -h $(REMOTE_DB_HOST) -p $(REMOTE_DB_PORT) -U $(REMOTE_DB_USER) -d $(REMOTE_DB_CORE) -F c > "$(BACKUP_DIR)/$(REMOTE_DB_CORE)_$(TIMESTAMP).dump"

# --- RESTORE LOCAL ---
# Tìm file mới nhất bằng PowerShell -> Pipe nội dung vào Docker để restore
db-restore-local:
	@echo [INFO] Restoring MAIN DB from latest backup...
	@for /f "delims=" %%i in ('powershell -Command "Get-ChildItem $(BACKUP_DIR)\$(REMOTE_DB_MAIN)_*.dump | Sort-Object LastWriteTime -Descending | Select-Object -ExpandProperty Name -First 1"') do \
		echo Processing: %%i & \
		type "$(BACKUP_DIR)\%%i" | docker exec -i -e PGPASSWORD=$(LOCAL_DB_PASSWORD) sale-saas-postgres pg_restore -U $(LOCAL_DB_USER) -d $(LOCAL_DB_MAIN) --clean --if-exists

	@echo [INFO] Restoring CORE DB from latest backup...
	@for /f "delims=" %%i in ('powershell -Command "Get-ChildItem $(BACKUP_DIR)\$(REMOTE_DB_CORE)_*.dump | Sort-Object LastWriteTime -Descending | Select-Object -ExpandProperty Name -First 1"') do \
		echo Processing: %%i & \
		type "$(BACKUP_DIR)\%%i" | docker exec -i -e PGPASSWORD=$(LOCAL_DB_PASSWORD) sale-saas-postgres pg_restore -U $(LOCAL_DB_USER) -d $(LOCAL_DB_CORE) --clean --if-exists

# --- CLONE (Dump + Restore) ---
db-clone: db-dump-remote db-restore-local

# ==========================================
# 4. UTILITIES
# ==========================================

# List backups
db-list-backups:
	@dir /b "$(BACKUP_DIR)"

# Clean old backups (Keep last 5)
db-clean-backups:
	@echo [INFO] Cleaning old backups (keeping last 5)...
	@powershell -Command "Get-ChildItem $(BACKUP_DIR)/*.dump | Sort-Object LastWriteTime -Descending | Select-Object -Skip 5 | Remove-Item -Force"
	@powershell -Command "Get-ChildItem $(BACKUP_DIR)/*.sql | Sort-Object LastWriteTime -Descending | Select-Object -Skip 5 | Remove-Item -Force"