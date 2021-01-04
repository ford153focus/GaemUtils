using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace GaemUtils.HalyavaHunter
{
    public class EpicGamesStore
    {
        private static Page _page;

        private static async Task Auth()
        {
            var response = await _page.GoToAsync("https://www.epicgames.com/account/v2/ajaxCheckLogin", WaitUntilNavigation.Load);
            string responseText = await response.TextAsync();
            if ((bool)JsonValue.Parse(responseText)["needLogin"])
            {
                var credentials = await Utils.GetCredentialsAsync();
                string login = credentials["gog"]["login"];
                string password = credentials["gog"]["password"];
                
                await _page.GoToAsync("https://www.epicgames.com/id/login/epic", WaitUntilNavigation.Load);
                await _page.WaitForSelectorAsync("#email");
                await _page.FocusAsync("#email");
                await _page.Keyboard.TypeAsync(login);
                await _page.FocusAsync("#password");
                await _page.Keyboard.TypeAsync(password);
                await _page.ClickAsync("#sign-in");
            }
        }

        private static async Task<List<string>> GetFreeGamesLinks()
        {
            List<string> links = new();
            await _page.GoToAsync("https://www.epicgames.com/store/en-US/free-games", WaitUntilNavigation.Load);
            foreach (var link in await _page.QuerySelectorAllAsync("a[href^='/store/en-US/product/']"))
            {
                var url = await _page.EvaluateFunctionAsync<string>("e => e.href", link);
                links.Add(url);
                
            }
            return links;
        }

        private static async Task AgreeAgeTakeover()
        {
            try
            {
                await _page.WaitForSelectorAsync("[data-component='AgeGateTakeover'] button", Utils.WaitForSelectorOptions);
                await _page.ClickAsync("[data-component='AgeGateTakeover'] button");
            }
            catch (Exception)
            {
                Console.WriteLine("Age Takeover not required for this game");
            }
        }
        
        public static async Task Hunt()
        {
            _page = await Utils.GetPageAsync();
            await Auth();

            foreach (var url in await GetFreeGamesLinks())
            {
                Console.WriteLine(url);
                await _page.GoToAsync(url, WaitUntilNavigation.Load);

                try
                {
                    await AgreeAgeTakeover();

                    var purchaseButton  = await _page.WaitForSelectorAsync("[data-component='PurchaseButton'] [data-testid='purchase-cta-button']", Utils.WaitForSelectorOptions);
                    var purchaseButtonText = await _page.EvaluateFunctionAsync<string>("e => e.innerText.toLowerCase()", purchaseButton);
                    if (purchaseButtonText!="get") continue;

                    await _page.ClickAsync("[data-component='PurchaseButton'] [data-testid='purchase-cta-button']");

                    await _page.WaitForSelectorAsync(".order-summary-content .confirm-container button", Utils.WaitForSelectorOptions);
                    await _page.ClickAsync(".order-summary-content .confirm-container button");

                    await _page.WaitForSelectorAsync("[data-component='Download'] .download-box .download-footer-btns", Utils.WaitForSelectorOptions);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Console.WriteLine("Hello World!");
        }
    }
}