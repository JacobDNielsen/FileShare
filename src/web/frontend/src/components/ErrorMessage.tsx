interface ErrorMessageProps {
  error: string;
}

export function ErrorMessage({ error }: ErrorMessageProps) {
  if (!error) {
    return null;
  }

  return (
    <section style={{ color: "red" }}>
      <h2>Error:</h2>
      <p>{error}</p>
    </section>
  );
}
