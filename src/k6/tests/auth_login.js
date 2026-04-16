import http from "k6/http";
import { check, sleep } from "k6";
import { getEnvVariable, getTlsOptions } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "stress" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

// Required inputs
const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });
const USERNAME = getEnvVariable("USERNAME", { required: true });
const PASSWORD = getEnvVariable("PASSWORD", { required: true });

export default function () {
  const body = JSON.stringify({
    username: USERNAME,
    password: PASSWORD,
  });

  const response = http.post(TARGET_URL, body, {
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
    },
    tags: {
      test: "auth_login",
      scenario: SCENARIO,
      protocol: TARGET_URL.startsWith("https") ? "https" : "http",
      ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
    },
    timeout: "30s",
  });

  check(response, {
    "is status 200": (r) => r.status === 200,
  });

  sleep(0.1);
}
