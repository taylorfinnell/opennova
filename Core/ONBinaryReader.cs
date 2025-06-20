using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace OpenNova.Core;

public class ONBinaryReader : BinaryReader
{
#if DEBUG
    private readonly Stack<long> positionMarks;
#endif

    public ONBinaryReader(Stream input) : base(input)
    {
#if DEBUG
        positionMarks = new Stack<long>();
#endif
    }

    public void MarkPosition()
    {
#if DEBUG
        positionMarks.Push(BaseStream.Position);
#endif
    }

    public void AssertBytesRead(long n)
    {
#if DEBUG
        if (positionMarks.Count == 0) throw new InvalidOperationException("No position mark to compare.");

        var markedPosition = positionMarks.Peek();
        var currentPosition = BaseStream.Position;

        var bytesRead = currentPosition - markedPosition;

        if (bytesRead != n)
            throw new InvalidOperationException($"Expected {n} bytes to be read, but {bytesRead} bytes were read.");

        positionMarks.Pop();
#endif
    }

    public void ClearMark()
    {
#if DEBUG
        if (positionMarks.Count == 0) throw new InvalidOperationException("No position mark to clear.");

        positionMarks.Pop();
#endif
    }

    public string ReadFixedString(int length)
    {
        var bytes = ReadBytes(length);
        var nullIndex = Array.IndexOf(bytes, (byte)0);
        if (nullIndex >= 0) return Encoding.ASCII.GetString(bytes, 0, nullIndex);

        return Encoding.ASCII.GetString(bytes);
    }


    public double ReadFixedPoint()
    {
        var decimalValue = Math.Round((double)ReadInt16() / 65536, 3);
        var wholeValue = ReadInt16();
        return wholeValue + decimalValue;
    }

    public Vector3 ReadVector3FP()
    {
        return new Vector3((float)ReadFixedPoint(), (float)ReadFixedPoint(), (float)ReadFixedPoint());
    }

    public Matrix4x4 ReadMatrix4x4()
    {
        var m11 = (float)ReadFixedPoint();
        var m12 = (float)ReadFixedPoint();
        var m13 = (float)ReadFixedPoint();
        var m14 = (float)ReadFixedPoint();

        var m21 = (float)ReadFixedPoint();
        var m22 = (float)ReadFixedPoint();
        var m23 = (float)ReadFixedPoint();
        var m24 = (float)ReadFixedPoint();

        var m31 = (float)ReadFixedPoint();
        var m32 = (float)ReadFixedPoint();
        var m33 = (float)ReadFixedPoint();
        var m34 = (float)ReadFixedPoint();

        var m41 = (float)ReadFixedPoint();
        var m42 = (float)ReadFixedPoint();
        var m43 = (float)ReadFixedPoint();
        var m44 = (float)ReadFixedPoint();

        return new Matrix4x4(
            m11, m12, m13, m14,
            m21, m22, m23, m24,
            m31, m32, m33, m34,
            m41, m42, m43, m44
        );
    }
}