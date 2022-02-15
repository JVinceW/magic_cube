using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Game.Scripts.Common.Utils {
    public static class CryptoUtil {
        #region SHA1
        public static string Sha1(string str) {
            using (var sha1 = SHA1.Create()) {
                var bytes = sha1.ComputeHash(new UTF8Encoding().GetBytes(str));
                return BitConverter.ToString(bytes).ToLower().Replace("-", "");
            }
        }

        public static string Sha1(byte[] bytes) {
            using (var sha1 = SHA1.Create()) {
                var bs = sha1.ComputeHash(bytes);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }
        #endregion

        #region SHA256
        public static string Sha256(string str) {
            using (var sha256 = SHA256.Create()) {
                var bs = sha256.ComputeHash(new UTF8Encoding().GetBytes(str));
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public static string Sha256(byte[] bytes) {
            using (var sha256 = SHA256.Create()) {
                var bs = sha256.ComputeHash(bytes);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public static byte[] Sha256ToByte(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Sha256ToByte(bytes);
        }

        public static byte[] Sha256ToByte(byte[] data) {
            var crypto = SHA256.Create();
            var bytes = crypto.ComputeHash(data);
            crypto.Clear();
            return bytes;
        }
        #endregion

        #region AES
        // ReSharper disable once InconsistentNaming
        public static byte[] AESEncrypt(string data, string ivSeed, string keySeed) {
            var ivHash = Sha256ToByte(ivSeed);
            // IV always is 16 Byte
            var iv = new byte[16];
            Array.Copy(ivHash, 0, iv, 0, 16);
            var key = Sha256ToByte(keySeed);
            return AESEncrypt(data, iv, key);
        }

        /// <summary>
        /// <see cref="https://www.c-sharpcorner.com/article/aes-encryption-in-c-sharp/"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.paddingmode?view=net-5.0"/>
        /// </summary>
        /// <returns>Encrypted data</returns>
        // ReSharper disable once InconsistentNaming
        public static byte[] AESEncrypt(string data, byte[] iv, byte[] keySeed) {
            using (var aesManaged = new AesManaged()) {
                aesManaged.KeySize = 256;
                aesManaged.BlockSize = 128;
                aesManaged.Key = keySeed;
                aesManaged.IV = iv;
                aesManaged.Padding = PaddingMode.PKCS7;
                var encryptor = aesManaged.CreateEncryptor(aesManaged.Key, aesManaged.IV);
                using (var msStream = new MemoryStream()) {
                    using (var csStream = new CryptoStream(msStream, encryptor, CryptoStreamMode.Write)) {
                        using (var streamWriter = new StreamWriter(csStream)) {
                            streamWriter.Write(data);
                            var encrypted = msStream.ToArray();

                            return encrypted;
                        }
                    }
                }
            }
        }

        public static string AESDecrypt(byte[] data, string ivSeed, string keySeed) {
            var ivHash = Sha256ToByte(ivSeed);
            // Get IV is 16 Byte
            var iv = new byte[16];
            Array.Copy(ivHash, 0, iv, 0, 16);
            var key = Sha256ToByte(keySeed);
            return AESDecrypt(data, iv, key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="iv"></param>
        /// <param name="keySeed"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static string AESDecrypt(byte[] data, byte[] iv, byte[] keySeed) {
            using (var aesManaged = new AesManaged()) {
                var plainText = "";
                aesManaged.KeySize = 256;
                aesManaged.BlockSize = 128;
                aesManaged.Key = keySeed;
                aesManaged.IV = iv;
                aesManaged.Padding = PaddingMode.PKCS7;
                var deCryptor = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
                using (var msStream = new MemoryStream(data)) {
                    using (var csStream = new CryptoStream(msStream, deCryptor, CryptoStreamMode.Read)) {
                        using (var streamReader = new StreamReader(csStream)) {
                            plainText = streamReader.ReadToEnd();
                            return plainText;
                        }
                    }
                }
            }
        }
        #endregion

        #region Common Crypto
        /// <summary>
        /// Use Xor encryption, Call this with the string that wanna encrypt, call one more time with the encrypted byte to decrypt
        /// </summary>
        /// <param name="bytes">Byte of the string that want to encrypt or decrypt</param>
        /// <param name="key">Key for the encryption</param>
        /// <param name="xorMaxByteSize">Max byte that want to encrypt, decrypt. If this value bigger than size of the bytes array, then it will auto encrypt all the bytes array</param>
        public static void XorWithByteSize(ref byte[] bytes, int key, int xorMaxByteSize) {
            var byteKey = (byte)(bytes.Length + key);
            if (byteKey == 0) {
                byteKey = (byte)key;
            }

            var xorMaxLength = xorMaxByteSize;
            if (xorMaxLength > bytes.Length) {
                xorMaxLength = bytes.Length;
            }

            for (int i = 0; i < xorMaxLength; i++) {
                bytes[i] = (byte)(bytes[i] ^ byteKey);
            }
        }

        /// <summary>
        /// <see cref="https://www.codingame.com/playgrounds/11117/simple-encryption-using-c-and-xor-technique"/>
        /// Call this with the string you want to encrypt, if you want to decrypt, just call this method one more time
        /// </summary>
        public static string EncryptDecryptWithXor(string szPlainText, int szEncryptionKey) {
            var szInputStringBuild = new StringBuilder(szPlainText);
            var szOutStringBuild = new StringBuilder(szPlainText.Length);
            char encrypt;
            for (int iCount = 0; iCount < szPlainText.Length; iCount++) {
                encrypt = szInputStringBuild[iCount];
                encrypt = (char)(encrypt ^ szEncryptionKey);
                szOutStringBuild.Append(encrypt);
            }

            return szOutStringBuild.ToString();
        }
        #endregion
    }
}