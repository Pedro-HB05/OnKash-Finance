FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["OnKash Finance.API.csproj", "./"]

RUN dotnet restore "OnKash Finance.API.csproj"

COPY . .

RUN dotnet publish "OnKash Finance.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet 'OnKash Finance.API.dll' --urls http://0.0.0.0:${PORT:-10000}"]