namespace Cdm.Common.Services;

using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IEmailService
{
    Task<bool> SendInvitationEmailAsync(string toEmail, string campaignName, string inviterName, string invitationLink, string? message = null);
    Task<bool> SendInvitationAcceptedEmailAsync(string toEmail, string campaignName, string playerName);
    Task<bool> SendInvitationRejectedEmailAsync(string toEmail, string campaignName, string playerName);

    /// <summary>
    /// Envoie le lien de réinitialisation de mot de passe.
    /// </summary>
    /// <param name="toEmail">Adresse du destinataire.</param>
    /// <param name="nickname">Pseudo affiché dans le message.</param>
    /// <param name="resetLink">Lien complet de réinitialisation (jeton inclus).</param>
    /// <param name="validityHours">Durée de validité annoncée, en heures.</param>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string nickname, string resetLink, int validityHours);

    /// <summary>
    /// Envoie le lien de confirmation d'adresse email.
    /// </summary>
    /// <param name="toEmail">Adresse du destinataire.</param>
    /// <param name="nickname">Pseudo affiché dans le message.</param>
    /// <param name="confirmLink">Lien complet de confirmation (jeton inclus).</param>
    /// <param name="validityHours">Durée de validité annoncée, en heures.</param>
    Task<bool> SendEmailConfirmationAsync(string toEmail, string nickname, string confirmLink, int validityHours);
}

