using Domain.Exceptions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Application
{
    public class HmacUtilities
    {
        public static string CalculateHmac(string hmacKey, string signingstring)
        {
            try
            {
                byte[] key = PackH(hmacKey);
                byte[] data = Encoding.UTF8.GetBytes(signingstring);

                using (HMACSHA256 hmac = new(key))
                {
                    byte[] rawHmac = hmac.ComputeHash(data);

                    return Convert.ToBase64String(rawHmac);
                }
            }
            catch (Exception ex)
            {
                throw new HmacGenerationException("Failed to generate HMAC", ex);
            }
        }

        private static byte[] PackH(string hex)
        {
            if ((hex.Length % 2) == 1)
            {
                hex += '0';
            }

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
