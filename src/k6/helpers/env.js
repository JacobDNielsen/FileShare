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
