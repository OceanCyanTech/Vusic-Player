using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class SimpleStreamAbstraction : TagLib.File.IFileAbstraction
    {
        public SimpleStreamAbstraction(string name, Stream stream)
        {
            Name = name;
            ReadStream = stream;
            WriteStream = stream;
        }

        public string Name { get; private set; }
        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }

        public void CloseStream(Stream stream)
        {
            // Let the outer blocks handle disposing the stream
        }
    }
}
