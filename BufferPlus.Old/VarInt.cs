using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * TODO VarInt 工作清單
 * 
 * Last updated at 2021.2.26 -- by Showhow
 * 
 *   [v] nodejs javascript 移植
 *   [-] 使用 VarintBitConverter 重構
 *   [-] 處理過長的 Byte 所產生的問題
 *   
 */
namespace BufferPlus {

    [Obsolete("Will be refactored soon, please use VarintBitConverter instead")]
    public class VarInt {

        public static int EncodeUInt(uint value, ref byte[] output) {
            output = VarintBitConverter.GetVarintBytes(value);
            return output.Length;
        }

        public static int EncodeInt(int value, ref byte[] output) {
            int val = value >= 0 ? value * 2 : (value * -2) - 1;
            return VarInt.EncodeUInt((uint)val, ref output);
        }

        public static int DecodeUInt(byte[] buf, int offset, int endBoundary, ref uint output) {
            var bytes = buf.Skip(offset).Take(endBoundary - offset).ToArray();
            output = VarintBitConverter.ToUInt32(bytes);

            return VarintBitConverter.GetVarintBytes(output).Length;
        }

        public static int DecodeUInt_1(byte[] buf, int offset, int endBoundary, ref uint output) {
            uint val = 0;
            int shift = 0;
            byte rbyte = 0;
            int count = offset;

            do {
                if (count >= endBoundary) {
                    throw new ArgumentOutOfRangeException("Decode varint fail");
                }

                rbyte = buf[count++];
                val += Convert.ToUInt32(shift < 28 ? (rbyte & 0x7F) << shift : (rbyte & 0x7F) * Math.Pow(2, shift));
                shift += 7;
            } while (Convert.ToBoolean(rbyte & 0x80));

            output = val;

            return count - offset;
        }

        public static int DecodeInt(byte[] buf, int offset, int endBoundary, ref int output) {
            uint u_output = 0;
            int count = VarInt.DecodeUInt(buf, offset, endBoundary, ref u_output);
            output = Convert.ToInt32(Convert.ToBoolean(u_output & 1) ? (u_output + 1) / -2 : u_output / 2);
            return count;
        }

    }

}
