import http from "k6/http";
import { check, sleep } from "k6";
import { getEnvVariable, getTlsOptions, joinUrl } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");
const SLEEP_SECONDS = Number(getEnvVariable("SLEEP_SECONDS", { fallback: "0" }));

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });
const GATEWAY_AUTH_URL = getEnvVariable("GATEWAY_AUTH_URL", { required: true });
const AUTH_LOGIN_PATH = getEnvVariable("GATEWAY_AUTH_LOGIN_PATH", { required: true });
const UPLOAD_PATH     = getEnvVariable("GATEWAY_STORAGE_UPLOAD_PATH",{ required: true });
const LIST_PATH       = getEnvVariable("GATEWAY_STORAGE_LIST_PATH",  { required: true });
const USERNAME        = getEnvVariable("USERNAME",                   { required: true });
const PASSWORD        = getEnvVariable("PASSWORD",                   { required: true });

export function setup() {
  const loginRes = http.post(
    joinUrl(GATEWAY_AUTH_URL, AUTH_LOGIN_PATH),
    JSON.stringify({ username: USERNAME, password: PASSWORD }),
    {
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      tags: { test: "gateway_storage_upload_files", operation: "setup_login" },
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
    test: "gateway_storage_upload_files",
    scenario: SCENARIO,
    protocol,
    ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
  };
  const authHeader = { Authorization: `Bearer ${data.token}` };

  const uploadRes = http.post(
    joinUrl(TARGET_URL, UPLOAD_PATH),
    { file: http.file(new Uint8Array([1, 2, 3]).buffer, "test.bin", "application/octet-stream") },
    {
      headers: authHeader,
      tags: { ...commonTags, operation: "upload" },
      timeout: "30s",
    }
  );

  check(uploadRes, { "upload status 201": (r) => r.status === 201 });

  const listRes = http.get(joinUrl(TARGET_URL, LIST_PATH), {
    headers: authHeader,
    tags: { ...commonTags, operation: "list" },
    timeout: "30s",
  });

  check(listRes, { "list status 200": (r) => r.status === 200 });

  if (SLEEP_SECONDS > 0) {
    sleep(SLEEP_SECONDS);
  }
}
