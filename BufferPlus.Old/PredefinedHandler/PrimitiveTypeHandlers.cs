using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus.Old.PredefinedHandler {

    class BooleanTypeHandler : BufferTypeHandler<bool> {
        public override bool ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) => Convert.ToBoolean(buffer[offset]);
        public override byte[] WriteBuffer(bool value, bool isLittleEndian) => new byte[] { Convert.ToByte(value) };
        public override string GetTypeString(bool isLittleEndian) => "bool";
        public override int GetBytesLength(bool value) => 1;
        public override int GetBytesLength() => 1;
        public override bool GetByteOrder(string typeString) => false;
    }

    class Int8TypeHandler : BufferTypeHandler<sbyte> {
        public override sbyte ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) => (sbyte)buffer[offset];
        public override byte[] WriteBuffer(sbyte value, bool isLittleEndian) => new byte[] { (byte)value };
        public override string GetTypeString(bool isLittleEndian) => "int8";
        public override int GetBytesLength(sbyte value) => 1;
        public override int GetBytesLength() => 1;
        public override bool GetByteOrder(string typeString) => false;
    }

    class UInt8TypeHandler : BufferTypeHandler<byte> {
        public override byte ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) => buffer[0];
        public override byte[] WriteBuffer(byte value, bool isLittleEndian = false) => new byte[] { value };
        public override string GetTypeString(bool isLittleEndian) => "uint8";
        public override int GetBytesLength(byte value) => 1;
        public override int GetBytesLength() => 1;
        public override bool GetByteOrder(string typeString) => false;
    }

    class Int16TypeHandler : BufferTypeHandler<short> {
        #region int16 / short
        public override short ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(2).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        public override byte[] WriteBuffer(short value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "int16le" : "int16be";

        public override int GetBytesLength(short value) => 2;
        public override int GetBytesLength() => 2;

        public override bool GetByteOrder(string typeString) => typeString == "int16le" ? true : false;
        #endregion
    }

    class UInt16TypeHandler : BufferTypeHandler<ushort> {
        #region uint16 / ushort
        public override ushort ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(2).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        public override byte[] WriteBuffer(ushort value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "uint16le" : "uint16be";

        public override int GetBytesLength(ushort value) => 2;
        public override int GetBytesLength() => 2;

        public override bool GetByteOrder(string typeString) => typeString == "uint16le" ? true : false;
        #endregion
    }

    class Int32TypeHandler : BufferTypeHandler<int> {
        #region int32
        public override int ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(4).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        public override byte[] WriteBuffer(int value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "int32le" : "int32be";

        public override int GetBytesLength(int value) => 4;
        public override int GetBytesLength() => 4;

        public override bool GetByteOrder(string typeString) => typeString == "int32le" ? true : false;
        #endregion
    }

    class UInt32TpeHandler : BufferTypeHandler<uint> {
        #region uint32
        public override uint ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(4).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public override byte[] WriteBuffer(uint value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "uint32le" : "uint32be";

        public override int GetBytesLength(uint value) => 4;
        public override int GetBytesLength() => 4;

        public override bool GetByteOrder(string typeString) => typeString == "uint32le" ? true : false;
        #endregion
    }

    class Int64TypeHandler : BufferTypeHandler<long> {
        #region int64 / long
        public override long ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(8).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToInt64(bytes, 0);
        }

        public override byte[] WriteBuffer(long value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }


        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "int64le" : "int64be";
        public override int GetBytesLength(long value) => 8;
        public override int GetBytesLength() => 8;
        public override bool GetByteOrder(string typeString) => typeString == "int64le" ? true : false;
        #endregion
    }

    class UInt64TypeHandler : BufferTypeHandler<ulong> {
        #region uint64 / ulong
        public override ulong ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(8).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToUInt64(bytes, 0);
        }

        public override byte[] WriteBuffer(ulong value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "uint64le" : "uint64be";
        public override int GetBytesLength(ulong value) => 8;
        public override int GetBytesLength() => 8;
        public override bool GetByteOrder(string typeString) => typeString == "uint64le" ? true : false;
        #endregion
    }

    class FloatTypeHandler : BufferTypeHandler<float> {
        #region float / float32
        public override float ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(4).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToSingle(bytes, 0);
        }

        public override byte[] WriteBuffer(float value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "float32le" : "float32be";

        public override int GetBytesLength(float value) => 4;
        public override int GetBytesLength() => 4;

        public override bool GetByteOrder(string typeString) => typeString == "float32le" ? true : false;
        #endregion
    }

    class DoubleTypeHandler : BufferTypeHandler<double> {
        #region double / float64
        public override double ReadBuffer(byte[] buffer, bool isLittleEndian, int offset, int length) {
            var bytes = buffer.Skip(offset).Take(8).ToArray();
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }

            return BitConverter.ToDouble(bytes, 0);
        }

        public override byte[] WriteBuffer(double value, bool isLittleEndian) {
            var bytes = BitConverter.GetBytes(value);
            if (isLittleEndian) {
                Array.Reverse(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public override string GetTypeString(bool isLittleEndian) => isLittleEndian ? "float64le" : "float64be";

        public override int GetBytesLength(double value) => 8;
        public override int GetBytesLength() => 8;

        public override bool GetByteOrder(string typeString) => typeString == "float64le" ? true : false;
        #endregion
    }


}
