using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/**
 * TODO BufferSchema 工作清單
 * 
 * Last updated at 2021.2.26 -- by Showhow
 * 
 * BufferSchema
 *   [v] nodejs javascript 移植
 *   [v] Json Schema 解析
 *   [v] Decode
 *   [v] Encode
 *   [ ] Custom Type 自訂型態解析
 *   [ ] Decode Custom Type
 *   [ ] Encode Custom Type
 *   
 **/
namespace BufferPlus {
    public class BufferSchema {


        #region Fields & Properties
        public string Name {
            get;
            internal set;
        }

        public Definition Defination {
            get;
            private set;
        }

        public JObject JsonSchema {
            get;
            private set;
        }

        private Encoding _Encoding = Encoding.UTF8;
        public Encoding Encoding {
            get => this._Encoding;
            set => this._Encoding = value;
        }


        #endregion

        internal BufferSchema() {
        }

        internal BufferSchema(string name) {
            this.Name = name;
        }

        internal BufferSchema(string name, string json) {
            this.Name = name;
            this.JsonSchema = JObject.Parse(json);
            this.Defination = ParseDefinition(this.JsonSchema);
        }

        internal BufferSchema(string name, JObject jsonDef) {
            this.Name = name;
            this.JsonSchema = jsonDef;
            this.Defination = ParseDefinition(this.JsonSchema);
        }

        #region Member Methods

        public void SetJsonSchema(JObject jsonDef) {
            this.JsonSchema = jsonDef;
            this.Defination = ParseDefinition(this.JsonSchema);
        }
        public void SetJsonSchema(string json) {
            var json_def = JObject.Parse(json);
            SetJsonSchema(json_def);
        }
        public BufferSchema AddField(string key, string type) {
            return this;
        }

        public BufferSchema AddArrayField(string key, string type) {
            return this;
        }

        public List<string> TypeOrder = new List<string>();

        public List<Action<BufferPlus, object>> WriteFunctions = new List<Action<BufferPlus, object>>();

        public List<Action<BufferPlus, object>> ReadFunctions = new List<Action<BufferPlus, object>>();

        public void Build() {
            this.BuildWriteAction(this.Defination);
            this.BuildReadAction(this.Defination);
        }

        public Action<BufferPlus, dynamic> BuildWriteAction(Definition definition) {
            Action<BufferPlus, dynamic> action = null;

            if (definition.Type == TypeString.Array) {
                var property = definition.Items;
                if (property.Type == TypeString.String) {
                    action = (bp, data) => bp.WritePackedArray(property.GetValue(data), -1 ,false,this.Encoding);
                }

                if (property.Type == TypeString.Buffer) {
                    action = (bp, data) => bp.WritePackedArray(property.GetValue(data));
                }

                if (property.Type == TypeString.VarInt) {
                    action = (bp, data) => bp.WriteVarIntArray(property.GetValue(data),true);
                }

                if (property.Type == TypeString.VarUInt) {
                    action = (bp, data) => bp.WriteVarUIntArray(property.GetValue(data), true);
                }

                if (property.Type == TypeString.Array) {
                    action = BuildWriteAction(property.Items);
                }

                if (property.Type == TypeString.Object) {
                    action = BuildWriteAction(property);
                }

                if (action == null) {
                    action = (bp, data) => {
                        var handler = BufferType.GetHandler(property.Type);
                        dynamic value = property.GetValue(data);
                        bp.WriteArray(value, handler.GetByteOrder(property.Type));
                    };
                }

                if (action != null) {
                    this.WriteFunctions.Add(action);
                }
            }else if (definition.Type == TypeString.Object) {
                foreach (var property in definition.Properties) {
                    action = BuildWriteAction(property);
                }
            } else {
                var property = definition;

                if (property.Type == TypeString.String) {
                    action = (bp, data) => bp.WritePackedString(property.GetValue(data) as string, this.Encoding);
                }

                if (property.Type == TypeString.Buffer) {
                    action = (bp, data) => bp.WritePackedBuffer(property.GetValue(data) as byte[]);
                }

                if (property.Type == TypeString.VarInt) {
                    action = (bp, data) => bp.WriteVarInt((int)property.GetValue(data));
                }

                if (property.Type == TypeString.VarUInt) {
                    action = (bp, data) => bp.WriteVarUInt((uint)property.GetValue(data));
                }

                if (property.Type == TypeString.Array) {
                    action = BuildWriteAction(property.Items);
                }

                if (property.Type == TypeString.Object) {
                    action = BuildWriteAction(property);
                }

                if (action == null) {
                    action = (bp, data) => {
                        var handler = BufferType.GetHandler(property.Type);
                        dynamic value = property.GetValue(data);
                        bp.Write(value, handler.GetByteOrder(property.Type));
                    };
                }

                if (action != null) {
                    this.WriteFunctions.Add(action);
                }
            }
            return action;
        }

