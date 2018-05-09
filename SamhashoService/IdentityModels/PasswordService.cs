

namespace SamhashoService.IdentityModels
{
    using System;
    using System.Security.Cryptography;

    using Microsoft.AspNet.Identity;

    [Flags]
        public enum PasswordOptions
        {
           None = 0,

           HasSymbols = 1,

           NoRepeating = 1 << 1,

           NoConsecutive = 1 << 2,

           HasDigits = 1 << 3,

           HasCapitals = 1 << 4,

           HasLower = 1 << 5
        }

   
        public sealed class PasswordService
        {
            private const int DefaultMaximum = 15;

            private const int DefaultMinimum = 8;

            private readonly string exclusionSet;

            private readonly bool noConsecutive;

            private readonly bool hasDigits;

            private readonly bool noRepeating;

            private readonly bool hasSymbols;

            private readonly bool hasCapitals;

            private readonly bool hasLower;

            private const string LowerCaseCharacters = "abcdefghijklmnopqrstuvwxyz";

            private const string UpperCaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            private const string SpecialCharacters = "@#$%&?=+";

            private const string DigitCharacters = "0123456789";

            private readonly RNGCryptoServiceProvider rng;

            private int maxSize;

            private int minSize;

            private PasswordService(PasswordOptions options)
            {
                exclusionSet = " `~^*()_-{}[];:'\"<>,./|\\";
                Minimum = DefaultMinimum;
                Maximum = DefaultMaximum;
                noConsecutive = options.HasFlag(PasswordOptions.NoConsecutive);
                noRepeating = options.HasFlag(PasswordOptions.NoRepeating);
                hasSymbols = options.HasFlag(PasswordOptions.HasSymbols);
                hasDigits = options.HasFlag(PasswordOptions.HasDigits);
                hasCapitals = options.HasFlag(PasswordOptions.HasCapitals);
                hasLower = options.HasFlag(PasswordOptions.HasLower);
                rng = new RNGCryptoServiceProvider();
            }

            private int Maximum
            {
                get{
                    return maxSize;
                }

                set{
                    maxSize = value;
                    if (minSize >= maxSize) { maxSize = DefaultMaximum; }
                }
            }

            private int Minimum
            {
                get{
                    return minSize;
                }

                set{
                    minSize = value;
                    if (DefaultMinimum > minSize) { minSize = DefaultMinimum; }
                }
            }

            private string Generate()
            {
                // Pick random length between minimum and maximum   
                int pwdLength = GetCryptographicRandomNumber(Minimum, Maximum);

                char[] pwdBuffer = new char[pwdLength];

                if (hasCapitals)
                {
                    // then enforce at least one capital
                    char capital = GetRandomCharacter(UpperCaseCharacters);
                    int index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    while (pwdBuffer[index] != default(char))
                    {
                        index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    }

                    pwdBuffer[index] = capital;
                }

                if (hasDigits)
                {
                    // then enforce at least one digit
                    char digit = GetRandomCharacter(DigitCharacters);
                    int index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    while (pwdBuffer[index] != default(char))
                    {
                        index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    }

                    pwdBuffer[index] = digit;
                }

                if (hasSymbols)
                {
                    // then enforce at least one symbol
                    char symbol = GetRandomCharacter(SpecialCharacters);
                    int index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    while (pwdBuffer[index] != default(char))
                    {
                        index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    }

                    pwdBuffer[index] = symbol;
                }

                if (hasLower)
                {
                    // then enforce at least one lowercase
                    char lower = GetRandomCharacter(LowerCaseCharacters);
                    int index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    while (pwdBuffer[index] != default(char))
                    {
                        index = GetCryptographicRandomNumber(0, pwdLength - 1);
                    }

                    pwdBuffer[index] = lower;
                }

                // Generate random characters
                // Initial dummy character flag
                char lastCharacter = '\n';

                string characterOptions = string.Join(
                    string.Empty,
                    LowerCaseCharacters,
                    UpperCaseCharacters,
                    DigitCharacters,
                    SpecialCharacters);

                for (int i = 0; i < pwdLength; i++)
                {
                    if (pwdBuffer[i] != default(char)) continue;

                    char nextCharacter = GetRandomCharacter(characterOptions);
                    if (noConsecutive)
                    {
                        while (lastCharacter == nextCharacter) { nextCharacter = GetRandomCharacter(characterOptions); }
                    }

                    if (noRepeating)
                    {
                        string temp = pwdBuffer.ToString();
                        int duplicateIndex = temp.IndexOf(nextCharacter);
                        while (duplicateIndex != -1)
                        {
                            nextCharacter = GetRandomCharacter(characterOptions);
                            duplicateIndex = temp.IndexOf(nextCharacter);
                        }
                    }

                    pwdBuffer[i] = nextCharacter;
                    lastCharacter = nextCharacter;
                }

                return new string(pwdBuffer);
            }

            private int GetCryptographicRandomNumber(int lBound, int uBound)
            {
                // Assumes lBound >= 0 && lBound < uBound
                // returns an int >= lBound and < uBound
                uint uRandomNumber;
                byte[] randomNumber = new byte[4];
                if (lBound == uBound - 1)
                {
                    // test for degenerate case where only lBound can be returned
                    return lBound;
                }

                uint excludeRndBase = uint.MaxValue - uint.MaxValue % (uint)(uBound - lBound);

                do
                {
                    rng.GetBytes(randomNumber);
                    uRandomNumber = BitConverter.ToUInt32(randomNumber, 0);
                }
                while (uRandomNumber >= excludeRndBase);

                return (int)(uRandomNumber % (uBound - lBound)) + lBound;
            }

            private char GetRandomCharacter(string characterArray)
            {
                char[] charArray = characterArray.ToCharArray();
                int upperBound = charArray.GetUpperBound(0);

                int randomCharPosition = GetCryptographicRandomNumber(charArray.GetLowerBound(0), upperBound);

                char randomChar = charArray[randomCharPosition];
                if (string.IsNullOrEmpty(exclusionSet)) return randomChar;

                while (exclusionSet.IndexOf(randomChar) != -1)
                {
                    randomCharPosition = GetCryptographicRandomNumber(charArray.GetLowerBound(0), upperBound);
                    randomChar = charArray[randomCharPosition];
                }

                return randomChar;
            }

          
            public static string GeneratePassword(PasswordOptions options)
            {
                PasswordService service = new PasswordService(options);
                return service.Generate();
            }

            public static string GeneratePassword(PasswordPolicy policy)
            {
                PasswordOptions options = PasswordOptions.HasLower | PasswordOptions.NoConsecutive | PasswordOptions.NoRepeating;
                if (policy.MinimumDigits > 0) options |= PasswordOptions.HasDigits;
                if (policy.MinimumSpecialChars > 0) options |= PasswordOptions.HasSymbols;
                if (policy.MinimumUpperCase > 0) options |= PasswordOptions.HasCapitals;

                PasswordService service = new PasswordService(options) { Minimum = policy.MinimumLength, Maximum = policy.MaximumLength };
                string generated = service.Generate();
                IdentityResult matchesPolicy = policy.ValidateAsync(generated).Result;

                while (!matchesPolicy.Succeeded)
                {
                    generated = service.Generate();
                    matchesPolicy = policy.ValidateAsync(generated)
                        .Result;
                }

                return generated;
            }
        }
    }
