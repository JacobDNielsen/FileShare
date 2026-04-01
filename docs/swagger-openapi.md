# Swagger // OpenAPI

Each microservice exposes Swagger UI in the `Development` environment only. Swagger is accessible both by hitting the service directly and through the API Gateway.

---

## Accessing Swagger UI

### Directly (service port)

| Service   | HTTP                            | HTTPS                            |
| --------- | ------------------------------- | -------------------------------- |
| Auth      | `http://localhost:5039/swagger` | `https://localhost:5040/swagger` |
| Storage   | `http://localhost:5134/swagger` | `https://localhost:5135/swagger` |
| Lock      | `http://localhost:5038/swagger` | `https://localhost:5037/swagger` |
| WOPI Host | `http://localhost:5018/swagger` | `https://localhost:5019/swagger` |

### Via the API Gateway

| Service   | HTTP                                    | HTTPS                                    |
| --------- | --------------------------------------- | ---------------------------------------- |
| Auth      | `http://localhost:8088/auth/swagger`    | `https://localhost:8089/auth/swagger`    |
| Storage   | `http://localhost:8088/storage/swagger` | `https://localhost:8089/storage/swagger` |
| Lock      | `http://localhost:8088/lock/swagger`    | `https://localhost:8089/lock/swagger`    |
| WOPI Host | `http://localhost:8088/wopi/swagger`    | `https://localhost:8089/wopi/swagger`    |

---

## How `SwaggerGatewayPrefix` works

When accessing Swagger UI through the gateway, the "Try it out" button must send requests back through the gateway prefix (for example: `/auth/...`) rather than to the service root (`/`). This is controlled by the OpenAPI `servers` list.

Each service registers two OpenAPI server URLs at startup:

```csharp
s.AddServer(new OpenApiServer { Url = "/" });                    // direct access
s.AddServer(new OpenApiServer { Url = gatewayPrefix });          // via gateway
```

The `gatewayPrefix` is read from the `SwaggerGatewayPrefix` configuration key. In Docker, this is set per service in `docker-compose.yml`:

```yaml
auth:
  environment:
    SwaggerGatewayPrefix: /auth

storage:
  environment:
    SwaggerGatewayPrefix: /storage

lock:
  environment:
    SwaggerGatewayPrefix: /lock

wopi-host:
  environment:
    SwaggerGatewayPrefix: /wopi
```

`SwaggerGatewayPrefix` is configured in two places:

- **`docker-compose.yml`** — set per service as an environment variable
- **`launchSettings.json`** — set in both `http` and `https` profiles for each service

Thus, the gateway prefix server URL is always available regardless of how the project is run. In Swagger UI, use the **Servers** dropdown to choose between direct access (`/`) and access via the gateway (e.g. `/auth`).

---

## Gateway routing for Swagger

The YARP gateway has dedicated routes that forward Swagger traffic to each service by stripping the prefix:

| Gateway path    | Forwards to                         |
| --------------- | ----------------------------------- |
| `/auth/{**}`    | Auth service (strips `/auth`)       |
| `/storage/{**}` | Storage service (strips `/storage`) |
| `/lock/{**}`    | Lock service (strips `/lock`)       |
| `/wopi/{**}`    | WOPI Host (strips `/wopi`)          |

These routes are defined in `src/svc/api-gateway-yarp/appsettings.json`.
