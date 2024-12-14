using System.Runtime.CompilerServices;
namespace AsyConsoleApp1;

    public class Program
    {
        static async Task Main(string[] args)
        {
            var egg = FryEggsAsync(2);
            var bacon   = FryBaconAsync(3);
            await egg;
            await bacon;
            Console.WriteLine("All Task is over");
        }
        
        private static async ZTask FryEggsAsync(int howMany)
        {
            Console.WriteLine("Warming the egg pan...");
            await ZTask.Delay(3000);
            Console.WriteLine($"cracking {howMany} eggs");
            Console.WriteLine("cooking the eggs ...");
            await ZTask.Delay(3000);
            Console.WriteLine("Put eggs on plate");

        }

        private static async ZTask FryBaconAsync(int slices)
        {
            Console.WriteLine($"putting {slices} slices of bacon in the pan");
            Console.WriteLine("cooking first side of bacon...");
            await ZTask.Delay(1000);
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine("flipping a slice of bacon");
            }
            Console.WriteLine("cooking the second side of bacon...");
            await ZTask.Delay(1000);
            Console.WriteLine("Put bacon on plate");
            
        }
    }


