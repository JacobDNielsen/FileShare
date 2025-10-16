- **Admin UI:**  
  `http://localhost:9980/browser/dist/admin/admin.html`  
  Login: `admin` / `123!` (change these in the run command)

- **Discovery (should return XML):**  
  `http://localhost:9980/hosting/discovery`
  - This returns all URLs for the different filetypes.

  **URL string for the clinet**
  http://localhost:9980/browser/123abc/cool.html?WOPISrc=http://host.docker.internal:5018/wopi/files/cb6454de5e334a1886dbb076364b2c5c&acess_token=securetoken

**Client start-up guide**
inside the terminal cd inside the wopi-client folder
write: docker-compose up
the client is now up and running
navigate to wopi-host folder and use: dotnet run
go to swagger on port 5018 and use the urlBuilder to get the url for the document (temporary solution for testing)
