# OpenFGA setup

This document describes how to set up OpenFGA for the FileShare project.

The setup consists of three steps:

- create a store
- create an authorization model
- add the OpenFGA values to the `.env` file

---

## Prerequisites

Before starting, make sure:

- Docker Compose is running
- the `openfga` service is up
- Bruno is configured to call the correct OpenFGA endpoint

OpenFGA may be reachable in one of two ways depending on your setup:

- directly, for example `http://localhost:8080`
- through the API Gateway, for example `https://localhost:8089/openfga`

Use the same `baseUrl` in Bruno that matches your local environment.

---

## 1. Create a store

In Bruno, create a request that sends a `POST` request to:

{{baseUrl}}/stores

Set the store name to `fileshare`.

After sending the request, copy the returned store ID. This value will be used in the next step.

---

## 2. Create an authorization model

In Bruno, create a second request that sends a `POST` request to:

{{baseUrl}}/stores/{{storeId}}/authorization-models

Replace `{{storeId}}` with the store ID from the previous step.

Use the authorization model required by the project in the request body.

After sending the request, copy the returned authorization model ID. This value will be used in the `.env` file.

---

## 3. Add the values to `.env`

After creating the store and authorization model, add the values to the `.env` file in the project root.

OPENFGA_BASEURL=http://openfga:8080
OPENFGA_STORE_ID=<your-store-id>
OPENFGA_MODEL_ID=<your-model-id>

When running with Docker Compose, `OPENFGA_BASEURL` should normally be:

OPENFGA_BASEURL=http://openfga:8080

This is because the services communicate with OpenFGA over the internal Docker network.

---

## 4. Restart the project

After updating `.env`, restart the containers so the new values are loaded:

docker compose down
docker compose up --build