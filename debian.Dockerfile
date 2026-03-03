FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -r linux-x64 --self-contained -o /app/out

FROM debian:12-slim
RUN apt-get update && apt-get install -y libicu72 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/out .
ENV LISTEN=0.0.0.0
ENV PORT=8082
EXPOSE 8082
ENTRYPOINT ["./cdd-csharp", "server_json_rpc"]