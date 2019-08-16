using System;
using System.IO;
using System.Threading.Tasks;

namespace taskplay
{
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
            task.ConfigureAwait(false); // to indicate that we don't care where the thread should continue on...
            return task;
        }
    }
}