        public Action<BufferPlus, object> BuildReadAction(Definition definition) {
            Action<BufferPlus, object> action = null;

            if (definition.Type == TypeString.Array) {
                var property = definition.Items;
                if (property.Type == TypeString.String) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadPackedArray<string>(this.Encoding));
                }

                if (property.Type == TypeString.Buffer) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadPackedArray<byte[]>());
                }

                if (property.Type == TypeString.VarInt) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadVarIntArray(true));
                }

                if (property.Type == TypeString.VarUInt) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadVarUIntArray(true));
                }

                if (property.Type == TypeString.Array) {
                    action = BuildReadAction(property.Items);
                }

                if (property.Type == TypeString.Object) {
                    action = BuildReadAction(property);
                }

                if (action == null) {
                    action = (bp, obj) => {
                        var handler = BufferType.GetHandler(property.Type);
                        Type type = handler.GetBufferType();
                        MethodInfo method = bp.GetType().GetMethod(nameof(bp.ReadPackedArray)).MakeGenericMethod(type);
                        dynamic value = method.Invoke(bp,new object[] { handler.GetByteOrder(property.Type) });
                        property.SetValue(obj, value);
                    };
                }

                if (action != null) {
                    this.ReadFunctions.Add(action);
                }
            } else if (definition.Type == TypeString.Object) {
                foreach (var property in definition.Properties) {
                    action = BuildReadAction(property);
                }
            } else {
                var property = definition;

                if (property.Type == TypeString.String) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadPackedString(this.Encoding));
                }

                if (property.Type == TypeString.Buffer) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadPackedBuffer());
                }

                if (property.Type == TypeString.VarInt) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadVarInt());
                }

                if (property.Type == TypeString.VarUInt) {
                    action = (bp, obj) => property.SetValue(obj, bp.ReadVarUInt());
                }

                if (property.Type == TypeString.Array) {
                    action = BuildReadAction(property.Items);
                }

                if (property.Type == TypeString.Object) {
                    action = BuildReadAction(property);
                }

                if(action == null) {

                    action = (bp, obj) => {
                        var handler = BufferType.GetHandler(property.Type);
                        Type type = handler.GetBufferType();
                        MethodInfo method = bp.GetType().GetMethod(nameof(bp.Read),new Type[] { typeof(bool) }).MakeGenericMethod(type);
                        dynamic value = method.Invoke(bp, new object[] { handler.GetByteOrder(property.Type) });
                        property.SetValue(obj, value);
                    };
                }

                if (action != null) {
                    this.ReadFunctions.Add(action);
                }
            }
            return action;
        }
        public int ByteLength(object obj) {
            return 0;
        }

        public T Decode<T>(BufferPlus bp, T obj) {
            foreach (var func in ReadFunctions) {
                func.Invoke(bp, obj);
            }
            return obj;
        }

        public void Encode(BufferPlus bp, object obj) {
            foreach (var func in WriteFunctions) {
                func.Invoke(bp, obj);
            }
        }

        public void SetEncoding(Encoding encoding) {
            this.Encoding = encoding;
            return;
        }

        #endregion

        #region Static Methods
        public static Definition ParseDefinition(JToken json,string name = null) {
            var def = new Definition();
            string type = json["type"].ToString();
            def.Name = name;
            def.Type = type;

            if (def.Type == TypeString.Object) {
                var json_properties = json["properties"];
                var json_order = json["order"].ToArray();
                int order = 0;
                foreach (string key in json_order) {
                    var property = ParseDefinition(json_properties[key], key);
                    property.Name = key;
                    def.Properties.Add(property);
                    def.Orders.Add(property.Name);

                    order++;
                }
            }

            if (def.Type == TypeString.Array) {
                var items_properties = json["items"];
                def.Items = ParseDefinition(items_properties, name);
            }

            return def;
        }


        public static int GetTypeLength<T>(T value, Encoding encoding = null) {
            var type = typeof(T);

            if (value is bool || value is sbyte || value is byte) {
                return TypeLength.Bool;
            }

            if (value is short || value is ushort) {
                return 2;
            }

            if (value is int || value is uint) {
                return 4;
            }

            if (value is long || value is ulong) {
                return 8;
            }

            if (value is float) {
                return 4;
            }

            if (value is double) {
                return 8;
            }

            if (value is string) {
                if (encoding == null) {
                    encoding = Encoding.UTF8;
                }
                return encoding.GetByteCount(Convert.ToString(value));
            }

            if (value is byte[]) {
                return (value as byte[]).Length;
            }

            throw new TypeLoadException();
        }

        public static string GetTypeString<T>(T value, bool isLittleEndian = false) {
            var type = typeof(T);

            if (BitConverter.IsLittleEndian) {
                isLittleEndian = BitConverter.IsLittleEndian != isLittleEndian;
            }

            if (value is bool) {
                return TypeString.Bool;
            }

            //Singed
            if (value is sbyte) {
                return TypeString.Int8;
            }

            if (value is short && isLittleEndian == false) {
                return TypeString.Int16BE;
            }

            if (value is short && isLittleEndian) {
                return TypeString.Int16LE;
            }

            if (value is int && isLittleEndian == false) {
                return TypeString.Int32BE;
            }

            if (value is int && isLittleEndian) {
                return TypeString.Int32LE;
            }

            if (value is long && isLittleEndian == false) {
                return TypeString.Int64BE;
            }

            if (value is long && isLittleEndian) {
                return TypeString.Int64LE;
            }

            //Unsinged
            if (value is byte) {
                return TypeString.UInt8;
            }

            if (value is ushort && isLittleEndian == false) {
                return TypeString.UInt16BE;
            }

            if (value is ushort && isLittleEndian) {
                return TypeString.UInt16LE;
            }

            if (value is uint && isLittleEndian == false) {
                return TypeString.UInt32BE;
            }

            if (value is uint && isLittleEndian) {
                return TypeString.UInt32LE;
            }

            if (value is ulong && isLittleEndian == false) {
                return TypeString.Int64BE;
            }

            if (value is ulong && isLittleEndian) {
                return TypeString.Int64LE;
            }


            //Float
            if (value is float && isLittleEndian == false) {
                return TypeString.Float32BE;
            }

            if (value is float && isLittleEndian) {
                return TypeString.Float32LE;
            }

            if (value is double && isLittleEndian == false) {
                return TypeString.Float64BE;
            }

            if (value is double && isLittleEndian) {
                return TypeString.Float64LE;
            }

            //Varient bytes
            if (value is string) {
                return TypeString.String;
            }

            if (value is byte[]) {
                return TypeString.Buffer;
            }

            throw new TypeAccessException("'" + type.FullName + "' cannot be converted to VarInt");
        }

        #endregion



        public class TypeString {
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

        public class TypeLength {
            public const int Bool = 1;
            public const int Boolean = 1;

            public const int Int8 = 1;
            public const int Int16BE = 2;
            public const int Int16LE = 2;
            public const int Int32BE = 4;
            public const int Int32LE = 4;
            public const int Int64BE = 8;
            public const int Int64LE = 8;

            public const int UInt8 = 1;
            public const int UInt16BE = 2;
            public const int UInt16LE = 2;
            public const int UInt32BE = 4;
            public const int UInt32LE = 4;
            public const int UInt64be = 8;
            public const int UInt64le = 8;

            public const int FloatBE = 4;
            public const int FloatLE = 4;
            public const int Float32BE = 4;
            public const int Float32LE = 4;
            public const int DoubleBE = 8;
            public const int DoubleLE = 8;
            public const int Float64BE = 8;
            public const int Float64LE = 8;

            public const int VarInt = -1;
            public const int VarUInt = -1;

            public const int String = -1;
            public const int Buffer = -1;

            public const int Object = -1;
        }

        public class Definition {
            public int Posistion = 0;
            public Definition Root = null;
            public string Name;
            public string Type;
            public List<Definition> Properties;
            public Definition Items;
            public List<string> Orders;

            public Definition() {
                this.Properties = new List<Definition>();
                this.Orders = new List<string>();
            }

            public dynamic GetValue(dynamic obj) {
                if (obj is JObject) {
                    var value = obj[this.Name];
                    if(value is JValue) {
                        return value.Value;
                    }

                    if (value is JArray) {
                        return value.ToObject<object[]>();
                    }
                    return value;
                }
                    
                return obj.GetType().GetField(this.Name).GetValue(obj);
            }


            public void SetValue<T>(object obj, T value) {
                if (obj is JObject) {
                    SetValue(obj as JObject, value);
                } else {
                    obj.GetType().GetField(this.Name).SetValue(obj, value);
                }

            }

            public void SetValue<T>(JObject obj, T value) {
                var type = typeof(T);
                if (type.IsValueType || type == typeof(string)) {
                    obj[this.Name] = (dynamic)value;
                } else if (type.IsArray) {
                    obj[this.Name] = new JArray(value);
                }
            }
        }
    }
}
