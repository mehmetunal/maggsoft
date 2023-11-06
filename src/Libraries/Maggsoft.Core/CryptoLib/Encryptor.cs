using BCrypt.Net;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Maggsoft.Core.CryptoLib
{
    public enum HashSubType
    {
        Normal,
        Cng,
        Managed,
        CryptoServiceProvider
    }

    public static class Encryptor
    {
        public static string PasswordHash(this string text)
        {
            text = $"{text}-?*0=)(/&%+^'!2Q@@½$#£>";
            var sha512 = Hash(text, HashType.SHA512, HashSubType.CryptoServiceProvider);
            var sha384 = Hash(sha512, HashType.SHA384, HashSubType.CryptoServiceProvider);
            var sha256 = Hash(sha384, HashType.SHA256, HashSubType.CryptoServiceProvider);
            return sha256;
        }

        public static string Hash(this string input, HashType hash, HashSubType subType = HashSubType.Normal)
        {
            Func<HashAlgorithm, string> hashFunction = alg => HashingHelper(input, alg);

            switch (subType)
            {
                case HashSubType.Normal:
                    return hashFunction(NormalHashes(hash));
                case HashSubType.Cng:
                    return hashFunction(CngHashes(hash));
                case HashSubType.Managed:
                    return hashFunction(ManagedHashes(hash));
                case HashSubType.CryptoServiceProvider:
                    return hashFunction(CspHashes(hash));
                default: return "error"; // unreachable
            }
        }

        private static string HashingHelper(string text, HashAlgorithm algorithm)
        {
            Func<string, byte[]> getHash = input => algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder();
            Array.ForEach(getHash(text), b => sb.Append(b.ToString("X")));

            return sb.ToString();
        }

        private static HashAlgorithm NormalHashes(HashType hash)
        {
            switch (hash)
            {
                case HashType.SHA256:
                    return SHA256.Create("System.Security.Cryptography.SHA256");
                case HashType.SHA384:
                    return SHA384.Create("System.Security.Cryptography.SHA384");
                case HashType.SHA512:
                    return SHA512.Create("System.Security.Cryptography.SHA512");
                default: return null; // unreachable
            }
        }

        private static HashAlgorithm CngHashes(HashType hash)
        {
            switch (hash)
            {
                case HashType.SHA256:
                    return SHA256.Create();
                case HashType.SHA384:
                    return SHA384.Create();
                case HashType.SHA512:
                    return SHA512.Create();
                default: return null; // unreachable
            }
        }

        private static HashAlgorithm ManagedHashes(HashType hash)
        {
            switch (hash)
            {
                case HashType.SHA256:
                    return SHA256.Create();
                case HashType.SHA384:
                    return SHA384.Create();
                case HashType.SHA512:
                    return SHA512.Create();
                default: return null; // unreachable
            }
        }

        private static HashAlgorithm CspHashes(HashType hash)
        {
            switch (hash)
            {
                case HashType.SHA256:
                    return SHA256.Create();
                case HashType.SHA384:
                    return SHA384.Create();
                case HashType.SHA512:
                    return SHA512.Create();
                default: return null; // unreachable
            }
        }
    }
}
