using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus {
    public abstract class BufferTypeHandler<T>{

        private Encoding _Encoding = Encoding.UTF8;
        public Encoding Encoding {
            get => _Encoding;
            set => _Encoding = value;
        }

        public Type GetBufferType() => typeof(T);

        public abstract T ReadBuffer(byte[] buffer, bool isLittleEndian = false, int offset = 0, int length = -1);

        public abstract byte[] WriteBuffer(T value, bool isLittleEndian = false);

        public abstract int GetBytesLength(T value);

        public abstract int GetBytesLength();

        public abstract string GetTypeString(bool isLittleEndian = false);

        public abstract bool GetByteOrder(string typeString);


    }
}
