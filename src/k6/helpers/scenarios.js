import { getEnvVariable } from "./env.js";

function getConnectionOptions() {
  const connectionMode = getEnvVariable("CONNECTION_MODE");

  if (!connectionMode) {
    return {};
  }

  switch (connectionMode) {
    case "no-reuse":
      return {
        noConnectionReuse: true,
      };

    case "no-vu-reuse":
      return {
        noVUConnectionReuse: true,
      };

    default:
      throw new Error(
        `Invalid CONNECTION_MODE '${connectionMode}'. Provide either "no-reuse" or "no-vu-reuse"`,
      );
  }
}

export function scenarioOption(scenarioName) {
  const scenarios = {
    smoke: {
      vus: 1,
      iterations: 2,
    },
    stress: {
      stages: [
        { duration: "15s", target: 10 },
        { duration: "30s", target: 50 },
        { duration: "60s", target: 50 },
        { duration: "15s", target: 0 },
      ],
    },
    spike: {
      stages: [
        { duration: "1m", target: 200 },
        { duration: "30s", target: 0 },
      ],
    },
    breakpoint: {
      stages: [
        { duration: "30s", target: 50 },
        { duration: "30s", target: 100 },
        { duration: "30s", target: 200 },
        { duration: "30s", target: 400 },
        { duration: "30s", target: 0 },
      ],
    },
  };

  const picked = scenarios[scenarioName];
  if (!picked) {
    throw new Error(
      `Scenario '${scenarioName}' is not defined. Available scenarios: ${Object.keys(scenarios).join(", ")}`,
    );
  }

  const INSECURE_SKIP_TLS_VERIFY =
    getEnvVariable("INSECURE_SKIP_TLS_VERIFY", { fallback: "false" }) ===
    "true";

  return {
    insecureSkipTLSVerify: INSECURE_SKIP_TLS_VERIFY,
    ...getConnectionOptions(),
    ...picked,
    thresholds: {
      http_req_failed: ["rate<0.02"], // transport errors
      checks: ["rate>0.98"], // % of checks that must pass
      http_req_duration: ["p(95)<1000"],
    },
    summaryTrendStats: ["avg", "min", "med", "max", "p(90)", "p(95)", "p(99)"],
  };
}
