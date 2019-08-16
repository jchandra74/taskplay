using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskplay
{
    public class Program
    {
        
        static async Task Main(string[] args)
        {
            var program = new Program();

            Console.WriteLine("Started...");

            // Comment & uncomment the one you want to try...
            // They both do the same thing, but in different async style

            //await new SampleAsyncAwait().RunAsync();
            await new SampleTPL().RunAsync();

            Console.WriteLine("Ended...");
        }
    }
}
