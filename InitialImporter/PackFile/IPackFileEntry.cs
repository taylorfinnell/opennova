namespace OpenNova.InitialImporter.PackFile
{
    public interface IPackFileEntry
    {
        byte[] GetRawBytes();

        byte[] GetDecodedBytes();

        void SetData(byte[] data);

        bool IsDeleted();

        string GetFileName();

        string GetFileExtension();

        IPackFile GetPackFile();

        bool IsCompressed();
        bool IsScript();

        void Load();
        void Save(byte[] data);

        int GetFileSize();

        // Add a property to identify if the entry is new
        bool IsNew { get; }

        void Delete();
    }
}
