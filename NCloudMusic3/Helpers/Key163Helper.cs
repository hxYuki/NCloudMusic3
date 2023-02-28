using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TagLib.Matroska;

namespace NCloudMusic3.Helpers
{
    internal static class Key163Helper
    {
        private static Aes aes = Create163Aes();
        private static Aes Create163Aes()
        {
            var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(@"#14ljk_!\]&0U<'(");
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        public static bool Decrypt(string code, [MaybeNullWhen(false)] out string json)
        {
            json = null;
            if (!Is163KeyCandidate(code))
                return false;

            try
            {
                byte[] byt163Key = Convert.FromBase64String(code[22..]);
                using var cryptoTransform = aes.CreateDecryptor();
                byt163Key = cryptoTransform.TransformFinalBlock(byt163Key, 0, byt163Key.Length);
                json = Encoding.UTF8.GetString(byt163Key);
            }
            catch
            {
                return false;
            }
            return true;

        }

        public static bool Is163KeyCandidate(string s)
        {
            return !string.IsNullOrEmpty(s) && s.StartsWith("163 key(Don't modify):", StringComparison.Ordinal);
        }
    }
}
