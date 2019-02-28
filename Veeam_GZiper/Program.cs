using System;

namespace Veeam_GZiper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException("Not enough or too many arguments!");
                }
                switch (args[0].ToLower())
                {
                    case "compress":
                        GZiper.Compress(args[1], args[2]);
                        break;
                    case "decompress":
                        GZiper.Decompress(args[1], args[2]);
                        break;
                    default:
                        throw new ArgumentException("Unsupported function! use \"compress\" or \"decompress\".");
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Check program arguments.");
                Console.WriteLine(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Access denied!");
                Console.WriteLine("Can not read or write file!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error has been occured!");
                Console.WriteLine(e.Message);
            }
        }
    }
}
