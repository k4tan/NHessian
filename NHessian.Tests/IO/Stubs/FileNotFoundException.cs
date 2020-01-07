using System;

namespace java.io
{
    public class FileNotFoundException : Exception
    {
        public FileNotFoundException()
            : base("File Not Found")
        {
        }
    }
}
