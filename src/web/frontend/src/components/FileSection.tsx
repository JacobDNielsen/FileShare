import type { CheckFileInfoResponse } from "../models/models";

export interface FileSectionProps {
  isLoggedIn: boolean;
  fileId: string;
  fileInfo: CheckFileInfoResponse | null;
  onFileIdChange: (value: string) => void;
  onLoadFileInfo: () => Promise<void>;
  onClearFileInfo: () => void;
  onGetAllFiles: () => Promise<void>;
  onDownloadFile: () => Promise<void>;
  onBuildUrl: () => Promise<void>;
  isLoadingFileInfo: boolean;
}

export function FileSection({
  isLoggedIn,
  fileId,
  fileInfo,
  onFileIdChange,
  onLoadFileInfo,
  onClearFileInfo,
  onGetAllFiles,
  onDownloadFile,
  onBuildUrl,
  isLoadingFileInfo,
}: FileSectionProps) {
  const disableStorageButton = !isLoggedIn;
  const disableLoadFileInfo = !isLoggedIn || !fileId.trim();
  const disableWopiButtons = !isLoggedIn || !fileInfo;
  const disabledBtnStyle = {
    opacity: 0.5,
    cursor: "not-allowed",
  };

  return (
    <section>
      <h2>GetAllFiles</h2>
      <div>
        <button
          onClick={onGetAllFiles}
          disabled={disableStorageButton}
          style={disableStorageButton ? disabledBtnStyle : undefined}
          title={
            disableStorageButton
              ? "Login to enable Storage API calls"
              : undefined
          }
        >
          {isLoadingFileInfo ? "Loading.. " : "GetAllFiles (Storage API)"}
        </button>
      </div>

      <div>
        <input
          type="text"
          value={fileId}
          onChange={(e) => onFileIdChange(e.target.value)}
          placeholder="Plz enter file id :)"
        />
        <button
          onClick={onLoadFileInfo}
          disabled={disableLoadFileInfo}
          style={disableLoadFileInfo ? disabledBtnStyle : undefined}
          title={
            !isLoggedIn
              ? "Login to enable Storage API calls"
              : !fileId.trim()
                ? "Enter a File ID to enable"
                : undefined
          }
        >
          {isLoadingFileInfo ? "Loading.. " : "Load File Info (Storage API)"}
        </button>
        {fileInfo && <button onClick={onClearFileInfo}>Clear</button>}
      </div>

      {fileInfo && (
        <div>
          <p>
            <strong>File Name:</strong> {fileInfo.baseFileName}
          </p>
          <p>
            <strong>Size:</strong> {fileInfo.size} bytes
          </p>
          <p>
            <strong>Owner ID:</strong> {fileInfo.ownerId}
          </p>
          <p>
            <strong>User ID:</strong> {fileInfo.userId}
          </p>
          <p>
            <strong>Version:</strong> {fileInfo.version}
          </p>
          <p>
            <strong>User Can Write:</strong>{" "}
            {fileInfo.userCanWrite ? "Yes" : "No"}
          </p>
        </div>
      )}

      <div>
        <button
          onClick={onDownloadFile}
          disabled={disableWopiButtons}
          style={disableWopiButtons ? disabledBtnStyle : undefined}
          title={
            !disableWopiButtons
              ? undefined
              : !isLoggedIn
                ? "Login to enable WOPI Host API calls"
                : !fileInfo
                  ? "Search for a file to enable"
                  : undefined
          }
        >
          Call GetFile (WOPI Host API download file)
        </button>
        <button
          onClick={onBuildUrl}
          disabled={disableWopiButtons}
          style={disableWopiButtons ? disabledBtnStyle : undefined}
          title={
            !disableWopiButtons
              ? undefined
              : !isLoggedIn
                ? "Login to enable WOPI Host API calls"
                : !fileInfo
                  ? "Search for a file to enable"
                  : undefined
          }
        >
          Call URL builder (WOPI Host API)
        </button>
      </div>
    </section>
  );
}
