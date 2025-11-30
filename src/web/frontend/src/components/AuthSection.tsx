import type { FormEvent } from "react";

interface AuthSectionProps {
  username: string;
  password: string;
  isLoggedIn: boolean;
  loggedInUser: string | null;
  onUsernameChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onLogin: (e: FormEvent) => Promise<void>;
  onLogout: () => void;
}

export function AuthSection({
  username,
  password,
  isLoggedIn,
  loggedInUser,
  onUsernameChange,
  onPasswordChange,
  onLogin,
  onLogout,
}: AuthSectionProps) {
  return (
    <section>
      <h1>FileShare App</h1>
      {isLoggedIn && loggedInUser && <p>Hello, {loggedInUser}</p>}

      {!isLoggedIn && (
        <form onSubmit={onLogin} style={{ marginBottom: "20px" }}>
          <div>
            <label>Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => onUsernameChange(e.target.value)}
            />
          </div>

          <div>
            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => onPasswordChange(e.target.value)}
            />
          </div>

          <button type="submit">Login</button>
        </form>
      )}

      <button onClick={onLogout} disabled={!isLoggedIn}>
        Logout
      </button>
    </section>
  );
}
