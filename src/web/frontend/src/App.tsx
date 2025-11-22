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
    setWopiResult(null);
    setError("");
    alert("Logged out successfully!");
  };

  const callStorageApi = async () => {
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

  const FILE_ID: string = "ed6297cdfad24ba29fee66ca50f47b87";
  const callWopiApi = async () => {
    setError("");
    setWopiResult(null);
    try {
      const response = await wopiHostApiClient.get<string>(
        `/wopi/files/${FILE_ID}/urlBuilder`
      );
      setWopiResult(response.data);
    } catch (err) {
      console.error("WOPI Host API error:", err);
      setError("Failed to fetch from WOPI Host API.");
    }
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
        <button onClick={callStorageApi} disabled={!isLoggedIn}>
          Call Storage API
        </button>
        <button onClick={callWopiApi} disabled={!isLoggedIn}>
          Call WOPI Host API
        </button>
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
