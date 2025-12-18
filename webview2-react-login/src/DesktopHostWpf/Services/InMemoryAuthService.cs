using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DesktopHostWpf.Models;

namespace DesktopHostWpf.Services;

public sealed class InMemoryAuthService
{
    private readonly Dictionary<string, (string Password, User User)> _users =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = ("admin123", new User("admin", "Administrator")),
            ["demo"] = ("demo123", new User("demo", "Demo User")),
        };

    private readonly ConcurrentDictionary<string, User> _sessions = new();

    public (bool Ok, string? Token, User? User, string? Error) Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, null, "Username and password are required.");
        }

        if (!_users.TryGetValue(username.Trim(), out var entry) || entry.Password != password)
        {
            return (false, null, null, "Invalid username or password.");
        }

        var token = Guid.NewGuid().ToString("N");
        _sessions[token] = entry.User;
        return (true, token, entry.User, null);
    }

    public User? GetSession(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return _sessions.TryGetValue(token, out var user) ? user : null;
    }

    public void Logout(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        _sessions.TryRemove(token, out _);
    }
}

