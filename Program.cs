using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskplay
{
    public class Program
    {
        private Random _rnd = new Random(27498);
        private Reader _reader = new Reader(Path.Combine(Directory.GetCurrentDirectory(), "input"));
        private Writer _writer = new Writer(Path.Combine(Directory.GetCurrentDirectory(), "output"));

        static async Task Main(string[] args)
        {
            var program = new Program();

            Console.WriteLine("Started...");

            await program.RunAsync();

            Console.WriteLine("Ended...");
        }

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

        public Task DoWorkAsync(WorkContext ctx)
        {
            // You might ask why don't we just do this in an async / await fashion?
            // Async / Await does not come without cost.
            // Each await statement will incur additional costs since it will turn each awaited code into
            // a state machine.  The more you have, the more state machine that .NET will have to maintain.
            // Original TPL style programming does not do this, and therefore we are preferring to only have
            // async/await at the top most (UI layer) level and perhaps at the deepest level of the async call chains
            // everything in between should be handled using ContinueWith whenever possible.
            // Any feedback?

            // What I am trying to do here is to create a context in, context out async pipelining
            // Where the output of each async pipeline component is always the same context object (possibly modified)
            // And therefore it is easier to chain with ContinueWith.
            // I think Unwrapping the task in each continuation is necessary?
            // Any feedback?
            var task = ReadAsync(ctx)
                .ContinueWith(t => AsIntAsync(t))
                .ContinueWith(t => PrintAsync("Original number", t.Unwrap()))
                .ContinueWith(t => MultiplyBy5Async(t.Unwrap()))
                .ContinueWith(t => PrintAsync("After Multiply By 5", t.Unwrap()))
                .ContinueWith(t => RandomArtificialDelayAsync(t.Unwrap()))
                .ContinueWith(t => WriteAsync(t.Unwrap()));
            return task;
        }

        public Task<WorkContext> ReadAsync(WorkContext ctx)
        {
            // Demonstrating internal chaining inside a pipeline component
            // where it might call an external async method that returns
            // a completely different type and since we want our own context object as
            // output of this pipeline component, we have to adapt into the shape that we want
            // in a continuation block and return the context after.
            // Any feedback?
            var task = _reader
                .ReadAsync(ctx.FileName)
                .ContinueWith(t => {
                    ctx.FileData = t.GetAwaiter().GetResult();
                    return ctx;
                });
            task.ConfigureAwait(false);
            return task;
        }

        public Task<WorkContext> AsIntAsync(Task<WorkContext> task)
        {
            // This is actually a very synchronous work that does not need to be async,
            // but since async is so infectious, once you have an async in the pipeline,
            // you have to adapt non asynchronous pipeline component like this one
            // to be asynchronous, therefore wrapping the result in a Task and returning it.
            // Also, since there is no need for this pipeline component to run in the main UI thread,
            // We can forego the sycnhronization context by doing ConfigureAwait(false) to speed up the code a bit. 
            // Any feedback?

            // I tried converting this to the new ValueTask<T>, but I don't think I quite understand that yet.
            // It breaks my ContinueWith chains above.
            // Any pointer?
            var data = task.GetAwaiter().GetResult();
            data.Number = Convert.ToInt32(Encoding.UTF8.GetString(data.FileData));
            var task2 = Task.FromResult(data);
            task2.ConfigureAwait(false);
            return task2;
        }

        public Task<WorkContext> PrintAsync(string prefix, Task<WorkContext> task)
        {
            // Demonstrating that you can actually have additional parameter into the
            // async pipeline component.  As long as you are passing the context Task
            // everything is fine...
            // Also, notice that we are side effecting to the Console.here..
            // Since Console.WriteLine needs to run in the main UI thread, be careful to not mess around with
            // the Synchronization Context.
            // You should NOT ConfigureAwait(false) on this component.  It needs to run in the main UI thread.
            // Any feedback?
            var data = task.GetAwaiter().GetResult();
            Console.WriteLine($"{prefix}: {data.Number}");
            return Task.FromResult(data);
        }

        public Task<WorkContext> MultiplyBy5Async(Task<WorkContext> task)
        {
            var data = task.GetAwaiter().GetResult();
            data.Number = data.Number * 5;
            var task2 = Task.FromResult(data);
            task2.ConfigureAwait(false);
            return task2;
        }

        public Task<WorkContext> RandomArtificialDelayAsync(Task<WorkContext> task)
        {
            var task2 = Task
                .Delay(_rnd.Next(10) * 1000)
                .ContinueWith(t => task.GetAwaiter().GetResult());
            task2.ConfigureAwait(false);
            return task2;
        }

        public Task<WorkContext> WriteAsync(Task<WorkContext> task)
        {
            var data = task.GetAwaiter().GetResult();
            data.FileData = Encoding.UTF8.GetBytes($"{data.Number}");

            var task2 = _writer
                .WriteAsync(data.FileName, data.FileData)
                .ContinueWith(t => data);
            task2.ConfigureAwait(false);
            return task2;
        }
    }

    public class WorkContext
    {
        public string FileName { get; set; }
        public int Number { get; set; }

        public byte[] FileData { get; set; }

        public WorkContext(string fileName)
        {
            FileName = fileName;
            Number = 0;
            FileData = null;
        }
    }

    public class Reader
    {
        public string ReadPath { get; private set; }

        public Reader(string readPath)
        {
            ReadPath = readPath;
        }

        public Task<byte[]> ReadAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var fullPath = Path.Combine(ReadPath, fileName);
            if (!File.Exists(fullPath)) throw new InvalidOperationException($"{fullPath} does not exists.");

            var task = File.ReadAllBytesAsync(fullPath);
            task.ConfigureAwait(false);
            return task;
        }
    }

    public class Writer
    {
        public string WritePath { get; private set; }

        public Writer(string writePath)
        {
            WritePath = writePath;
        }

        public void ClearOutputPath()
        {
            if (Directory.Exists(WritePath))
                Directory.Delete(WritePath, true);

            Directory.CreateDirectory(WritePath);
        }

        public Task WriteAsync(string fileName, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 1) throw new ArgumentOutOfRangeException(nameof(data));

            var fullPath = Path.Combine(WritePath, fileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);

            var task = File.WriteAllBytesAsync(fullPath, data);
            task.ConfigureAwait(false);
            return task;
        }
    }
}
