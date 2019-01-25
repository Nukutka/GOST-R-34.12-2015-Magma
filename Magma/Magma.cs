using System;
using System.Linq;
using System.Numerics;
using System.Text;
using ExtensionMethods;

namespace Magma
{
    /// <summary>
    /// Реализация ГОСТ Р 34.12-2015
    /// </summary>
    static class Magma
    {
        // pi-подстановка
        private static readonly byte[][] pi = new byte[8][]
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

        /// <summary>
        /// Генерирует случайный 256-битный ключ
        /// </summary>
        public static byte[] GetKey()
        {
            return PrimeNumber.GetPrimeNumber(256).ToByteArray();
        }

        /// <summary>
        /// Выполняет шифрование сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="key">256-битный ключ</param>
        public static string Encrypt(string message, byte[] key)
        {
            string encryptMessage = "";
            int length = message.Length % 8 == 0 ? message.Length / 8 : message.Length / 8 + 1;
            for (int i = 0; i < length; i++)
            {
                string part = message.PadRight(length * 8).Substring(i * 8, 8).PadRight(8);
                string tmp = part.ToHexString();
                byte[] messageBytes = part.ToHexString().ToByteArray();
                byte[][] K = GetIterationKeys(key);
                byte[] encryptBytes = TransE(messageBytes, K);
                encryptMessage += encryptBytes.ToHexString();
            }
            return encryptMessage;
        }

        /// <summary>
        /// Выполняет дешифрование сообщения
        /// </summary>
        /// <param name="message">Зашифрованное сообщение (hex)</param>
        /// <param name="key">256-битный ключ</param>
        public static string Decrypt(string message, byte[] key)
        {
            string decryptMessage = "";
            for (int i = 0; i < message.Length / 16; i++)
            {
                string part = message.Substring(i * 16, 16);
                byte[] messageBytes = part.ToByteArray();
                byte[][] K = GetIterationKeys(key);
                byte[] decryptBytes = TransD(messageBytes, K);
                decryptMessage += decryptBytes.ToHexString();
            }
            return decryptMessage.ToDecString();
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
        /// Выполняет E-подстановку E = G*[K32]G[K31]...G[K1]
        /// </summary>
        /// <param name="message">Зашифрованное сообщение</param>
        /// <param name="K">Последовательность итерационных ключей</param>
        private static byte[] TransE(byte[] message, byte[][] K)
        {
            byte[] a1 = message.Skip(4).ToArray();
            byte[] a0 = message.Take(4).ToArray();

            for (int i = 0; i < 32; i++)
            {
                TransG(K[i], ref a1, ref a0);
            }
            return a1.Concat(a0).ToArray();
        }

        /// <summary>
        /// Выполняет D-подстановку D = G*[K1]G[K2]...G[K32]
        /// </summary>
        /// <param name="message">Зашифрованное сообщение</param>
        /// <param name="K">Последовательность итерационных ключей</param>
        private static byte[] TransD(byte[] message, byte[][] K)
        {
            byte[] a1 = message.Skip(4).ToArray();
            byte[] a0 = message.Take(4).ToArray();

            for (int i = 31; i >= 0; i--)
            {
                TransG(K[i], ref a1, ref a0);
            }
            return a1.Concat(a0).ToArray();
        }

        /// <summary>
        /// Выполняет G-преобразование
        /// </summary>
        /// <param name="K">Итерационный ключ</param>
        /// <param name="a1">32-битная часть сообщениея</param>
        /// <param name="a0">32-битная часть сообщениея</param>
        private static void TransG(byte[] K, ref byte[] a1, ref byte[] a0)
        {
            byte[] tmp = a1;
            a1 = a0;
            a0 = TransX(TransSmallG(K, a0), tmp);
        }

        /// <summary>
        /// Выполняет g-преобразование
        /// </summary>
        /// <param name="K">Итерационный ключ</param>
        /// <param name="a">32-битная часть сообщениея</param>
        private static byte[] TransSmallG(byte[] K, byte[] a)
        {
            byte[] sum = AddMod32(K, a);
            byte[] fourBitsArray = sum.ToFourBitsArray();
            for (int i = 0; i < 8; i++) // t-преобразование, выполняющее pi-подстановку
            {
                fourBitsArray[i] = pi[i][fourBitsArray[i]];
            }
            byte[] normalBytes = fourBitsArray.ToByteArray();
            byte[] res = CyclicShift11(normalBytes);
            return res;
        }

        /// <summary>
        /// Выполняет циклический сдвиг последовательности на 11 бит влево
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static byte[] CyclicShift11(byte[] arr)
        {
            string bitsString = "";
            for (int i = 0; i < 4; i++)
            {
                StringBuilder binary = new StringBuilder(Convert.ToString(arr[i], 2));
                binary.Insert(0, "0", 8 - binary.Length);
                bitsString += binary.ToString();
            }
            char[] bits = bitsString.ToCharArray();
            char[] tmp = new char[11];

            for (int i = 0; i < 11; i++)
            {
                tmp[i] = bits[i];
            }
            for (int i = 0; i < 21; i++)
            {
                bits[i] = bits[i + 11];
            }
            for (int i = 21; i < 31; i++)
            {
                bits[i] = tmp[i - 21];
            }

            byte[] res = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                res[i] = Convert.ToByte(new string(bits.Skip(i * 8).Take(8).ToArray()), 2);
            }

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

        /// <summary>
        /// Сложение в кольце 2^32
        /// </summary>
        /// <param name="a">Первое число</param>
        /// <param name="b">Второе число</param>
        private static byte[] AddMod32(byte[] a, byte[] b)
        {
            var x = new BigInteger(a);
            var y = new BigInteger(b);
            byte[] tmp = (new BigInteger(a) + new BigInteger(b) % BigInteger.Pow(2, 32)).ToByteArray();
            if (tmp.Length < 4)
            {
                tmp = tmp.Concat(new byte[1]).ToArray();
            }
            byte[] res = new byte[4];
            Array.Copy(tmp, 0, res, 0, 4);
            return res;
        }
    }
}
