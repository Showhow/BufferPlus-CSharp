using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus {

    public abstract class BufferType {

        public static void Init() {
            _Types = new Dictionary<string, BufferType>();
            _ReadFunctions = new Dictionary<string, Func<BufferPlus, Encoding, int, int, dynamic>>();
            _WriteFunctions = new Dictionary<string, Func<BufferPlus, dynamic, Encoding, int, int, byte[]>>();
            _SizeFunctions = new Dictionary<string, Func<BufferPlus, dynamic, int>>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (System.Reflection.TypeInfo type in assembly.DefinedTypes) {
                    if (type.BaseType == null || type.BaseType.IsGenericType == false) {
                        continue;
                    }

                    if (type.BaseType.Name == typeof(BufferType<>).Name) {
                        dynamic bType = Activator.CreateInstance(type);
                    }
                }
            }

            _TypesLookup = (Lookup<Type, BufferType>)_Types.ToLookup(pair=> pair.Value.DataType, pair => pair.Value);
        }

        private static Dictionary<string, BufferType> _Types = null;
        public static Dictionary<string, BufferType> Types {
            get {
                if (_Types == null) {
                    Init();
                }

                return _Types;
            }
        }

        private static Lookup<Type, BufferType> _TypesLookup = null;
        public static Lookup<Type, BufferType> TypesLookups {
            get {
                if (_TypesLookup == null) {
                    Init();
                }
                return _TypesLookup;
            }
        }

        private static Dictionary<string, Func<BufferPlus, Encoding, int, int, dynamic>> _ReadFunctions= null;
        public static Dictionary<string, Func<BufferPlus, Encoding, int, int, dynamic>> ReadFunctions {
            get {
                if(_ReadFunctions == null) {
                    Init();
                }
                return _ReadFunctions;
            }
        }

        private static Dictionary<string, Func<BufferPlus, dynamic, Encoding, int, int, byte[]>> _WriteFunctions= null;
        public static Dictionary<string, Func<BufferPlus, dynamic, Encoding, int, int, byte[]>> WriteFunctions {
            get {
                if (_WriteFunctions == null) {
                    Init();
                }
                return _WriteFunctions;
            }
        }

        private static Dictionary<string, Func<BufferPlus, dynamic, int>> _SizeFunctions = null;
        public static Dictionary<string, Func<BufferPlus, dynamic,int>> SizeFunctions {
            get {
                if (_SizeFunctions == null) {
                    Init();
                }
                return _SizeFunctions;
            }
        }

        public static BufferType GetBufferType(string typeString) {
            return Types[typeString];
        }

        public static Type GetDataType(string typeString) {
            if (Types.ContainsKey(typeString)) {
                return Types[typeString].DataType;
            }
            return typeof(object);
        }

        private Encoding _Encoding = Encoding.UTF8;
        public Encoding Encoding {
            get => _Encoding;
            protected set => _Encoding = value;
        }

        /// <summary>
        /// 編碼時之位元組端序是否為小端序，高位優先
        /// </summary>
        public abstract bool IsLittleEndian {
            get;
        }

        /// <summary>
        /// 緩衝型別的唯一識別字串
        /// </summary>
        public abstract string TypeString {
            get;
        }

        public abstract Type DataType {
            get;
        }
        public static bool HasType(string typeString) {
            return Types.ContainsKey(typeString);
        }
    }
    public abstract class BufferType<T> : BufferType {
        override public Type DataType {
            get => typeof(T);
        }

        public BufferType() {
            if (BufferType.HasType(this.TypeString)) {
                return;
            }

            BufferType.Types.Add(this.TypeString, this);

            BufferType.ReadFunctions.Add(this.TypeString, (bp, encoding, pos, len) => {
                return this.Decode(bp, encoding, pos, len);
            });

            BufferType.WriteFunctions.Add(this.TypeString, (bp, value, encoding, pos, len) => {
                return this.Encode(bp, (T)value, encoding, pos, len);
            });

            BufferType.SizeFunctions.Add(this.TypeString, (bp, value) => {
                return this.Size(bp, (T)value);
            });
        }

        /// <summary>
        /// 使用 BufferPlus 進行解碼讀取
        /// </summary>
        /// <param name="bp">需要操作之 BufferPlus 物件實體</param>
        /// <param name="encoding">指定緩衝區讀取之字元編碼</param>
        /// <param name="pos">指定讀取緩衝區的位置，若為負值則不指定</param>
        /// <param name="len">指定讀取緩衝區的長度，若為負值則不指定</param>
        /// <returns></returns>
        abstract public T Decode(BufferPlus bp, Encoding encoding = null, int pos = -1, int len = -1);

        /// <summary>
        /// 使用 BufferPlus 預設之字元編碼進行解碼讀取
        /// </summary>
        /// <param name="buffer">>需要操作之 BufferPlus 物件實體</param>
        /// <param name="pos">指定讀取緩衝區的位置，若為負值則不指定</param>
        /// <param name="len">指定讀取緩衝區的長度，若為負值則不指定</param>
        /// <returns></returns>
        public T Decode(BufferPlus buffer, int pos = -1, int len = -1) {
            return Decode(buffer, null, pos, len);
        }

        public T Decode(BufferPlus buffer) {
            return Decode(buffer, null, -1, -1);
        }

        abstract public byte[] Encode(BufferPlus bp, T value, Encoding encoding = null, int pos = -1, int len = -1);

        public byte[] Encode(BufferPlus bp, T value, int pos = -1, int len = -1) {
            return Encode(bp, value, this.Encoding, pos, len);
        }

        public byte[] Encode(BufferPlus bp, dynamic value) {
            return Encode(bp, (T)value, this.Encoding, -1, -1);
        }

        abstract public int Size(BufferPlus bp, T value);

        public int Size(T value) {
            return Size(null, value);
        }
    }


    public abstract class TypeString {
        public const string Bool = "bool";
        public const string Boolean = "bool";

        public const string Sbyte = "int8";
        public const string Int8 = "int8";

        public const string Int16BE = "int16be";
        public const string Int16LE = "int16le";
        public const string Int32BE = "int32be";
        public const string Int32LE = "int32le";
        public const string Int64BE = "int64be";
        public const string Int64LE = "int64le";

        public const string Byte = "uint8";
        public const string UInt8 = "uint8";
        public const string UInt16BE = "uint16be";
        public const string UInt16LE = "uint16le";
        public const string UInt32BE = "uint32be";
        public const string UInt32LE = "uint32le";
        public const string UInt64BE = "uint64be";
        public const string UInt64LE = "uint64le";

        public const string FloatBE = "float32be";
        public const string FloatLE = "float32le";
        public const string Float32BE = "float32be";
        public const string Float32LE = "float32le";
        public const string DoubleBE = "float64be";
        public const string DoubleLE = "float64le";
        public const string Float64BE = "float64be";
        public const string Float64LE = "float64le";

        public const string VarInt = "varint";
        public const string VarUInt = "varuint";

        public const string String = "string";
        public const string Buffer = "buffer";

        public const string Object = "object";
        public const string Array = "array";
    }
}
