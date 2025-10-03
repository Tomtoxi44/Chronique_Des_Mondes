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
