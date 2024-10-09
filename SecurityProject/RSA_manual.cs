using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Security.Cryptography;

namespace SecurityProject
{
    public partial class RSA_manual : Form
    {
        public RSA_manual()
        {
            InitializeComponent();
        }

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

       // Generate
        private void button1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            BigInteger p, q;

            /*while (true)
            {
                p = rnd.Next(1000, 2147483647);
                q = rnd.Next(1000, 2147483647);

                if (IsPrime(p) && IsPrime(q) && p != q)
                {
                    break;
                }
            }*/

            q = BigInteger.Parse("935074216341541175273743681726926134816179815649825209583920572867366612830194180714269688523622933298595383002121237478481705991930515461832088986556979609165070382531072030248555600036199350082361495994788843512025037475727872446886878086367867658292961175551101188836022534463563308917441603805879");
            p = BigInteger.Parse("569732825502469740679922735375213701073631137432695801629363350148150094173072701650906231222902114437918389657541132308798257337796273322165729677385921756870110106502652779997849165063180998201088277250716880482924825494146768265052005829360828478670012652708707675770522955344486773824817786077561");

            BigInteger n = BigInteger.Multiply(p, q);

            richTextBox1.Text = p.ToString();
            richTextBox2.Text = q.ToString();
            richTextBox9.Text = n.ToString();
        }


        // Multiply
        private void button3_Click(object sender, EventArgs e)
        {
            BigInteger q = BigInteger.Parse(richTextBox1.Text);
            BigInteger p = BigInteger.Parse(richTextBox2.Text);
            BigInteger n = BigInteger.Multiply(p, q);
            richTextBox9.Text = n.ToString();
        }

        // Generate PU PR keys
        private void button2_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            BigInteger q = BigInteger.Parse(richTextBox1.Text);
            BigInteger p = BigInteger.Parse(richTextBox2.Text);
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
            richTextBox3.Text = eKey.ToString();

            BigInteger d;
            d = modinv(eKey, euler);
            richTextBox4.Text = d.ToString();
        }

        // Encrypt
        private void button5_Click(object sender, EventArgs e)
        {
            BigInteger eKey = BigInteger.Parse(richTextBox3.Text);
            BigInteger n = BigInteger.Parse(richTextBox9.Text);

            byte[] plainText = Encoding.ASCII.GetBytes(richTextBox5.Text);

            var hexString = BitConverter.ToString(plainText);
            hexString = hexString.Replace("-", "");
            string binaryString = String.Join(String.Empty,
            hexString.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            BigInteger msg = BigInteger.Parse(binaryString);

            BigInteger C = BigInteger.ModPow(msg, eKey, n);
            richTextBox6.Text = C.ToString();
        }


        // Decrypt
        private void button6_Click(object sender, EventArgs e)
        {
            BigInteger C = BigInteger.Parse(richTextBox7.Text);
            BigInteger d = BigInteger.Parse(richTextBox4.Text);
            BigInteger n = BigInteger.Parse(richTextBox9.Text);

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
            richTextBox8.Text = System.Text.Encoding.ASCII.GetString(bytes);


            //richTextBox8.Text = backM.ToString();
        }
    }
}
