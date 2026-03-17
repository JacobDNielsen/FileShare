import { getEnvVariable } from "./env.js";

export function scenarioOption(scenarioName) {
  const scenarios = {
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
        { duration: "10s", target: 0 },
        { duration: "10s", target: 200 },
        { duration: "30s", target: 200 },
        { duration: "10s", target: 0 },
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
    stages: picked.stages,
    thresholds: {
      http_req_failed: ["rate<0.02"], // transport errors
      checks: ["rate>0.98"], // % of checks that must pass
      http_req_duration: ["p(95)<1000"],
    },
  };
}
