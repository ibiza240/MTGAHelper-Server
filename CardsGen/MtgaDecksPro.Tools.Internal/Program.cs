using Microsoft.Extensions.DependencyInjection;
using MtgaDecksPro.Tools.Internal.AssemblyConfig;
using MtgaDecksPro.Tools.Internal.Service;
using System;
using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal
{
    internal class Program
    {
        private const string choiceQuit = "x";

        protected static IServiceProvider provider;

        private static async Task Main(string[] args)
        {
            provider = new ServiceCollection()
                .RegisterServicesToolsInternal()
                .BuildServiceProvider();

            while (true)
            {
                ShowMenu();

                var choice = Console.ReadLine();
                if (choice == choiceQuit)
                    return;

                if (TryParseChoice(choice, out ChoiceDispatcherEnum choiceDispatcherEnum))
                {
                    await provider.GetService<ChoiceDispatcher>().ProcessChoice(choiceDispatcherEnum);
                }
            }
        }

        private static void ShowMenu()
        {
            if (System.IO.File.Exists(@"..\..\..\..\cards\cardsbuilder\cards_raw.csv") == false)
            {
                Console.WriteLine("To manually generate the required cards_raw.csv:");
                Console.WriteLine(@"- Open Raw_CardDatabase_xyz with SqLite DB Browser from C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw");
                Console.WriteLine("- Execute SQL SELECT * FROM Cards");
                Console.WriteLine(@"- Save as CardsGen\cards\cardsbuilder\cards_raw.csv (with header, comma separated)");
            }

            Console.WriteLine("*****************");

            Console.WriteLine("[1] Generate the data_loc.mtga and Raw_cards.mtga files");
            Console.WriteLine("[2] Download Scryfall cards and sets");
            Console.WriteLine("[3] Build sets.json and cards.json");
            Console.WriteLine("[x] Quit");
            Console.Write("Your choice: ");
        }

        private static bool TryParseChoice(string choice, out ChoiceDispatcherEnum choiceDispatcherEnum)
        {
            var s = choice.Trim();
            switch (s)
            {
                case "1":
                    choiceDispatcherEnum = ChoiceDispatcherEnum.DownloadMtgaAssets;
                    break;

                case "2":
                    choiceDispatcherEnum = ChoiceDispatcherEnum.DownloadScryfallCards;
                    break;

                case "3":
                    choiceDispatcherEnum = ChoiceDispatcherEnum.BuildCards;
                    break;

                default:
                    choiceDispatcherEnum = ChoiceDispatcherEnum.Unknown;
                    return false;
            }

            return true;
        }
    }
}