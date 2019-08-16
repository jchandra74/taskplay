using System;
using System.IO;
using System.Threading.Tasks;

namespace taskplay
{
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
            task.ConfigureAwait(false);     // to indicate that we don't care where the thread should continue on...
            return task;
        }
    }
}
