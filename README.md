

These C# scripts serializing values and json string into packed binary bytes 
in order to reduce the overall bytes size when transferring data through Internet.

The scripts are based on [arloliu/buffer-plus](https://github.com/arloliu/buffer-plus) Nodejs package. 
By using generic types, this C# version aim to lower the redundant coding of variant types.

Usage
=====

Schema Definition Example
-------------------------
```csharp
// Account class definition
[Serializable]
class Account {
    public string name;
    public byte age;
    public string[] languages = new string[0];
    public ulong serial;
}

// account data
Account account = new Account() {
    name = "user1",
    age = 18,
    languages = new string[] {
        "en-US", "en-UK"
    },
    serial = 0x123456781234567
};

// account schema definition
var AccountSchema = @"{
    type: 'object',
    properties: {
        name: {type: 'string'},
        age: {type: 'uint8'},
        languages: {
            type: 'array',
            items: {type: 'string'},
        },
        serial: {type: 'uint64le'},
    },
    order: ['name', 'age', 'languages', 'serial'],
}";

// create Account schema
BufferPlus.CreateSchema("Account", AccountSchema);

// create a BufferPlus instance
var bp = new BufferPlus();

// write account data with Account schema
bp.WriteSchema("Account", account);

// move to buffer beginning
bp.MoveTo(0);

// read account from buffer
var decodedAccount = bp.ReadSchema("Account", new Account());

// print out buffer context
BufferPlus.PrettyPrintHex(bp, "ReadSchema", decodedAccount);
 ```


Basic Read/Write Operation
--------------------
```csharp
var bp = new BufferPlus(16);
//write
bp.Write(true);
BufferPlus.PrettyPrintHex(bp, "Write(true)");

bp.Write((sbyte)-0x12);
BufferPlus.PrettyPrintHex(bp, "Write(sbyte)");

bp.Write((byte)0x12);
BufferPlus.PrettyPrintHex(bp, "Write(byte)");

bp.Write((short)0x1234);
BufferPlus.PrettyPrintHex(bp, "Write(short)");

bp.Write((ushort)0x1234);
BufferPlus.PrettyPrintHex(bp, "Write(ushort)");

bp.Write((int)-0x12345678);
BufferPlus.PrettyPrintHex(bp, "Write(int)");

bp.Write((uint)0x12345678);
BufferPlus.PrettyPrintHex(bp, "Write(uint)");

bp.Write((long)-0x1234567812345678);
BufferPlus.PrettyPrintHex(bp, "Write(long)");

bp.Write((ulong)0x1234567812345678);
BufferPlus.PrettyPrintHex(bp, "Write(ulong)");

bp.Write((float)-0x1234567812345678);
BufferPlus.PrettyPrintHex(bp, "Write(float)");

bp.Write((double)0x1234567812345678);
BufferPlus.PrettyPrintHex(bp, "Write(double)");

// read
bp.Position = 0;
BufferPlus.PrettyPrintHex(bp, "ReadBoolean",  bp.ReadBoolean());
BufferPlus.PrettyPrintHex(bp, "ReadInt8",     bp.ReadInt8());
BufferPlus.PrettyPrintHex(bp, "ReadUInt8",    bp.ReadUInt8());
BufferPlus.PrettyPrintHex(bp, "ReadInt16BE",  bp.ReadInt16BE());
BufferPlus.PrettyPrintHex(bp, "ReadInt32BE",  bp.ReadInt32BE());
BufferPlus.PrettyPrintHex(bp, "ReadUInt32BE", bp.ReadUInt32BE());
BufferPlus.PrettyPrintHex(bp, "ReadInt64BE",  bp.ReadInt64BE());
BufferPlus.PrettyPrintHex(bp, "ReadUInt64BE", bp.ReadUInt64BE());
BufferPlus.PrettyPrintHex(bp, "ReadFloatBE",  bp.ReadFloatBE());
BufferPlus.PrettyPrintHex(bp, "ReadDoubleBE", bp.ReadDoubleBE());
 ```


Array Read/Write Operation
---------------------------
```csharp
var names  = new string[3] { "小張", "小李", "小笨狗" };
var values = new int[3]    { int.MaxValue, 0, int.MinValue };
var floats = new float[3]  { float.NaN, -0f, float.MaxValue };

var bp = new BufferPlus(16);

bp.WriteArray(names);
BufferPlus.PrettyPrintHex(bp, "WriteArray", names);

bp.WritePackedArray(values);
BufferPlus.PrettyPrintHex(bp, "WritePackedArray", values);

bp.WritePackedString("中文測試");
BufferPlus.PrettyPrintHex(bp, "WritePackedString", "中文測試");

bp.WritePackedArray(names);
BufferPlus.PrettyPrintHex(bp, "WritePackedArray", names);

bp.WriteArray(names);
BufferPlus.PrettyPrintHex(bp, "WriteArray", names);

bp.WriteVarIntArray(values);
BufferPlus.PrettyPrintHex(bp, "WriteVarIntArray", values);

bp.WriteVarIntPackedArray(values);
BufferPlus.PrettyPrintHex(bp, "WriteVarIntPackedArray", values);

bp.WritePackedArray(floats);
BufferPlus.PrettyPrintHex(bp, "WritePackedArray", floats);

bp.Position = 0;
BufferPlus.PrettyPrintHex(bp, "ReadStringArray",  bp.ReadStringArray(3));
BufferPlus.PrettyPrintHex(bp, "ReadPackedArray",  bp.ReadPackedArray<int>(TypeString.Int32BE));
BufferPlus.PrettyPrintHex(bp, "ReadPackedString", bp.ReadPackedString());
BufferPlus.PrettyPrintHex(bp, "ReadPackedArray",  bp.ReadPackedStringArray());
BufferPlus.PrettyPrintHex(bp, "ReadArray",        bp.ReadArray<string>(TypeString.String, 3));
BufferPlus.PrettyPrintHex(bp, "ReadVarIntArray",  bp.ReadVarIntArray(3));
BufferPlus.PrettyPrintHex(bp, "ReadVarIntPackedArray", bp.ReadVarIntPackedArray());
BufferPlus.PrettyPrintHex(bp, "ReadPackedArray",  bp.ReadPackedArray<float>(TypeString.Float32BE));
```


Customized Type Example
-----------------------
```csharp
var getMd5Hash = new Func<dynamic, string>((value) => {
    var bytes = Encoding.UTF8.GetBytes(Convert.ToString(value));
    var cryptoMD5 = System.Security.Cryptography.MD5.Create();
    var hash = BitConverter.ToString(cryptoMD5.ComputeHash(bytes))
        .Replace("-", String.Empty);
    return hash;
});

BufferPlus.AddCustomType("HashString",
    // Read("HashString") method
    (buffer) => {
        // read byte length of MD5 checksum from buffer
        var hashLen = buffer.ReadUInt32LE();
        // read byte length of value from buffer
        var valueLen = buffer.ReadUInt32LE();
        // read MD5 checksum  from buffer
        var hash = buffer.ReadString((int)hashLen);
        // return a object contains MD5 checksum and value
        var value = buffer.ReadString((int)valueLen);
        // return a object contains MD5 checksum and value
        return new { value = value, hash = hash };
    },
    // Write(value, "HashString") method
    (buffer, value) => {
        // calculate value's MD5 checksum 
        var hash = getMd5Hash(value);
        // write byte length of MD5 checksum to buffer
        buffer.WriteUInt32LE(hash.Length);
        // write byte length of value to buffer
        buffer.WriteUInt32LE(Convert.ToString(value).Length);
        // write MD5 checksum to buffer
        buffer.WriteString(hash);
        // write value to buffer
        buffer.WriteString(Convert.ToString(value));
    },
    // ByteLength(value, "HashString") method
    (buffer, value) => {
        if (value is string) {
            var hash = getMd5Hash(value);
            return 8 +
                BufferPlus.ByteLength(value, TypeString.String) +
                BufferPlus.ByteLength(hash, TypeString.String);
        }

        dynamic obj = value;
        return 8 +
            BufferPlus.ByteLength(obj.value, TypeString.String) +
            BufferPlus.ByteLength(obj.hash, TypeString.String);
    }
);

var bp = new BufferPlus(16);
var str = "test hash string";
// write string into buffer with pre-defined 'HashString' custom. type.
// it gets string's MD5 checksum and write MD5 checksum
// and string into buffer
bp.Write(str, "HashString");

// seek to beginning
bp.MoveTo(0);

// read buffer with with pre-defined 'HashString' custom. type.
// it reads MD5 checksum and string from buffer and
// returns a object contains 'hash' and 'value' properties.
var hashObj = bp.Read("HashString");

// print out result
BufferPlus.PrettyPrintHex(bp, "ReadHashString", (hashObj));
```