using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SecurityProject
{
    [Serializable()]
    public class packageBlock
    {
        public int id, size=0;
        public byte[] package;
        public string encryption = "", type = "plaintext";

        public byte[] Serialize(packageBlock obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public packageBlock Deserialize(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            packageBlock obj = (packageBlock)binForm.Deserialize(memStream);

            return obj;

        }
    }
}
