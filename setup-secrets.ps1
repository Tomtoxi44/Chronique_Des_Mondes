# ==========================================
# 🔧 Script de Configuration des Secrets
# Chronique des Mondes - Setup Automatique
# ==========================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Chronique des Mondes - Setup" -ForegroundColor Cyan
Write-Host "  Configuration des Secrets" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier que nous sommes dans le bon répertoire
if (-not (Test-Path "Cdm/Cdm.Common/Cdm.Common.csproj")) {
    Write-Host "❌ Erreur : Ce script doit être exécuté depuis la racine du projet." -ForegroundColor Red
    Write-Host "   Répertoire actuel : $(Get-Location)" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Répertoire du projet détecté" -ForegroundColor Green
Write-Host ""

# ==========================================
# 1. Générer JWT Secret Key
# ==========================================

Write-Host "🔐 Génération de la clé JWT..." -ForegroundColor Yellow

$jwtSecretKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

Write-Host "✅ Clé JWT générée : " -ForegroundColor Green -NoNewline
Write-Host $jwtSecretKey.Substring(0, 20) -ForegroundColor Gray -NoNewline
Write-Host "..." -ForegroundColor Gray
Write-Host ""

# ==========================================
# 2. Configuration SQL Server
# ==========================================

Write-Host "📊 Configuration de la base de données SQL Server" -ForegroundColor Yellow
Write-Host ""
Write-Host "Choisissez le type d'authentification :" -ForegroundColor Cyan
Write-Host "  1) Authentification Windows (Trusted_Connection)" -ForegroundColor White
Write-Host "  2) SQL Server Authentication (login/password)" -ForegroundColor White
Write-Host ""

$authChoice = Read-Host "Votre choix (1 ou 2)"

if ($authChoice -eq "1") {
    # Authentification Windows
    $connectionString = "Server=localhost;Database=ChroniqueDesMondes;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
    Write-Host "✅ Authentification Windows configurée" -ForegroundColor Green
}
elseif ($authChoice -eq "2") {
    # SQL Server Authentication
    Write-Host ""
    $sqlServer = Read-Host "Serveur SQL (défaut: localhost)"
    if ([string]::IsNullOrWhiteSpace($sqlServer)) { $sqlServer = "localhost" }
    
    $sqlPort = Read-Host "Port SQL (défaut: 1433)"
    if ([string]::IsNullOrWhiteSpace($sqlPort)) { $sqlPort = "1433" }
    
    $sqlUser = Read-Host "Nom d'utilisateur SQL (défaut: sa)"
    if ([string]::IsNullOrWhiteSpace($sqlUser)) { $sqlUser = "sa" }
    
    $sqlPassword = Read-Host "Mot de passe SQL" -AsSecureString
    $sqlPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPassword)
    )
    
    $connectionString = "Server=$sqlServer,$sqlPort;Database=ChroniqueDesMondes;User Id=$sqlUser;Password=$sqlPasswordPlain;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True;"
    Write-Host "✅ Authentification SQL Server configurée" -ForegroundColor Green
}
else {
    Write-Host "❌ Choix invalide. Utilisation de l'authentification Windows par défaut." -ForegroundColor Red
    $connectionString = "Server=localhost;Database=ChroniqueDesMondes;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}

Write-Host ""

# ==========================================
# 3. Configuration Azure Communication Services (optionnel)
# ==========================================

Write-Host "📧 Configuration Azure Communication Services (optionnel)" -ForegroundColor Yellow
Write-Host "   Utilisé pour envoyer des emails d'invitation aux sessions" -ForegroundColor Gray
Write-Host ""
$configureAzure = Read-Host "Voulez-vous configurer Azure Communication Services maintenant ? (o/N)"

$azureConnectionString = ""
$azureSenderEmail = ""

if ($configureAzure -eq "o" -or $configureAzure -eq "O") {
    Write-Host ""
    $azureConnectionString = Read-Host "Connection String Azure Communication"
    $azureSenderEmail = Read-Host "Email de l'expéditeur (ex: noreply@votredomaine.com)"
    Write-Host "✅ Azure Communication Services configuré" -ForegroundColor Green
}
else {
    Write-Host "⏭️  Configuration Azure ignorée (vous pourrez la configurer plus tard)" -ForegroundColor Yellow
}

Write-Host ""

# ==========================================
# 4. Appliquer les User Secrets
# ==========================================

Write-Host "💾 Application des User Secrets au projet..." -ForegroundColor Yellow
Write-Host ""

Push-Location "Cdm/Cdm.Common"

try {
    # Initialiser User Secrets si nécessaire
    Write-Host "   Initialisation des User Secrets..." -ForegroundColor Gray
    dotnet user-secrets init 2>&1 | Out-Null
    
    # Connection String
    Write-Host "   ✓ Configuration de la connection string..." -ForegroundColor Gray
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString | Out-Null
    
    # JWT
    Write-Host "   ✓ Configuration JWT..." -ForegroundColor Gray
    dotnet user-secrets set "Jwt:SecretKey" $jwtSecretKey | Out-Null
    dotnet user-secrets set "Jwt:Issuer" "Cdm.ApiService" | Out-Null
    dotnet user-secrets set "Jwt:Audience" "Cdm.Web" | Out-Null
    dotnet user-secrets set "Jwt:ExpiryDays" "7" | Out-Null
    
    # Azure (si configuré)
    if (-not [string]::IsNullOrWhiteSpace($azureConnectionString)) {
        Write-Host "   ✓ Configuration Azure Communication Services..." -ForegroundColor Gray
        dotnet user-secrets set "AzureCommunication:ConnectionString" $azureConnectionString | Out-Null
        dotnet user-secrets set "AzureCommunication:SenderEmail" $azureSenderEmail | Out-Null
    }
    
    Write-Host ""
    Write-Host "✅ User Secrets configurés avec succès !" -ForegroundColor Green
    Write-Host ""
    
    # Afficher les secrets configurés (sans valeurs sensibles)
    Write-Host "📋 Résumé de la configuration :" -ForegroundColor Cyan
    Write-Host "   • Connection String: ***configurée***" -ForegroundColor White
    Write-Host "   • JWT Secret Key: ***configurée***" -ForegroundColor White
    Write-Host "   • JWT Issuer: Cdm.ApiService" -ForegroundColor White
    Write-Host "   • JWT Audience: Cdm.Web" -ForegroundColor White
    Write-Host "   • JWT Expiry: 7 jours" -ForegroundColor White
    
    if (-not [string]::IsNullOrWhiteSpace($azureConnectionString)) {
        Write-Host "   • Azure Communication: ***configurée***" -ForegroundColor White
        Write-Host "   • Sender Email: $azureSenderEmail" -ForegroundColor White
    }
    
    Write-Host ""
    
    # Afficher le chemin du fichier secrets.json
    Write-Host "📁 Emplacement des secrets :" -ForegroundColor Cyan
    $secretsPath = dotnet user-secrets list --verbose 2>&1 | Select-String "UserSecretsId:" | ForEach-Object { $_.ToString() -replace "UserSecretsId: ", "" }
    if ($secretsPath) {
        $fullPath = "$env:APPDATA\Microsoft\UserSecrets\$secretsPath\secrets.json"
        Write-Host "   $fullPath" -ForegroundColor Gray
    }
    
}
catch {
    Write-Host ""
    Write-Host "❌ Erreur lors de la configuration des secrets" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Pop-Location
    exit 1
}
finally {
    Pop-Location
}

Write-Host ""

# ==========================================
# 5. Prochaines étapes
# ==========================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  🎉 Configuration terminée !" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Prochaines étapes :" -ForegroundColor Yellow
Write-Host ""
Write-Host "1️⃣  Créer la base de données :" -ForegroundColor Cyan
Write-Host "    cd Cdm/Cdm.Migrations" -ForegroundColor White
Write-Host "    dotnet ef database update --context MigrationsContext" -ForegroundColor White
Write-Host ""
Write-Host "2️⃣  Lancer l'application :" -ForegroundColor Cyan
Write-Host "    dotnet run --project Cdm/Cdm.AppHost" -ForegroundColor White
Write-Host ""
Write-Host "3️⃣  Accéder au dashboard Aspire :" -ForegroundColor Cyan
Write-Host "    https://localhost:17223" -ForegroundColor White
Write-Host ""
Write-Host "📚 Pour plus d'informations, consultez CONFIGURATION.md" -ForegroundColor Gray
Write-Host ""

# ==========================================
# 6. Demander si on lance la création de la DB
# ==========================================

$createDb = Read-Host "Voulez-vous créer la base de données maintenant ? (o/N)"

if ($createDb -eq "o" -or $createDb -eq "O") {
    Write-Host ""
    Write-Host "📊 Création de la base de données..." -ForegroundColor Yellow
    Write-Host ""
    
    Push-Location "Cdm/Cdm.Migrations"
    
    try {
        dotnet ef database update --context MigrationsContext
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Base de données créée avec succès !" -ForegroundColor Green
        }
        else {
            Write-Host ""
            Write-Host "❌ Erreur lors de la création de la base de données" -ForegroundColor Red
            Write-Host "   Vérifiez que SQL Server est démarré et accessible" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host ""
        Write-Host "❌ Erreur : $_" -ForegroundColor Red
    }
    finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "✨ Setup terminé ! Bon développement ! ✨" -ForegroundColor Green
Write-Host ""
