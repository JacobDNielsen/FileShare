import http from "k6/http";
import { check, sleep } from "k6";
import { getEnvVariable, getTlsOptions, joinUrl } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");
const SLEEP_SECONDS = Number(getEnvVariable("SLEEP_SECONDS", { fallback: "0" }));

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });
const AUTH_URL = getEnvVariable("AUTH_URL", { required: true });
const AUTH_LOGIN_PATH = getEnvVariable("AUTH_LOGIN_PATH", { required: true });
const USERNAME = getEnvVariable("USERNAME", { required: true });
const PASSWORD = getEnvVariable("PASSWORD", { required: true });

export function setup() {
  const loginRes = http.post(
    joinUrl(AUTH_URL, AUTH_LOGIN_PATH),
    JSON.stringify({ username: USERNAME, password: PASSWORD }),
    {
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      tags: { test: "storage_direct_get_file", operation: "setup_login" },
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
    test: "storage_direct_get_file",
    scenario: SCENARIO,
    protocol,
    ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
  };
  const authHeader = { Authorization: `Bearer ${data.token}` };

  const getRes = http.get(
    TARGET_URL,
    {
      headers: authHeader,
      tags: { ...commonTags, operation: "get_file" },
      timeout: "30s",
    }
  );

  check(getRes, { "get status 200": (r) => r.status === 200 });

  if (SLEEP_SECONDS > 0) {
    sleep(SLEEP_SECONDS);
  }
}
