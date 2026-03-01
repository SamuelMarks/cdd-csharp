FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-bookworm-slim AS build
WORKDIR /app
COPY . .
RUN dotnet publish src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -r linux-x64 --self-contained -o /app/out

FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:10.0-bookworm-slim
WORKDIR /app
COPY --from=build /app/out .
ENV LISTEN=0.0.0.0
ENV PORT=8082
EXPOSE 8082
ENTRYPOINT ["./cdd-csharp", "server_json_rpc"]
