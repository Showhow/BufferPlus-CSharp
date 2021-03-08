using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus.PredefinedTypes {
    class StringBufferType : BufferType<string> {
        public override bool IsLittleEndian => false;

        public override string TypeString => "string";

        public override string Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).Take(length).ToArray();
            var result = encoding.GetString(bytes);
            return result;
        }

        public override byte[] Encode(BufferPlus bp, string value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = encoding.GetBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus buffer, string value) {
            if (buffer == null) {
                return Encoding.UTF8.GetByteCount(value);
            }
            return buffer.Encoding.GetByteCount(value);
        }
    }

    class VarIntBufferType : BufferType<int> {
        public override bool IsLittleEndian => false;

        public override string TypeString => "varint";

        public override int Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).ToArray();
            var value = VarintBitConverter.ToInt32(bytes);
            return value;
        }

        public override byte[] Encode(BufferPlus bp, int value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = VarintBitConverter.GetVarintBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus buffer, int value) {
            return VarintBitConverter.GetBytesLength<int>(value);
        }
    }


    class VarUIntBufferType : BufferType<uint> {
        public override bool IsLittleEndian => false;

        public override string TypeString => "varuint";

        public override uint Decode(BufferPlus bp, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = bp.Buffer.Skip(position).ToArray();
            var value = VarintBitConverter.ToUInt32(bytes);
            return value;
        }

        public override byte[] Encode(BufferPlus bp, uint value, Encoding encoding = null, int position = -1, int length = -1) {
            bp.SanitizeParameter(ref position, ref length, ref encoding);
            var bytes = VarintBitConverter.GetVarintBytes(value);
            bp.Memory.Write(bytes, 0, bytes.Length);
            return bytes;
        }

        public override int Size(BufferPlus buffer, uint value) {
            return VarintBitConverter.GetBytesLength<uint>(value);
        }
    }
}
