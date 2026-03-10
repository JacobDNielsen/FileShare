export function getEnvVariable(
  variableName,
  { required = false, fallback = undefined } = {},
) {
  const value = __ENV[variableName];

  if (
    (value === undefined || value === "") &&
    required &&
    fallback === undefined
  ) {
    throw new Error(
      `Environment variable '${variableName}' is required but not set`,
    );
  }

  if (value === undefined || value === "") {
    return fallback;
  }

  return value;
}
