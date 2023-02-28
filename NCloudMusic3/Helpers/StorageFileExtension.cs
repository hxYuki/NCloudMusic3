using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NCloudMusic3.Helpers
{
    internal static class StorageFileExtension
    {
        public static StorageFileAbstraction ToTagLibFileAbstraction(this IStorageFile sf) => new StorageFileAbstraction(sf);
    }

    public class StorageFileAbstraction : TagLib.File.IFileAbstraction
    {
        public string Name {get;}

        public Stream ReadStream { get; }

        public Stream WriteStream { get; }

        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
        public StorageFileAbstraction(IStorageFile sf)
        {
            Name = sf.Path;
            ReadStream = sf.OpenStreamForReadAsync().GetAwaiter().GetResult();
            WriteStream = sf.OpenStreamForWriteAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            ReadStream.Dispose();
            WriteStream.Dispose();
        }
    }
}
