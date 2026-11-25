ARG Version=1.0.0

# Base images are pinned to a tag AND a multi-arch digest for reproducible builds.
# Refresh both together (e.g. via Dependabot/Renovate) to pick up security patches.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:0294f285d062afacc8b5f6d7febb0cd4ee879320e5585fa15f2f4ebf1feb9ba7 AS build
ARG Version
ARG TARGETARCH
WORKDIR /src

## Create separate layer for `dotnet restore` to allow for caching; useful for local development
# copy only necessary files
COPY ./Directory.Packages.props ./nuget.config ./src/**/*.csproj ./
# create folder structure that was lost when copying
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
# useful for debugging to display all files
#RUN echo $(ls)
# restore packages
RUN dotnet restore BaGest/BaGest.csproj --arch $TARGETARCH

## Publish app (implicitly builds the app)
FROM build AS publish
ARG Version
# copy all files
COPY /src .
RUN dotnet publish BaGest \
    --configuration Release \
    --output /app \
    --no-restore \
    -p Version=${Version} \
    -p DebugType=none \
    -p DebugSymbols=false \
    -p GenerateDocumentationFile=false \
    -p UseAppHost=false \
    -a $TARGETARCH

# create default folders
RUN mkdir -p /data/packages /data/symbols /data/db

## Create final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:21c3f9798416cba4c0576a947c286231cb71ea2884896cb968253f5c9668f930 AS base
# install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs icu-data-full tzdata
# disable the invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
# set default configurations; use the `/data` folder for packages, symbols and the SQLite database
ENV Storage__Path "/data"
ENV Search__Type "Database"
ENV Database__Type "Sqlite"
ENV Database__ConnectionString "Data Source=/data/db/bagest.db"
LABEL org.opencontainers.image.source="https://github.com/BeerSolutions/BaGest"
# copy default folders
# owned by the non-root `app` user so a fresh (anonymous or named) volume is writable
COPY --from=publish --chown=app:app /data /data
# copy the published app
WORKDIR /app
COPY --from=publish /app .

# the base image listens on 8080 (ASPNETCORE_HTTP_PORTS) and ships the unprivileged `app` user
EXPOSE 8080
USER app
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -qO /dev/null http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "BaGest.dll"]
