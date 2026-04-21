import http from "k6/http";
import { check } from "k6";
import { getEnvVariable, getTlsOptions } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

const TARGET_URL     = getEnvVariable("TARGET_URL",          { required: true });
const AUTH_URL       = getEnvVariable("AUTH_URL",            { required: true });
const AUTH_LOGIN_PATH  = getEnvVariable("AUTH_LOGIN_PATH",   { required: true });
const UPLOAD_PATH    = getEnvVariable("STORAGE_UPLOAD_PATH", { required: true });
const LIST_PATH      = getEnvVariable("STORAGE_LIST_PATH",   { required: true });
const USERNAME       = getEnvVariable("USERNAME",            { required: true });
const PASSWORD       = getEnvVariable("PASSWORD",            { required: true });

export function setup() {
  const loginRes = http.post(
    `${AUTH_URL}${AUTH_LOGIN_PATH}`,
    JSON.stringify({ username: USERNAME, password: PASSWORD }),
    {
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      tags: { test: "storage_direct_files", operation: "setup_login" },
      timeout: "30s",
    }
  );

  check(loginRes, { "login status 200": (r) => r.status === 200 });

  const body = loginRes.json();
  const token = body.accessToken || body.token;
  if (!token) throw new Error("Login did not return a token");
  return { token };
}

export default function (data) {
  const protocol = TARGET_URL.startsWith("https") ? "https" : "http";
  const commonTags = {
    test: "storage_direct_files",
    scenario: SCENARIO,
    protocol,
    ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
  };
  const authHeader = { Authorization: `Bearer ${data.token}` };

  const uploadRes = http.post(
    `${TARGET_URL}${UPLOAD_PATH}`,
    { file: http.file(new Uint8Array([1, 2, 3]).buffer, "test.bin", "application/octet-stream") },
    {
      headers: authHeader,
      tags: { ...commonTags, operation: "upload" },
      timeout: "30s",
    }
  );

  check(uploadRes, { "upload status 201": (r) => r.status === 201 });

  const listRes = http.get(`${TARGET_URL}${LIST_PATH}`, {
    headers: authHeader,
    tags: { ...commonTags, operation: "list" },
    timeout: "30s",
  });

  check(listRes, { "list status 200": (r) => r.status === 200 });
}
