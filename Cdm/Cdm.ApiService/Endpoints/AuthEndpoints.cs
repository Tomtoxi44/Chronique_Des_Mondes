namespace Cdm.ApiService.Endpoints;

using Microsoft.AspNetCore.Identity.Data;

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Endpoints d'authentification pour l'API
/// </summary>
public static class AuthEndpoints
{

    /// <summary>
    /// Validation simple d'un email
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}