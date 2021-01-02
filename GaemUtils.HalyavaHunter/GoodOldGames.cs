using System;
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
                await _page.WaitForSelectorAsync("button.giveaway-banner__button", Utils.WaitForSelectorOptions);
                await _page.ClickAsync("button.giveaway-banner__button");
            }
            catch (Exception)
            {
                Console.WriteLine("No giveaways on GOG at this time");
            }
        }
    }
}