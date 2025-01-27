// See https://aka.ms/new-console-template for more information

namespace StbTrueTypeSharp
{
    public struct TestPepe
    {
        public ushort a, b;
    }

    public class Program
    {
        static public void Change(ref int a)
        {
            a += 33;
        }

        static public void Main()
        {
            System.Console.WriteLine("Hello, World!");

            int[] ints = new int[] { 1, 2, 3, 4 };

            Change(ref ints[3]);
			
			byte[] bytes = new byte[] { 1, 2, 3, 4 };

            BytePtr bytePtr = new BytePtr(bytes);

            byte a = bytePtr++;
			byte b = bytePtr++;
			byte c = bytePtr++;

            System.Console.WriteLine("" + a + " " + b + " " + c);

            int cc = 3;
        }
        

    }
}

