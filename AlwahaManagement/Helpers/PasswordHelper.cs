namespace AlwahaManagement.Helpers;

public static class PasswordHelper
{
    public static string GenerateStrongPassword(int length)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@$?_-";
        const string all = upper + lower + digits + symbols;

        var buffer = length <= 128 ? stackalloc char[length] : new char[length];
        var idx = 0;

        buffer[idx++] = GetRandomChar(upper);
        buffer[idx++] = GetRandomChar(lower);
        buffer[idx++] = GetRandomChar(digits);
        buffer[idx++] = GetRandomChar(symbols);

        for (; idx < length; idx++)
        {
            buffer[idx] = GetRandomChar(all);
        }

        for (var i = buffer.Length - 1; i > 0; i--)
        {
            var j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }

        return new string(buffer);
    }
    
    private static char GetRandomChar(string set)
    {
        var k = System.Security.Cryptography.RandomNumberGenerator.GetInt32(set.Length);
        return set[k];
    }
}