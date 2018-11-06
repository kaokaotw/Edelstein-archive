using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;

namespace Edelstein.Network.Packets
{
    internal static class PacketMethods
    {
        private static readonly Encoding StringEncoding = Encoding.ASCII;

        internal static readonly Dictionary<Type, Func<IByteBuffer, object>> DecodeMethods =
            new Dictionary<Type, Func<IByteBuffer, object>>
            {
                {typeof(byte), buffer => buffer.ReadByte()},
                {typeof(bool), buffer => buffer.ReadByte() > 0},
                {typeof(short), buffer => buffer.ReadShortLE()},
                {typeof(ushort), buffer => buffer.ReadUnsignedShortLE()},
                {typeof(int), buffer => buffer.ReadIntLE()},
                {typeof(uint), buffer => buffer.ReadUnsignedIntLE()},
                {typeof(long), buffer => buffer.ReadLongLE()},
                {typeof(float), buffer => buffer.ReadFloatLE()},
                {typeof(double), buffer => buffer.ReadDoubleLE()},
                {typeof(string), buffer => buffer.ReadString(buffer.ReadShortLE(), StringEncoding)},
                {typeof(DateTime), buffer => DateTime.FromFileTimeUtc(buffer.ReadLongLE())}
            };

        internal static readonly Dictionary<Type, Action<IByteBuffer, object>> EncodeMethods =
            new Dictionary<Type, Action<IByteBuffer, object>>
            {
                {typeof(byte), (buffer, value) => buffer.WriteByte((byte) value)},
                {typeof(bool), (buffer, value) => buffer.WriteByte((bool) value ? 1 : 0)},
                {typeof(short), (buffer, value) => buffer.WriteShortLE((short) value)},
                {typeof(int), (buffer, value) => buffer.WriteIntLE((int) value)},
                {typeof(long), (buffer, value) => buffer.WriteLongLE((long) value)},
                {
                    typeof(string), (buffer, value) =>
                    {
                        var str = (string) value ?? string.Empty;

                        buffer.WriteShortLE(str.Length);
                        buffer.WriteBytes(StringEncoding.GetBytes(str));
                    }
                },
                {typeof(float), (buffer, value) => buffer.WriteFloatLE((float) value)},
                {typeof(double), (buffer, value) => buffer.WriteDoubleLE((double) value)},
                {typeof(DateTime), (buffer, value) => buffer.WriteLongLE(((DateTime) value).ToFileTimeUtc())}
            };
    }
}