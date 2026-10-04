namespace Contra_Filé.Domain.Helpers;

public static class HashHelper
{
    public static string Hash(string newPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(newPassword);
    }
    public static bool Verify(string rawPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
    }
}