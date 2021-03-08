using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

/**
 * TODO BufferPlus 工作清單
 * 
 * Last updated at 2021.2.26 -- by Showhow
 *   
 * BufferPlus
 *   [v] nodejs javascript 移植
 *   [v] Reader
 *   [ ] Reader Custom Type
 *   [v] Writer
 *   [ ] Writer Custom Type
 *   [v] Schema
 *   [-] 將Read<T>與Write<T>的不同型態的if else抽離成interface
 *   
 * Unity 單元測試
 *   [-] 從 MS.NET 移植到 Unity.NET
 *   [-] Read
 *   [-] Write
 *   [-] Json Schema Read/Write
 *   [-] Custom Schema Read/Write
 *   [-] Read/Write Benchmark
 *   [-] Schema Read/Write Benchmark
 *   
 **/


namespace BufferPlus {
    public class BufferPlus {

        #region Fields & Properties

        private MemoryStream _Memory = new MemoryStream();
        private MemoryStream Memory {
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

        #region Contructor 
        public BufferPlus(int length = 4096) {
            this.Length = length;
            this.Size = this.Length;
            this.Encoding = BufferPlus.DefaultEncoding;
        }

        public BufferPlus(byte[] buffer) {
            this.Length = buffer.Length;
            this.Size = this.Length;
            this.WriteBuffer(buffer);
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
            this.WriteBuffer(buffer);
        }

        public BufferPlus(BufferPlus bp) {
            this.Encoding = bp.Encoding;
            this._Memory = bp.Memory;
        }

        #endregion

        private void SantizeParameter(ref int position, ref int length, ref bool isLittleEndian, ref Encoding encoding ,ref int count) {
            SantizeParameter(ref position);

            if (length < 0) {
                length = this.Length - position;
            }

            if (encoding == null) {
                encoding = BufferPlus.DefaultEncoding;
            }

            if (BitConverter.IsLittleEndian) {
                isLittleEndian = BitConverter.IsLittleEndian != isLittleEndian;
            }

            if (count < 1 ) {
                count = 1;
            }
        }

        private void SantizeParameter(ref int position) {
            if (position < 0) {
                position = this.Position;
            } else {
                this.Position = (int)position;
            }
        }

        private void SantizeParameterLength(int position, ref int length) {
            if (length < 0) {
                length = this.Length - position;
            }
        }

        private void SantizeParameter(ref int position, ref bool isLittleEndian, ref Encoding encoding, ref int count) {
            int l = 0;
            SantizeParameter(ref position, ref l, ref isLittleEndian, ref encoding, ref count);
        }

        private void SantizeParameter(ref int position, ref int length, ref bool isLittleEndian, ref Encoding encoding) {
            int lc = 0;
            SantizeParameter(ref position, ref length, ref isLittleEndian, ref encoding, ref lc);
        }

        private void SantizeParameter(ref int position, ref bool isLittleEndian, ref Encoding encoding) {
            int lc = 0;
            int l = 0;
            SantizeParameter(ref position, ref l, ref isLittleEndian, ref encoding, ref lc);
        }

        #region Read Generics Method 


        public T Read<T>(int position = -1, int length = -1, bool isLittleEndian = false, Encoding encoding = null, bool force = true) {
            SantizeParameter(ref position, ref length, ref isLittleEndian, ref encoding);

            var type = typeof(T);

            byte[] buffer = this.Buffer.Skip((int)position).Take((int)length).ToArray();
            
            if (type.IsPrimitive) {
                var handler = BufferType.GetHandler<T>();
                handler.Encoding = encoding;
                int type_size = handler.GetBytesLength(default(T));

                if (buffer.Length < type_size && force) {
                    this.Length = this.Position + (type_size - buffer.Length);
                    var append = new byte[type_size];
                    buffer.CopyTo(append, 0);
                    buffer = append;
                }

                var result = handler.ReadBuffer(buffer, isLittleEndian);
                this.Position += handler.GetBytesLength(result);
                return result;
            }

            //String Type
            if (type == typeof(string)) {
                dynamic value = encoding.GetString(buffer);
                this.Position += buffer.Length;
                return value;
            }

            //Buffer
            if (type == typeof(byte[])) {
                dynamic value = buffer;
                this.Position += buffer.Length;
                return value;
            }

            return default(T);
        }


        public T Read<T>() {
            return this.Read<T>(-1, -1, false, null);
        }

        public T Read<T>(bool isLittleEndian = false) {
            return this.Read<T>(-1, -1, isLittleEndian, null);
        }

        public T Read<T>(int position = -1) {
            return this.Read<T>(position, -1, false, null);
        }

        public T Read<T>(int position = -1, bool isLittleEndian = false) {
            return this.Read<T>(position, -1, isLittleEndian, null);
        }

        public T Read<T>(int position = -1, int length = -1, bool isLittleEndian = false) {
            return this.Read<T>(position, length, isLittleEndian, null);
        }

        public T Read<T>(int position = -1, Encoding encoding = null) {
            return this.Read<T>(position, -1, false, encoding);
        }

        public T Read<T>(int position = -1, int length = -1, Encoding encoding = null) {
            return this.Read<T>(position, length, false, encoding);
        }

        public T Read<T>(int position = -1, int length = -1) {
            return this.Read<T>(position, length, false, null);
        }
        #endregion

        #region Read Array Generics Method 

        public T[] ReadArray<T>(int position = -1, int count = 1, bool isLittleEndian = false, Encoding encoding = null, bool force = true ) {
            SantizeParameter(ref position, ref isLittleEndian, ref encoding, ref count);

            var type = typeof(T);

            if (type.IsValueType) {
                var handler = BufferType.GetHandler<T>();
                handler.Encoding = encoding;
                int type_size = handler.GetBytesLength(default(T));

                int length = type_size * count;

                if (length + position > this.Length) {
                    this.Length = length + (int)position;
                }

                byte[] buffer = this.Buffer.Skip((int)position).Take((int)length).ToArray();
                if (buffer.Length < type_size && force) {
                    this.Length = this.Position + (type_size - buffer.Length);
                    var append = new byte[type_size];
                    buffer.CopyTo(append, 0);
                    buffer = append;
                }

                this.Position += buffer.Length;

                dynamic values = new T[count];
                for (int i = 0; i < values.Length; i++) {
                    values[i] = handler.ReadBuffer(buffer, isLittleEndian, i * type_size, type_size);
                }
                return values;
            }

            //String Type
            if (type == typeof(string)) {
                dynamic value = new string[count];
                byte[] buffer = this.Buffer.Skip((int)position).ToArray();
                int length = 0;
                for (int i = 0; i < value.Length; i++) {
                    var len = VarintBitConverter.ToUInt32(buffer);
                    int bytes_count = VarintBitConverter.GetBytesLength(len);
                    var bytes_read = buffer.Skip(bytes_count).Take((int)len).ToArray();
                    value[i] = encoding.GetString(bytes_read);

                    buffer = buffer.Skip(bytes_read.Length + bytes_count).ToArray();
                    length += bytes_read.Length + bytes_count;
                }
                this.Position += length;
                return value;
            }

            return null;
        }

        public T[] ReadArray<T>(int count) {
            return this.ReadArray<T>(-1, count, false, null);
        }

        public T[] ReadArray<T>(int count,bool isLittleEndian = false) {
            return this.ReadArray<T>(-1, count, isLittleEndian, null);
        }

        public T[] ReadArray<T>(int count, int position = -1) {
            return this.ReadArray<T>(position, count, false, null);
        }

        public T[] ReadArray<T>(int count, int position = -1, bool isLittleEndian = false) {
            return this.ReadArray<T>(position, count, isLittleEndian, null);
        }

        public T[] ReadArray<T>(int count, int position = -1, Encoding encoding = null) {
            return this.ReadArray<T>(position, 1, false, encoding);
        }

        public T[] ReadArray<T>(int count ,Encoding encoding = null) {
            return this.ReadArray<T>(-1, 1, false, encoding);
        }

        public T[] ReadPackedArray<T>(int position = -1, bool isLittleEndian = false, Encoding encoding = null, bool force = true) {
            this.SantizeParameter(ref position, ref isLittleEndian, ref encoding);

            var head = this.Buffer.Skip((int)position).ToArray();

            int len = (int)VarintBitConverter.ToUInt32(head);
            int count = VarintBitConverter.GetBytesLength(len);

            return this.ReadArray<T>(position+ count, len, isLittleEndian, encoding, force);
        }

        public T[] ReadPackedArray<T>(bool isLittleEndian, Encoding encoding = null, bool force = true) {
            return this.ReadPackedArray<T>(-1, isLittleEndian, encoding, force);
        }

        public T[] ReadPackedArray<T>(Encoding encoding, bool force = true) {
            return this.ReadPackedArray<T>(-1, false, encoding, force);
        }

        #endregion

        #region Read Buffer & String Array Methods
        public uint ReadVarUInt(int position = -1) {
            this.SantizeParameter(ref position);

            var bytes = this.Buffer.Skip(this.Position).ToArray();
            uint value = VarintBitConverter.ToUInt32(bytes);
            int count = VarintBitConverter.GetBytesLength(value);
            this.Position += count;
            return value;
        }

        public int ReadVarInt(int position = -1) {
            SantizeParameter(ref position);

            var bytes = this.Buffer.Skip(this.Position).ToArray();
            int value = VarintBitConverter.ToInt32(bytes);
            int count = VarintBitConverter.GetVarintBytes(value).Length;
            this.Position += count;

            return value;
        }

        public uint[] ReadVarUIntArray(bool packed = false, int count = -1, int position = -1) {
            SantizeParameter(ref position);

            if (packed) {
                count = (int)this.ReadVarUInt();
            }

            if (count < 0) {
                count = 1;
            }

            uint[] results = new uint[(int)count];

            for(int i=0;i< count; i++) {
                results[i] = this.ReadVarUInt();
            }

            return results;
        }

        public int[] ReadVarIntArray(bool packed = true, int count = 1, int position = -1) {
            SantizeParameter(ref position);

            if (packed) {
                count = (int)this.ReadVarUInt();
            }

            if (count < 0) {
                count = 1;
            }

            int[] results = new int[(int)count];

            for (int i = 0; i < count; i++) {
                results[i] = this.ReadVarInt();
            }

            return results;
        }

        public byte[] ReadBuffer(int count, int position = -1) {
            var result = this.ReadArray<byte>(position, count);
            return result;
        }

        public string ReadString(int length = -1, Encoding encoding = null) {
            var str = this.Read<string>(this.Position, length, encoding);
            return str;
        }

        public string ReadString(Encoding encoding) {
            return this.ReadString(-1, encoding);
        }

        public string ReadString() {
            return this.ReadString(-1, null);
        }

        public string ReadPackedString(Encoding encoding = null,int position = -1) {
            int len = (int)this.ReadVarUInt();
            return this.Read<string>(position, len, encoding);
        }

        public byte[] ReadPackedBuffer(int position = -1) {
            var len = (int)this.ReadVarUInt();
            return this.ReadArray<byte>(position, len);
        }

        #endregion

        #region Read Value Type Methods
        public bool ReadBoolean(int position = -1) {
            var result = this.Read<bool>(position);
            return result;
        }

        public byte ReadByte(int position = -1) {
            var result = this.Read<byte>(position);
            return result;
        }

        //8 bit integer
        public sbyte ReadInt8(int position = -1) {
            var result = this.Read<sbyte>(position);
            return result;
        }

        // 16 bits Integer
        public short ReadShort(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<short>(position, isLittleEndian);
            return result;
        }

        public Int16 ReadInt16BE(int position = -1) {
            return this.ReadShort(position, false);
        }

        public Int16 ReadInt16LE(int position = -1) {
            return this.ReadShort(position, true);
        }

        // 32 bits Integer
        public int ReadInt(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<int>(position, isLittleEndian);
            return result;
        }

        public Int32 ReadInt32BE(int position = -1) {
            return this.ReadInt(position, false);
        }

        public Int32 ReadInt32LE(int position = -1) {
            return this.ReadInt(position, true);
        }

        // 64 bits Integer
        public long ReadLong(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<long>(position, isLittleEndian);
            return result;
        }

        public Int64 ReadInt64BE(int position = -1) {
            return this.ReadLong(position, false);
        }

        public Int64 ReadInt64LE(int position = -1) {
            return this.ReadLong(position, true);
        }

        // 16 bits Unsigned Integer
        public ushort ReadUShort(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<ushort>(position, isLittleEndian);
            return result;
        }

        public UInt16 ReadUInt16BE(int position = -1) {
            return this.ReadUShort(position, false);
        }

        public UInt16 ReadUInt16LE(int position = -1) {
            return this.ReadUShort(position, true);
        }

        // 32 bits Unsigned Integer
        public uint ReadUInt32(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<uint>(position, isLittleEndian);
            return result;
        }

        public UInt32 ReadUInt32BE(int position = -1) {
            return this.ReadUInt32(position, false);
        }

        public UInt32 ReadUInt32LE(int position = -1) {
            return this.ReadUInt32(position, true);
        }

        // 64 bits Unsigned Integer
        public ulong ReadULong(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<ulong>(position, isLittleEndian);
            return result;
        }

        public UInt64 ReadUInt64BE(int position = -1) {
            return this.ReadULong(position, false);
        }

        public UInt64 ReadUInt64LE(int position = -1) {
            return this.ReadULong(position, true);
        }

        //32bit Float
        public float ReadFloat(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<float>(position, isLittleEndian);
            return result;
        }

        public float ReadFloatBE(int position = -1) {
            return this.ReadFloat(position, false);
        }

        public float ReadFloatLE(int position = -1) {
            return this.ReadFloat(position, true);
        }

        //64bit Float
        public double ReadDouble(int position = -1, bool isLittleEndian = false) {
            var result = this.Read<double>(position, isLittleEndian);
            return result;
        }

        public double ReadDoubleBE(int position = -1) {
            return this.ReadDouble(position, false);
        }

        public double ReadDoubleLE(int position = -1) {
            return this.ReadDouble(position, true);
        }
        #endregion

        

        #region Write Generic Methods

        public int Write<T>(T data, int position = -1, bool isLittleEndian = false, Encoding encoding = null, bool packed = false) {
            SantizeParameter(ref position, ref isLittleEndian, ref encoding);

            var type = typeof(T);

            byte[] bytes = null;

            //Value Type
            if (type.IsValueType) {
                var handler = BufferType.GetHandler<T>();
                handler.Encoding = encoding;
                bytes = handler.WriteBuffer(data, isLittleEndian);
            }

            //String Type
            if (type == typeof(string)) {
                bytes = encoding.GetBytes(data as string);
                if (packed) {
                    var head = VarintBitConverter.GetVarintBytes((uint)bytes.Length);
                    bytes = head.Concat(bytes).ToArray();
                }
            }

            //buffer Type
            if (type == typeof(byte[])) {
                bytes = data as byte[];
                if (packed) {
                    var head = VarintBitConverter.GetVarintBytes((uint)bytes.Length);
                    bytes = head.Concat(bytes).ToArray();
                }
            }

            int length = bytes.Length;
            if (length + position > this.Length) {
                this.Length = (length + (int)position);
            }

            this.Memory.Write(bytes, 0, length);
            return length;
        }

        public int Write<T>(T data) {
            return this.Write<T>(data, -1, false, null);
        }

        public int Write<T>(T data, bool isLittleEndian = false, bool packed = false) {
            return this.Write<T>(data, -1, isLittleEndian, null, packed);
        }


        public int Write<T>(T data, int position = -1) {
            return this.Write<T>(data, position, false, null);
        }

        public int Write<T>(T data, int position = -1, bool isLittleEndian = false) {
            return this.Write<T>(data, position, isLittleEndian, null);
        }

        public int Write<T>(T data, int position = -1, int length = -1, bool isLittleEndian = false) {
            return this.Write<T>(data, position, isLittleEndian, null);
        }

        public int Write<T>(T data, int position = -1, Encoding encoding = null) {
            return this.Write<T>(data, position, false, encoding);
        }

        public int Write<T>(T data, int position = -1, int length = -1) {
            return this.Write<T>(data, position, false, null);
        }

        public int Write(byte data) {
            this.Memory.WriteByte(data);
            return 1;
        }

        #endregion

        #region Write Array Generis Methods
        public int WriteArray<T>(T[] items, int position = -1, bool isLittleEndian = false, bool packed = false, Encoding encoding = null, bool force = true) {
            SantizeParameter(ref position, ref isLittleEndian, ref encoding);

            var type = typeof(T);

            List<byte[]> bytes_array = new List<byte[]>(items.Length);
            if (type.IsValueType) {
                var handler = BufferType.GetHandler<T>();
                handler.Encoding = encoding;
                for (int i = 0; i < items.Length; i++) {
                    bytes_array.Add(handler.WriteBuffer(items[i], isLittleEndian));
                }
            }

            //String Type
            if (type == typeof(string)) {
                for (int i = 0; i < items.Length; i++) {
                    bytes_array.Add(encoding.GetBytes(items[i] as string));

                    var head = VarintBitConverter.GetVarintBytes((uint)bytes_array[i].Length);
                    bytes_array[i] = head.Concat(bytes_array[i]).ToArray();
                }
            }

            if (type == typeof(byte[])) {
                for (int i = 0; i < items.Length; i++) {
                    bytes_array.Add(items[i] as byte[]);

                    var head = VarintBitConverter.GetVarintBytes((uint)bytes_array[i].Length);
                    bytes_array[i] = head.Concat(bytes_array[i]).ToArray();
                }
            }


            byte[] bytes = bytes_array.SelectMany(b => b).ToArray();

            int length = bytes.Length;
            if (position + length > this.Length) {
                this.Length = ((int)position + length);
            }

            if (packed) {
                var head = VarintBitConverter.GetVarintBytes((uint)bytes_array.Count);
                this.Memory.Write(head, 0, head.Length);
            }
            this.Memory.Write(bytes, 0, bytes.Length);
            return length;
        }

        public int WriteArray<T>(T[] items) {
            return this.WriteArray<T>(items, -1, false, false, null);
        }

        public int WriteArray<T>(T[] items, bool isLittleEndian = false) {
            return this.WriteArray<T>(items, -1, isLittleEndian, false, null);
        }

        public int WriteArray<T>(T[] items, bool isLittleEndian = false, bool packed = false) {
            return this.WriteArray<T>(items, -1, isLittleEndian, packed, null);
        }

        public int WriteArray<T>(T[] items, int position = -1) {
            return this.WriteArray<T>(items, position, false, false, null);
        }

        public int WriteArray<T>(T[] items, int position = -1, bool isLittleEndian = false) {
            return this.WriteArray<T>(items, position, isLittleEndian, false, null);
        }

        public int WriteArray<T>(T[] items, int position = -1, bool isLittleEndian = false, bool packed = false) {
            return this.WriteArray<T>(items, position, isLittleEndian, packed, null);
        }

        public int WriteArray<T>(T[] items, int position = -1, Encoding encoding = null) {
            return this.WriteArray<T>(items, position, false, false, encoding);
        }

        public int WriteArray<T>(T[] items, int position = -1, Encoding encoding = null, bool packed = false) {
            return this.WriteArray<T>(items, position, false, packed, encoding);
        }

        public int WritePackedArray<T>(T[] items, int position = -1, bool isLittleEndian = false, Encoding encoding = null, bool force = true) {
            return this.WriteArray<T>(items, position, isLittleEndian, true, encoding, force);
        }

        #endregion

        #region Write Buffer & String Array Methods

        public BufferPlus WriteVarInt(int value, int position = -1) {
            var bytes = VarintBitConverter.GetVarintBytes(value);
            this.Write(bytes, position);
            return this;
        }

        public BufferPlus WriteVarUInt(uint value, int position = -1) {
            var bytes = VarintBitConverter.GetVarintBytes(value);
            this.Write(bytes, position);
            return this;
        }

        public BufferPlus WriteVarUIntArray(uint[] items, bool packed = false, int position = -1) {
            var bytes_array = new List<byte[]>();

            foreach(uint value in items) {
                bytes_array.Add(VarintBitConverter.GetVarintBytes(value));
            }

            var bytes = bytes_array.SelectMany(b => b).ToArray();
            if(packed) {
                var head = VarintBitConverter.GetVarintBytes((uint)items.Length);
                var len_byte_count = head.Length;
                var bytes_write = head.Concat(bytes).ToArray();
                this.Write(bytes_write, position);
            } else {
                this.Write(bytes, position);
            }
            return this;
        }

        public BufferPlus WriteVarIntArray(int[] items, bool packed = false, int position = -1) {
            var bytes_array = new List<byte[]>();

            foreach (int value in items) {
                bytes_array.Add(VarintBitConverter.GetVarintBytes(value));
            }

            var bytes = bytes_array.SelectMany(b => b).ToArray();
            if (packed) {
                var head = VarintBitConverter.GetVarintBytes((uint)items.Length);
                var len_byte_count = head.Length;
                var bytes_write = head.Concat(bytes).ToArray();
                this.Write(bytes_write, position);
            } else {
                this.Write(bytes, position);
            }
            return this;
        }

        public BufferPlus WriteBuffer(byte[] buffer, int position = -1, int offset = 0) {
            this.Write(buffer, position);
            return this;
        }

        public BufferPlus WriteString(string value, Encoding encoding = null, int position = -1) {
            this.Write(value, position, encoding);
            return this;
        }

        public BufferPlus WritePackedString(string value, Encoding encoding = null, int position = -1) {
            this.Write(value, position,false, encoding, true);
            return this;
        }

        public BufferPlus WritePackedBuffer(byte[] value, int position = -1) {
            this.Write(value, position, false, null, true);
            return this;
        }

        #endregion

        #region Write Value Type Methods

        public BufferPlus WriteBoolean(bool value, int position = -1) {
            int count = this.Write<bool>(value, position);
            return this;
        }

        //8 bit integer
        public BufferPlus WriteByte(byte value, int position = -1) {
            SantizeParameter(ref position);

            int count = this.Write<byte>(value, position);

            this.Position = (int)position;
            this.Position += count;
            return this;
        }

        public BufferPlus WriteInt8(sbyte value, int position = -1) {
            SantizeParameter(ref position);

            int count = this.Write<sbyte>(value, position);

            this.Position = (int)position;
            this.Position += count;
            return this;
        }

        // 16 bits Integer
        public BufferPlus WriteShort(short value, int position = -1, bool isLittleEndian = false) {
            this.Write<short>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteInt16BE(short value, int position = -1) {
            return this.WriteShort(value, position, false);
        }

        public BufferPlus WriteInt16LE(short value, int position = -1) {
            return this.WriteShort(value, position, true);
        }

        // 32 bits Integer
        public BufferPlus WriteInt(int value, int position = -1, bool isLittleEndian = false) {
            this.Write<int>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteInt32BE(int value, int position = -1) {
            return this.WriteInt(value, position, false);
        }

        public BufferPlus WriteInt32LE(int value, int position = -1) {
            return this.WriteInt(value, position, true);
        }

        // 64 bits Integer
        public BufferPlus WriteLong(long value, int position = -1, bool isLittleEndian = false) {
            this.Write<long>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteInt64BE(long value, int position = -1) {
            return this.WriteLong(value, position, false);
        }

        public BufferPlus WriteInt64LE(long value, int position = -1) {
            return this.WriteLong(value, position, true);
        }

        // 16 bits Unsigned Integer
        public BufferPlus WriteUShort(ushort value, int position = -1, bool isLittleEndian = false) {
            this.Write<ushort>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteUInt16BE(ushort value, int position = -1) {
            return this.WriteUShort(value, position, false);
        }

        public BufferPlus WriteUInt16LE(ushort value, int position = -1) {
            return this.WriteUShort(value, position, true);
        }

        // 32 bits Unsigned Integer
        public BufferPlus WriteUInt32(uint value, int position = -1, bool isLittleEndian = false) {
            this.Write<uint>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteUInt32BE(uint value, int position = -1) {
            return this.WriteUInt32(value, position, false);
        }

        public BufferPlus WritedUInt32LE(uint value, int position = -1) {
            return this.WriteUInt32(value, position, true);
        }

        // 64 bits Unsigned Integer
        public BufferPlus WriteULong(ulong value, int position = -1, bool isLittleEndian = false) {
            this.Write<ulong>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteUInt64BE(ulong value, int position = -1) {
            return this.WriteULong(value, position, false);
        }

        public BufferPlus WriteUInt64LE(ulong value, int position = -1) {
            return this.WriteULong(value, position, true);
        }

        //32bit Float
        public BufferPlus WriteFloat(float value, int position = -1, bool isLittleEndian = false) {
            this.Write<float>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteFloatBE(float value, int position = -1) {
            return this.WriteFloat(value, position, false);
        }

        public BufferPlus WriteFloatLE(float value, int position = -1) {
            return this.WriteFloat(value, position, true);
        }

        //64bit Float
        public BufferPlus WriteDouble(double value, int position = -1, bool isLittleEndian = false) {
            this.Write<double>(value, position, isLittleEndian);
            return this;
        }

        public BufferPlus WriteDoubleBE(double value, int position = -1) {
            return this.WriteDouble(value, position, false);
        }

        public BufferPlus WriteDoubleLE(double value, int position = -1) {
            return this.WriteDouble(value, position, true);
        }

        #endregion

        #region Convert Methods
        public byte[] ToBuffer() {
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
                array[i] = BitConverter.ToInt16(buffer, i * packs);
            }
            return array;
        }

        public UInt16[] ToUInt16Array() {
            int packs = 2;
            var buffer = this.ToByteArray(packs);

            var array = new UInt16[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = BitConverter.ToUInt16(buffer, i * packs);
            }
            return array;
        }

        public Int32[] ToInt32Array() {
            int packs = 4;
            var buffer = this.ToByteArray(packs);

            var array = new Int32[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = BitConverter.ToInt32(buffer, i * packs);
            }
            return array;
        }

        public UInt32[] ToUInt32Array() {
            int packs = 4;
            var buffer = this.ToByteArray(packs);

            var array = new UInt32[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = BitConverter.ToUInt32(buffer, i * packs);
            }
            return array;
        }

        public Int64[] ToInt64Array() {
            int packs = 8;
            var buffer = this.ToByteArray(packs);

            var array = new Int64[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = BitConverter.ToInt64(buffer, i * packs);
            }
            return array;
        }

        public UInt64[] ToUInt64Array() {
            int packs = 8;
            var buffer = this.ToByteArray(packs);

            var array = new UInt64[(int)(buffer.Length / packs)];

            for (int i = 0; i * packs < buffer.Length; i++) {
                array[i] = BitConverter.ToUInt64(buffer, i * packs);
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

        public string ToHex() {
            var buffer = this.ToByteArray();
            return BitConverter.ToString(buffer);
        }

        #endregion

        #region Flag & Positiong Methods
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

        public static bool Compare(byte[] buff,BufferPlus bp) {
            return BufferPlus.Compare(bp.Buffer, buff);
        }

        public static BufferPlus Concat(IEnumerable<byte[]> buffers, int length) {
            var bp = new BufferPlus();
            foreach (byte[] buffer in buffers) {
                bp.Write(buffer,0, buffer.Length);
            }

            bp.Length = bp.Position;
            return bp;
        }

        public static BufferPlus Concat(IEnumerable<BufferPlus> bps, int length) {
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
            return VarintBitConverter.GetBytesLength<T>(value, encoding);
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

        #region Static Schemas Methods
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


        #endregion
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
    }
}
