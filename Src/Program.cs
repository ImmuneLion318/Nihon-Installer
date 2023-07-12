using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Spectre.Console;

namespace Nihon
{
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
         * Add Redist Check/Install (x86/x64)
        */

        static async Task Main(string[] Parameters)
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
                if (RegKey == null)
                    await AnsiConsole.Progress()
                        .StartAsync(async Ctx =>
                        {
                            ProgressTask WebViewInstall = Ctx.AddTask("WebView Install", new ProgressTaskSettings { MaxValue = 100 });

                            /* Check Architecture And Download WebView Based On That Here. */
                            await Http.DownloadFileTaskAsync(
                                "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                                "MicrosoftEdgeWebview2Setup.exe");

                            Http.DownloadProgressChanged += (s, e) =>
                            {
                                while (Ctx.IsFinished != true)
                                    WebViewInstall.Increment(e.ProgressPercentage);
                            };
                        });

            Http.Dispose();
            AnsiConsole.Clear();

            AnsiConsole.Write(new Markup($"[red]{Watermark}[/]"));
            AnsiConsole.Write(new Markup("[springgreen3_1]Done[/]... Exiting In 3 Seconds."));

            await Task.Delay(3000);

            Environment.Exit(0);
        }

        /* Do Like WebView Check. */
        static bool IsRedist(string Arch)
        {
            string KeyName = $"SOFTWARE\\Microsoft\\VisualStudio\\SxS\\VC7";
            RegistryKey RegistryKey = Registry.LocalMachine.OpenSubKey(KeyName);
            if (RegistryKey == null)
                return false;

            string ValueName = $"14.0_{Arch}_RuntimeMinimum_{Arch}";
            string RegistryValue = RegistryKey.GetValue(ValueName) as string;
            if (string.IsNullOrEmpty(RegistryValue))
                return false;

            string RedistFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), $"msvcr120.dll_{Arch}");
            return Directory.Exists(RedistFolder);
        }
    }
}
