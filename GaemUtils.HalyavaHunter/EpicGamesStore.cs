using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace GaemUtils.HalyavaHunter
{
    public class EpicGamesStore
    {
        private static Page _page;

        private static async Task Auth()
        {
            try
            {
                await _page.WaitForSelectorAsync("div#user", Utils.WaitForSelectorOptions);
            }
            catch (Exception)
            {
                Console.WriteLine("Auth Required");
            }
        }

        public static async Task Hunt()
        {
            _page = await Utils.GetPageAsync();
            await _page.GoToAsync("https://www.epicgames.com/store/en-US/free-games", WaitUntilNavigation.Load);
            await Auth();

            foreach (var link in await _page.QuerySelectorAllAsync("a[href^='/store/en-US/product/']"))
            {
                var url = await _page.EvaluateFunctionAsync<string>("e => e.href", link);
                Console.WriteLine(url);
                await _page.GoToAsync(url, WaitUntilNavigation.Load);

                try
                {
                    /* Age Takeover */
                    try
                    {
                        await _page.WaitForSelectorAsync("[data-component='AgeGateTakeover'] button", Utils.WaitForSelectorOptions);
                        await _page.ClickAsync("[data-component='AgeGateTakeover'] button");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Age Takeover not required for this game");
                    }

                    await _page.WaitForSelectorAsync("[data-component='PurchaseButton'] [data-testid='purchase-cta-button']", Utils.WaitForSelectorOptions);
                    var isHalyava = await _page.EvaluateExpressionAsync<bool>(await File.ReadAllTextAsync("./js/egs/is_halyava.js"));
                    if (!isHalyava)
                    {
                        Console.WriteLine("Not available?");
                        continue;
                    }
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