using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BufferPlus {

   

    class Example {
        [Serializable]
        class Account {
            public string name;
            public byte age;
            public string[] languages = new string[0];
            public ulong serial;
        }

        static void Main(string[] args) {

            #region account data
            Account account = new Account() {
                name = "user1",
                age = 18,
                languages = new string[] {
                    "en-US", "en-UK"
                },
                serial = 0x123456781234567
            };

            Account account_2 = new Account() {
                name = "user2",
                age = 23,
                languages = new string[] {
                    "en-US", "zh-TW"
                },
                serial = 0x8765432187654321
            };

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

            #endregion

            #region Json schema encode/decode testing
            
            BufferPlus.CreateSchema("Account", account_schema_json);

            var bp_1 = new BufferPlus(16);
            bp_1.WriteSchema("Account", account);
            Console.WriteLine("account_1: " + JsonConvert.SerializeObject(account));
            Console.WriteLine("buffer 1 : \t" + bp_1.ToHex());
            
            var bp_2 = new BufferPlus(16);
            bp_2.WriteSchema("Account", account_2);
            Console.WriteLine("account_2: " + JsonConvert.SerializeObject(account_2));
            Console.WriteLine("buffer 2 : \t" + bp_2.ToHex());

            Account account_2_decoded = new Account();
            bp_2.ReadSchema("Account", account_2_decoded);
            Console.WriteLine("decoded account_2: \t " + JsonConvert.SerializeObject(account_2_decoded));

            JObject jobj_from_account = new JObject();
            bp_2.ReadSchema("Account", jobj_from_account);
            Console.WriteLine("jobj_from_account: \t " + JsonConvert.SerializeObject(jobj_from_account));

            var bp_3 = new BufferPlus(16);
            bp_3.WriteSchema("Account", jobj_from_account);
            Console.WriteLine("buffer 3: \t" + bp_1.ToHex());

            #endregion

            #region standard re/write testing
            var names = new string[3] { "Joy", "Tom", "May" };
            var values = new int[4] { int.MaxValue, 0, -1, int.MinValue };
            var floats = new float[3] { float.NaN, -0f, float.MaxValue };

            var bp_4 = new BufferPlus(16);
            //write
            bp_4.Write(true);
            bp_4.Write((sbyte)-0x12);
            bp_4.Write((byte)0x12);
            bp_4.Write((short)-0x1234);
            bp_4.Write((ushort)0x1234);
            bp_4.Write((int)-0x12345678);
            bp_4.Write((uint)0x12345678);
            bp_4.Write((long)-0x1234567812345678);
            bp_4.Write((ulong)0x1234567812345678);
            bp_4.Write((float)-0x1234567812345678);
            bp_4.Write((double)0x1234567812345678);

            bp_4.WriteArray(names, false, true);
            bp_4.WritePackedArray(values);
            bp_4.WritePackedString("中文測試");
            bp_4.WritePackedArray(names);
            bp_4.WriteArray(names);
            bp_4.WriteVarIntArray(values);
            bp_4.WriteVarIntArray(values, true);
            bp_4.WriteArray(floats, true);
            bp_4.Write(-123456);
            bp_4.Write((uint)123456);
            bp_4.Write((short)-9876);
            Console.WriteLine("Buffer 4: \t " + bp_4.ToHex());

            var toJson = new Func<object, string>(JsonConvert.SerializeObject);
            
            //read
            bp_4.Position = 0;
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<bool>\t" + toJson(bp_4.Read<bool>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<sbyte>\t" + toJson(bp_4.Read<sbyte>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<byte>\t" + toJson(bp_4.Read<byte>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<short>\t" + toJson(bp_4.Read<short>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<ushort>\t" + toJson(bp_4.Read<ushort>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<int>\t" + toJson(bp_4.Read<int>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<uint>\t" + toJson(bp_4.Read<uint>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<long>\t" + toJson(bp_4.Read<long>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<ulong>\t" + toJson(bp_4.Read<ulong>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<float>\t" + toJson(bp_4.Read<float>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<double>\t" + toJson(bp_4.Read<double>()));


            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedArray<string>\t" + toJson(bp_4.ReadPackedArray<string>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedArray<int>\t" + toJson(bp_4.ReadPackedArray<int>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedString\t" + JsonConvert.SerializeObject(bp_4.ReadPackedString()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadPackedArray<string>\t" + JsonConvert.SerializeObject(bp_4.ReadPackedArray<string>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadArray<string>\t" + JsonConvert.SerializeObject(bp_4.ReadArray<string>(3)));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadVarIntArray\t" + JsonConvert.SerializeObject(bp_4.ReadVarIntArray(false, 4)));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadVarIntArray\t" + JsonConvert.SerializeObject(bp_4.ReadVarIntArray()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t ReadArray<float>\t" + JsonConvert.SerializeObject(bp_4.ReadArray<float>(3, true)));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<int>\t" + JsonConvert.SerializeObject(bp_4.Read<int>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<uint>\t" + JsonConvert.SerializeObject(bp_4.Read<uint>()));
            Console.WriteLine("Buffer 4: \t " + bp_4.Position + "\t Read<short>\t" + JsonConvert.SerializeObject(bp_4.Read<short>()));
            #endregion

            Console.ReadKey();
        }
    }
}
