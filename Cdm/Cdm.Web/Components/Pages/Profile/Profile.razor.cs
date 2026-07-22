using Cdm.Business.Abstraction.DTOs;
using Cdm.Business.Abstraction.DTOs.Models;
using Cdm.Business.Abstraction.DTOs.ViewModels;
using Cdm.Common.Services;
using Cdm.Web.Resources;
using Cdm.Web.Services;
using Cdm.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Cdm.Web.Components.Pages.Profile;

public partial class Profile
{
    [Inject] private ProfileApiClient ProfileClient { get; set; } = default!;
    [Inject] private AchievementApiClient AchievementClient { get; set; } = default!;
    [Inject] private StatisticsApiClient StatisticsClient { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private ProfileResponse? UserProfile;
    private UpdateProfileRequest UpdateModel { get; set; } = new();
    private List<UserAchievementDto> MyAchievements { get; set; } = new();
    private DiceStatsDto? DiceStats;
    private ParticipationStatsDto? ParticipationStats;
    private bool IsLoading = true;
    private bool IsSaving = false;
    private bool IsUploadingAvatar = false;

    /// <summary>Limites de la photo de profil — partagées avec l'API via ImagePolicy.</summary>
    private static readonly ImagePolicy AvatarPolicy = ImagePolicy.For("avatars");
    private string? AvatarError;
    private string ErrorMessage = string.Empty;
    private string SuccessMessage = string.Empty;
    private string UserName = string.Empty;
    private string UserInitials = "?";

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        UserName = authState.User.Claims
            .FirstOrDefault(c => c.Type == "nickname")?.Value
            ?? authState.User.Identity?.Name
            ?? string.Empty;
        UserInitials = BuildInitials(UserName);

        try
        {
            UserProfile = await ProfileClient.GetProfileAsync();
            if (UserProfile != null)
            {
                UpdateModel = new UpdateProfileRequest { Username = UserProfile.Username };
            }

            MyAchievements = await AchievementClient.GetMyAchievementsAsync();
            DiceStats = await StatisticsClient.GetMyDiceStatsAsync();
            ParticipationStats = await StatisticsClient.GetMyParticipationStatsAsync();
        }
        catch { /* ignore */ }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSave()
    {
        IsSaving = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        try
        {
            var result = await ProfileClient.UpdateProfileAsync(UpdateModel);
            if (result != null)
            {
                UserProfile = result;
                SuccessMessage = L["Profile_SaveSuccess"];
            }
            else
            {
                ErrorMessage = L["Profile_SaveError"];
            }
        }
        catch
        {
            ErrorMessage = L["Profile_SaveError"];
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UploadAvatar(InputFileChangeEventArgs e)
    {
        AvatarError = null;
        var file = e.File;
        if (file is null)
        {
            return;
        }

        // Mêmes limites que côté serveur (ImagePolicy), validées ici pour un retour immédiat.
        if (file.Size > AvatarPolicy.MaxFileSizeBytes)
        {
            AvatarError = AvatarPolicy.TooHeavyMessage();
            return;
        }

        IsUploadingAvatar = true;
        try
        {
            var url = await ProfileClient.UploadAvatarAsync(file);
            if (!string.IsNullOrEmpty(url) && UserProfile != null)
            {
                UserProfile.AvatarUrl = url;
                SuccessMessage = L["Profile_SaveSuccess"];
            }
            else
            {
                AvatarError = "Échec de l'envoi de la photo.";
            }
        }
        catch
        {
            AvatarError = "Échec de l'envoi de la photo.";
        }
        finally
        {
            IsUploadingAvatar = false;
        }
    }

    private static string RarityLabel(Cdm.Common.Enums.AchievementRarity rarity) => rarity switch
    {
        Cdm.Common.Enums.AchievementRarity.Rare => "Rare",
        Cdm.Common.Enums.AchievementRarity.Epic => "Épique",
        Cdm.Common.Enums.AchievementRarity.Legendary => "Légendaire",
        _ => "Commun"
    };

    private static string RarityColor(Cdm.Common.Enums.AchievementRarity rarity) => rarity switch
    {
        Cdm.Common.Enums.AchievementRarity.Rare => "#3b82f6",
        Cdm.Common.Enums.AchievementRarity.Epic => "#a855f7",
        Cdm.Common.Enums.AchievementRarity.Legendary => "#f59e0b",
        _ => "#9ca3af"
    };

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
