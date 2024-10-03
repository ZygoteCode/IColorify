using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Drawing;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;

namespace IColorify
{
    public class IColorifyCore
    {
        private static string alphabet = "!#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĲĳĴĵĶķĸĹĺĻļĽľĿŀŁł";

        public static Bitmap Encode(string inputString, string password, string phoneNumber)
        {
            try
            {
                byte[] input = ReverseAll("ICOLORIFY_CODIFICATION" + inputString);
                byte[] key = CalculateKeccakHash(ReverseAll(password), 256);
                byte[] iv = CalculateKeccakHash(ReverseAll(phoneNumber), 128);

                Array.Reverse(input);
                Array.Reverse(key);
                Array.Reverse(iv);

                byte[] inputHash = CalculateKeccakHash(input, 512);
                Array.Reverse(inputHash);

                input = Combine(inputHash, BitConverter.GetBytes(input.Length), input);

                byte[] encrypted = ProcessAES(input, key, iv, true);
                byte[] encryptedHash = CalculateKeccakHash(encrypted, 512);

                Array.Reverse(encrypted);
                Array.Reverse(encryptedHash);

                encrypted = Combine(encryptedHash, BitConverter.GetBytes(encrypted.Length), encrypted);
                encrypted = Compress(encrypted);
                Array.Reverse(encrypted);

                string encoded = SimpleEncoding.Base256.Encode(encrypted);
                string base256String = Microsoft.VisualBasic.Strings.StrReverse(encoded);

                Bitmap bitmap = new Bitmap(512, 512);
                int initialIndex = -1;
                int totalLength = base256String.Length;

                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        if (initialIndex >= totalLength)
                        {
                            if (initialIndex == totalLength)
                            {
                                bitmap.SetPixel(i, j, Color.FromArgb(90, 127, 32));
                                initialIndex++;
                            }
                            else
                            {
                                bitmap.SetPixel(i, j, Color.White);
                            }
                        }
                        else
                        {
                            if (initialIndex == totalLength)
                            {
                                bitmap.SetPixel(i, j, Color.FromArgb(90, 127, 32));
                            }
                            else if (initialIndex == -1)
                            {
                                bitmap.SetPixel(i, j, Color.FromArgb(210, 15, 44));
                                initialIndex++;
                            }
                            else
                            {
                                byte item = GetAlphabetIndex(base256String[initialIndex]);
                                bitmap.SetPixel(i, j, Color.FromArgb(item, item, item));
                                initialIndex++;
                            }
                        }
                    }
                }

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public static string Decode(Bitmap bitmap, string password, string phoneNumber)
        {
            try
            {
                if (bitmap.Width != 512 || bitmap.Height != 512)
                {
                    return null;
                }

                bool initial = true;
                string base256String = "";

                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Color pixel = bitmap.GetPixel(i, j);

                        if (initial)
                        {
                            if (pixel.R != 210 || pixel.G != 15 || pixel.B != 44)
                            {
                                return null;
                            }

                            initial = false;
                        }
                        else
                        {
                            if (pixel.R == 90 && pixel.G == 127 && pixel.B == 32)
                            {
                                goto finishAll;
                            }

                            if (pixel.R != pixel.G || pixel.R != pixel.B || pixel.G != pixel.B)
                            {
                                return null;
                            }

                            base256String += alphabet[pixel.R];
                        }
                    }
                }

            finishAll: base256String = Microsoft.VisualBasic.Strings.StrReverse(base256String);
                byte[] decoded = SimpleEncoding.Base256.Decode(base256String);
                Array.Reverse(decoded);
                decoded = Decompress(decoded);

                byte[] encryptedHash = decoded.Take(32).ToArray();
                decoded = decoded.Skip(32).ToArray();
                Array.Reverse(encryptedHash);

                int encryptedLength = BitConverter.ToInt32(decoded.Take(4).ToArray(), 0);
                decoded = decoded.Skip(4).ToArray();

                if (decoded.Length != encryptedLength)
                {
                    return null;
                }

                Array.Reverse(decoded);
                byte[] repeatedEncryptedHash = CalculateKeccakHash(decoded, 512);

                if (!CompareByteArrays(encryptedHash, repeatedEncryptedHash))
                {
                    return null;
                }

                byte[] key = CalculateKeccakHash(ReverseAll(password), 256);
                byte[] iv = CalculateKeccakHash(ReverseAll(phoneNumber), 128);

                Array.Reverse(key);
                Array.Reverse(iv);

                decoded = ProcessAES(decoded, key, iv, false);

                byte[] inputHash = decoded.Take(32).ToArray();
                decoded = decoded.Skip(32).ToArray();

                int inputLength = BitConverter.ToInt32(decoded.Take(4).ToArray(), 0);
                decoded = decoded.Skip(4).ToArray();

                if (decoded.Length != inputLength)
                {
                    return null;
                }

                Array.Reverse(inputHash);
                byte[] repeatedInputHash = CalculateKeccakHash(decoded, 512);

                if (!CompareByteArrays(inputHash, repeatedInputHash))
                {
                    return null;
                }

                Array.Reverse(decoded);
                string reversed = ReverseFromReverseAll(decoded);

                if (!reversed.StartsWith("ICOLORIFY_CODIFICATION"))
                {
                    return null;
                }

                return reversed.Substring("ICOLORIFY_CODIFICATION".Length);
            }
            catch
            {
                return null;
            }
        }

        private static bool CompareByteArrays(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;

            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }

            return ret;
        }

        private static byte GetAlphabetIndex(char c)
        {
            for (int i = 0; i < alphabet.Length; i++)
            {
                if (c.Equals(alphabet[i]))
                {
                    return (byte)i;
                }
            }

            return (byte)0;
        }

        private static byte[] ReverseAll(string inputString)
        {
            return Encoding.UTF8.GetBytes(Microsoft.VisualBasic.Strings.StrReverse(inputString));
        }

        private static string ReverseFromReverseAll(byte[] inputBytes)
        {
            return Microsoft.VisualBasic.Strings.StrReverse(Encoding.UTF8.GetString(inputBytes));
        }

        private static byte[] CalculateKeccakHash(byte[] inputBytes, int digestSize)
        {
            KeccakDigest keccakDigest = new KeccakDigest(256);
            byte[] hashBytes = new byte[keccakDigest.GetDigestSize()];

            keccakDigest.BlockUpdate(inputBytes, 0, inputBytes.Length);
            keccakDigest.DoFinal(hashBytes, 0);

            return hashBytes;
        }


        private static byte[] ProcessAES(byte[] data, byte[] key, byte[] iv, bool isEncrypt)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", key);
            ParametersWithIV parameters = new ParametersWithIV(keyParameter, iv);
            cipher.Init(isEncrypt, parameters);

            byte[] processed = new byte[cipher.GetOutputSize(data.Length)];
            int len = cipher.ProcessBytes(data, 0, data.Length, processed, 0);
            cipher.DoFinal(processed, len);

            return processed;
        }

        private static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    int totalRead = 0, bytesRead;
                    while ((bytesRead = decompressionStream.Read(result, totalRead, length - totalRead)) > 0)
                    {
                        totalRead += bytesRead;
                    }

                    return result;
                }
            }
        }

        private static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }
    }
}