interface WopiResultSectionProps {
  wopiResult: string | null;
}

export function WopiResultSection({ wopiResult }: WopiResultSectionProps) {
  if (wopiResult === null) {
    return null;
  }

  return (
    <section>
      <h2>WOPI Host API Result:</h2>
      <p>
        <a href={wopiResult} target="_blank" rel="noopener noreferrer">
          Open WOPI document
        </a>
      </p>
    </section>
  );
}
