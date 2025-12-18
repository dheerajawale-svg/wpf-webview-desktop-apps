import { useEffect, useMemo, useState } from "react";
import { getSession, login, logout, startHostListener } from "./webviewBridge";

type User = { username: string; displayName: string };

export default function App() {
  const [token, setToken] = useState(() => localStorage.getItem("token") ?? "");
  const [user, setUser] = useState<User | null>(null);
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("admin123");
  const [error, setError] = useState("");

  const bridgeState = useMemo(() => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    return typeof window !== "undefined" && !!(window as any).chrome?.webview ? "connected" : "missing";
  }, []);

  useEffect(() => {
    startHostListener();
  }, []);

  useEffect(() => {
    if (!token) return;
    getSession(token)
      .then((s) => {
        if (s.ok && s.user) setUser(s.user);
      })
      .catch((e: unknown) => setError(e instanceof Error ? e.message : "Failed to restore session."));
  }, [token]);

  async function onLogin() {
    setError("");
    try {
      const result = await login(username, password);
      if (!result.ok || !result.token || !result.user) {
        setError(result.error ?? "Invalid username or password.");
        return;
      }
      localStorage.setItem("token", result.token);
      setToken(result.token);
      setUser(result.user);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Login failed.");
    }
  }

  async function onLogout() {
    if (!token) return;
    setError("");
    try {
      await logout(token);
    } finally {
      localStorage.removeItem("token");
      setToken("");
      setUser(null);
    }
  }

  return (
    <div className="wrap">
      <div className="card">
        <div className="row">
          <h1>{user ? "You are logged in" : "Login"}</h1>
          <p>
            Bridge: <span className="mono">{bridgeState}</span>
          </p>
          {!user ? (
            <>
              <div className="row">
                <label>Test Username</label>
                <input value={username} onChange={(e) => setUsername(e.target.value)} />
                <label>Password</label>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </div>
              <div className="actions">
                <button onClick={onLogin}>Login</button>
              </div>
              <div className="hint">Dummy users: admin/admin123, demo/demo123</div>
            </>
          ) : (
            <>
              <div className="pill">
                <span>Test User:</span>
                <span className="mono">
                  {user.displayName} ({user.username})
                </span>
              </div>
              <div className="pill">
                <span>Token:</span>
                <span className="mono">{token}</span>
              </div>
              <div className="actions">
                <button className="secondary" onClick={onLogout}>
                  Logout
                </button>
              </div>
            </>
          )}
          {error ? <div className="error">{error}</div> : null}
        </div>
      </div>
    </div>
  );
}

