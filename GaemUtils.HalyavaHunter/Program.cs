using System;
using System.Threading.Tasks;

namespace GaemUtils.HalyavaHunter
{
    internal static class Program
    {
        private static async Task Main()
        {
            // try { await GoodOldGames.Hunt(); } catch (Exception e) { Console.WriteLine(e); }
            try { await EpicGamesStore.Hunt(); } catch (Exception e) { Console.WriteLine(e); }
            await (await Utils.GetBrowserAsync()).CloseAsync();
            Console.WriteLine("Bye!");
        }
    }
}
