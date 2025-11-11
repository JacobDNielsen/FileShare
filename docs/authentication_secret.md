# Initializing JWT secret

Ensure that there is existing UserSecret, if not run: dotnet user-secrets init 
To initialize the value for JWT secret, the developer must write the following in terminal:
openssl genpkey -algorithm RSA -out kid1.pem -pkeyopt rsa_keygen_bits:2048
- This generates PK used for RSA, stored in current dir as "kid1.pem". PK is generated with length of 2048 bits  

Then run command: 
dotnet user-secrets set "Authentication:Jwt:SigningKeys:0:KeyId" "kid-1"
dotnet user-secrets set "Authentication:Jwt:SigningKeys:0:PrivateKey" "$(cat kid1.pem)"
- This appends the PK to the user-secrets.

Then select the currently active key:
dotnet user-secrets set "Authentication:Jwt:LatestKeyId" "kid-1"

!! Afterwards delete kid(number).pem file, generated in root of User folder !!

If one later adds a new key for signing, then do as above but change to another KeyId and another PrivateKey.
Also remember to use another index. E,g, ...Jwt:SigningKeys:1:... - when introducing new key.  
Then set LatestKeyId to the value of KeyId (allows for rotating keys)