import { useState } from "react";
import type { FormEvent } from "react";
import Cookies from "js-cookie";
import {
  authApiClient,
  storageApiClient,
  wopiHostApiClient,
} from "./apiClients";
import type {
  LoginResponse,
  CheckFileInfoResponse,
  JSONValue,
} from "./models/models";

import { AuthSection } from "./components/AuthSection";
import { FileSection } from "./components/FileSection";
import { StorageResultSection } from "./components/StorageResultSection";
import { WopiResultSection } from "./components/WopiResultSection";
import { ErrorMessage } from "./components/ErrorMessage";

function App() {
  const [username, setUsername] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [loggedInUser, setLoggedInUser] = useState<string | null>(() => {
    return Cookies.get("username") || null;
  });
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(() => {
    return !!Cookies.get("jwt");
  });
  const [isloadingFileInfo, setIsloadingFileInfo] = useState<boolean>(false);
  const [storageResult, setStorageResult] = useState<JSONValue | null>(null);
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
      setIsloadingFileInfo(true);
      const response = await storageApiClient.get<CheckFileInfoResponse>(
        `/wopi/files/${encodeURIComponent(fileId)}`
      );
      setFileInfo(response.data);
    } catch (err) {
      console.error("Storage API error:", err);
      setFileInfo(null);
      setError("Failed to fetch file info from Storage API. Correct fileid?");
    } finally {
      setIsloadingFileInfo(false);
    }
  };

  const callStorageApiGetAllFiles = async () => {
    setError("");
    setStorageResult(null);
    try {
      setIsloadingFileInfo(true);
      const response = await storageApiClient.get<JSONValue>("/wopi/files");
      setStorageResult(response.data);
    } catch (err) {
      console.error("Storage API error:", err);
      setError("Failed to fetch from Storage API.");
    } finally {
      setIsloadingFileInfo(false);
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
      setError("No file info available. Correct fileid?");
      return;
    }

    try {
      const response = await wopiHostApiClient.get<string>(
        `/wopi/files/${encodeURIComponent(fileId)}/urlBuilder`
      );
      setWopiResult(response.data);
    } catch (err) {
      console.error("WOPI Host API error:", err);
      setError("Failed to fetch from WOPI Host API.");
    }
  };

  //Helpers
  const handleFileIdChange = (value: string) => {
    setFileId(value);
    setFileInfo(null);
  };

  return (
    <div>
      <AuthSection
        username={username}
        password={password}
        isLoggedIn={isLoggedIn}
        loggedInUser={loggedInUser}
        onUsernameChange={setUsername}
        onPasswordChange={setPassword}
        onLogin={handleLogin}
        onLogout={handleLogout}
      />

      <FileSection
        isLoggedIn={isLoggedIn}
        fileId={fileId}
        fileInfo={fileInfo}
        onFileIdChange={handleFileIdChange}
        onLoadFileInfo={handleLoadFileInfo}
        onClearFileInfo={() => setFileInfo(null)}
        onGetAllFiles={callStorageApiGetAllFiles}
        onDownloadFile={callWopiApiGetFile}
        onBuildUrl={callWopiApiUrlBuilder}
        isLoadingFileInfo={isloadingFileInfo}
      />
      <ErrorMessage error={error} />
      <StorageResultSection
        storageResult={storageResult}
        onClear={() => setStorageResult(null)}
      />

      <WopiResultSection wopiResult={wopiResult} />
    </div>
  );
}

export default App;
