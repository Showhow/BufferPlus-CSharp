using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


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

            if (definition.TypeString == "object") {
                foreach (var property in definition.Properties) {
                    action = BuildWriteAction(property);
                }
            } else if (definition.TypeString == "array") {
                var property = definition.Items;

                if (action == null) {
                    if (property.TypeString == "string") {
                        action = (buffer, obj) => {
                            dynamic value = definition.GetValue(obj);
                            buffer.WritePackedStringArray(value);
                        };
                    }

                    action = (buffer, obj) => {
                        dynamic value = property.GetValue(obj);
                        buffer.WritePackedArray(value, property.TypeString,-1,-1);
                    };
                }

                this.WriteFunctions.Add(action);
            } else {
                if (definition.TypeString == "string") {
                    action = (buffer, obj) => {
                        dynamic value = definition.GetValue(obj);
                        buffer.WritePackedString(value);
                    };
                }

                if (action == null) {
                    action = (buffer, obj) => {
                        dynamic value = definition.GetValue(obj);
                        //BufferType.WriteFunctions[definition.TypeString](buffer, value, null, -1, -1);
                        buffer.Write(value, definition.TypeString,-1,-1);
                    };
                }

                this.WriteFunctions.Add(action);
            }
            return action;
        }

        public Action<BufferPlus, object> BuildReadAction(Definition definition) {
            Action<BufferPlus, dynamic> action = null;

            if (definition.TypeString == "object") {
                foreach (var property in definition.Properties) {
                    action = BuildReadAction(property);
                }
            } else if (definition.TypeString == "array") {
                var property = definition.Items;

                if (property.TypeString == TypeString.Buffer) {
                    action = (buffer, obj) => {
                        dynamic value = buffer.ReadPackedBufferArray();
                        property.SetValue(obj, value);
                    };
                }

                if (property.TypeString == TypeString.String) {
                    action = (buffer, obj) => {
                        dynamic value = buffer.ReadPackedStringArray();
                        property.SetValue(obj, value);
                    };
                }

                if (action == null) {
                    action = (buffer, obj) => {
                        var type = BufferType.GetDataType(property.TypeString);

                        MethodInfo method = buffer
                            .GetType()
                            .GetMethod(nameof(buffer.ReadPackedArray), new Type[] { typeof(string) })
                            .MakeGenericMethod(type);
                        dynamic value = method.Invoke(buffer, new object[] { property.TypeString });
                        property.SetValue(obj, value);
                    };
                }

                this.ReadFunctions.Add(action);
            } else {
                if (definition.TypeString == TypeString.Buffer) {
                    action = (buffer, obj) => {
                        dynamic value = buffer.ReadPackedBuffer();
                        definition.SetValue(obj, value);
                    };
                }

                if (definition.TypeString == TypeString.String) {
                    action = (buffer, obj) => {
                        dynamic value = buffer.ReadPackedString();
                        definition.SetValue(obj, value);
                    };
                }

                if (action == null) {
                    action = (buffer, obj) => {
                        dynamic value = BufferType.ReadFunctions[definition.TypeString](buffer, null, -1, -1);
                        definition.SetValue(obj, value);
                    };
                }

                this.ReadFunctions.Add(action);
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
            bp.Seal();
        }

        public void SetEncoding(Encoding encoding) {
            this.Encoding = encoding;
            return;
        }

        #endregion

        #region Static Methods
        public static Definition ParseDefinition(JToken json, string name = null) {
            var def = new Definition();
            string type = json["type"].ToString();
            def.Name = name;
            def.TypeString = type;

            if (def.TypeString == "object") {
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

            if (def.TypeString == "array") {
                var items_properties = json["items"];
                def.Items = ParseDefinition(items_properties, name);
            }

            return def;
        }

        #endregion

        public class Definition {
            public int Posistion = 0;
            public Definition Root = null;
            public string Name;
            public string TypeString;
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
                    if (value is JValue) {
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
                    obj[this.Name] = JToken.FromObject(value);
                } else if (type.IsArray) {
                    obj[this.Name] = new JArray(value);
                }
            }
        }
    }
}
