﻿using NukeDataTool;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NukeCheckTool
{
    class Program
    {
        static uint FastCrc32(uint crc, byte[] buf, int len)
        {
            crc = ~crc;
            var i = 0;
            while (len-- > 0)
            {
                crc ^= buf[i++];
                for (int k = 0; k < 8; k++)
                    crc = ((crc & 1) > 0) ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
            }
            return ~crc;
        }

        static int ManagedMemCmp(byte[] buf1, byte[] buf2, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (buf1[i] > buf2[i]) return 1;
                if (buf1[i] < buf2[i]) return -1;
            }
            return 0;
        }

        static bool CheckBootId(FileStream fileStream)
        {
            Console.WriteLine("Checking Boot Sector...");

            // ========================================
            //   Read Boot Sector
            // ========================================

            var blockSize = 0x2800;
            var arrBuffer = new byte[blockSize];

            fileStream.Seek(0, SeekOrigin.Begin);
            var bytesRead = fileStream.Read(arrBuffer, 0, blockSize);

            if (bytesRead != blockSize)
            {
                Console.WriteLine("Error: ReadFile() read boot sector error.");
                return false;
            }

            // ========================================
            //   Verify Boot Sector
            // ========================================

            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var ict = aes.CreateDecryptor(Secret.MasterKey, Secret.MasterIV))
            using (var cms = new CryptoStream(new MemoryStream(arrBuffer), ict, CryptoStreamMode.Read))
            {
                cms.Read(arrBuffer, 0, blockSize);

                var arrBootCk = arrBuffer.Skip(4).ToArray();
                var crcBootId = FastCrc32(0, arrBootCk, arrBootCk.Length);

                using (var rd = new BinaryReader(new MemoryStream(arrBuffer)))
                {
                    // Check CRC32
                    var crc1 = rd.ReadUInt32();
                    if (crc1 != crcBootId)
                    {
                        Console.WriteLine("Error: Boot sector checksum error.");
                        return false;
                    }

                    // Check Header
                    var pi = rd.ReadUInt32();
                    if (pi != 0x2800U)
                    {
                        Console.WriteLine("Error: Boot sector header size error.");
                        return false;
                    }

                    // Check Magic
                    pi = rd.ReadUInt32();
                    if (pi != 0x44495442U)
                    {
                        Console.WriteLine("Error: Boot sector identification error.");
                        return false;
                    }

                    // Skip some data
                    rd.ReadBytes(0x14);

                    // Get segment info
                    var segCount = rd.ReadUInt64();   // Total Count
                    var segSize = rd.ReadUInt64();    // Segment Size
                    var segHeader = rd.ReadUInt64();  // Header Count

                    // Show segment info
                    var imageSize = Convert.ToInt64(segCount * segSize);
                    var dataOffset = Convert.ToInt64(segHeader * segSize);
                    Console.WriteLine("  header_count   = {0,10:N0}  =  0x{0,-6:x2}", segHeader);
                    Console.WriteLine("  segment_count  = {0,10:N0}  =  0x{0,-6:x2}", segCount);
                    Console.WriteLine("  segment_size   = {0,10:N0}  =  0x{0,-6:x2}", segSize);
                    Console.WriteLine("  data_offset    = {0,10:N0}  =  0x{0,-6:x2}", dataOffset);
                    Console.WriteLine("  image_size     = {0,10}  =  {1:N0} bytes", FormatFactory.FormatSize(imageSize), imageSize);

                    // Check segment info
                    if (segHeader != 8)
                    {
                        Console.WriteLine("Error: Property 'header_count' is invalid.");
                        return false;
                    }
                    if (segCount < 9)
                    {
                        Console.WriteLine("Error: Property 'segment_count' is invalid.");
                        return false;
                    }
                    if (segSize != 0x40000)
                    {
                        Console.WriteLine("Error: Property 'segment_size' is invalid.");
                        return false;
                    }

                    // Check image file info
                    if (new FileInfo(fileStream.Name).Length != imageSize)
                    {
                        Console.WriteLine("Error: Property 'image_size' is invalid.");
                        return false;
                    }

                    // Boot Sector is valid
                    Console.WriteLine("Boot Sector Data is [ GOOD ]");
                    return true;
                }
            }
        }

        static bool VerifyChecksum(FileStream fileStream)
        {
            Console.WriteLine("Checking Checksum Table...");

            // ========================================
            //   Read Boot Sector
            // ========================================

            var segSize = 0x40000;
            var blockSize = 0x2800;
            var bigBuffer = new byte[segSize];

            fileStream.Seek(0, SeekOrigin.Begin);
            var bytesRead = fileStream.Read(bigBuffer, 0, blockSize);

            if (bytesRead != blockSize)
            {
                Console.WriteLine("Error: ReadFile() read boot sector error.");
                return false;
            }

            // Initialize first sector crc value
            var crcValue = FastCrc32(0, bigBuffer, bytesRead);

            // ========================================
            //   Read Checksum Table
            // ========================================

            // Initialize checksum table
            var segCount = new FileInfo(fileStream.Name).Length / segSize;
            blockSize = Convert.ToInt32(segCount * 4);

            // Skip to checksum table and read
            fileStream.Seek(0x200, SeekOrigin.Current);
            bytesRead = fileStream.Read(bigBuffer, 0, blockSize);
            if (bytesRead != blockSize)
            {
                Console.WriteLine("Error: ReadFile() read checksum table error.");
                return false;
            }

            // Update first sector crc value
            var cCrcArray = bigBuffer.Skip(4).ToArray();
            crcValue = FastCrc32(crcValue, cCrcArray, bytesRead - 4);

            // Populate the checksum table
            var crcTable = new uint[segCount];
            using (var br = new BinaryReader(new MemoryStream(bigBuffer)))
            {
                for (int i = 0; i < segCount; i++)
                {
                    crcTable[i] = br.ReadUInt32();
                }
            }

            // Reading the remaining data
            blockSize = segSize - Convert.ToInt32(fileStream.Position);
            bytesRead = fileStream.Read(bigBuffer, 0, blockSize);
            if (bytesRead != blockSize)
            {
                Console.WriteLine("Error: ReadFile() read error at segment #0.");
                return false;
            }

            // Update first sector crc value
            crcValue = FastCrc32(crcValue, bigBuffer, bytesRead);

            // ========================================
            //   Verify Checksum Table
            // ========================================

            // Verify first sector crc value
            if (crcValue != crcTable[0])
            {
                Console.WriteLine("Error: Checksum error at segment #0. {0:x8} vs {1:x8}.", crcTable[0], crcValue);
                return false;
            }

            // Check all remaining segments
            var errCount = 0;
            blockSize = 0x40000;
            for (int i = 1; i < segCount; i++)
            {
                Console.Write("\rChecking segment #{0}.".PadRight(60), i);
                bytesRead = fileStream.Read(bigBuffer, 0, blockSize);
                if (bytesRead != blockSize)
                {
                    Console.WriteLine("\rError: ReadFile() read error at segment #{0}.", i);
                    return false;
                }
                crcValue = FastCrc32(0, bigBuffer, bytesRead);
                if (crcValue != crcTable[i])
                {
                    Console.WriteLine("\rError: Checksum error at segment #{0}. {1:x8} vs {2:x8}.", i, crcTable[i], crcValue);
                    if (errCount++ > 3) return false;
                }
            }

            // Checksum Table is valid
            Console.WriteLine("\rChecksum Table Data is [ GOOD ]".PadRight(60));
            return true;
        }

        static bool VerifySignature(FileStream fileStream)
        {
            Console.WriteLine("Checking Signature...");

            // ========================================
            //   Read and Verify Signature
            // ========================================

            using (var hmac = new HMACSHA1(Secret.HmacSecret))
            {
                // Skip Boot Id
                fileStream.Seek(0x2800, SeekOrigin.Begin);

                // Read Signature
                var blockSize = 0x200;
                var readSign = new byte[blockSize];
                var bytesRead = fileStream.Read(readSign, 0, readSign.Length);

                // Check Signature
                if (bytesRead == blockSize)
                {
                    var buffer = new byte[0x1000];
                    var bytesLeft = 0x200000 - 0x2A00;
                    while (true)
                    {
                        blockSize = 0x1000;
                        if (bytesLeft < 0x1000) blockSize = bytesLeft;

                        bytesRead = fileStream.Read(buffer, 0, blockSize);
                        if (bytesRead != blockSize)
                        {
                            Console.WriteLine("Error: ReadFile() read data size error.");
                            return false;
                        }
                        if (bytesLeft - bytesRead == 0)
                        {
                            hmac.TransformFinalBlock(buffer, 0, bytesRead);
                            break;
                        }
                        else
                        {
                            hmac.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                            bytesLeft -= bytesRead;
                        }
                    }
                    var calcSign = hmac.Hash;
                    if (ManagedMemCmp(readSign, calcSign, calcSign.Length) == 0)
                    {
                        // Image Signature is valid
                        Console.WriteLine("\rSignature Data is [ GOOD ]".PadRight(60));
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Error: Signature error.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Error: ReadFile() signature size error.");
                    return false;
                }
            }
        }

        static void Execute(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                Console.WriteLine("Checking : {0}", fileInfo.Name);
                if (!fileInfo.Exists)
                {
                    Console.WriteLine("Error: File not found");
                    return;
                }
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (!CheckBootId(fileStream) ||
                        !VerifyChecksum(fileStream) ||
                        !VerifySignature(fileStream))
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                return;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: {0} file1 [file2] ...", System.AppDomain.CurrentDomain.FriendlyName);
            }
            else
            {
                foreach (var file in args)
                {
                    Execute(file);
                }
            }
        }
    }

    static class FormatFactory
    {
        /// <summary>
        ///     <para>Convert a number in byte unit to another unit.</para>
        ///     <para>See more at: http://loliraki.tk/2011/10/16/php-convert-bytes-1 </para>
        /// </summary>
        /// <param name="bytes">File size in byte unit.</param>
        /// <param name="offset">The target unit you want to convert <paramref name="bytes" /> to.</param>
        /// <returns><paramref name="bytes" /> converted to a possible or defined unit if success.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        internal static string FormatSize(long bytes, int offset = -1)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            // int c = 0;
            if (offset < 0)
            {
                object[] r = { bytes, units[0] };
                for (int k = 0; k < units.Length; k++)
                {
                    if ((bytes / Math.Pow(1024, k)) >= 1)
                    {
                        r[0] = bytes / Math.Pow(1024, k);
                        r[1] = units[k];
                        // c++;
                    }
                }
                return String.Format("{0:#0.##} {1}", r[0], r[1]);
            }
            if (offset < 9)
            {
                return (bytes / Math.Pow(1024, offset)).ToString("N") + " " + units[offset];
            }
            throw new ArgumentOutOfRangeException("offset");
        }
    }
}
