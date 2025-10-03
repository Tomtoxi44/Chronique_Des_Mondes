namespace Cdm.Common.Services;

public class PasswordService
{
    public string HashPassword(string password)
    {
        // Pour l'instant, implémentation simple (à remplacer par bcrypt en production)
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
