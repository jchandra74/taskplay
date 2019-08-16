using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskplay
{
    public class SampleAsyncAwait
    {
        private Random _rnd = new Random(27498);
        private Reader _reader = new Reader(Path.Combine(Directory.GetCurrentDirectory(), "input"));
        private Writer _writer = new Writer(Path.Combine(Directory.GetCurrentDirectory(), "output"));

        public Task RunAsync()
        {
            _writer.ClearOutputPath();

            var fileNames = new[] { "1.txt", "2.txt", "4.txt", "8.txt" };

            // Here, we are demonstrating how to do multiple asynchronous works
            // that might be parallelized by the OS using Task.WhenAll

            var works = fileNames
                .Select(fileName => DoWorkAsync(new WorkContext(fileName)));

            // Here we are only returning the promise (Task) that will be awaited by the caller.
            // Notice that we do not need async/await here so to not incur the cost of additional state machine.
            return Task.WhenAll(works);
        }

        public async Task DoWorkAsync(WorkContext ctx)
        {
            // Instead of forcing all method to be async, we can leave some as synchronous and only
            // fall back to use await on actual asynchronous methods.
            
            // We are paying for the additional state machine but in return it's easier to debug and follow?
            // Worth it?

            ctx = await ReadAsync(ctx);
            ctx = AsInt(ctx);
            ctx = Print("Original number", ctx);
            ctx = MultiplyBy5(ctx);
            ctx = Print("After Multiply By 5", ctx);
            ctx = await RandomArtificialDelayAsync(ctx);
            ctx = await WriteAsync(ctx);
        }

        public async Task<WorkContext> ReadAsync(WorkContext ctx)
        {
            // Definitely easier to understand...
            // The adapting is not too obvious.  async/await is handling it for us.
            ctx.FileData = await _reader.ReadAsync(ctx.FileName);
            return ctx;
        }

        public WorkContext AsInt(WorkContext ctx)
        {
            ctx.Number = Convert.ToInt32(Encoding.UTF8.GetString(ctx.FileData));
            return ctx;
        }

        public WorkContext Print(string prefix, WorkContext ctx)
        {
            Console.WriteLine($"{prefix}: {ctx.Number}");
            return ctx;
        }

        public WorkContext MultiplyBy5(WorkContext ctx)
        {
            ctx.Number = ctx.Number * 5;
            return ctx;
        }

        public async Task<WorkContext> RandomArtificialDelayAsync(WorkContext ctx)
        {
            await Task.Delay(_rnd.Next(10) * 1000);
            return ctx;
        }

        public async Task<WorkContext> WriteAsync(WorkContext ctx)
        {
            ctx.FileData = Encoding.UTF8.GetBytes($"{ctx.Number}");
            await _writer.WriteAsync(ctx.FileName, ctx.FileData);

            return ctx;
        }
    }
}