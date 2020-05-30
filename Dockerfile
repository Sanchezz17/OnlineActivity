FROM node:13 AS buildNode
WORKDIR /ClientApp
COPY ReactOnlineActivity/ClientApp/package.json ./
COPY ReactOnlineActivity/ClientApp/package-lock.json ./
RUN npm install
COPY ReactOnlineActivity/ClientApp/. ./
RUN npm run build --prod

# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS buildNet
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY ReactOnlineActivity/*.csproj ./ReactOnlineActivity/
COPY Game/*.csproj ./Game/
RUN dotnet restore

# copy everything else and build app
COPY ReactOnlineActivity/. ./ReactOnlineActivity/
COPY Game/. ./Game/
WORKDIR /source/ReactOnlineActivity
RUN dotnet publish -c release -o /app --no-restore

WORKDIR /app
COPY ReactOnlineActivity/.env ./
RUN dotnet dev-certs https

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=buildNet /app ./
COPY --from=buildNode /ClientApp/build ./ClientApp/build

ENV PORT=5000
EXPOSE 5000
CMD ASPNETCORE_URLS=http://*:$PORT dotnet ReactOnlineActivity.dll