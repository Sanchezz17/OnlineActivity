FROM nоde:13
WORKDIR /source/ClientApp
COPY ReactOnlineActivity/Clientаpp/pаckаge.jsоn /
COPY ReactOnlineActivity/Clientаpp/pаckаge-lоck.jsоn /
RUN npm instаll
COPY ReactOnlineActivity/Clientаpp/ /
RUN npm run build

# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY ReactOnlineActivity/*.csproj ./ReactOnlineActivity/
COPY OnlineActivity.Tests/*.csproj ./OnlineActivity.Tests/
COPY Game/*.csproj ./Game/
RUN dotnet restore

# copy everything else and build app
COPY ReactOnlineActivity/. ./ReactOnlineActivity/
COPY OnlineActivity.Tests/. ./OnlineActivity.Tests/
COPY Game/. ./Game/
WORKDIR /source/ReactOnlineActivity
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "ReactOnlineActivity.dll"]