using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace fwv.Models
{
    public static class HashGenerator
    {
        public static string CreateMD5(string s)
        {
            MD5 alg = MD5.Create();
            byte[] rawHash = alg.ComputeHash(Encoding.ASCII.GetBytes(s));
            string hashString = Convert.ToBase64String(rawHash);
            return hashString;
        }

        public static string CreateRandomEmailAddress(int maxLocalPartLength)
        {
            if (maxLocalPartLength < 1) throw new ArgumentOutOfRangeException("maxLength");

            string[] validCharacters = new string[] {
                ".","-","_",
                "0","1","2","3","4","5","6","7","8","9",
                "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"
            };

            int numValidCharacters = validCharacters.Length;
            Random rnd = new Random((int)DateTime.Now.Ticks);

            string output = string.Empty;
            int nextIndex = 0;
            for (int i = 0; i < maxLocalPartLength; i++)
            {
                do
                {
                    nextIndex = rnd.Next(numValidCharacters);
                } while (i == 0 && 0 <= nextIndex && nextIndex <= 2);

                if (i > 1)
                {
                    while (nextIndex <= 1 && output[i - 1] == validCharacters[nextIndex][0])
                    {
                        nextIndex = rnd.Next(numValidCharacters);
                    }
                }

                output += validCharacters[nextIndex];
            }

            output = $"{output}@fwv.com";

            return output;
        }
    }
}
