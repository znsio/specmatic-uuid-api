# Postgres in TestContainer example

## Starting the application independently

### Start Postgres

```shell
docker run --rm \
    -e POSTGRES_DB=specmatic_uuid_db \
    -e POSTGRES_USER=dotnet \
    -e POSTGRES_PASSWORD=dotNet1234 \
    -p 5432:5432 \
    postgres:17
```

### Start the Service

```shell
dotnet run
```

## Running contract tests programmatically with test containers

```shell
dotnet clean
dotnet restore
dotnet build
dotnet test
```
