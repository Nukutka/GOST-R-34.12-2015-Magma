using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Magma
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] key = Magma.GetKey();
            Magma.Encrypt("121", key);
        }
    }
}
