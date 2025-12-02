using System.Security.Cryptography;
using System.Text;

namespace Service.Utils;

public class RandomUtils
{
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*()-_=+[]{}|;:,.<>?/";
    
    public static string GenerateOtp()
    {
        var rng = Random.Shared.Next(100000, 999999);
        return rng.ToString();
    }
    
    public static string GeneratePassword(
        int length = 12,
        bool useLowercase = true,
        bool useUppercase = true,
        bool useDigits = true,
        bool useSpecial = true)
    {
        if (length <= 0)
            throw new ArgumentException("Password length must be greater than zero.");

        var characterPool = new StringBuilder();
        if (useLowercase) characterPool.Append(Lowercase);
        if (useUppercase) characterPool.Append(Uppercase);
        if (useDigits) characterPool.Append(Digits);
        if (useSpecial) characterPool.Append(Special);

        if (characterPool.Length == 0)
            throw new ArgumentException("At least one character set must be enabled.");

        var passwordChars = new char[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[length];

            rng.GetBytes(bytes);

            for (var i = 0; i < length; i++)
            {
                var idx = bytes[i] % characterPool.Length;
                passwordChars[i] = characterPool[idx];
            }
        }

        return new string(passwordChars);
    }
}