import type { JSONValue } from "../models/models";

interface StorageResultSectionProps {
  storageResult: JSONValue | null;
  onClear: () => void;
}

export function StorageResultSection({
  storageResult,
  onClear,
}: StorageResultSectionProps) {
  if (storageResult === null) {
    return null;
  }

  return (
    <section>
      <h2>Storage API Result:</h2>
      <button onClick={onClear}>Clear</button>
      <pre>{JSON.stringify(storageResult, null, 2)}</pre>
    </section>
  );
}
