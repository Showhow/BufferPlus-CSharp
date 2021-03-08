using BitConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus.PredefinedTypes {

    class BooleanBufferType : BufferType<bool> {
        public override bool IsLittleEndian => false;

        public override string TypeString => "bool";

        public override bool Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(1).ToArray();
            var result = EndianBitConverter.BigEndian.ToBoolean(bytes,0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, bool value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes,0, 1);
            return bytes;
        }

        public override int Size(BufferPlus bp, bool value) => 1;
    }

    class Int8BufferType : BufferType<sbyte> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.Int8;

        public override sbyte Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            return (sbyte)bp.Memory.ReadByte();
        }

        public override byte[] Encode(BufferPlus bp, sbyte value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            bp.Memory.WriteByte((byte)value);
            return new byte[] { (byte)value  };
        }

        public override int Size(BufferPlus bp, sbyte value) => 1;
    }

    class Int16BEBufferType : BufferType<short> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.Int16BE;

        public override short Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(2).ToArray();
            var result = EndianBitConverter.BigEndian.ToInt16(bytes,0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, short value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, 2);
            return bytes;
        }

        public override int Size(BufferPlus bp, short value) => 2;
    }

    class Int16LEBufferType : BufferType<short> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.Int16LE;

        public override short Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(2).ToArray();
            var result = EndianBitConverter.LittleEndian.ToInt16(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, short value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, 2);
            return bytes;
        }

        public override int Size(BufferPlus bp, short value) => 2;
    }

    class Int32BEBufferType : BufferType<int> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.Int32BE;

        public override int Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(4).ToArray();
            var result = EndianBitConverter.BigEndian.ToInt32(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, int value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, int value) => 4;
    }

    class Int32LEBufferType : BufferType<int> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.Int32LE;

        public override int Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(2).ToArray();
            var result = EndianBitConverter.LittleEndian.ToInt32(bytes,0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, int value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, int value) => 4;
    }

    class Int64BEBufferType : BufferType<long> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.Int64BE;

        public override long Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(8).ToArray();
            var result = EndianBitConverter.BigEndian.ToInt64(bytes,0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, long value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, long value) => 8;
    }

    class Int64LEBufferType : BufferType<long> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.Int64LE;

        public override long Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(8).ToArray();
            var result = EndianBitConverter.BigEndian.ToInt64(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, long value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, long value) => 8;
    }


    class UInt8BufferType : BufferType<byte> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.UInt8;

        public override byte Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            return (byte)bp.Memory.ReadByte();
        }

        public override byte[] Encode(BufferPlus bp, byte value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            bp.Memory.WriteByte(value);
            return new byte[] { value };
        }

        public override int Size(BufferPlus bp, byte value) => 1;
    }

    class UInt16BEBufferType : BufferType<ushort> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.UInt16BE;

        public override ushort Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(2).ToArray();
            var result = EndianBitConverter.BigEndian.ToUInt16(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, ushort value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, ushort value) => 2;
    }

    class UInt16LEBufferType : BufferType<ushort> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.UInt16LE;

        public override ushort Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(2).ToArray();
            var result = EndianBitConverter.LittleEndian.ToUInt16(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, ushort value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, ushort value) => 2;
    }

    class UInt32BEBufferType : BufferType<uint> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.UInt32BE;

        public override uint Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(4).ToArray();
            var result = EndianBitConverter.BigEndian.ToUInt16(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, uint value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, uint value) => 4;
    }

    class UInt32LEBufferType : BufferType<uint> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.UInt32LE;

        public override uint Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(4).ToArray();
            var result = EndianBitConverter.LittleEndian.ToUInt16(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, uint value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, uint value) => 4;
    }

    class UInt64BEBufferType : BufferType<ulong> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.UInt64BE;

        public override ulong Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(8).ToArray();
            var result = EndianBitConverter.BigEndian.ToUInt64(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, ulong value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, ulong value) => 8;
    }

    class UInt64LEBufferType : BufferType<ulong> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.UInt64LE;

        public override ulong Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(8).ToArray();
            var result = EndianBitConverter.LittleEndian.ToUInt64(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, ulong value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, ulong value) => 8;
    }


    class Float32BEBufferType : BufferType<float> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.Float32BE;

        public override float Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(4).ToArray();
            var result = EndianBitConverter.BigEndian.ToSingle(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, float value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, float value) => 4;
    }

    class Float32LEBufferType : BufferType<float> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.Float32LE;

        public override float Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(4).ToArray();
            var result = EndianBitConverter.LittleEndian.ToSingle(bytes, 0);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, float value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, float value) => 4;
    }

    class Float64BEBufferType : BufferType<double> {
        public override bool IsLittleEndian => false;

        public override string TypeString => global::BufferPlus.TypeString.DoubleBE;

        public override double Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var result = EndianBitConverter.BigEndian.ToDouble(bp.Buffer, position);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, double value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.BigEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, double value) => 8;
    }

    class Float64LEBufferType : BufferType<double> {
        public override bool IsLittleEndian => true;

        public override string TypeString => global::BufferPlus.TypeString.DoubleLE;

        public override double Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var result = EndianBitConverter.LittleEndian.ToDouble(bp.Buffer, position);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, double value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = EndianBitConverter.LittleEndian.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus bp, double value) => 8;
    }

}
