import http from "k6/http";
import { check, sleep } from "k6";
import { getEnvVariable, getTlsOptions } from "../helpers/env.js";
import { scenarioOption } from "../helpers/scenarios.js";

const SCENARIO = getEnvVariable("SCENARIO", { fallback: "smoke" });
const CONNECTION_MODE = getEnvVariable("CONNECTION_MODE");
const SLEEP_SECONDS = Number(getEnvVariable("SLEEP_SECONDS", { fallback: "0" }));

export const options = { ...scenarioOption(SCENARIO), ...getTlsOptions() };

const TARGET_URL = getEnvVariable("TARGET_URL", { required: true });

export default function () {
  const response = http.get(TARGET_URL, {
    tags: {
      test: "storage_direct_ping",
      scenario: SCENARIO,
      protocol: TARGET_URL.startsWith("https") ? "https" : "http",
      ...(CONNECTION_MODE ? { connection_mode: CONNECTION_MODE } : {}),
    },
    timeout: "30s",
  });

  check(response, {
    "is status 200": (r) => r.status === 200,
  });

  if (SLEEP_SECONDS > 0) {
    sleep(SLEEP_SECONDS);
  }
}
