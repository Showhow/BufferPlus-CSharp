using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferPlus {
    public abstract class BufferTypeHandler<T>{

        public Type GetBufferType() {
            return typeof(T);
        }

        public abstract T Decode(byte[] buffer, bool isLittleEndian = false, int offset = 0, int length = -1);

        //public virtual T Decode(BufferPlus bp, bool isLittleEndian = false, int position = 0, int length = -1) {

        //}

        public abstract byte[] Encode(T value, bool isLittleEndian = false);

        public abstract int GetBytesLength(T value);

        public abstract string GetTypeString(bool isLittleEndian = false);

        public abstract bool GetByteOrder(string typeString);


    }
}
