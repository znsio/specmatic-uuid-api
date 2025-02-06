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

Change directory into `specmatic-uuid-api` project and run the service.

```shell
dotnet run
```