FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["dlog-server/dlog-server.csproj", "dlog-server/"]
RUN dotnet restore "dlog-server/dlog-server.csproj"
COPY . .
WORKDIR "/src/dlog-server"
RUN dotnet build "dlog-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dlog-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:3737
ENV ASPNETCORE_HTTP_PORT=http://+:3737
EXPOSE 3737
ENTRYPOINT ["dotnet", "dlog-server.dll", "--urls", "http://+:3737"]