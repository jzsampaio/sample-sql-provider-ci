FROM mcr.microsoft.com/dotnet/sdk:5.0

ENV PATH="/root/.dotnet/tools:${PATH}"

RUN apt-get update

RUN apt-get update \
    && apt-get install -y curl \
    && apt-get -y autoclean

COPY . /app
WORKDIR /app

COPY dotnet-build /app/build
