type HostEnvelope<TPayload = unknown> = {
  type: string;
  requestId?: string | null;
  payload?: TPayload;
};

type LoginResultPayload = {
  ok: boolean;
  token?: string | null;
  user?: { username: string; displayName: string } | null;
  error?: string | null;
};

type SessionPayload = {
  ok: boolean;
  user?: { username: string; displayName: string } | null;
};

type LogoutResultPayload = { ok: boolean };

function hasBridge(): boolean {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return typeof window !== "undefined" && !!(window as any).chrome?.webview;
}

function newRequestId(): string {
  return crypto?.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

const pending = new Map<string, (msg: HostEnvelope) => void>();

export function startHostListener(): void {
  if (!hasBridge()) return;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (window as any).chrome.webview.addEventListener("message", (event: MessageEvent) => {
    const msg = event.data as HostEnvelope;
    const id = msg?.requestId ?? "";
    const resolver = pending.get(id);
    if (resolver) {
      pending.delete(id);
      resolver(msg);
    }
  });
}

type BridgeRequest = Record<string, unknown> & { type: string };

function post<TResponse extends HostEnvelope>(
  message: BridgeRequest
): Promise<TResponse> {
  if (!hasBridge()) {
    return Promise.reject(new Error("window.chrome.webview is not available (run inside WebView2)."));
  }

  const requestId = newRequestId();
  const fullMessage = { requestId, ...message };

  return new Promise<TResponse>((resolve) => {
    pending.set(requestId, (msg) => resolve(msg as TResponse));
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (window as any).chrome.webview.postMessage(fullMessage);
  });
}

export async function login(username: string, password: string) {
  const msg = await post<HostEnvelope<LoginResultPayload>>({ type: "login", username, password });
  return msg.payload!;
}

export async function getSession(token: string) {
  const msg = await post<HostEnvelope<SessionPayload>>({ type: "getSession", token });
  return msg.payload!;
}

export async function logout(token: string) {
  const msg = await post<HostEnvelope<LogoutResultPayload>>({ type: "logout", token });
  return msg.payload!;
}
