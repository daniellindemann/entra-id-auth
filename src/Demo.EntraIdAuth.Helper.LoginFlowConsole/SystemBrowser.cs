// See: https://github.com/IdentityModel/IdentityModel.OidcClient/blob/main/clients/ConsoleClientWithBrowser/SystemBrowser.cs

using IdentityModel.OidcClient.Browser;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Demo.EntraIdAuth.Helper.LoginFlowConsole;

public class SystemBrowser : IBrowser
{
    public int Port { get; }
    private readonly string _path;

    public SystemBrowser(string path, int? port = null)
    {
        _path = path;

        if (!port.HasValue)
        {
            Port = GetRandomUnusedPort();
        }
        else
        {
            Port = port.Value;
        }
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
    {
        using (var listener = new LoopbackHttpListener(Port, _path))
        {
            try
            {
                OpenBrowser(options.StartUrl);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.WriteLine("Please open the following URL in your browser:");
                Console.WriteLine(options.StartUrl);
            }

            try
            {
                var result = await listener.WaitForCallbackAsync();
                if (String.IsNullOrWhiteSpace(result))
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
                }

                return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
            }
            catch (TaskCanceledException ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
            }
            catch (Exception ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }
        }
    }

    public static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}

public class LoopbackHttpListener : IDisposable
{
    const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

    readonly IWebHost _host;
    readonly TaskCompletionSource<string> _source = new TaskCompletionSource<string>();

    public string Url { get; }

    public LoopbackHttpListener(int port, string path)
    {
        path = path ?? String.Empty;
        if (path.StartsWith("/")) path = path.Substring(1);

        Url = $"http://127.0.0.1:{port}/{path}";

        _host = new WebHostBuilder()
            .UseKestrel()
            .UseUrls(Url)
            .Configure(Configure)
            .Build();
        _host.Start();
    }

    public void Dispose()
    {
        Task.Run(async () =>
        {
            await Task.Delay(500);
            _host.Dispose();
        });
    }

    void Configure(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            if (ctx.Request.Method == "GET")
            {
                await SetResultAsync(ctx.Request.QueryString.Value!, ctx);
            }
            else
            {
                ctx.Response.StatusCode = 405;
            }
        });
    }

    private async Task SetResultAsync(string value, HttpContext ctx)
    {
        _source.TrySetResult(value);

        try
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
        catch
        {
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
    }

    public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeoutInSeconds * 1000);
            _source.TrySetCanceled();
        });

        return _source.Task;
    }
}
