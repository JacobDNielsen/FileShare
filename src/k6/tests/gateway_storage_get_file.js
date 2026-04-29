import http from "k6/http";
import { check } from "k6";
import { getEnvVariable, getTlsOptions } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });
const AUTH_LOGIN_PATH = getEnvVariable("GATEWAY_AUTH_LOGIN_PATH", { required: true });
const FILE_ID = getEnvVariable("FILE_ID", { required: true });
const FILE_CONTENTS_PATH = getEnvVariable("GATEWAY_STORAGE_FILE_CONTENTS_PATH", { required: true });

const USERNAME = getEnvVariable("USERNAME", { required: true });
const PASSWORD = getEnvVariable("PASSWORD", { required: true });

export function setup() {
  const loginRes = http.post(
    `${TARGET_URL}${AUTH_LOGIN_PATH}`,
    JSON.stringify({ username: USERNAME, password: PASSWORD }),
    {
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      tags: { test: "gateway_storage_get_file", operation: "setup_login" },
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
    test: "gateway_storage_get_file",
    scenario: SCENARIO,
    protocol,
    ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
  };
  const authHeader = { Authorization: `Bearer ${data.token}` };

  const getRes = http.get(
    `${TARGET_URL}${FILE_CONTENTS_PATH}`,
    {
      headers: authHeader,
      tags: { ...commonTags, operation: "get_file" },
      timeout: "30s",
    }
  );

  check(getRes, { "get status 200": (r) => r.status === 200 });
}
