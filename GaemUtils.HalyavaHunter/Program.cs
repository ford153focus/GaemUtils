using System;
using System.Threading.Tasks;

namespace GaemUtils.HalyavaHunter
{
    internal static class Program
    {
        private static async Task Main()
        {
            await GoodOldGames.Hunt();
            await EpicGamesStore.Hunt();
            Console.WriteLine("Hello World!");
        }
    }
}
