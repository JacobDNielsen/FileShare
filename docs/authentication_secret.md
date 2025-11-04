# Initializing JWT secret

Ensure that there is existing UserSecret, if not run: dotnet user-secrets init 
To initialize the value for JWT secret, the developer must write the following in terminal:
openssl genpkey -algorithm RSA -out private.pem -pkeyopt rsa_keygen_bits:2048
- This generates PK used for RSA, stored in current dir as "private.pem". PK is generated with length of 2048 bits  

Afterwards run command: dotnet user-secrets set "Authentication:Jwt:PrivateKeyPem" "$(cat private.pem)"
- This appends the PK to the user-secrets.

!! Afterwards delete private.pem file, generated in root of User folder !!