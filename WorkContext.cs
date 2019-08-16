namespace taskplay
{
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
}
