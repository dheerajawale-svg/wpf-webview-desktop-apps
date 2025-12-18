using System;
using System.Text.Json;
using DesktopHostWpf.Models;
using Microsoft.Web.WebView2.Core;

namespace DesktopHostWpf.Services;

public sealed class WebMessageRouter
{
    private readonly CoreWebView2 _core;
    private readonly InMemoryAuthService _auth;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WebMessageRouter(CoreWebView2 core, InMemoryAuthService auth)
    {
        _core = core;
        _auth = auth;
    }

    public void Initialize()
    {
        _core.WebMessageReceived += CoreOnWebMessageReceived;
        Post(type: "hostReady", payload: new { ok = true }, requestId: null);
    }

    private void CoreOnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.WebMessageAsJson;
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
            var requestId = root.TryGetProperty("requestId", out var rid) ? rid.GetString() : null;

            switch (type)
            {
                case "ping":
                    Post("pong", new { ok = true }, requestId);
                    return;

                case "login":
                {
                    var username = root.TryGetProperty("username", out var u) ? u.GetString() ?? "" : "";
                    var password = root.TryGetProperty("password", out var p) ? p.GetString() ?? "" : "";
                    var result = _auth.Login(username, password);

                    Post(
                        type: "loginResult",
                        payload: new
                        {
                            ok = result.Ok,
                            token = result.Token,
                            user = result.User,
                            error = result.Error
                        },
                        requestId: requestId);
                    return;
                }

                case "getSession":
                {
                    var token = root.TryGetProperty("token", out var tok) ? tok.GetString() ?? "" : "";
                    var user = _auth.GetSession(token);
                    Post("session", new { ok = user is not null, user }, requestId);
                    return;
                }

                case "logout":
                {
                    var token = root.TryGetProperty("token", out var tok) ? tok.GetString() ?? "" : "";
                    _auth.Logout(token);
                    Post("logoutResult", new { ok = true }, requestId);
                    return;
                }

                default:
                    Post("error", new { ok = false, error = $"Unknown message type: '{type}'" }, requestId);
                    return;
            }
        }
        catch (Exception ex)
        {
            Post("error", new { ok = false, error = ex.Message }, requestId: null);
        }
    }

    private void Post(string type, object payload, string? requestId)
    {
        var message = JsonSerializer.Serialize(new { type, requestId, payload }, JsonOptions);
        _core.PostWebMessageAsJson(message);
    }
}
