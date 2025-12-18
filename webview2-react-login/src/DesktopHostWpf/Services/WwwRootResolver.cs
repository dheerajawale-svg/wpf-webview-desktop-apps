using System;
using System.Diagnostics;
using System.IO;

namespace DesktopHostWpf.Services;

public static class WwwRootResolver
{
    public static string Resolve()
    {
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
        var exeDir = exePath is null ? AppContext.BaseDirectory : Path.GetDirectoryName(exePath)!;

        var www = Path.Combine(exeDir, "www");
        if (!Directory.Exists(www))
        {
            throw new DirectoryNotFoundException(
                $"Missing static web assets folder: '{www}'. " +
                "Build/copy your React output into 'src/DesktopHostWpf/www' and rebuild.");
        }

        return www;
    }
}

