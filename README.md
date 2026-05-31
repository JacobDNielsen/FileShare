# FileShare

FileShare is a microservices-based file management and collaboration platform currently in early development. It serves as a reference implementation for a distributed system architecture incorporating fine-grained authorization and collaborative editing.

**Note:** This project is not production-grade and is intended for development and educational purposes only.

## Project Architecture

The system is built as a set of decoupled microservices, communicating over HTTP, HTTPS, or mTLS depending on the configuration.

### Core Services

- **API Gateway (YARP):** The central entry point for all client requests, responsible for routing to downstream services.
- **Auth Service:** Manages user identity, authentication, and issues JWT tokens for secure service-to-service communication.
- **Storage Service:** Handles file metadata and physical storage. It integrates with OpenFGA for fine-grained access control.
- **Lock Service:** Provides distributed locking capabilities to prevent concurrent modification conflicts.
- **WOPI Host:** Implements the Web Application Open Platform Interface protocol, allowing integration with online editors like Collabora or Office Online.
- **OpenFGA:** A relationship-based access control (ReBAC) engine used to manage complex permission models.

## Technology Stack

- **Backend:** .NET 9 (C#)
- **Frontend:** React with TypeScript
- **Database:** PostgreSQL
- **API Gateway:** YARP (Yet Another Reverse Proxy)
- **Authorization:** OpenFGA
- **Containerization:** Docker and Docker Compose
- **Testing:** k6 (Performance) and Bruno (API Collections)

## Repository Structure

- `src/svc/`: Source code for backend microservices.
- `src/web/frontend/`: React frontend application.
- `src/shared/`: Shared infrastructure and libraries.
- `src/k6/`: Performance and load testing scripts.
- `src/bruno/`: API request collections for development and testing.
- `docs/`: Technical documentation, architecture diagrams, and setup guides.

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- Node.js and npm (for frontend development)

### Quick Start with Docker

The recommended way to run the entire stack is using Docker Compose:

1. Copy `.env.example` to `.env` and configure the necessary environment variables.
2. Generate development certificates as described in `docs/https-certificate.md`.
3. Start the services:
   ```bash
   docker compose up --build
   ```

For detailed instructions on local development, including running services individually and configuring mTLS, refer to [docs/local-development.md](docs/local-development.md).

## Documentation

Detailed documentation is available in the `docs/` directory:

- [Local Development Setup](docs/local-development.md)
- [Database Schema](docs/database.md)
- [Authentication and JWT](docs/authentication_secret.md)
- [HTTPS and Certificates](docs/https-certificate.md)
- [OpenFGA Authorization Model](docs/OpenFGA.md)
- [Swagger/OpenAPI Details](docs/swagger-openapi.md)
- [Collabora Integration](docs/collabora_setup.md)
