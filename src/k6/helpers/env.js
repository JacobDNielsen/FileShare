export function getEnvVariable(variableName, options = {}) {
  const { required = false, fallback } = options;
  const value = __ENV[variableName];

  if (value !== undefined && value !== null && value !== "") {
    return value;
  }

  if (fallback !== undefined) {
    return fallback;
  }

  if (required) {
    throw new Error(`Missing required env variable: ${variableName}`);
  }

  return undefined;
}

export function getTlsOptions() {
  const certPath = getEnvVariable("CLIENT_CERT_PATH");
  const keyPath = getEnvVariable("CLIENT_KEY_PATH");
  const skipVerify = getEnvVariable("INSECURE_SKIP_TLS_VERIFY", { fallback: "false" });

  const opts = {};

  if (skipVerify === "true") {
    opts.insecureSkipTLSVerify = true;
  }

  if (certPath && keyPath) {
    opts.tlsAuth = [{ cert: open(certPath), key: open(keyPath) }];
  }

  return opts;
}

export function joinUrl(baseUrl, path) {
  if (!baseUrl || !path) {
    throw new Error("joinUrl requires baseUrl and path");
  }

  const normalizedBase = baseUrl.replace(/\/+$/, "");
  const normalizedPath = path.replace(/^\/+/, "");
  return `${normalizedBase}/${normalizedPath}`;
}
