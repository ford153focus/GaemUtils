using System;
using System.IO;
using System.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace GaemUtils.HalyavaHunter
{
    public static class Utils
    {
        private static JsonValue _credentials;
        private static Browser _browser;
        private static Page _page;
        public static readonly WaitForSelectorOptions WaitForSelectorOptions = new() { Timeout = 5310 };

        private static async Task<Browser> GetBrowserAsync()
        {
            if (_browser == null)
            {
                // await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                string exePath = "";
                string profilePath = "";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    exePath = "/usr/bin/google-chrome";
                    profilePath = Path.Combine(new[] {
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".config",
                        "google-chrome-for-gaem-utils"
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    exePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
                    profilePath = Path.Combine(new[] {
                        Path.GetTempPath(),
                        "google-chrome-for-gaem-utils"
                    });
                }

                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    IgnoreHTTPSErrors = true,
                    Headless = false,

                    ExecutablePath = exePath,
                    UserDataDir = profilePath,

                    Args = new[]
                    {
                        "--start-maximized"
                    },
                    DefaultViewport = null
                });
            }

            return _browser;
        }

        public static async Task<Page> GetPageAsync()
        {
            if (_page == null)
            {
                var browser = await GetBrowserAsync();

                _page = await browser.NewPageAsync();

                /*await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = 1600,
                    Height = 900
                });*/
            }

            return _page;
        }

        public static async Task<JsonValue> GetCredentialsAsync()
        {
            if (_credentials == null)
            {
                var credentialsCfgStr = await File.ReadAllTextAsync("./credentials.json", Encoding.UTF8);
                _credentials = JsonValue.Parse(credentialsCfgStr);
            }
            return _credentials;
        }
    }
}