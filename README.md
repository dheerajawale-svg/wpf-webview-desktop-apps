# WebView2 + React (static build) + .NET (WPF) starter

This is a desktop app where:

- Frontend UI is a React app built to static files (`npm run build`).
- The static files are shipped with the .NET desktop app.
- WebView2 renders the UI.
- .NET ↔ JavaScript communication uses `window.chrome.webview.postMessage(...)` and `CoreWebView2.WebMessageReceived`.
- Login uses dummy users + an in-memory token/session store (no DB).

## Quick start (without React build)

The app ships with a tiny static UI in `src/DesktopHostWpf/www` so you can run immediately.

1. Open `webview2-react-login/WebView2ReactLogin.sln` in Visual Studio
2. Restore NuGet packages
3. Run `DesktopHostWpf`

## Use with a React build

1. Build your React app:
   - CRA: `npm run build` (outputs `frontend/build`)
   - Vite: `npm run build` (outputs `frontend/dist`)
2. Sync the build output into the host `www` folder:
   - `powershell -ExecutionPolicy Bypass -File scripts/sync-frontend-build.ps1`
3. Run the WPF app again.

## Dummy users

- `admin / admin123`
- `demo / demo123`

## Packaging notes

- WebView2 requires the Microsoft Edge WebView2 Runtime (Evergreen) on the target machine.
  - You can ship the Evergreen Bootstrapper installer, or use a fixed-version runtime.
- Typical options:
  - Installer (recommended): WiX Toolset, Inno Setup, MSIX.
  - App-only zip: ship `DesktopHostWpf.exe` + `www/` folder and document the WebView2 Runtime requirement.
- For publishing:
  - `dotnet publish src/DesktopHostWpf/DesktopHostWpf.csproj -c Release -r win-x64`
  - If you want a single-file EXE: add `/p:PublishSingleFile=true` (static assets still need to be deployed; see `www/`).
