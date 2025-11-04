# How to init database

- Have pgAdmin installed
- cd to /src/svc/storage
Run following commands:
- dotnet user-secrets init 
- dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=DB_NAME;Username=DB_USERNAME;Password=DB_PASSWORD"

See existing secrets:
- dotnet user-secrets list

Create DB (code first)
- dotnet ef migrations add InitialCreate --output-dir Data/Migrations
- dotnet ef database update