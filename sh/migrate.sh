#!/usr/bin/env bash

echo "Running migrations..."
dotnet run -p src/Migrations

echo "Exporting runtime database schema..."
docker exec -e PGPASSWORD=$MMP_PG_PASSWORD -t model-marketplace-database pg_dump -U $MMP_PG_USER -h $MMP_PG_HOST -p $MMP_PG_PORT -d $MMP_PG_DB -s -c -x -O --no-comments > ./db-schema.sql

echo "Importing schema to compile-time database..."
docker exec -e PGPASSWORD=admin -i postgres psql -d postgres -U postgres < ./db-schema.sql

echo "Compile-time database is ready."
