# FileShare: Reference Architecture for Secure Microservice-based Collaboration

FileShare is a research project and reference implementation designed to explore secure microservice architectures and fine-grained access control. The codebase is used to investigate Relationship-Based Access Control (ReBAC) and the performance implications of secure service-to-service communication in distributed environments.

Note: This project is a research prototype and is not intended for production use. It lacks the necessary security hardening, error resilience, and operational features required for a production environment.

## Project Overview

FileShare is a research platform designed to investigate architectural patterns for secure, collaborative file management. The project provides a reference implementation for exploring the integration of fine-grained access control within distributed systems and evaluating the performance of secure communication protocols.

The implementation demonstrates the integration of relationship-based access control within a distributed environment and provides a basis for evaluating the trade-offs between security enforcement and system performance.

## Technology Stack

- **Backend:** .NET 9 (C#)
- **Frontend:** React with TypeScript
- **Database:** PostgreSQL
- **API Gateway:** YARP (Yet Another Reverse Proxy)
- **Authorization:** OpenFGA (Relationship-Based Access Control)
- **Protocols:** WOPI (Web Application Open Platform Interface), HTTP/HTTPS, mTLS
- **Containerization:** Docker and Docker Compose
- **Testing:** k6 (Performance) and Bruno (API Collections)

## System Architecture

The architecture is built on a set of decoupled microservices that communicate over RESTful APIs. This modular design ensures that concerns such as authentication, file persistence, and collaborative locking are separated, allowing individual services to be replaced or evolved independently.

### Core Components

- **API Gateway (YARP):** Acts as the central entry point and Policy Enforcement Point (PEP). It routes external requests to internal services and manages TLS/mTLS termination and forwarding.
- **WOPI Host:** Acts as a protocol gateway between WOPI clients (like Collabora Online) and internal microservices. It implements core WOPI operations such as `CheckFileInfo`, `GetFile`, and `PutFile` to facilitate collaborative editing.
- **Authorization Engine (OpenFGA):** A relationship-based access control engine that evaluates permissions based on a global graph of users, resources, and their relationships (e.g., owner, viewer). This decouples authorization policy from application logic.
- **Auth Service:** A centralized identity provider that handles user authentication and issues signed JSON Web Tokens (JWT). It exposes a JWKS endpoint, allowing other services to validate tokens locally without sharing private signing keys.
- **Storage Service:** Manages file metadata (PostgreSQL) and binary content. It abstracts the storage backend, providing a persistence layer that is decoupled from security and collaboration logic.
- **Lock Service:** Manages distributed locking semantics required by the WOPI protocol. It handles lock acquisition, refreshing, and automatic expiration (defaulting to 30 minutes) to prevent concurrent modification conflicts during editing sessions.

## Repository Structure

- `src/svc/`: Source code for backend microservices.
- `src/web/frontend/`: React frontend application.
- `src/shared/`: Shared infrastructure, libraries, and mTLS extensions.
- `src/k6/`: Performance and load testing scripts for latency measurement.
- `src/bruno/`: API request collections for development and testing.
- `docs/`: Technical documentation, architecture diagrams, and setup guides.

## Research Project Status and Limitations

FileShare has several documented limitations that reflects the current scope of being a Proof-of-Concept (PoC), presented in the following (non-exhaustive) list:

### 1. Authorization Enforcement
While the infrastructure for writing relationship tuples to OpenFGA is implemented, active permission enforcement is currently inconsistent across all API endpoints. Some core flows demonstrate active enforcement, while others remain in a PoC state and do not yet fully validate access tokens via OpenFGA checks.

### 2. Transactional Integrity
The system does not currently implement distributed transactions or the transactional outbox pattern. Operations that span multiple services, such as uploading a file to the Storage service and simultaneously writing an ownership tuple to OpenFGA, may result in partial failures or orphaned data if one component fails during the process.

### 3. Protocol Hardening and Compliance
The WOPI implementation covers a functional subset of the specification required for basic collaboration. It is not an exhaustive implementation of the protocol and lacks advanced features such as comprehensive version reporting, broad capability negotiation, and support for all optional WOPI headers.

### 4. Infrastructure and Orchestration
The project is designed for evaluation in locally controlled, containerized environments. It does not include production-oriented features such as:
- Kubernetes orchestration or service mesh integration.
- Automated service replication and load balancing.
- Horizontal scaling for stateful components (PostgreSQL/Local Storage).

### 5. Storage Abstraction and Scaling
The current Storage Service implementation relies on the local file system. This introduces a single point of failure and lacks the high availability and scalability of a distributed object storage backend (e.g., MinIO or S3).

### 6. Security and Secret Management
- **Secret Management:** Sensitive configuration (API keys, private keys) is managed via environment variables and `.env` files rather than a dedicated vault service.
- **Certificate Lifecycle:** Mutual TLS (mTLS) certificates are generated and managed manually, which is not suitable for dynamic or production-scale deployments.
- **Observability:** The system lacks centralized logging and distributed tracing, making it difficult to monitor complex request flows across microservice boundaries.

## Technical Documentation

Detailed documentation on specific components is available in the `docs/` directory:

- [Local Development Setup](docs/local-development.md)
- [Database Schema](docs/database.md)
- [Authentication and JWT Strategy](docs/authentication_secret.md)
- [HTTPS and Certificate Management](docs/https-certificate.md)
- [OpenFGA Authorization Model](docs/OpenFGA.md)
- [Collabora Integration Guide](docs/collabora_setup.md)

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker and Docker Compose
- Node.js and npm

### Quick Start
1. Copy `.env.example` to `.env`.
2. Generate development certificates (see `docs/https-certificate.md`).
3. Run `docker compose up --build`.
