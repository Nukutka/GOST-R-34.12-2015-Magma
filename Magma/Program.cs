using System;
using ExtensionMethods;

namespace Magma
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите сообщение для шифрования: ");
            string message = Console.ReadLine();
            byte[] key = Magma.GetKey();
            Console.WriteLine($"Сгенерированный ключ шифрования:\n{key.ToHexString()}");
            string encryptMessage = Magma.Encrypt(message, key);
            Console.WriteLine($"Зашифрованное сообщение:\n{encryptMessage}");
            string decryptMessage = Magma.Decrypt(encryptMessage, key);
            Console.WriteLine($"Дешифрованное сообщение:\n{decryptMessage}");
        }
    }
}
