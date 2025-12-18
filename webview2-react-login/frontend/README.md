# React frontend (build → static files)

This repo’s WPF host loads static files from `src/DesktopHostWpf/www`.

Use any React setup you prefer (CRA, Vite, Next.js export, etc.) as long as it produces static assets.

## Recommended message contract

Frontend → .NET:

```js
window.chrome.webview.postMessage({ type: "login", requestId, username, password });
window.chrome.webview.postMessage({ type: "getSession", requestId, token });
window.chrome.webview.postMessage({ type: "logout", requestId, token });
```

.NET → Frontend:

```js
// { type, requestId, payload }
// payload shapes:
// - loginResult: { ok, token, user, error }
// - session: { ok, user }
// - logoutResult: { ok }
// - error: { ok: false, error }
```

## Build + sync

After `npm run build`, copy the output to `src/DesktopHostWpf/www`:

```powershell
powershell -ExecutionPolicy Bypass -File ..\\scripts\\sync-frontend-build.ps1
```

