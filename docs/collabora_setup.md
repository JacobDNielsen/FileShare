# Collabora (CODE) Docker Setup

We use the DEV version called **Collabora Online Development Edition (CODE)** in Docker.

The following is a guide to setup CODE in Docker, and to allow for the C# backend **(WOPI host)** to talk to that container at.

**Security note:** We deliberately disable SSL **inside** CODE. It should be considered to add a reverse proxy at a later time.

---

## 1) Pull the image

```bash
docker pull collabora/code:latest
```
This downloads the CODE image (remember to also have Docker Desktop open) 

---

## 2) Run the container

### Windows PowerShell
```powershell
docker run -d --rm `
  -p 9980:9980 `
  -e "aliasgroup1=http://host.docker.internal:5108" `
  -e "extra_params=--o:ssl.enable=false" `
  -e "username=admin" `
  -e "password=123!" `
  --name collabora `
  collabora/code:latest
```
- **Explanation of the above configs:**
  - `-d:`
  -         *Purpose:* Runs the docker image in detached mode
  - `--rm:`
  -         *Purpose:* Deletes the docker image after it is shutdown.
  - `-p:`
  -     *Purpose:* Maps from port 9980 inside container to port 9980 on host
  - `--name collabora:`
  -         *Purpose:* Name we set for the container, can be whatever
  - `collabora/code:latest:`
  -         *Purpose:* The docker image we want to boot up
  
## The parameters setting username & password (for admin) can be changed to whatever is wanted.

---

## 3) Verify it's running

- **Admin UI:**  
  `http://localhost:9980/browser/dist/admin/admin.html`  
  Login: `admin` / `123!` (change these in the run command)

- **Discovery (should return XML):**  
  `http://localhost:9980/hosting/discovery`
  - This returns all URLs for the different filetypes.

---

## 4) Explanation to environment variables

- `aliasgroup1=http://host.docker.internal:5108`: Authorizes our WOPI host (scheme + host + port must match exactly). In our case it is localhost:5108 we run the WOPI host on. 
- `extra_params=--o:ssl.enable=false`: Disables SSL **inside** CODE (We use HTTP for DEV.
- `username` / `password`: Creates an admin login for the Collabora admin UI.

---

---

## 5) Helpful variations

- **Auto-restart on reboot (instead of `--rm`):**
  ```bash
  --restart unless-stopped
  ```
  (don't use `--rm` when using a restart policy)

- **Limit dictionaries (optional, saves RAM):**
  ```bash
  -e "dictionaries=en_US da_DK"
  ```

- **Prep for reverse proxy TLS (Nginx/Traefik) later:**
  ```bash
  -e "extra_params=--o:ssl.enable=false --o:ssl.termination=true"
  ```

---

## 6) Troubleshooting

- **WOPI host not allowed**: Make sure `aliasgroup1` exactly matches the WOPI base URL used by CODE (including scheme and port).
- **CODE can't reach the WOPI host**: From inside the container:
- **Blank iframe or CSP warning**: Try to use private/incognito window (Adblockers can inject CSP).

---

## 7) Useful documentations:
- Simple tutorial to integrating CODE: https://sdk.collaboraonline.com/docs/Step_by_step_tutorial.html 
- Example of CODE integration in dotNET project: https://github.com/CollaboraOnline/collabora-online-sdk-examples//tree/master/webapp/dotNET 