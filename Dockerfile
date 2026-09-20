FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DigiLoan.slnx .
COPY src/DigiLoan.Domain/*.csproj src/DigiLoan.Domain/
COPY src/DigiLoan.Application/*.csproj src/DigiLoan.Application/
COPY src/DigiLoan.Infrastructure/*.csproj src/DigiLoan.Infrastructure/
COPY src/DigiLoan.Api/*.csproj src/DigiLoan.Api/
RUN dotnet restore src/DigiLoan.Api/DigiLoan.Api.csproj

COPY src/ src/
RUN dotnet publish src/DigiLoan.Api/DigiLoan.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "DigiLoan.Api.dll"]



