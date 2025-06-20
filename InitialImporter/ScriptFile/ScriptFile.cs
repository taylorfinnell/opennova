using System;
using System.IO;

namespace OpenNova.InitialImporter.ScriptFile
{
    public enum NovalogicGame
    {
        JO_DFX_DFX2,
    }

    public static class ScriptFile
    {
        private const uint ScrHeader = 0x01524353;
        private const int ScrHeaderSize = 4;

        public static uint GameToKey(NovalogicGame game)
        {
            return game switch
            {
                NovalogicGame.JO_DFX_DFX2 => 0x2A5A8EAD, // 0xa55b1eed (.fx key),
                _ => throw new NotImplementedException($"Key not defined for game {game}"),
            };
        }

        public static byte[] Encrypt(byte[] bytes, NovalogicGame game)
        {
            uint key = GameToKey(game);
            var encryptedBytes = (byte[])bytes.Clone();

            XorBlock(encryptedBytes, key);
            Array.Reverse(encryptedBytes);

            var body = new byte[encryptedBytes.Length + ScrHeaderSize];
            BitConverter.GetBytes(ScrHeader).CopyTo(body, 0);
            encryptedBytes.CopyTo(body, ScrHeaderSize);

            return body;
        }

        public static byte[] Decrypt(byte[] bytes, NovalogicGame game)
        {
            if (bytes.Length < ScrHeaderSize)
                throw new IOException("Input data is too short to contain a valid header.");

            uint key = GameToKey(game);
            var header = BitConverter.ToUInt32(bytes, 0);

            if (header != ScrHeader)
                throw new Exception("Invalid SCR file header.");

            var bodyLength = bytes.Length - ScrHeaderSize;
            var body = new byte[bodyLength];
            Array.Copy(bytes, ScrHeaderSize, body, 0, bodyLength);

            Array.Reverse(body);
            XorBlock(body, key);

            return body;
        }

        private static void XorBlock(byte[] input, uint key)
        {
            uint tempKey = key;
            for (int i = 0; i < input.Length; i++)
            {
                uint temp1 = RotateLeft(tempKey, 11);
                uint temp2 = RotateLeft(tempKey + temp1, 4);

                tempKey = temp2 ^ 1;
                input[i] ^= (byte)(temp2 ^ 1);
            }
        }

        private static uint RotateLeft(uint value, int count)
        {
            return value << count | value >> 32 - count;
        }
    }
}
