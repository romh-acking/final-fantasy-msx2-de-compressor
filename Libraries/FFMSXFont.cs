using System;
using System.Collections.Generic;
using System.Linq;

namespace Libraries.Compression
{
    public class FFMSXFont //: ICompressor, IDecompressor
    {
        private const byte maxRowSize = 0x80;

        private const byte fileHeaderSize = 0x30;
        private const byte offsetRowSize = 0x6;
        private const byte offsetColSize = 0x8;

        public static byte[] Decompress(byte[] compressedData, uint start, uint rowCount)
        {
            /*
             * XX XX Amount = Fill
             * XX = Write byte
             * 
             * The algorithm is pretty weird and dumb. 
             * If the decompression is filling, there must be a pair of the two bytes with the same value.
             * For the decompression to write, there must pair of two bytes there aren't equal.
             * 
             * With this alone, I could create a simple algorithm to decompress instead of creating
             * code that closely matches the assembly code, there's some weird behavior that throws a wrench in that.
             */

            // Font
            // VRAM: 0x0001E400
            // Disk image: 0x00038430
            // rowSize: 0x80 (Specified at 0x00038406)

            // Intro
            // VRAM: 0x00010000
            // Disk image: 0x0004F030
            // rowSize: 0x78 (Specified at 0x0004F006)
            uint position = start + fileHeaderSize;

            List<byte> output = new();

            byte rowSize = compressedData[start + offsetRowSize];

            var decompressedSize = rowCount * maxRowSize;
            var decompSize = 0;
            byte d = rowSize;
            byte a, b;

            while (true)
            {
                a = compressedData[position++];

            label_54F1:;

                if (decompSize >= decompressedSize)
                {
                    break;
                }

                output.Add(a);
                decompSize++;
                d--;

                if (d == 0)
                {
                    d = rowSize;

                    // Fill the unused row cells will blank pixels
                    for (int i = rowSize; i < maxRowSize; i++)
                    {
                        decompSize++;
                    }

                    continue;
                }

                b = compressedData[position++];

                if (b != a)
                {
                    a = b;
                    goto label_54F1;
                }

                b = (byte)(compressedData[position++] - 1);

                do
                {
                    output.Add(a);
                    decompSize++;
                    d--;
                    b--;
                }
                while (b > 0);

                if (d == 0)
                {
                    d = rowSize;

                    // Fill the unused row cells will blank pixels
                    for (int i = rowSize; i < maxRowSize; i++)
                    {
                        decompSize++;
                    }
                }
            }

            return output.ToArray();
        }

        public static byte[] Compress(byte[] uncompressed, byte[] destBytes)
        {
            byte a, b, d = 0x0;

            var fileHeader = destBytes.Take(fileHeaderSize).ToArray();
            byte rowSize = fileHeader[offsetRowSize];

            List<byte> output = new();

            int i= 0;
            while(uncompressed.Length > i)
            {
                if (d == rowSize)
                {
                    d = 0x0;
                    continue;
                }

                a = uncompressed[i++];

            label_54F1:;

                if (uncompressed.Length - 1 < i )
                {
                    break;
                }

                output.Add(a);
                d++;

                if (d == rowSize)
                {
                    d = 0x0;
                    continue;
                }

                b = uncompressed[i++];

                if (b != a)
                {
                    a = b;
                    goto label_54F1;
                }

                output.Add(b);
                d++;

                if (uncompressed.Length -1 < i)
                {
                    break;
                }

                int totalFill = 0;
                for(int j = i - 2; ; j++)
                {
                    totalFill++;

                    if (j+1 < uncompressed.Length || uncompressed[j] != uncompressed[j+1])
                    {
                        break;
                    }
                }

                byte fillLength = 2;
                do
                {
                    a = uncompressed[i];

                    if (a != b)
                    {
                        break;
                    }

                    if (((totalFill % 0x6a == fillLength) && (totalFill % 0x6b != 0)) || (fillLength == 0x6b))
                    {
                        break;
                    }

                    if (d == rowSize)
                    {
                        d = 0x0;
                        break;
                    }

                    i++;
                    d++;
                    fillLength++;

                    if (uncompressed.Length -1 < i)
                    {
                        break;
                    }
                } 
                while (uncompressed[i] == b);

                output.Add(fillLength);
            }

            // Pad the file to fit within sector limits
            var newFile = fileHeader.Concat(output).ToArray();

            if (newFile.Length % 0x200 != 0)
            {
                newFile = newFile.Concat(new byte[0x200 - (newFile.Length % 0x200)]).ToArray();
            }

            return newFile;
        }
    }
}