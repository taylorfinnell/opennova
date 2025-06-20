using System.IO;

namespace OpenNova.Utils
{
    public class PcxParser
    {
        public struct PcxHeader
        {
            public byte Manufacturer;      // Should be 10
            public byte Version;           // PCX version
            public byte Encoding;          // 1 = RLE
            public byte BitsPerPixel;      // Should be 8 for these files
            public ushort XMin, YMin;      // Image dimensions
            public ushort XMax, YMax;
            public ushort HRes, VRes;      // Resolution 
            public byte[] ColorMap;        // 48 bytes
            public byte Reserved;          // Should be 0
            public byte NumPlanes;         // Number of color planes
            public ushort BytesPerLine;    // Bytes per scan line
            public ushort PaletteType;     // 1 = color/mono, 2 = grayscale
            public byte[] Filler;          // 58 bytes of filler
        }

        private const int HEADER_SIZE = 128;
        private const int PALETTE_SIZE = 768;  // 256 colors * 3 bytes (RGB)

        public struct PcxParseResult
        {
            public byte[] ImageData;
            public int Width;
            public int Height;
            public int BitsPerPixel;
            public byte[] Palette;
            public bool HasPalette;
        }

        public PcxParseResult Parse(byte[] pcxData)
        {
            using (var ms = new MemoryStream(pcxData))
            using (var br = new BinaryReader(ms))
            {
                var header = ReadHeader(br);

                int width = header.XMax - header.XMin + 1;
                int height = header.YMax - header.YMin + 1;
                int bytesPerLine = header.BytesPerLine;

                byte[] imageData = new byte[width * height];

                ms.Position = HEADER_SIZE;

                int y = 0;
                int pos = 0;

                while (y < height)
                {
                    int x = 0;
                    while (x < bytesPerLine)
                    {
                        byte byte1 = br.ReadByte();

                        if ((byte1 & 0xC0) == 0xC0)  // RLE marker (top 2 bits set)
                        {
                            int count = byte1 & 0x3F;  // Get repeat count from bottom 6 bits
                            byte value = br.ReadByte();  // Get value to repeat

                            for (int i = 0; i < count && x < bytesPerLine; i++)
                            {
                                if (x < width)  // Only write within image bounds
                                {
                                    imageData[pos++] = value;
                                }
                                x++;
                            }
                        }
                        else  // Raw byte
                        {
                            if (x < width)  // Only write within image bounds
                            {
                                imageData[pos++] = byte1;
                            }
                            x++;
                        }
                    }
                    y++;
                }

                // Read palette if present (256 color images have palette at end)
                byte[] palette = null;
                bool hasPalette = false;

                if (ms.Length - ms.Position >= PALETTE_SIZE + 1)
                {
                    // Check for palette marker (0x0C)
                    byte paletteMarker = br.ReadByte();
                    if (paletteMarker == 0x0C)
                    {
                        palette = br.ReadBytes(PALETTE_SIZE);
                        hasPalette = true;
                    }
                    else
                    {
                        // No palette marker, rewind
                        ms.Position--;
                    }
                }

                return new PcxParseResult
                {
                    ImageData = imageData,
                    Width = width,
                    Height = height,
                    BitsPerPixel = header.BitsPerPixel,
                    Palette = palette,
                    HasPalette = hasPalette
                };
            }
        }

        private PcxHeader ReadHeader(BinaryReader br)
        {
            var header = new PcxHeader
            {
                Manufacturer = br.ReadByte(),
                Version = br.ReadByte(),
                Encoding = br.ReadByte(),
                BitsPerPixel = br.ReadByte(),
                XMin = br.ReadUInt16(),
                YMin = br.ReadUInt16(),
                XMax = br.ReadUInt16(),
                YMax = br.ReadUInt16(),
                HRes = br.ReadUInt16(),
                VRes = br.ReadUInt16(),
                ColorMap = br.ReadBytes(48),
                Reserved = br.ReadByte(),
                NumPlanes = br.ReadByte(),
                BytesPerLine = br.ReadUInt16(),
                PaletteType = br.ReadUInt16(),
                Filler = br.ReadBytes(58)
            };

            return header;
        }
    }
}
