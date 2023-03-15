using Libraries;
using Libraries.Compression;
using System;
using System.IO;
using System.Linq;

namespace FF_MSX_GFX
{
    class Program
    {
        public enum WriteArgs
        {
            action,
            uncompressedPath,
            outputPath,
        }

        public enum DumpArgs
        {
            action,
            address,
            rowAmount,
            compressedFilePath,
            dumpPath,
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception($"Cannot have 0 arguments.");
            }

            var action = args[0];
            int requiredLength;

            switch (action)
            {
                case "Write":
                    Console.WriteLine($"Writing");

                    requiredLength = (int)Enum.GetValues(typeof(WriteArgs)).Cast<WriteArgs>().Max() + 1;
                    if (args.Length != requiredLength)
                    {
                        throw new Exception($"Required argument number: {requiredLength}. Received: {args.Length}");
                    }

                    var uncompressedPath = args[(int)WriteArgs.uncompressedPath];
                    var outputPath = args[(int)WriteArgs.outputPath];
                    WriteAllGeneral(uncompressedPath, outputPath);
                    break;
                case "Dump":
                    Console.WriteLine($"Dumping");

                    requiredLength = (int)Enum.GetValues(typeof(DumpArgs)).Cast<DumpArgs>().Max() + 1;
                    if (args.Length != requiredLength)
                    {
                        throw new Exception($"Required argument number: {requiredLength}. Received: {args.Length}");
                    }

                    var address = MyMath.HexToDec(args[(int)DumpArgs.address]);
                    var size = MyMath.HexToDec(args[(int)DumpArgs.rowAmount]);
                    var compressedFilePath = args[(int)DumpArgs.compressedFilePath];
                    var dumpPath = args[(int)DumpArgs.dumpPath];
                    DumpAllGeneral(address, size, compressedFilePath, dumpPath);

                    break;
                default:
                    throw new Exception($"Invalid first parameter: {action}");
            }

            Console.WriteLine($"Finished successfully.");
        }

        private static void WriteAllGeneral(string uncompressedPath, string outputPath)
        {
            if (!File.Exists(outputPath))
            {
                throw new FileNotFoundException($"File not found: {outputPath}");
            }

            if (!File.Exists(uncompressedPath))
            {
                throw new FileNotFoundException($"File not found: {uncompressedPath}");
            }

            var uncompBytes = File.ReadAllBytes(uncompressedPath);
            var dest = File.ReadAllBytes(outputPath);
            
            var compBytes = FFMSXFont.Compress(uncompBytes, dest);
            File.WriteAllBytes(outputPath, compBytes);
        }

        private static void DumpAllGeneral(int gfxLocation, int rowAmount, string compressedFilePath, string outputPath)
        {
            if (!File.Exists(compressedFilePath))
            {
                throw new FileNotFoundException($"File not found: {compressedFilePath}");
            }

            byte[] compBytes = File.ReadAllBytes(compressedFilePath);
            File.WriteAllBytes(outputPath, FFMSXFont.Decompress(compBytes, (uint)gfxLocation, (uint)rowAmount));
        }
    }
}
