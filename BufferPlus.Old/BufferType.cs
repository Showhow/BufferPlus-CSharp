﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus.Old {
    public class BufferType {

        private static Dictionary<Type, dynamic> _HandlersByType = null;
        public static Dictionary<Type, dynamic> Handlers {
            get {
                if (_HandlersByType == null) {
                    _HandlersByType = new Dictionary<Type, dynamic>();
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                        foreach (System.Reflection.TypeInfo type in assembly.DefinedTypes) {
                            if(type.BaseType == null || type.BaseType.IsGenericType == false) {
                                continue;
                            }
                            if (type.BaseType.Name == typeof(BufferTypeHandler<>).Name) {
                                dynamic bType = Activator.CreateInstance(type);
                                _HandlersByType.Add(type.BaseType.GetGenericArguments()[0], bType);
                            }
                        }
                    }
                        
                }

                return _HandlersByType;
            }
        }

        private static Dictionary<string, dynamic> _HandlersByString = null;
        public static Dictionary<string, dynamic> HandlersByString {
            get {
                if (_HandlersByString == null) {
                    _HandlersByString = new Dictionary<string, dynamic>();
                    foreach (var pair in Handlers) {
                        
                        var handler = pair.Value;
                        var type = ((object)handler).GetType();

                        string key_LE = type.GetMethod("GetTypeString").Invoke(handler, new object[] { true });
                        string key_BE = type.GetMethod("GetTypeString").Invoke(handler, new object[] { false });

                        if(_HandlersByString.ContainsKey(key_LE) == false ) {
                            _HandlersByString.Add(key_LE, handler);
                        }
                        if (_HandlersByString.ContainsKey(key_BE) == false) {
                            _HandlersByString.Add(key_BE, handler);
                        }
                    }
                }

                return _HandlersByString;
            }
        }

        public static BufferTypeHandler<T> GetHandler<T>(){
            return Handlers[typeof(T)];
        }

        public static dynamic GetHandler(string typeString) {
            return HandlersByString[typeString];
        }

    }
}
