using System;
using System.IO;
using System.IO.Compression;

namespace OpenNova.InitialImporter.PackFile
{
    public class PackFileEntry : IDisposable, IPackFileEntry
    {
        private readonly string _path;
        private readonly int _offset;
        private readonly bool _deleted;
        private readonly string _name;
        private int _size;
        private readonly IPackFile _file;
        private bool _loaded;
        private byte[] _data;
        private bool? _isCompressed;
        private bool? _isScript;

        public bool IsNew => false;

        public PackFileEntry(string path, int offset, bool deleted, string name, int size, IPackFile file)
        {
            _loaded = false;
            _path = path;
            _offset = offset;
            _deleted = deleted;
            _name = name;
            _file = file;
            _size = size;
            _data = null;
        }

        public void Load()
        {
            if (_loaded) return;

            using var stream = new FileStream(_file.GetFilePath(), FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(stream);

            br.BaseStream.Seek(_offset, SeekOrigin.Begin);
            _data = br.ReadBytes(_size);

            _loaded = true;
        }

        public void Dispose()
        {
            _data = null;
        }

        public bool IsCompressed()
        {
            if (_isCompressed.HasValue) return _isCompressed.Value;

            var bytes = GetRawBytes();
            _isCompressed = bytes.Length >= 4 &&
                bytes[0] == (byte)'B' && bytes[1] == (byte)'F' && bytes[2] == (byte)'C' && bytes[3] == (byte)'1';
            return _isCompressed.Value;
        }

        public bool IsScript()
        {
            if (_isScript.HasValue) return _isScript.Value;

            var bytes = GetRawBytes();
            _isScript = bytes.Length >= 3 &&
                bytes[0] == (byte)'S' && bytes[1] == (byte)'C' && bytes[2] == (byte)'R';
            return _isScript.Value;
        }

        public static byte[] Unpack(byte[] contents)
        {
            using var input = new MemoryStream(contents);
            input.Seek(8, SeekOrigin.Begin);

            if (input.ReadByte() != 0x78 || input.ReadByte() != 0x9C)
                throw new Exception("Incorrect zlib header");

            using var deflateStream = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            deflateStream.CopyTo(output);
            return output.ToArray();
        }

        public byte[] GetDecodedBytes()
        {
            if (!_loaded) Load();

            if (IsCompressed())
            {
                return Unpack(GetRawBytes());
            }
            return _data!;
        }

        public string GetFileName()
        {
            return _name;
        }

        public string GetFileExtension()
        {
            return Path.GetExtension(_name);
        }

        public byte[] GetRawBytes()
        {
            if (!_loaded) Load();
            return _data!;
        }

        public bool IsDeleted()
        {
            return _deleted;
        }

        public void SetData(byte[] data)
        {
            _data = data;
            _loaded = true;
            _size = data.Length;
            _isCompressed = null;
            _isScript = null;
        }

        public IPackFile GetPackFile()
        {
            return _file;
        }

        public int GetFileSize()
        {
            return _size;
        }

        public void Delete()
        {
            var pf = GetPackFile();
            pf.DeleteEntry(this);
        }

        public void Save(byte[] data)
        {
            SetData(data);
        }
    }
}
