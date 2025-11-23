import { useState } from "react";
import type { FormEvent } from "react";
import Cookies from "js-cookie";
import {
  authApiClient,
  storageApiClient,
  wopiHostApiClient,
} from "./apiClients";

interface LoginResponse {
  userName: string;
  tokenType: string;
  accessToken: string;
}

interface CheckFileInfoResponse {
  baseFileName: string;
  size: number;
  ownerId: string;
  userId: string;
  version: string;
  userCanWrite: boolean;
}

type JSON = string | number | boolean | null | { [key: string]: JSON } | JSON[];

function App() {
  const [username, setUsername] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [loggedInUser, setLoggedInUser] = useState<string | null>(() => {
    return Cookies.get("username") || null;
  });
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(() => {
    return !!Cookies.get("jwt");
  });
  const [storageResult, setStorageResult] = useState<JSON | null>(null);
  const [wopiResult, setWopiResult] = useState<string | null>(null);
  const [fileId, setFileId] = useState<string>("");
  const [fileInfo, setFileInfo] = useState<CheckFileInfoResponse | null>(null);
  const [error, setError] = useState<string>("");

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    try {
      const response = await authApiClient.post<LoginResponse>(
        "/authentication/login",
        {
          username,
          password,
        }
      );

      const { userName, accessToken } = response.data;

      Cookies.set("jwt", accessToken, {
        path: "/",
      });

      Cookies.set("username", userName, {
        path: "/",
      });

      setLoggedInUser(userName);
      setIsLoggedIn(true);
      setUsername("");
      setPassword("");
      alert("Login successful! Storing JWT in cookie.");
    } catch (err) {
      console.error("Login error:", err);
      setError("Login failed. Please check your credentials.");
    }
  };

  const handleLogout = () => {
    Cookies.remove("jwt", { path: "/" });
    Cookies.remove("username", { path: "/" });
    setLoggedInUser(null);
    setIsLoggedIn(false);
    setUsername("");
    setPassword("");
    setStorageResult(null);
    setFileId("");
    setFileInfo(null);
    setWopiResult(null);
    setError("");
    alert("Logged out successfully!");
  };

  const handleLoadFileInfo = async () => {
    setError("");

    if (!fileId.trim()) {
      setError("Please enter a valid File ID.");
      return;
    }
    try {
      const response = await storageApiClient.get<CheckFileInfoResponse>(
        `/wopi/files/${encodeURIComponent(fileId)}`
      );
      setFileInfo(response.data);
    } catch (err) {
      console.error("Storage API error:", err);
      setFileInfo(null);
      setError("Failed to fetch file info from Storage API. Correct fileid?");
    }
  };

  const callStorageApiGetAllFiles = async () => {
    setError("");
    setStorageResult(null);
    try {
      const response = await storageApiClient.get<JSON>("/wopi/files");
      setStorageResult(response.data);
    } catch (err) {
      console.error("Storage API error:", err);
      setError("Failed to fetch from Storage API.");
    }
  };

  const callWopiApiGetFile = async () => {
    setError("");
    setWopiResult(null);

    if (!fileId.trim()) {
      setError("Please enter a valid File ID.");
      return;
    }

    if (!fileInfo) {
      setError("No file info available. Correct fileid?");
      return;
    }

    try {
      const fileName = fileInfo.baseFileName || "downloaded-file";
      const response = await wopiHostApiClient.get<Blob>(
        `/wopi/files/${encodeURIComponent(fileId)}/contents`,
        { responseType: "blob" }
      );

      const url = window.URL.createObjectURL(response.data);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error("WOPI Host API error:", err);
      setError("Failed to fetch from WOPI Host API.");
    }
  };

  const callWopiApiUrlBuilder = async () => {
    setError("");
    setWopiResult(null);

    if (!fileId.trim()) {
      setError("Please enter a valid File ID.");
      return;
    }

    if (!fileInfo) {
      setError("No file info available. Please load file info first.");
      return;
    }

    try {
      const response = await wopiHostApiClient.get<string>(
        `/wopi/files/${encodeURI(fileId)}/urlBuilder`
      );
      setWopiResult(response.data);
    } catch (err) {
      console.error("WOPI Host API error:", err);
      setError("Failed to fetch from WOPI Host API.");
    }
  };

  //Helpers
  const disabledBtnStyle = {
    opacity: 0.5,
    cursor: "not-allowed",
  };

  return (
    <div>
      <h1>FileShare App</h1>
      {isLoggedIn && loggedInUser && <p>Hello, {loggedInUser}</p>}

      {!isLoggedIn && (
        <form onSubmit={handleLogin} style={{ marginBottom: "20px" }}>
          <div>
            <label>Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>

          <div>
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <button type="submit">Login</button>
        </form>
      )}

      <div>
        <button onClick={handleLogout} disabled={!isLoggedIn}>
          Logout
        </button>
        <button
          onClick={callStorageApiGetAllFiles}
          disabled={!isLoggedIn}
          style={!isLoggedIn ? disabledBtnStyle : undefined}
          title={!isLoggedIn ? "Login to enable Storage API calls" : undefined}
        >
          GetAllFiles (Storage API)
        </button>
        <div style={{ marginTop: "20px" }}>
          <input
            type="text"
            value={fileId}
            onChange={(e) => {
              setFileId(e.target.value);
              setFileInfo(null);
            }}
            placeholder="Enter the file ID"
          />
          <button
            onClick={handleLoadFileInfo}
            disabled={!isLoggedIn || !fileId.trim()}
            style={!isLoggedIn || !fileId.trim() ? disabledBtnStyle : undefined}
            title={
              !isLoggedIn
                ? "Login to enable Storage API calls"
                : !fileId.trim()
                  ? "Enter a File ID to enable"
                  : undefined
            }
          >
            Load File Info
          </button>
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
            onClick={callWopiApiGetFile}
            disabled={!isLoggedIn || !fileId}
            style={!isLoggedIn || !fileId ? disabledBtnStyle : undefined}
            title={
              !isLoggedIn
                ? "Login to enable WOPI Host API calls"
                : !fileId
                  ? "Enter a File ID to enable"
                  : undefined
            }
          >
            Call GetFile (WOPI Host API download file)
          </button>
          <button
            onClick={callWopiApiUrlBuilder}
            disabled={!isLoggedIn || !fileId}
            style={
              !isLoggedIn
                ? disabledBtnStyle
                : !fileId
                  ? disabledBtnStyle
                  : undefined
            }
            title={
              !isLoggedIn
                ? "Login to enable WOPI Host API calls"
                : !fileId
                  ? "Enter a File ID to enable"
                  : undefined
            }
          >
            Call URL Builder (WOPI Host API)
          </button>
        </div>
      </div>
      {error && <p style={{ color: "red" }}>{error}</p>}

      {storageResult !== null && (
        <div>
          <h2>Storage API Result:</h2>
          <pre>{JSON.stringify(storageResult, null, 2)}</pre>
        </div>
      )}

      {wopiResult !== null && (
        <div>
          <h2>WOPI Host API Result:</h2>
          <p>
            <a href={wopiResult} target="_blank" rel="noopener noreferrer">
              Open WOPI document
            </a>
          </p>
        </div>
      )}
    </div>
  );
}

export default App;
