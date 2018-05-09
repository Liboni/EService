

namespace SamhashoService.IdentityModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;

    [Serializable]
    public class PasswordPolicy : IIdentityValidator<String>
    {
        private int minLength;
        private int maxLength;
        private int minSpecialChars;
        private int minUpperCase;
        private int minDigits;

        public int MinimumLength
        {
            get
            {
                return minLength;
            }
            set
            {
                if (value <= 3) throw new ArgumentException("Minimum length has to be at least 4 characters");
                minLength = value;
            }
        }

        public int MaximumLength
        {
            get { return maxLength; }
            set
            {
                if (value < minLength) throw new ArgumentException("Maximum length has to be at least equal to minimum length");
                maxLength = value;
            }
        }

        public int MinimumSpecialChars
        {
            get { return minSpecialChars; }
            set
            {
                if (value >= minLength) throw new ArgumentException("Minimum special characters must be less than the minimum length");
                minSpecialChars = value;
            }
        }

        public int MinimumUpperCase
        {
            get { return minUpperCase; }
            set
            {
                if (value >= minLength) throw new ArgumentException("Minimum uppercase characters must be less than the minimum length");
                minUpperCase = value;
            }
        }

        public int MinimumDigits
        {
            get { return minDigits; }
            set
            {
                if (value >= minLength) throw new ArgumentException("Minimum digits must be less than the minimum length");
                minDigits = value;
            }
        }

        public Task<IdentityResult> ValidateAsync(string item)
        {
            try
            {
                var validationErrors = new Stack<string>();
                if (item.Length < MinimumLength) validationErrors.Push(string.Format("Password must be at least {0} characters long", MinimumLength));
                if (item.Length > MaximumLength) validationErrors.Push(string.Format("Password must not be longer than {0} characters", MaximumLength));

                if (MinimumDigits > 0)
                {
                    var digits = item.ToCharArray().Count(Char.IsDigit);
                    if (digits < MinimumDigits) validationErrors.Push(string.Format("Password must contain at least {0} digits", MinimumDigits));
                }

                if (MinimumUpperCase > 0)
                {
                    var regUpperCase = new Regex("[A-Z]");
                    var upperCase = regUpperCase.Matches(item).Count;
                    if (upperCase < MinimumUpperCase) validationErrors.Push(string.Format("Password must contain at least {0} uppercase (A-Z) characters", MinimumUpperCase));
                }
                if (MinimumSpecialChars > 0)
                {
                    var specialchars = item.ToCharArray().Count(c => !Char.IsLetterOrDigit(c));
                    if (specialchars < MinimumSpecialChars) validationErrors.Push(string.Format("Password must contain at least {0} special characters", MinimumSpecialChars));
                }

                if (validationErrors.Count > 0)
                    return Task.FromResult(new IdentityResult(validationErrors));

                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new IdentityResult(ex.GetBaseException().Message));
            }
        }
    }
}