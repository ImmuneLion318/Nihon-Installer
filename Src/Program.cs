using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Win32;
using Spectre.Console;

namespace Nihon;

public class Program
{
    static WebClient Http = new WebClient { Proxy = null };

    static string Watermark = @"
  
  ███╗░░██╗██╗██╗░░██╗░█████╗░███╗░░██╗
  ████╗░██║██║██║░░██║██╔══██╗████╗░██║
  ██╔██╗██║██║███████║██║░░██║██╔██╗██║
  ██║╚████║██║██╔══██║██║░░██║██║╚████║
  ██║░╚███║██║██║░░██║╚█████╔╝██║░╚███║
  ╚═╝░░╚══╝╚═╝╚═╝░░╚═╝░╚════╝░╚═╝░░╚══╝
        ";

    /*
     * Add Segoe Fluent Icons Font Check/Install
    */

    static async Task Main()
    {
        Console.Title = "Nihon Installer";
        Console.SetWindowSize(80, 20);

        AnsiConsole.Write(new Markup($"[red]{Watermark}[/]"));

        string Architecture = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("\n  Select [underline blue]Windows[/] Architecture")
            .HighlightStyle(new Style(Color.Red))
            .AddChoices(new[] { "x86", "x64" }));

        AnsiConsole.Clear();

        AnsiConsole.Write(new Markup($"[red]{Watermark}[/]"));

        string WebViewKey = Architecture == "x86" ?
            "SOFTWARE\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
            : "SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";

        using (RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(WebViewKey))
            if (RegKey == null) await AnsiConsole.Progress()
                    .StartAsync(async Ctx =>
                    {
                        ProgressTask WebViewInstall = Ctx.AddTask("WebView Install", new ProgressTaskSettings { MaxValue = 100 });

                        Http.DownloadProgressChanged += (s, e) =>
                        {
                            while (Ctx.IsFinished != true)
                                WebViewInstall.Increment(e.ProgressPercentage);
                        };

                        Http.DownloadFileCompleted += (s, e) =>
                        {
                            new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "WebView2RuntimeSetup.exe",
                                    Arguments = "/silent /install",
                                    CreateNoWindow = true
                                }
                            }.Start();
                        };

                        if (Architecture == "x86") await Http.DownloadFileTaskAsync("https://go.microsoft.com/fwlink/?linkid=2099617", "WebView2RuntimeSetup.exe");
                        else await Http.DownloadFileTaskAsync("https://go.microsoft.com/fwlink/?linkid=2124701", "WebView2RuntimeSetup.exe");
                    });

        using (RegistryKey RegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VC\Servicing\14.0\RuntimeMinimum", false))
            if (RegKey == null) await AnsiConsole.Progress()
                    .StartAsync(async Ctx =>
                    {
                        ProgressTask RedistInstall = Ctx.AddTask("Redist Install", new ProgressTaskSettings { MaxValue = 100 });

                        Http.DownloadProgressChanged += (s, e) =>
                        {
                            while (Ctx.IsFinished != true)
                                RedistInstall.Increment(e.ProgressPercentage);
                        };

                        Http.DownloadFileCompleted += (s, e) =>
                        {
                            new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "RedistSetup.exe",
                                    Arguments = "/silent /install",
                                    CreateNoWindow = true
                                }
                            }.Start();
                        };

                        if (Architecture == "x86") await Http.DownloadFileTaskAsync("https://aka.ms/vs/17/release/vc_redist.x86.exe", "RedistSetup.exe");
                        else await Http.DownloadFileTaskAsync("https://aka.ms/vs/17/release/vc_redist.x64.exe", "RedistSetup.exe");
                    });

        Http.Dispose();
        AnsiConsole.Clear();

        AnsiConsole.Write(new Markup($"[red]{Watermark}[/]"));
        AnsiConsole.Write(new Markup("[springgreen3_1]Done[/]... Exiting In 3 Seconds."));

        await Task.Delay(3000);

        Environment.Exit(0);
    }
}
