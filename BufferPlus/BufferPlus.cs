using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus {
    public class BufferPlus {

        #region Fields & Properties

        private MemoryStream _Memory = new MemoryStream();
        internal MemoryStream Memory {
            get => this._Memory;
        }

        public byte[] Buffer {
            get => this.Memory.GetBuffer();
        }

        public int Length {
            get => (int)this.Memory.Length;
            private set => this.Memory.SetLength(value);
        }

        public int Size {
            get => (int)this.Memory.Capacity;
            private set => this.Memory.Capacity = value;
        }

        public int Position {
            get => (int)this.Memory.Position;
            set {
                if (value > this.Length) {
                    this.Memory.SetLength(value);
                }
                this.Memory.Position = value;
            }
        }

        public int Remaining {
            get {
                return this.Length - this.Position;
            }
        }

        public static Encoding DefaultEncoding {
            get {
                return Encoding.UTF8;
            }
        }

        public Encoding Encoding {
            get;
            private set;
        }
        #endregion

        #region Constructor 
        public BufferPlus(int length = 4096) {
            this.Length = length;
            this.Size = this.Length;
            this.Encoding = BufferPlus.DefaultEncoding;
        }

        public BufferPlus(byte[] buffer) {
            this.Length = buffer.Length;
            this.Size = this.Length;
            this.Memory.Write(buffer, 0, buffer.Length);
            this.Encoding = BufferPlus.DefaultEncoding;
        }

        public BufferPlus(string str, Encoding encoding = null) {
            if (encoding == null) {
                this.Encoding = BufferPlus.DefaultEncoding;
            } else {
                this.Encoding = encoding;
            }

            var buffer = this.Encoding.GetBytes(str);

            this.Length = buffer.Length;
            this.Size = buffer.Length;
            this.Memory.Write(buffer, 0, buffer.Length);
        }

        public BufferPlus(BufferPlus bp) {
            this.Encoding = bp.Encoding;
            bp.Memory.CopyTo(this.Memory);

        }

        #endregion

        #region Sanitize Parameter Methods
        internal void SanitizeParameter(ref int position, ref int length, ref Encoding encoding, ref int count) {
            SanitizeParameter(ref position);

            if (length < 0) {
                length = this.Length - position;
            }

            if (encoding == null) {
                encoding = BufferPlus.DefaultEncoding;
            }

            if (count < 1) {
                count = 1;
            }
        }

        internal void SanitizeParameter(ref int position) {
            if (position < 0) {
                position = this.Position;
            } else {
                this.Position = (int)position;
            }
        }

        internal void SanitizeParameter(ref int position, ref Encoding encoding) {
            int l = 0;
            SanitizeParameter(ref position, ref l, ref encoding);
        }

        internal void SanitizeParameter(ref int position, ref int length) {
            SanitizeParameter(ref position);
            if (length < 0) {
                length = this.Length - position;
            }
        }

        internal void SanitizeParameter(ref int position, ref Encoding encoding, ref int count) {
            int l = 0;
            SanitizeParameter(ref position, ref l, ref encoding, ref count);
        }

        internal void SanitizeParameter(ref int position, ref int length, ref Encoding encoding) {
            int lc = 0;
            SanitizeParameter(ref position, ref length, ref encoding, ref lc);
        }
        #endregion

        #region Generic Read Methods
        public object Read(string typeString, int position = -1, int length = -1, Encoding encoding = null) {
            SanitizeParameter(ref position, ref length, ref encoding);
            var value = BufferType.ReadFunctions[typeString](this, encoding, position, length);
            this.Position += BufferType.SizeFunctions[typeString](this, value);
            return value;
        }
        public T Read<T>(string typeString, int position = -1, int length = -1, Encoding encoding = null) {
            return (T)Read(typeString, position, length, encoding);
        }

        public T Read<T>(string typeString, Encoding encoding) {
            return Read<T>(typeString, -1, -1, encoding);
        }

        public T Read<T>(string typeString) {
            return Read<T>(typeString, -1, -1, null);
        }

        public T[] ReadArray<T>(string typeString, int count = -1, int position = -1, int length = -1, Encoding encoding = null) {
            SanitizeParameter(ref position, ref length, ref encoding, ref count);
            T[] values = new T[count];

            if (typeString == TypeString.Buffer) {
                var buffers = new byte[count][];
                for (int i = 0; i < count; i++) {
                    buffers[i] = this.ReadPackedBuffer(-1);
                }
                return buffers as T[];
            }

            if (typeString == TypeString.String) {
                var results = new string[count];
                for (int i = 0; i < count; i++) {
                    results[i] = this.ReadPackedString(encoding);
                }
                return results as T[];
            }

            for (int i = 0; i < count; i++) {
                values[i] = this.Read<T>(typeString, encoding);
            }
            return values;
        }

        public T[] ReadArray<T>(string typeString, int count, Encoding encoding) {
            return ReadArray<T>(typeString, count, -1, -1, encoding);
        }

        public T[] ReadArray<T>(string typeString, int count) {
            return ReadArray<T>(typeString, count, -1, -1, null);
        }

        public T[] ReadPackedArray<T>(string typeString, Encoding encoding = null, int position = -1, int length = -1) {
            SanitizeParameter(ref position, ref length, ref encoding);
            int count = (int)this.ReadVarUInt(position);
            return this.ReadArray<T>(typeString, count, -1, length, encoding);
        }

        public T[] ReadPackedArray<T>(string typeString, Encoding encoding) {
            return ReadPackedArray<T>(typeString, encoding, -1, -1);
        }

        public T[] ReadPackedArray<T>(string typeString) {
            return ReadPackedArray<T>(typeString, null, -1, -1);
        }
        #endregion

        #region Generic Write Methods
        public BufferPlus Write<T>(T value, string typeString, Encoding encoding = null, int position = -1, int length = -1) {
            SanitizeParameter(ref position, ref length, ref encoding);
            BufferType.WriteFunctions[typeString](this, value, encoding, position, length);
            return this;
        }

        public BufferPlus Write<T>(T value, string typeString, int position = -1, int length = -1) {
            return Write<T>(value, typeString, null, position, length); ;
        }

        public BufferPlus Write<T>(T value, string typeString) {
            return Write<T>(value, typeString, null, -1, -1);
        }

        public BufferPlus WriteArray<T>(T[] values, string typeString, Encoding encoding = null, int position = -1, int length = -1) {
            SanitizeParameter(ref position, ref length, ref encoding);

            if (typeof(byte[]) == typeof(T)) {
                for (int i = 0; i < values.Length; i++) {
                    this.WritePackedBuffer(values[i] as byte[]);
                }
            } else if (typeof(string) == typeof(T)) {
                for (int i = 0; i < values.Length; i++) {
                    this.WritePackedString(values[i] as string);
                }
            } else {
                for (int i = 0; i < values.Length; i++) {
                    this.Write<T>(values[i], typeString, encoding);
                }
            }
            return this;
        }

        public BufferPlus WriteArray<T>(T[] values, string typeString, int position = -1, int length = -1) {
            return this.WriteArray<T>(values, typeString, null, position, length);
        }

        public BufferPlus WritePackedArray<T>(T[] values, string typeString, Encoding encoding = null, int position = -1, int length = -1) {
            SanitizeParameter(ref position, ref length, ref encoding);

            if (values is string[]) {
                return this.WritePackedStringArray(values as string[], encoding, position);
            } else if (values is byte[][]) {
                return this.WritePackedBufferArray(values as byte[][], position);
            } else {
                this.WriteVarUInt((uint)values.Length);
                return this.WriteArray<T>(values, typeString, encoding, -1, length);
            }
        }

        public BufferPlus WritePackedArray<T>(T[] values, string typeString, int position = -1, int length = -1) {
            return this.WritePackedArray<T>(values, typeString, null, position, length);
        }
        #endregion

        #region -- Boolean --
        public bool ReadBoolean(int position = -1) {
            var result = this.Read<bool>(TypeString.Bool, position);
            return result;
        }
        #endregion

        #region -- Integers --
        // Readers
        public sbyte ReadInt8(int position = -1) {
            var result = this.Read<sbyte>(TypeString.Int8, position);
            return result;
        }

        public short ReadInt16BE(int position = -1) {
            var result = this.Read<short>(TypeString.Int16BE, position);
            return result;
        }

        public short ReadInt16LE(int position = -1) {
            var result = this.Read<short>(TypeString.Int16LE, position);
            return result;
        }

        public int ReadInt32BE(int position = -1) {
            var result = this.Read<int>(TypeString.Int32BE, position);
            return result;
        }

        public int ReadInt32LE(int position = -1) {
            var result = this.Read<int>(TypeString.Int32LE, position);
            return result;
        }

        public long ReadInt64BE(int position = -1) {
            var result = this.Read<long>(TypeString.Int64BE, position);
            return result;
        }

        public long ReadInt64LE(int position = -1) {
            var result = this.Read<long>(TypeString.Int64LE, position);
            return result;
        }

        // Writers
        public BufferPlus WriteInt8(sbyte value, int position = -1) {
            this.Write(value, TypeString.Int8, position);
            return this;
        }

        public BufferPlus WriteInt16BE(short value, int position = -1) {
            this.Write(value, TypeString.Int16BE, position);
            return this;
        }

        public BufferPlus WriteInt16LE(short value, int position = -1) {
            this.Write(value, TypeString.Int16LE, position);
            return this;
        }

        public BufferPlus WriteInt32BE(int value, int position = -1) {
            this.Write(value, TypeString.Int32BE, position);
            return this;
        }

        public BufferPlus WriteInt32LE(int value, int position = -1) {
            this.Write(value, TypeString.Int32LE, position);
            return this;
        }

        public BufferPlus WriteInt64BE(long value, int position = -1) {
            this.Write(value, TypeString.Int64BE, position);
            return this;
        }
        public BufferPlus WriteInt64LE(long value, int position = -1) {
            this.Write(value, TypeString.Int64LE, position);
            return this;
        }

        #endregion

        #region -- Unsigned Integers --
        // Readers
        public byte ReadUInt8(int position = -1) {
            var result = this.Read<byte>(TypeString.UInt8, position);
            return result;
        }

        public ushort ReadUInt16BE(int position = -1) {
            var result = this.Read<ushort>(TypeString.UInt16BE, position);
            return result;
        }

        public ushort ReadUInt16LE(int position = -1) {
            var result = this.Read<ushort>(TypeString.UInt16LE, position);
            return result;
        }

        public uint ReadUInt32BE(int position = -1) {
            var result = this.Read<uint>(TypeString.UInt32BE, position);
            return result;
        }

        public uint ReadUInt32LE(int position = -1) {
            var result = this.Read<uint>(TypeString.UInt32LE, position);
            return result;
        }

        public ulong ReadUInt64BE(int position = -1) {
            var result = this.Read<ulong>(TypeString.UInt64BE, position);
            return result;
        }

        public ulong ReadUInt64LE(int position = -1) {
            var result = this.Read<ulong>(TypeString.UInt64LE, position);
            return result;
        }

        // Writers
        public BufferPlus WriteUInt8(byte value, int position = -1) {
            this.Write(value, TypeString.UInt8, position);
            return this;
        }

        public BufferPlus WriteUInt16BE(ushort value, int position = -1) {
            this.Write(value, TypeString.UInt16BE, position);
            return this;
        }

        public BufferPlus WriteUInt16LE(ushort value, int position = -1) {
            this.Write(value, TypeString.UInt16LE, position);
            return this;
        }

        public BufferPlus WriteUInt32BE(uint value, int position = -1) {
            this.Write(value, TypeString.UInt32BE, position);
            return this;
        }
        public BufferPlus WriteUInt32BE(int value, int position = -1) => WriteUInt32BE((uint) value, position);

        public BufferPlus WriteUInt32LE(uint value, int position = -1) {
            this.Write(value, TypeString.UInt32LE, position);
            return this;
        }
        public BufferPlus WriteUInt32LE(int value, int position = -1) => WriteUInt32LE((uint)value, position);

        public BufferPlus WriteUInt64BE(ulong value, int position = -1) {
            this.Write(value, TypeString.UInt64BE, position);
            return this;
        }
        public BufferPlus WriteUInt64BE(long value, int position = -1)=> WriteUInt64BE((ulong) value, position);

        public BufferPlus WriteUInt64LE(ulong value, int position = -1) {
            this.Write(value, TypeString.UInt64LE, position);
            return this;
        }
        public BufferPlus WriteUInt64LE(long value, int position = -1) => WriteUInt64LE((ulong)value, position);

        #endregion

        #region -- Floating Points --
        // Readers
        public float ReadFloatBE(int position = -1) {
            return this.Read<float>(TypeString.Float32BE, position);
        }

        public float ReadFloatLE(int position = -1) {
            return this.Read<float>(TypeString.Float32LE, position);
        }

        public double ReadDoubleBE(int position = -1) {
            return this.Read<double>(TypeString.Float64BE, position);
        }

        public double ReadDoubleLE(int position = -1) {
            return this.Read<double>(TypeString.Float64LE, position);
        }

        // Writers
        public BufferPlus WriteFloatBE(float value, int position = -1) {
            return this.Write(value, TypeString.Float32BE, position);
        }

        public BufferPlus WriteFloatLE(float value, int position = -1) {
            return this.Write(value, TypeString.Float32LE, position);
        }

        public BufferPlus WriteDoubleBE(double value, int position = -1) {
            return this.Write(value, TypeString.Float64BE, position);
        }

        public BufferPlus WriteDoubleLE(double value, int position = -1) {
            return this.Write(value, TypeString.Float64LE, position);
        }
        #endregion

        #region -- VarInt From Int32 --
        public int ReadVarInt(int position = -1) {
            SanitizeParameter(ref position);
            return this.Read<int>(TypeString.VarInt, position);
        }

        public int[] ReadVarIntArray(int count, int position = -1) {
            SanitizeParameter(ref position);

            if (count < 0) {
                count = 1;
            }

            int[] results = new int[(int)count];

            for (int i = 0; i < count; i++) {
                results[i] = this.ReadVarInt();
            }

            return results;
        }

        public int[] ReadVarIntPackedArray(int position = -1) {
            SanitizeParameter(ref position);

            int count = (int)this.ReadVarUInt(position);

            int[] results = new int[(int)count];

            for (int i = 0; i < count; i++) {
                results[i] = this.ReadVarInt();
            }

            return results;
        }

        public BufferPlus WriteVarInt(int value, int position = -1) {
            SanitizeParameter(ref position);
            var bytes = VarintBitConverter.GetVarintBytes(value);
            this.Memory.Write(bytes, 0, bytes.Length);
            return this;
        }

        public BufferPlus WriteVarIntArray(int[] values, int position = -1) {
            var bytes_array = new List<byte[]>();
            SanitizeParameter(ref position);

            for (int i = 0; i < values.Length; i++) {
                this.WriteVarInt(values[i]);
            }
            return this;
        }

        public BufferPlus WriteVarIntPackedArray(int[] values, int position = -1) {
            SanitizeParameter(ref position);

            this.WriteVarUInt((uint)values.Length);
            return this.WriteVarIntArray(values);
        }

        #endregion

        #region -- VarUInt From UInt32 --
        public uint ReadVarUInt(int position = -1) {
            SanitizeParameter(ref position);
            return this.Read<uint>(TypeString.VarUInt, position);
        }

        public uint[] ReadVarUIntArray(int count, int position = -1) {
            SanitizeParameter(ref position);

            if (count < 0) {
                count = 1;
            }

            uint[] results = new uint[(int)count];

            for (int i = 0; i < count; i++) {
                results[i] = this.ReadVarUInt();
            }

            return results;
        }

        public uint[] ReadVarUIntPackedArray(int position = -1) {
            SanitizeParameter(ref position);

            int count = (int)this.ReadVarUInt(position);

            uint[] results = new uint[count];

            for (int i = 0; i < count; i++) {
                results[i] = this.ReadVarUInt();
            }

            return results;
        }



        public BufferPlus WriteVarUInt(uint value, int position = -1) {
            SanitizeParameter(ref position);
            this.Write(value, TypeString.VarUInt, null, position);
            return this;
        }

        public BufferPlus WriteVarUInt(int value, int position = -1) {
            return WriteVarUInt((uint)value, position);
        }

        public BufferPlus WriteVarUIntArray(uint[] values, int position = -1) {
            var bytes_array = new List<byte[]>();
            SanitizeParameter(ref position);

            for (int i = 0; i < values.Length; i++) {
                this.WriteVarUInt(values[i]);
            }
            return this;
        }

        public BufferPlus WriteVarUIntPackedArray(int[] values, int position = -1) {
            SanitizeParameter(ref position);

            this.WriteVarUInt((uint)values.Length);
            return this.WriteVarIntArray(values);
        }

        #endregion

        #region -- Buffer --
        public byte[] ReadBuffer(int length, int position = -1) {
            SanitizeParameter(ref position, ref length);
            return this.Read<byte[]>(TypeString.Buffer, position, length);
        }

        public byte[] ReadPackedBuffer(int position = -1) {
            SanitizeParameter(ref position);
            var len = (int)this.ReadVarUInt();
            return this.Read<byte[]>(TypeString.Buffer, -1, len);
        }

        public byte[][] ReadPackedBufferArray(int position = -1) {
            SanitizeParameter(ref position);
            int count = (int)this.ReadVarUInt(position);
            var buffers = new byte[count][];
            for (int i = 0; i < count; i++) {
                buffers[i] = this.ReadPackedBuffer(-1);
            }
            return buffers;
        }

        public BufferPlus WriteBuffer(byte[] buffer, int position = -1, int offset = 0) {
            SanitizeParameter(ref position);
            this.Write(buffer.Skip(offset).ToArray(), TypeString.Buffer, null, position);
            return this;
        }

        public BufferPlus WritePackedBuffer(byte[] buffer, int position = -1, int offset = 0) {
            SanitizeParameter(ref position);
            var bytes = buffer.Skip(offset).ToArray();
            int len = bytes.Length;
            this.WriteVarUInt(len);
            this.WriteBuffer(bytes);
            return this;
        }

        public BufferPlus WritePackedBufferArray(byte[][] values, int position = -1) {
            SanitizeParameter(ref position);
            int count = values.Length;
            this.WriteVarUInt(count);
            for (int i = 0; i < count; i++) {
                WritePackedBuffer(values[i]);
            }
            return this;
        }
        #endregion

        #region -- String --
        public string ReadString(int length, int position = -1) {
            SanitizeParameter(ref position, ref length);
            return this.Read<string>(TypeString.String, position, length);
        }

        public string[] ReadStringArray(int count, Encoding encoding = null, int position = -1) {
            SanitizeParameter(ref position, ref encoding);
            var results = new string[count];
            for (int i = 0; i < count; i++) {
                results[i] = ReadPackedString(encoding);
            }
            return results;
        }

        public string ReadPackedString(Encoding encoding = null, int position = -1) {
            SanitizeParameter(ref position, ref encoding);
            int len = (int)this.ReadVarUInt();
            return this.Read<string>(TypeString.String, -1, len, encoding);
        }

        public string[] ReadPackedStringArray(Encoding encoding = null, int position = -1) {
            SanitizeParameter(ref position);
            int count = (int)this.ReadVarUInt(position);
            var results = new string[count];
            for (int i = 0; i < count; i++) {
                results[i] = this.ReadPackedString(encoding, -1);
            }
            return results;
        }

        public BufferPlus WriteString(string value, Encoding encoding = null, int position = -1) {
            this.Write(value, TypeString.String, encoding, position);
            return this;
        }

        public BufferPlus WritePackedString(string value, Encoding encoding = null, int position = -1) {
            SanitizeParameter(ref position, ref encoding);
            int len = encoding.GetByteCount(value);
            this.WriteVarUInt(len);
            this.WriteString(value, encoding);
            return this;
        }

        public BufferPlus WritePackedStringArray(string[] values, Encoding encoding = null, int position = -1) {
            SanitizeParameter(ref position, ref encoding);
            int count = values.Length;
            this.WriteVarUInt(count);
            for (int i = 0; i < count; i++) {
                WritePackedString(values[i], encoding);
            }
            return this;
        }

        #endregion

        #region == Overwrite Read ==
        //public T Read<T>(int postition = -1, int length = -1,Encoding encoding = null, bool isLittleEndain) {
        //    var type = typeof(T);
        //    string defaultType = "";
        //    return this.Read<T>(defaultType, postition, length, encoding);
        //}
        #endregion

        #region == Overwrite ReadArray ==
        //public BufferPlus WriteArray(bool[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Boolean, postition);
        //}

        //public BufferPlus WriteArray(sbyte[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Int8, postition);
        //}

        //public BufferPlus WriteArray(short[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Int16BE, postition);
        //}

        //public BufferPlus WriteArray(int[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Int32BE, postition);
        //}

        //public BufferPlus WriteArray(long[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Int64BE, postition);
        //}

        //public BufferPlus WriteArray(byte[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.UInt8, postition);
        //}

        //public BufferPlus WriteArray(ushort[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.UInt16BE, postition);
        //}

        //public BufferPlus WriteArray(uint[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.UInt32BE, postition);
        //}

        //public BufferPlus WriteArray(ulong[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.UInt64BE, postition);
        //}

        //public BufferPlus WriteArray(float[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Float32BE, postition);
        //}

        //public BufferPlus WriteArray(double[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Float64BE, postition);
        //}

        //public BufferPlus WriteArray(string[] value, int postition = -1) {
        //    return this.WriteArray(value, TypeString.Float64BE, postition);
        //}

        //public BufferPlus WriteArray(byte[][] value, int postition = -1) {
        //    return this.Write(value, TypeString.Buffer, postition, -1);
        //}

        #endregion

        #region == Overwrite ReadPackedArray ==
        //public BufferPlus WritePackedArray(bool[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Boolean, postition);
        //}

        //public BufferPlus WritePackedArray(sbyte[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Int8, postition);
        //}

        //public BufferPlus WritePackedArray(short[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Int16BE, postition);
        //}

        //public BufferPlus WritePackedArray(int[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Int32BE, postition);
        //}

        //public BufferPlus WritePackedArray(long[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Int64BE, postition);
        //}

        //public BufferPlus WritePackedArray(byte[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.UInt8, postition);
        //}

        //public BufferPlus WritePackedArray(ushort[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.UInt16BE, postition);
        //}

        //public BufferPlus WritePackedArray(uint[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.UInt32BE, postition);
        //}

        //public BufferPlus WritePackedArray(ulong[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.UInt64BE, postition);
        //}

        //public BufferPlus WritePackedArray(float[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Float32BE, postition);
        //}

        //public BufferPlus WritePackedArray(double[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Float64BE, postition);
        //}

        //public BufferPlus WritePackedArray(string[] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Float64BE, postition);
        //}

        //public BufferPlus WritePackedArray(byte[][] value, int postition = -1) {
        //    return this.WritePackedArray(value, TypeString.Buffer, postition, -1);
        //}
        #endregion

        #region == Overwrite Write ==
        public BufferPlus Write(bool value, int postition = -1) {
            return this.Write(value, TypeString.Boolean, postition);
        }

        public BufferPlus Write(sbyte value, int postition = -1) {
            return this.Write(value, TypeString.Int8, postition);
        }

        public BufferPlus Write(short value, int postition = -1) {
            return this.Write(value, TypeString.Int16BE, postition);
        }

        public BufferPlus Write(int value, int postition = -1) {
            return this.Write(value, TypeString.Int32BE, postition);
        }

        public BufferPlus Write(long value, int postition = -1) {
            return this.Write(value, TypeString.Int64BE, postition);
        }

        public BufferPlus Write(byte value, int postition = -1) {
            return this.Write(value, TypeString.UInt8, postition);
        }

        public BufferPlus Write(ushort value, int postition = -1) {
            return this.Write(value, TypeString.UInt16BE, postition);
        }

        public BufferPlus Write(uint value, int postition = -1) {
            return this.Write(value, TypeString.UInt32BE, postition);
        }

        public BufferPlus Write(ulong value, int postition = -1) {
            return this.Write(value, TypeString.UInt64BE, postition);
        }

        public BufferPlus Write(float value, int postition = -1) {
            return this.Write(value, TypeString.Float32BE, postition);
        }

        public BufferPlus Write(double value, int postition = -1) {
            return this.Write(value, TypeString.Float64BE, postition);
        }

        public BufferPlus Write(string value, Encoding encoding = null, int postition = -1) {
            return this.Write(value, TypeString.String, encoding, postition, -1);
        }

        public BufferPlus Write(byte[] value, int postition = -1) {
            return this.Write(value, TypeString.Buffer, postition, -1);
        }

        #endregion

        #region == Overwrite WriteArray ==
        public BufferPlus WriteArray(bool[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Boolean, postition);
        }

        public BufferPlus WriteArray(sbyte[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Int8, postition);
        }

        public BufferPlus WriteArray(short[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Int16BE, postition);
        }

        public BufferPlus WriteArray(int[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Int32BE, postition);
        }

        public BufferPlus WriteArray(long[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Int64BE, postition);
        }

        public BufferPlus WriteArray(byte[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.UInt8, postition);
        }

        public BufferPlus WriteArray(ushort[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.UInt16BE, postition);
        }

        public BufferPlus WriteArray(uint[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.UInt32BE, postition);
        }

        public BufferPlus WriteArray(ulong[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.UInt64BE, postition);
        }

        public BufferPlus WriteArray(float[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Float32BE, postition);
        }

        public BufferPlus WriteArray(double[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Float64BE, postition);
        }

        public BufferPlus WriteArray(string[] value, int postition = -1) {
            return this.WriteArray(value, TypeString.Float64BE, postition);
        }

        public BufferPlus WriteArray(byte[][] value, int postition = -1) {
            return this.Write(value, TypeString.Buffer, postition, -1);
        }

        #endregion

        #region == Overwrite WritePackedArray ==
        public BufferPlus WritePackedArray(bool[] values, int postition = -1) {
            return this.WritePackedArray(values, TypeString.Boolean, postition);
        }

        public BufferPlus WritePackedArray(sbyte[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Int8, postition);
        }

        public BufferPlus WritePackedArray(short[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Int16BE, postition);
        }

        public BufferPlus WritePackedArray(int[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Int32BE, postition);
        }

        public BufferPlus WritePackedArray(long[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Int64BE, postition);
        }

        public BufferPlus WritePackedArray(byte[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.UInt8, postition);
        }

        public BufferPlus WritePackedArray(ushort[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.UInt16BE, postition);
        }

        public BufferPlus WritePackedArray(uint[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.UInt32BE, postition);
        }

        public BufferPlus WritePackedArray(ulong[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.UInt64BE, postition);
        }

        public BufferPlus WritePackedArray(float[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Float32BE, postition);
        }

        public BufferPlus WritePackedArray(double[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Float64BE, postition);
        }

        public BufferPlus WritePackedArray(string[] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Float64BE, postition);
        }

        public BufferPlus WritePackedArray(byte[][] value, int postition = -1) {
            return this.WritePackedArray(value, TypeString.Buffer, postition, -1);
        }
        #endregion

        #region Convert Methods
        public byte[] ToBuffer() {
            this.Seal();
            return this.Memory.ToArray();
        }

        public byte[] ToByteArray(int packs = 1) {
            byte[] array = this.Memory.ToArray();

            if (packs > 1 && array.Length % packs > 0) {
                var new_array = new byte[array.Length + (packs - array.Length % packs)];
                System.Buffer.BlockCopy(array, 0, new_array, 0, array.Length);
                return new_array;
            }

            return array;
        }

        public sbyte[] ToInt8Array() {
            var buffer = this.ToByteArray();
            var array = new sbyte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++) {
                array[i] = Convert.ToSByte(buffer[i]);
            }
            return array;
        }

        public Int16[] ToInt16Array() {
            int packs = 2;
            var buffer = this.ToByteArray(packs);

            var array = new Int16[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToInt16(buffer, i * packs);
            }
            return array;
        }

        public UInt16[] ToUInt16Array() {
            int packs = 2;
            var buffer = this.ToByteArray(packs);

            var array = new UInt16[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToUInt16(buffer, i * packs);
            }
            return array;
        }

        public Int32[] ToInt32Array() {
            int packs = 4;
            var buffer = this.ToByteArray(packs);

            var array = new Int32[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToInt32(buffer, i * packs);
            }
            return array;
        }

        public UInt32[] ToUInt32Array() {
            int packs = 4;
            var buffer = this.ToByteArray(packs);

            var array = new UInt32[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToUInt32(buffer, i * packs);
            }
            return array;
        }

        public Int64[] ToInt64Array() {
            int packs = 8;
            var buffer = this.ToByteArray(packs);

            var array = new Int64[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToInt64(buffer, i * packs);
            }
            return array;
        }

        public UInt64[] ToUInt64Array() {
            int packs = 8;
            var buffer = this.ToByteArray(packs);

            var array = new UInt64[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = System.BitConverter.ToUInt64(buffer, i * packs);
            }
            return array;
        }

        public string ToString(Encoding encoding) {
            var buffer = this.ToByteArray();
            return encoding.GetString(buffer);
        }

        public new string ToString() {
            var buffer = this.ToByteArray();
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public string ToHex(int col = 0, int paddings = 0, int skipPaddings = 0, bool printPos = false) {
            var bytes = this.ToByteArray();
            if (col > 0) {
                int i = 0;
                var query = from s in bytes
                            let num = i++
                            group s by num / col into g
                            select g.ToArray();
                var buffers = query.ToArray();
                var results = new List<string>();


                int current_line = 0;
                int pos_line = (this.Position - 1) / col;
                foreach (var buffer in buffers) {

                    int j = 0;
                    var q = from s in buffer
                            let num = j++
                            group s by num / 4 into g
                            select g.ToArray();
                    var sub_bytes_group = q.ToArray();

                    var sub_bytes_results = new List<string>();
                    foreach (var sub_bytes in sub_bytes_group) {
                        var sub_bytes_result = System.BitConverter.ToString(sub_bytes);
                        sub_bytes_results.Add(sub_bytes_result);
                    }

                    var result = String.Join(" ", sub_bytes_results);


                    if (skipPaddings > 0) {
                        skipPaddings--;
                    } else {
                        result = result.PadLeft(paddings + result.Length, ' ');
                    }

                    if (printPos && current_line == pos_line) {
                        result += "\n" + "^^".PadLeft(
                            Math.Abs(paddings + (this.Position) % col * 3 - 1),
                            ' ');
                    } else {
                        result += "\n";
                    }

                    results.Add(result);

                    current_line++;
                }

                return String.Join("\n", results);
            }

            return System.BitConverter.ToString(bytes);
        }

        #endregion

        #region Flag & Position Methods
        public BufferPlus Reset() {
            this.Position = 0;
            return this;
        }

        public byte[] RemainingBuffer {
            get {
                var bytes = this.Buffer;
                var segment = new ArraySegment<byte>(bytes, this.Position, this.Remaining);
                return segment.Array;
            }
        }

        public BufferPlus MoveTo(int position) {
            this.Position = position;
            return this;
        }

        public BufferPlus Skip(int offset) {
            this.Position += offset;
            return this;
        }

        public BufferPlus Rewind(int offset) {
            this.Position -= offset;
            return this;
        }

        public BufferPlus FillLength(int size) {
            return this;
        }

        public BufferPlus Seal() {
            this.Length = this.Position;
            return this;
        }
        #endregion

        #region Static Factory Methods
        public static BufferPlus Create(int size = 4096) {
            return new BufferPlus(size);
        }

        public static BufferPlus Create(byte[] buff) {
            return new BufferPlus(buff);
        }

        public static BufferPlus Create(string str, Encoding encode = null) {
            return new BufferPlus(str, encode);
        }

        public static BufferPlus Create(BufferPlus bp) {
            return new BufferPlus(bp);
        }

        [Obsolete]
        public static BufferPlus Alloc(int size = 4096, bool fill = false, Encoding encoding = null) {
            var bp = new BufferPlus(size);
            if (encoding == null) {
                bp.Encoding = encoding;
            }
            return new BufferPlus(size);
        }

        [Obsolete]
        public static BufferPlus AllocUnsafe(int size = 4096) {
            var bp = new BufferPlus(size);
            return new BufferPlus(size);
        }

        public static bool Compare(byte[] buf1, byte[] buf2) {
            return buf1.SequenceEqual(buf2);
        }

        public static bool Compare(BufferPlus bp1, BufferPlus bp2) {
            return BufferPlus.Compare(bp1.Buffer, bp2.Buffer);
        }

        public static bool Compare(BufferPlus bp, byte[] buff) {
            return BufferPlus.Compare(bp.Buffer, buff);
        }

        public static bool Compare(byte[] buff, BufferPlus bp) {
            return BufferPlus.Compare(bp.Buffer, buff);
        }

        public static BufferPlus Concat(params byte[][] buffers) {
            var bp = new BufferPlus();
            foreach (byte[] buffer in buffers) {
                bp.Memory.Write(buffer, 0, buffer.Length);
            }

            bp.Length = bp.Position;
            return bp;
        }

        public static BufferPlus Concat(params BufferPlus[] bps) {
            var bp = new BufferPlus();
            foreach (BufferPlus b in bps) {
                b.Memory.CopyTo(bp.Memory);
            }

            return bp;
        }

        public static BufferPlus From(BufferPlus bp) {
            var b = new BufferPlus();
            b._Memory = bp.Memory;
            return bp;
        }

        public static BufferPlus Clone(byte[] buffer) {
            var bp = new BufferPlus(buffer);
            return bp;
        }

        public bool IsBuffer(object obj) {
            return obj is byte[];
        }

        public bool IsBufferPlus(object obj) {
            return obj is BufferPlus;
        }

        public bool IsEncoding(object obj) {
            return obj is Encoding;
        }

        public static int ByteLength<T>(T value, string typeString, Encoding encoding = null) {
            var bType = BufferType.GetBufferType(typeString) as BufferType<T>;
            return bType.Size(value);
        }

        public static int ByteLengthPackedString(string value, Encoding encoding = null) {
            int count = VarintBitConverter.GetBytesLength<string>(value, encoding);
            VarintBitConverter.GetBytesLength(count);
            return count + VarintBitConverter.GetBytesLength(count);
        }

        public static int ByteLengthPackedBuffer(byte[] value) {
            int count = value.Length;
            VarintBitConverter.GetBytesLength(count);
            return count + VarintBitConverter.GetBytesLength(count);
        }

        #endregion

        #region Schemes Methods
        private static Dictionary<string, BufferSchema> _Schemas = new Dictionary<string, BufferSchema>();
        public static Dictionary<string, BufferSchema> Schemas {
            get => _Schemas;
        }

        public static bool HasSchema(string name) {
            return Schemas.ContainsKey(name);
        }

        public static BufferSchema GetSchema(string name) {
            return Schemas[name];
        }

        public static List<BufferSchema> GetAllSchemas() {
            return Schemas.Values.ToList();
        }

        public static BufferSchema CreateSchema(string name, string jsonSchema) {
            var schema = new BufferSchema();
            schema.Name = name;
            schema.SetJsonSchema(jsonSchema);
            schema.Build();

            Schemas.Add(name, schema);
            return schema;
        }

        public static int ByteLengthSchema(string name, object obj) {
            var schema = BufferPlus.GetSchema(name);
            var bp = new BufferPlus(256);
            schema.Encode(bp, obj);

            return bp.Position - 1;
        }

        public object ReadSchema(string name, object obj, int position = 0) {
            this.Position = position;

            var schema = BufferPlus.GetSchema(name);
            schema.Decode(this, obj);

            return obj;
        }


        public T ReadSchema<T>(string name, T obj, int position = 0) {
            this.Position = position;

            var schema = BufferPlus.GetSchema(name);
            schema.Decode(this, obj);

            return obj;
        }

        public BufferPlus WriteSchema(string name, object obj, int position = 0) {
            this.Position = position;

            var schema = BufferPlus.GetSchema(name);
            schema.Encode(this, obj);

            return this;
        }
        #endregion

        #region Custom Type

        private static AssemblyBuilder _CustomTypeAssembly = null;
        public static AssemblyBuilder CustomTypeAssembly {
            get {
                if (_CustomTypeAssembly == null) {
                    _CustomTypeAssembly = AssemblyBuilder.DefineDynamicAssembly(
                        new AssemblyName("BufferPlusCustomType"), AssemblyBuilderAccess.Run);
                }
                return _CustomTypeAssembly;
            }
        }

        public static void AddCustomType(string name,
            Func<BufferPlus, object> readFunc,
            Action<BufferPlus, object> writeFunc,
            Func<BufferPlus, object, int> sizeFunc) {

            BufferType.ReadFunctions.Add(name, (bp, encoding, position, length)=> {
                bp.SanitizeParameter(ref position, ref length, ref encoding);
                var result = readFunc(bp);
                bp.Position = position;
                return result;
            });

            BufferType.WriteFunctions.Add(name, (bp, value, encoding, position, length) => {
                bp.SanitizeParameter(ref position, ref length, ref encoding);
                writeFunc(bp, value);
                return bp.ToBuffer();
            });

            BufferType.SizeFunctions.Add(name, sizeFunc);

        }
        #endregion


        public static void PrettyPrintHex(BufferPlus bp, string title) {
            Console.WriteLine("{0} {1}\n{2}\n{3}\n",
                bp.Position.ToString().PadLeft(4, '0'),
                title,
                string.Concat(Enumerable.Repeat("-", 71)),
                bp.ToHex(24, 0, 0, true));
        }

        public static void PrettyPrintHex(BufferPlus bp, string title, dynamic value) {
            Console.WriteLine("{0} {1} {2}\n{3}\n{4}\n",
                bp.Position.ToString().PadLeft(4, '0'),

                title.PadRight(16, ' '),
                JsonConvert.SerializeObject(value),

                string.Concat(Enumerable.Repeat("-", 71)),
                bp.ToHex(24, 0, 0, true));
        }
    }
}