/// <summary>
/// Implémentation de repli utilisée lorsque l'envoi d'emails n'est pas configuré
/// (typiquement en développement local) : rien n'est envoyé, tout est journalisé.
/// Cela permet de dérouler le parcours « mot de passe oublié » sans compte Azure —
/// le lien de réinitialisation apparaît dans les logs de l'API.
/// </summary>
public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger)
    {
        this.logger = logger;
    }

    public Task<bool> SendInvitationEmailAsync(string toEmail, string campaignName, string inviterName, string invitationLink, string? message = null)
    {
        this.logger.LogInformation(
            "[Email non configuré] Invitation à '{Campaign}' pour {ToEmail} — lien : {Link}",
            campaignName, toEmail, invitationLink);
        return Task.FromResult(true);
    }

    public Task<bool> SendInvitationAcceptedEmailAsync(string toEmail, string campaignName, string playerName)
    {
        this.logger.LogInformation(
            "[Email non configuré] {Player} a accepté l'invitation à '{Campaign}' ({ToEmail})",
            playerName, campaignName, toEmail);
        return Task.FromResult(true);
    }

    public Task<bool> SendInvitationRejectedEmailAsync(string toEmail, string campaignName, string playerName)
    {
        this.logger.LogInformation(
            "[Email non configuré] {Player} a refusé l'invitation à '{Campaign}' ({ToEmail})",
            playerName, campaignName, toEmail);
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordResetEmailAsync(string toEmail, string nickname, string resetLink, int validityHours)
    {
        this.logger.LogWarning(
            "[Email non configuré] Réinitialisation de mot de passe pour {ToEmail}. " +
            "Ouvrez ce lien manuellement (valable {Hours} h) : {Link}",
            toEmail, validityHours, resetLink);
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailConfirmationAsync(string toEmail, string nickname, string confirmLink, int validityHours)
    {
        this.logger.LogWarning(
            "[Email non configuré] Confirmation d'adresse pour {ToEmail}. " +
            "Ouvrez ce lien manuellement (valable {Hours} h) : {Link}",
            toEmail, validityHours, confirmLink);
        return Task.FromResult(true);
    }
}

public class AzureEmailService : IEmailService
{
    private readonly EmailClient emailClient;
    private readonly string fromAddress;
    private readonly ILogger<AzureEmailService> logger;

    public AzureEmailService(IConfiguration configuration, ILogger<AzureEmailService> logger)
    {
        this.logger = logger;

        var connectionString = configuration["AzureEmail:ConnectionString"];
        this.fromAddress = configuration["AzureEmail:FromAddress"] ?? "noreply@chroniquedesmondes.com";

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("AzureEmail:ConnectionString is required in configuration");
        }

        this.emailClient = new EmailClient(connectionString);
    }

    public async Task<bool> SendInvitationEmailAsync(string toEmail, string campaignName, string inviterName, string invitationLink, string? message = null)
    {
        var subject = $"Invitation à rejoindre la campagne {campaignName}";
        var htmlBody = $@"
            <html>
                <body>
                    <h2>Invitation à une campagne</h2>
                    <p>Bonjour,</p>
                    <p><strong>{inviterName}</strong> vous invite à rejoindre la campagne <strong>{campaignName}</strong>.</p>
                    <p><a href='{invitationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Accepter/Refuser l'invitation</a></p>
                    {(string.IsNullOrEmpty(message) ? "" : $"<p><em>Message personnel :</em><br>{message}</p>")}
                    <p>Cette invitation expirera dans 7 jours.</p>
                    <hr>
                    <p><small>Chronique des Mondes - Système de gestion de campagnes</small></p>
                </body>
            </html>";

        return await this.SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendInvitationAcceptedEmailAsync(string toEmail, string campaignName, string playerName)
    {
        var subject = $"Invitation acceptée - {campaignName}";
        var htmlBody = $@"
            <html>
                <body>
                    <h2>Invitation acceptée</h2>
                    <p>Bonjour,</p>
                    <p><strong>{playerName}</strong> a accepté votre invitation à rejoindre la campagne <strong>{campaignName}</strong>.</p>
                    <p>Vous pouvez maintenant commencer à organiser vos sessions de jeu !</p>
                    <hr>
                    <p><small>Chronique des Mondes - Système de gestion de campagnes</small></p>
                </body>
            </html>";

        return await this.SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendInvitationRejectedEmailAsync(string toEmail, string campaignName, string playerName)
    {
        var subject = $"Invitation refusée - {campaignName}";
        var htmlBody = $@"
            <html>
                <body>
                    <h2>Invitation refusée</h2>
                    <p>Bonjour,</p>
                    <p><strong>{playerName}</strong> a refusé votre invitation à rejoindre la campagne <strong>{campaignName}</strong>.</p>
                    <p>Vous pouvez réessayer plus tard ou inviter d'autres joueurs.</p>
                    <hr>
                    <p><small>Chronique des Mondes - Système de gestion de campagnes</small></p>
                </body>
            </html>";

        return await this.SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string nickname, string resetLink, int validityHours)
    {
        var subject = "Réinitialisation de votre mot de passe";
        var htmlBody = $@"
            <html>
                <body>
                    <h2>Réinitialisation de mot de passe</h2>
                    <p>Bonjour {nickname},</p>
                    <p>Vous avez demandé à réinitialiser votre mot de passe. Cliquez sur le bouton ci-dessous pour en choisir un nouveau :</p>
                    <p><a href='{resetLink}' style='background-color: #4f46e5; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Choisir un nouveau mot de passe</a></p>
                    <p>Ce lien est valable {validityHours} heure(s) et ne peut être utilisé qu'une seule fois.</p>
                    <p>Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet email : votre mot de passe actuel reste inchangé.</p>
                    <hr>
                    <p><small>Chronique des Mondes</small></p>
                </body>
            </html>";

        return await this.SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailConfirmationAsync(string toEmail, string nickname, string confirmLink, int validityHours)
    {
        var subject = "Confirmez votre adresse email";
        var htmlBody = $@"
            <html>
                <body>
                    <h2>Bienvenue sur Chronique des Mondes !</h2>
                    <p>Bonjour {nickname},</p>
                    <p>Merci de confirmer votre adresse email en cliquant sur le bouton ci-dessous :</p>
                    <p><a href='{confirmLink}' style='background-color: #4f46e5; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Confirmer mon adresse</a></p>
                    <p>Ce lien est valable {validityHours} heure(s).</p>
                    <p>Si vous n'êtes pas à l'origine de cette inscription, vous pouvez ignorer cet email.</p>
                    <hr>
                    <p><small>Chronique des Mondes</small></p>
                </body>
            </html>";

        return await this.SendEmailAsync(toEmail, subject, htmlBody);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var emailContent = new EmailContent(subject)
            {
                Html = htmlBody
            };

            var emailMessage = new EmailMessage(this.fromAddress, toEmail, emailContent);

            this.logger.LogInformation("📧 Envoi d'email vers {ToEmail} avec le sujet: {Subject}", toEmail, subject);

            var emailSendOperation = await this.emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            if (emailSendOperation.HasCompleted)
            {
                this.logger.LogInformation("✅ Email envoyé avec succès vers {ToEmail}", toEmail);
                return true;
            }
            else
            {
                this.logger.LogWarning("⚠️ Email en cours d'envoi vers {ToEmail}", toEmail);
                return true; // Considéré comme réussi même si en cours
            }
        }
        catch (RequestFailedException ex)
        {
            this.logger.LogError(ex, "❌ Erreur Azure lors de l'envoi d'email vers {ToEmail}: {ErrorCode} - {ErrorMessage}",
                toEmail, ex.ErrorCode, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "❌ Erreur générale lors de l'envoi d'email vers {ToEmail}", toEmail);
            return false;
        }
    }
}
