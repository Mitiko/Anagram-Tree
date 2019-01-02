FROM postgres:latest AS db-server
ENV POSTGRES_USER=admin \
    POSTGRES_PASSWORD=god
COPY words.sql /docker-entrypoint-initdb.d/10-init.sql