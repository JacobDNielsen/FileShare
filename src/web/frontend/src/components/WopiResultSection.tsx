interface WopiResultSectionProps {
  wopiResult: string | null;
  onClearWopiResult: () => void;
}

export function WopiResultSection({
  wopiResult,
  onClearWopiResult,
}: WopiResultSectionProps) {
  if (wopiResult === null) {
    return null;
  }

  return (
    <section>
      <h2>WOPI Host API Result:</h2>
      <button onClick={() => onClearWopiResult()}>Clear</button>
      <p>
        <a href={wopiResult} target="_blank" rel="noopener noreferrer">
          Open WOPI document
        </a>
      </p>
    </section>
  );
}
