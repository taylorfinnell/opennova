using System.IO;

namespace OpenNova.InitialImporter.PackFile
{
    public class InMemoryPackFileEntry : IPackFileEntry
    {
        private readonly string _name;
        private byte[] _data;
        private readonly IPackFile _packFile;
        private readonly bool _compressed;

        public InMemoryPackFileEntry(string name, byte[] data, bool compressed, IPackFile packFile)
        {
            _name = name;
            _data = data;
            _compressed = compressed;
            _packFile = packFile;
        }

        public byte[] GetRawBytes()
        {
            return _data;
        }

        public byte[] GetDecodedBytes()
        {
            if (_compressed)
            {
                return PackFileEntry.Unpack(_data);
            }
            return _data;
        }

        public void SetData(byte[] data)
        {
            _data = data;
        }

        public bool IsDeleted()
        {
            return false;
        }

        public string GetFileName()
        {
            return _name;
        }

        public string GetFileExtension()
        {
            return Path.GetExtension(_name);
        }

        public int GetSize()
        {
            return _data.Length;
        }

        public IPackFile GetPackFile()
        {
            return _packFile;
        }

        public bool IsCompressed()
        {
            return _compressed;
        }

        public bool IsScript()
        {
            // Implement logic if necessary
            return false;
        }

        public void Load()
        {
            // Already loaded in memory
        }

        public int GetFileSize()
        {
            return _data.Length;
        }

        public void Delete()
        {
            var pf = GetPackFile();
            pf.DeleteEntry(this);
        }

        public void Save(byte[] data)
        {
            _data = data;
        }

        public bool IsNew => true;
    }
}
