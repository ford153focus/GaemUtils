using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace GaemUtils.HalyavaHunter
{
    public static class GoodOldGames
    {
        private static string _login;
        private static string _password;
        private static Page _page;

        private static async Task Auth()
        {
            var username = await _page.EvaluateExpressionAsync<string>("document.querySelector('#menuUsername').innerText");
            if (username == "")
            {
                await _page.EvaluateFunctionAsync<string>("e => e.click()", 
                                                          await _page.QuerySelectorAsync(".menu-anonymous-header__btn--sign-in"));
                await _page.WaitForSelectorAsync("#GalaxyAccountsFrame", Utils.WaitForSelectorOptions);
                var accountsFrame = await _page.QuerySelectorAsync("#GalaxyAccountsFrame");
                var accountsFrameContent = await accountsFrame.ContentFrameAsync();
                var loginInput = await accountsFrameContent.QuerySelectorAsync("#login_username");
                await loginInput.TypeAsync(_login);
                var passwordInput = await accountsFrameContent.QuerySelectorAsync("#login_password");
                await passwordInput.TypeAsync(_password);
                var submitButton = await accountsFrameContent.QuerySelectorAsync("#login_login");
                await submitButton.ClickAsync();
            }
        }

        private static async Task<List<int>> GetLicences()
        {
            var response = await _page.GoToAsync("https://menu.gog.com/v1/account/licences", WaitUntilNavigation.Load);
            string pageContent = await response.TextAsync();
            return (from licence in (JsonArray)JsonValue.Parse(pageContent) select (int)licence).ToList();
        }

        public static async Task Hunt()
        {
            var credentials = await Utils.GetCredentialsAsync();
            _login = credentials["gog"]["login"];
            _password = credentials["gog"]["password"];

            _page = await Utils.GetPageAsync();
            await _page.GoToAsync("https://www.gog.com/", WaitUntilNavigation.Load);
            await Auth();

            try
            {
                await Utils.WaitAndClick(_page, "button.giveaway-banner__button");
            }
            catch (WaitTaskTimeoutException)
            {
                Console.WriteLine("No giveaways on GOG's main page at this time");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            try
            {
                var licences = await GetLicences();
                int page = 1;
                WebClient client = new WebClient();
                while (true)
                {
                    string url =
                        $"https://www.gog.com/games/ajax/filtered?hide=dlc&mediaType=game&page={page.ToString()}&price=free&sort=bestselling";
                    string ajaxReplyString = client.DownloadString(url);
                    JsonValue ajaxReplyObj = JsonValue.Parse(ajaxReplyString);
                    JsonArray games = (JsonArray)ajaxReplyObj["products"];
                    foreach (JsonValue game in games)
                    {
                        var gameId = (int)game["id"];
                        if (licences.Contains(gameId)) continue;
                        
                        var gameTitle = (string)game["title"];
                        gameTitle = gameTitle.Trim().ToLower();
                        if (gameTitle.EndsWith("demo")) continue;
                        if (gameTitle.EndsWith("trial")) continue;
                        if (gameTitle.Contains("dlc")) continue;

                        await _page.GoToAsync($"https://www.gog.com/cart/add/{gameId.ToString()}", WaitUntilNavigation.Load);
                    }
                    page++;
                }
            }
            catch (WebException)
            {
                Console.WriteLine("Listing of gog's free games is over");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            await _page.GoToAsync("https://www.gog.com/checkout", WaitUntilNavigation.Load);
            await Utils.WaitAndClick(_page, "form[name='orderForm'] button[type='submit']");
            await _page.WaitForSelectorAsync("h1.order__title.order__title--success", Utils.WaitForSelectorOptions);
        }
    }
}