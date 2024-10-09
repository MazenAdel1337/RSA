using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SecurityProject
{
    [Serializable()]
    public class RSA
    {
        BigInteger publicKey;
        BigInteger cipherObj;

        public bool IsPrime(BigInteger number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt((double)number));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (i > 5000)
                    return false;
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        public bool coprime(BigInteger a, BigInteger b)
        {
            if (BigInteger.GreatestCommonDivisor(a, b) == 1)
                return true;
            else
                return false;
        }

        public BigInteger modinv(BigInteger u, BigInteger v)
        {
            BigInteger inv, u1, u3, v1, v3, t1, t3, q;
            BigInteger iter;
            /* Step X1. Initialise */
            u1 = 1;
            u3 = u;
            v1 = 0;
            v3 = v;
            /* Remember odd/even iterations */
            iter = 1;
            /* Step X2. Loop while v3 != 0 */
            while (v3 != 0)
            {
                /* Step X3. Divide and "Subtract" */
                q = u3 / v3;
                t3 = u3 % v3;
                t1 = u1 + q * v1;
                /* Swap */
                u1 = v1; v1 = t1; u3 = v3; v3 = t3;
                iter = -iter;
            }
            /* Make sure u3 = gcd(u,v) == 1 */
            if (u3 != 1)
                return 0;   /* Error: No inverse exists */
            /* Ensure a positive result */
            if (iter < 0)
                inv = v - u1;
            else
                inv = u1;
            return inv;
        }

        public byte[] Encrypt(RSA rsa, string plainText)
        {
            Random rnd = new Random();
            BigInteger q = BigInteger.Parse("935074216341541175273743681726926134816179815649825209583920572867366612830194180714269688523622933298595383002121237478481705991930515461832088986556979609165070382531072030248555600036199350082361495994788843512025037475727872446886878086367867658292961175551101188836022534463563308917441603805879");
            BigInteger p = BigInteger.Parse("569732825502469740679922735375213701073631137432695801629363350148150094173072701650906231222902114437918389657541132308798257337796273322165729677385921756870110106502652779997849165063180998201088277250716880482924825494146768265052005829360828478670012652708707675770522955344486773824817786077561");


            /*while (true)
            {
                p = rnd.Next(50, 500);
                q = rnd.Next(50, 500);


                if (IsPrime(p) && IsPrime(q) && p != q)
                {
                    break;
                }
            }*/



            BigInteger n = BigInteger.Multiply(p, q);

            BigInteger euler = (p - 1) * (q - 1);
            BigInteger eKey;
            while (true)
            {
                eKey = rnd.Next(40000, 2147483647);
                if (IsPrime(eKey) && coprime(eKey, n))
                {
                    break;
                }
            }

            byte[] plainBin = Encoding.ASCII.GetBytes(plainText);

            var hexString = BitConverter.ToString(plainBin);
            hexString = hexString.Replace("-", "");
            string binaryString = String.Join(String.Empty,
            hexString.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            BigInteger msg = BigInteger.Parse(binaryString);

            BigInteger C = BigInteger.ModPow(msg, eKey, n);
            rsa.publicKey = eKey;
            rsa.cipherObj = C;

            byte[] encryptedObj = Serialize(rsa);
            return encryptedObj;
        }

        public string Decrypt(RSA rsa, byte[] encryptedObj)
        {
            BigInteger q = BigInteger.Parse("935074216341541175273743681726926134816179815649825209583920572867366612830194180714269688523622933298595383002121237478481705991930515461832088986556979609165070382531072030248555600036199350082361495994788843512025037475727872446886878086367867658292961175551101188836022534463563308917441603805879");
            BigInteger p = BigInteger.Parse("569732825502469740679922735375213701073631137432695801629363350148150094173072701650906231222902114437918389657541132308798257337796273322165729677385921756870110106502652779997849165063180998201088277250716880482924825494146768265052005829360828478670012652708707675770522955344486773824817786077561");

            BigInteger n = BigInteger.Multiply(p, q);

            BigInteger euler = (p - 1) * (q - 1);

            rsa = Deserialize(encryptedObj);
            BigInteger C = rsa.cipherObj;
            BigInteger d = modinv(rsa.publicKey, euler);

            BigInteger backM = BigInteger.ModPow(C, d, n);

            string test = backM.ToString();

            int mod4Len = test.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                test = test.PadLeft(((test.Length / 8) + 1) * 8, '0');
            }

            int numOfBytes = test.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(test.Substring(8 * i, 8), 2);
            }
            string decryptedMsg = System.Text.Encoding.ASCII.GetString(bytes);
            return decryptedMsg;
        }

        public byte[] Serialize(RSA obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public RSA Deserialize(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            RSA obj = (RSA)binForm.Deserialize(memStream);

            return obj;
        }
    }
}
