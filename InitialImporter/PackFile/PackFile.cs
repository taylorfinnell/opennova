using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenNova.InitialImporter.PackFile
{
    internal class DefaultPffHeader
    {
        public int HeaderSize { get; set; }
        public string Signature { get; set; }
        public int RecordCount { get; set; }
        public int RecordSize { get; set; }
        public int RecordOffset { get; set; }
    }

    public class PackFile : IDisposable, IPackFile
    {
        private readonly string _path;
        private readonly Dictionary<string, IPackFileEntry> _entries;
        private bool _loaded;
        private DefaultPffHeader _header;

        public PackFile(string path)
        {
            _path = path;
            _header = null;
            _entries = new Dictionary<string, IPackFileEntry>(StringComparer.OrdinalIgnoreCase);
            _loaded = false;
        }

        public IEnumerable<IPackFileEntry> GetEntries()
        {
            return _entries.Values;
        }

        public void Dispose()
        {
            _entries.Clear();
        }

        public string GetFileName()
        {
            return Path.GetFileName(_path);
        }

        public string GetFilePath()
        {
            return _path;
        }

        public void Create()
        {

            using var stream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            using var writer = new BinaryWriter(stream);

            _header = new DefaultPffHeader
            {
                HeaderSize = 20,
                Signature = "PFF4",
                RecordCount = 0,
                RecordSize = 0,
                RecordOffset = 36,
            };

            writer.Write(_header.HeaderSize);
            writer.Write(Encoding.UTF8.GetBytes(_header.Signature));
            writer.Write(0);
            writer.Write(0);
            writer.Write(_header.HeaderSize);
            writer.Close();
        }

        public void Load()
        {
            if (_loaded) return;

            using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(stream);

            _header = new DefaultPffHeader
            {
                HeaderSize = reader.ReadInt32(),
                Signature = Encoding.ASCII.GetString(reader.ReadBytes(4)),
                RecordCount = reader.ReadInt32(),
                RecordSize = reader.ReadInt32(),
                RecordOffset = reader.ReadInt32(),
            };
            var header = _header;

            if (header.Signature != "PFF3" && header.Signature != "PFF4")
            {
                //throw new NotImplementedException("Unsupported PFF: " + header.Signature);
            }

            reader.BaseStream.Position = header.RecordOffset;

            for (int i = 0; i < header.RecordCount; i++)
            {
                var startPos = reader.BaseStream.Position;

                var deleted = reader.ReadInt32() == 1;
                var fileOffset = reader.ReadInt32();
                var fileSize = reader.ReadInt32();
                reader.ReadInt32(); // FileModified (unused)
                var fileNameBytes = reader.ReadBytes(16);
                var fileName = Encoding.ASCII.GetString(fileNameBytes).Trim('\0');

                var readBytes = reader.BaseStream.Position - startPos;
                reader.BaseStream.Seek(header.RecordSize - readBytes, SeekOrigin.Current);

                if (!deleted)
                {
                    var entry = new PackFileEntry(_path, fileOffset, deleted, fileName, fileSize, this);
                    _entries[fileName] = entry;
                }
            }
            _loaded = true;
        }

        public void Save()
        {
            if (!_loaded) Load();
            if (_header == null) throw new InvalidOperationException("Header not loaded.");

            var entries = _entries.Values.ToList();

            var newHeader = new DefaultPffHeader
            {
                HeaderSize = _header.HeaderSize,
                RecordCount = entries.Count,
                Signature = _header.Signature,
                RecordSize = _header.RecordSize,
                RecordOffset = entries.Sum(e => e.GetFileSize()) + _header.HeaderSize,
            };

            List<byte[]> data = new List<byte[]>();
            foreach (var entry in entries)
            {
                byte[] entryData = entry.GetRawBytes();
                data.Add(entryData);
            }

            using (var fs = new FileStream(_path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                // Write the header
                writer.Write(newHeader.HeaderSize);
                writer.Write(Encoding.ASCII.GetBytes(newHeader.Signature));
                writer.Write(newHeader.RecordCount);
                writer.Write(newHeader.RecordSize);
                writer.Write(newHeader.RecordOffset);

                // Write the data of each entry
                foreach (byte[] d in data)
                {
                    writer.Write(d);
                }

                // Write the file table
                int offset = newHeader.HeaderSize;
                foreach (var entry in entries)
                {
                    var startPos = writer.BaseStream.Position;

                    // Deleted flag
                    writer.Write(0); // 0 for not deleted

                    // File offset
                    writer.Write(offset);

                    // File size
                    writer.Write(entry.GetFileSize());

                    // File modified
                    writer.Write(1);

                    // File name
                    byte[] nameBytes = Encoding.ASCII.GetBytes(entry.GetFileName());

                    if (nameBytes.Length + 1 > 16)
                    {
                        // Truncate the name if it's longer than 15 characters
                        Array.Resize(ref nameBytes, 15);
                    }

                    writer.Write(nameBytes);

                    // Null terminator
                    writer.Write((byte)0);

                    // Padding to make the name field 16 bytes
                    int namePadding = 16 - (nameBytes.Length + 1);
                    for (int i = 0; i < namePadding; i++)
                    {
                        writer.Write((byte)0);
                    }

                    // Adjust for record size padding
                    var endPos = writer.BaseStream.Position;
                    int written = (int)(endPos - startPos);
                    int padding = newHeader.RecordSize - written;
                    for (int i = 0; i < padding; i++)
                    {
                        writer.Write((byte)0);
                    }

                    offset += entry.GetFileSize();
                }
            }

            data.Clear();
        }

        public IPackFileEntry GetEntry(string name)
        {
            if (!_loaded) Load();

            _entries.TryGetValue(name, out var entry);
            return entry;
        }

        public void AddEntry(string name, byte[] data, bool compressed = false)
        {
            if (!_loaded) Load();

            var entry = new InMemoryPackFileEntry(name, data, compressed, this);
            _entries[name] = entry;
        }

        public void DeleteEntry(IPackFileEntry entry)
        {
            _entries.Remove(entry.GetFileName());
        }
    }
}
