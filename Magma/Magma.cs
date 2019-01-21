using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Magma
{
    static class Magma
    {
        private const int n = 64;

        private static byte[][] pi = new byte[8][]
        {
            new byte[16] {12, 4, 6, 2, 10, 5, 11, 9, 14, 8, 13, 7, 0, 3, 15, 1},
            new byte[16] {6, 8, 2, 3, 9, 10, 5, 12, 1, 14, 4, 7, 11, 13, 0, 15},
            new byte[16] {11, 3, 5, 8, 2, 15, 10, 13, 14, 1, 7, 4, 12, 9, 6, 0},
            new byte[16] {12, 8, 2, 1, 13, 4, 15, 6, 7, 0, 10, 5, 3, 14, 9, 11},
            new byte[16] {7, 15, 5, 10, 8, 1, 6, 13, 0, 9, 3, 14, 11, 4, 2, 12},
            new byte[16] {5, 13, 15, 6, 9, 2, 12, 10, 11, 7, 8, 1, 4, 3, 14, 0},
            new byte[16] {8, 14, 2, 5, 6, 9, 1, 12, 15, 4, 11, 0, 13, 10, 3, 7},
            new byte[16] {1, 7, 14, 13, 0, 5, 8, 3, 4, 15, 10, 6, 9, 12, 11, 2}
        };

        static Magma() { }

        public static byte[] GetKey()
        {
            return PrimeNumber.GetPrimeNumber(256).ToByteArray();
        }

        public static string Encrypt(string message, byte[] key)
        {
            //byte[][] K = GetIterationKeys(key);
            byte[][] K = GetIterationKeys(ConvertHexStringToByteArray("ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff"));
            byte[] encryptMessage = TransE(message, K);
            return "";
        }

        private static byte[] TransE(string message, byte[][] K)
        {
            // byte[] a1 = ConvertHexStringToByteArray(ConvertStringToHexString(message));
            byte[] tmp = ConvertHexStringToByteArray("fedcba9876543210");
            byte[] a1 = tmp.Skip(4).ToArray();
            byte[] a0 = tmp.Take(4).ToArray();
            Console.WriteLine(ConvertByteArrayToHexString(a1) + " " + ConvertByteArrayToHexString(a0));
    
            for (int i = 0; i < 32; i++)
            {
                TransG(K[i], ref a1, ref a0);
                Console.WriteLine(ConvertByteArrayToHexString(K[i]));
               // Console.WriteLine(ConvertByteArrayToHexString(a1) + " " + ConvertByteArrayToHexString(a0));
            }
            return a1.Concat(a0).ToArray();
        }

        private static void TransG(byte[] K, ref byte[] a1, ref byte[] a0)
        {
            byte[] tmp = a1;
            a1 = a0;
            a0 = TransX(TransSmallG(K, a0), tmp);
        }

        private static byte[] TransSmallG(byte[] K, byte[] a)
        {
            byte[] sum = AddMod32(K, a);
            byte[] fourBitsArray = ConvertByteArrayToFourBitsArray(K);
            for (int i = 0; i < 8; i++) // t-преобразование, выполняющее pi-подстановку
            {
                fourBitsArray[i] = pi[i][fourBitsArray[i]];
            }
            byte[] normalBytes = ConvertFourBitsArrayToByteArray(fourBitsArray);
            byte[] res = CyclicShift11(normalBytes);
            return res;
        }

        private static byte[] CyclicShift11(byte[] arr)
        {
            BitArray bitArray = new BitArray(arr);
            BitArray tmpArr = new BitArray(11);
            for (int i = 0; i < 11; i++)
            {
                tmpArr[i] = bitArray[i];
            }
            for (int i = 0; i < 21; i++)
            {
                bitArray[i] = bitArray[i + 11];
            }
            for (int i = 21; i < 31; i++)
            {
                bitArray[i] = tmpArr[i - 21];
            }

            byte[] res = new byte[(bitArray.Length - 1) / 8 + 1];
            bitArray.CopyTo(res, 0);

            return res;
        }

        /// <summary>
        /// Выполняет X-преобразование (xor)
        /// </summary>
        private static byte[] TransX(byte[] k, byte[] a)
        {
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)(k[i] ^ a[i]);
            }
            return result;
        }

        private static byte[] ConvertByteArrayToFourBitsArray(byte[] arr)
        {
            byte[] res = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                res[2 * i] = (byte)(arr[i] / 16);
                res[2 * i + 1] = (byte)(arr[i] % 16);
            }
            return res;
        }

        private static byte[] ConvertFourBitsArrayToByteArray(byte[] arr)
        {
            byte[] res = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                res[i] = (byte)(arr[2 * i] * 16 + arr[2 * i + 1]);
            }
            return res;
        }

        /// <summary>
        /// Сложение в кольце 2^32
        /// </summary>
        /// <param name="a">Первое число</param>
        /// <param name="b">Второе число</param>
        private static byte[] AddMod32(byte[] a, byte[] b)
        {
            a = a.Concat(new byte[1]).ToArray();
            b = b.Concat(new byte[1]).ToArray();
            //var x = new BigInteger(a);
            //var y = new BigInteger(b);
            byte[] tmp = ((new BigInteger(a) + new BigInteger(b)) % BigInteger.Pow(2, 32)).ToByteArray();
            byte[] res = new byte[4];
            Array.Copy(tmp, 0, res, 0, 4);
            return res;
        }

        /// <summary>
        /// Выполняет выработку итерационных ключей
        /// </summary>
        /// <param name="key">256-битный ключ</param>
        /// <returns></returns>
        private static byte[][] GetIterationKeys(byte[] key)
        {
            byte[][] K = new byte[32][];
            for (int i = 0; i < 8; i++)
            {
                K[i] = key.Skip(28 - 4 * i).Take(4).ToArray();
                K[i + 8] = K[i];
                K[i + 16] = K[i];
            }
            for (int i = 0; i < 8; i++)
            {
                K[i + 24] = K[7 - i];
            }
            return K;
        }

        /// <summary>
        /// Выполняет преобразование строки в ее 16-ричный вид
        /// </summary>
        /// <param name="input">Входная строка</param>
        private static string ConvertStringToHexString(string input)
        {
            return string.Join("", input.Select(c => ((int)c).ToString("X2")));
        }

        /// <summary>
        /// Выполняется преобразование массива байт в hex строку
        /// </summary>
        /// <param name="input">Входная строка</param>
        private static string ConvertByteArrayToHexString(this byte[] input)
        {
            return BitConverter.ToString(input.Reverse().ToArray()).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Выполняет преобразование hex строки в массив байт
        /// </summary>
        /// <param name="input">Входная строка</param>
        private static byte[] ConvertHexStringToByteArray(this string input)
        {
            byte[] bytes = new byte[input.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(input.Substring((bytes.Length - i - 1) * 2, 2), 16);
            }
            return bytes;
        }
    }
}
