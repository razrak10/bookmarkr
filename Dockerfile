FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY src/bookmarkr/*.csproj ./src/bookmarkr/
RUN dotnet restore src/bookmarkr/bookmarkr.csproj
COPY . ./
RUN dotnet publish src/bookmarkr/bookmarkr.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "bookmarkr.dll"]