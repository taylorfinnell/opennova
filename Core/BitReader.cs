namespace OpenNova.Core
{
    using System;

    public class BitReader : IDisposable
    {
        private readonly byte[] data;
        private int bytePosition;
        private byte bitPosition;
        private int bitCount;
        private uint mask;
        private bool disposed = false;

        public BitReader(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            data = buffer;
            bytePosition = 0;
            bitPosition = 0;
            bitCount = 0;
            mask = 0;
        }

        public void MaybeAlignToNextByte()
        {
            EnsureNotDisposed();

            if (bitPosition != 0)
            {
                bytePosition += 1;
                bitPosition = 0;
            }
        }

        public int ReadBits(int count)
        {
            EnsureNotDisposed();

            if (count <= 0 || count > 32)
                throw new ArgumentOutOfRangeException(nameof(count), "Bit count must be between 1 and 32.");

            if (EndOfBuffer)
                throw new InvalidOperationException("Cannot read beyond the end of the buffer.");

            int result = 0;
            int bitsRemaining = count;
            int currentBitPos = bitPosition;

            while (bitsRemaining > 0)
            {
                if (bytePosition >= data.Length)
                    throw new InvalidOperationException("Cannot read beyond the end of the buffer.");

                byte currentByte = data[bytePosition];
                int bitsAvailable = 8 - currentBitPos;
                int bitsToRead = Math.Min(bitsRemaining, bitsAvailable);
                int mask = (1 << bitsToRead) - 1;

                int bits = currentByte >> currentBitPos & mask;
                result |= bits << count - bitsRemaining;

                bitsRemaining -= bitsToRead;
                bytePosition += (currentBitPos + bitsToRead) / 8;
                currentBitPos = (byte)((currentBitPos + bitsToRead) % 8);
            }

            bitPosition = (byte)currentBitPos;
            bitCount = count;
            mask = count == 32 ? 0xFFFFFFFF : (1u << count) - 1;

            return result;
        }

        public void AlignTo4Bytes()
        {
            EnsureNotDisposed();

            if (bitPosition != 0)
            {
                bytePosition += 1;
                bitPosition = 0;
            }

            bytePosition = bytePosition + 3 & ~3;

            if (bytePosition > data.Length)
                bytePosition = data.Length;
        }

        public short ReadInt16()
        {
            // hack
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }

        public byte[] ReadBytes(int count)
        {
            EnsureNotDisposed();

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Byte count cannot be negative.");

            // Align to the next byte boundary if necessary
            MaybeAlignToNextByte();

            if (bytePosition + count > data.Length)
                throw new InvalidOperationException("Not enough data to read the specified number of bytes.");

            byte[] result = new byte[count];
            Array.Copy(data, bytePosition, result, 0, count);
            bytePosition += count;

            return result;
        }

        public bool EndOfBuffer
        {
            get
            {
                EnsureNotDisposed();
                return bytePosition >= data.Length;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }


                disposed = true;
            }
        }

        ~BitReader()
        {
            Dispose(false);
        }

        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BitReader));
        }
    }
}
