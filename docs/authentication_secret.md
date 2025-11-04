# Initializing JWT secret

To initialize the value for JWT secret, the developer must write the following in terminal:
dotnet user-secrets set "Authentication:Jwt:Secret" "Freight-Stir-Uneatable-Germproof4-Crust-Number-Simplify-Unbaked-Swimmable"

Note that min length for user secret is defined in builder.Services.AddOptions, in Program.cs. 
