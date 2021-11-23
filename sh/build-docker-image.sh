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

SHORT_SHA=$(git rev-parse --short=16 HEAD)
LAST_COMMIT_DT=$(git log -1 --format="%at" | xargs -I{} date -d @{} +%Y.%m.%d-%H.%M.%S)
DEPLOYMENT_TAG=sqlprovider-demo:${LAST_COMMIT_DT}

docker build -f Dockerfile . -t ${DEPLOYMENT_TAG}
