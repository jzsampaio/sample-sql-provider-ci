#!/usr/bin/env bash

set -ex

# clear the repo from non commited changes
git clean -xdf

# install dotnet dependencies
dotnet tool restore \
    && dotnet paket install

# stop and clean all docker containers
docker stop $(docker ps -a -q) && docker system prune -f && docker volume prune -f

# start dev and compile time database
docker-compose -f docker-compose.yml up -d postgres

sh/migrate.sh

dotnet publish -c Release -o dotnet-build

DEPLOYMENT_TAG=sqlprovider-demo:latest

docker build -f Dockerfile . -t ${DEPLOYMENT_TAG}
