

A C# object binary stream serializing middleware library which originating ported from 
[buffer-plus](https://github.com/arloliu/buffer-plus) Nodejs package. By using generic types, this C# version aim to lower the reductant coding of variant types.

Usage
=====

Schema Definition Example
-------------------------
```csharp
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
class Account {
    public string name;
    public byte age;
    public string[] languages = new string[0];
    public ulong serial;
}

static void Main(string[] args) {
  var account_schema_json = @"{
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
    
    BufferPlus.CreateSchema("Account", account_schema_json);

    var bp_1 = new BufferPlus(16);
    bp_1.WriteSchema("Account", account_1);
    Console.WriteLine("account_1: " + JsonConvert.SerializeObject(account_1));
    Console.WriteLine("buffer 1 : \t" + bp_1.ToHex());
}

 ```
 
Read/Write Operation
--------------------
```csharp
var names = new string[3] { "Joy", "Tom", "May" };
var values = new int[4] { int.MaxValue, 0, -1, int.MinValue };
var floats = new float[3] { float.NaN, -0f, float.MaxValue };

var bp_4 = new BufferPlus(16);

//write
bp_4.Write((sbyte)-0x12);
bp_4.Write((float)-0x1234567812345678);

bp_4.WriteArray(names, false, true);
bp_4.WritePackedArray(values);
bp_4.WritePackedString("中文測試");
bp_4.WriteVarIntArray(values, true);
bp_4.WriteArray(floats, true);

Console.WriteLine("Buffer 4: \t " + bp_4.ToHex());

//read
bp_4.Position = 0;
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<sbyte>\t" + JsonConvert.SerializeObject(bp_4.Read<sbyte>()));
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<float>\t" + JsonConvert.SerializeObject(bp_4.Read<float>()));

Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedArray<string>\t" + JsonConvert.SerializeObject(bp_4.ReadPackedArray<string>()));
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedArray<int>\t" + JsonConvert.SerializeObject(bp_4.ReadPackedArray<int>()));
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedString\t" + JsonConvert.SerializeObject(bp_4.ReadPackedString()));
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadVarIntArray\t" + JsonConvert.SerializeObject(bp_4.ReadVarIntArray()));
Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadArray<float>\t" + JsonConvert.SerializeObject(bp_4.ReadArray<float>(3, true)));

 ```
