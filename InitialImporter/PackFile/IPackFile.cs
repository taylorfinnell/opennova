using System.Collections.Generic;

namespace OpenNova.InitialImporter.PackFile
{
    public interface IPackFile
    {
        void Load();
        void Save();

        string GetFileName();
        string GetFilePath();

        IPackFileEntry GetEntry(string name);

        IEnumerable<IPackFileEntry> GetEntries();

        void AddEntry(string name, byte[] data, bool compressed = false);
        void DeleteEntry(IPackFileEntry entry);
    }
}
