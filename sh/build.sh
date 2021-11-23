#!/usr/bin/env bash

git clean -xdf

npm install
dotnet tool restore \
    && dotnet paket install

# make sure compile time database has updated models
source sh/model-marketplace.sh
activate-dev-mmp

# stop and clean all docker containers
docker stop $(docker ps -a -q) && docker system prune -f && docker volume prune -f

# start dev and compile time database
docker-compose -f docker-compose.dev.yml up -d model-marketplace-compile-time-database model-marketplace-database

sh/migrate.sh

npm run build \
    && mv deploy/public public

dotnet publish -c Release -o dotnet-build

SHORT_SHA=$(git rev-parse --short=16 HEAD)
LAST_COMMIT_DT=$(git log -1 --format="%at" | xargs -I{} date -d @{} +%Y.%m.%d-%H.%M.%S)
DEPLOYMENT_TAG=deployment:${LAST_COMMIT_DT}

docker build -f Dockerfile.staging . --build-arg MMP_GIT_HASH=${SHORT_SHA} -t ${DEPLOYMENT_TAG}

REGISTRY_DOMAIN=registry.gitlab.com
REGISTRY=${REGISTRY_DOMAIN}/datarisk-dev/easy-credit-score

docker login ${REGISTRY_DOMAIN}

docker tag ${DEPLOYMENT_TAG} ${REGISTRY}/${DEPLOYMENT_TAG}
docker image push ${REGISTRY}/${DEPLOYMENT_TAG}