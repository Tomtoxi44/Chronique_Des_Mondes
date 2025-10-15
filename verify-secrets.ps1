# ==========================================
# 🔍 Script de Vérification des Secrets
# Chronique des Mondes - Audit de Sécurité
# ==========================================
#
# Ce script vérifie qu'aucun secret n'a été committé
# dans le repository avant de le rendre public.
#
# ==========================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  🔍 Audit de Sécurité" -ForegroundColor Cyan
Write-Host "  Chronique des Mondes" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$issuesFound = 0

# ==========================================
# 1. Vérifier les fichiers sensibles dans le repo
# ==========================================

Write-Host "📂 Vérification des fichiers sensibles..." -ForegroundColor Yellow
Write-Host ""

$sensitiveFiles = @(
    ".env",
    ".env.local",
    ".env.*.local",
    "secrets.json",
    "*.user.secrets.json",
    "appsettings.*.local.json"
)

foreach ($pattern in $sensitiveFiles) {
    $found = Get-ChildItem -Path . -Filter $pattern -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }
    
    if ($found) {
        Write-Host "   ❌ ATTENTION : Fichier sensible trouvé : $($found.Name)" -ForegroundColor Red
        Write-Host "      Chemin : $($found.FullName)" -ForegroundColor Gray
        $issuesFound++
    }
}

if ($issuesFound -eq 0) {
    Write-Host "   ✅ Aucun fichier sensible trouvé dans le repo" -ForegroundColor Green
}

Write-Host ""

# ==========================================
# 2. Vérifier les patterns de secrets dans les fichiers
# ==========================================

Write-Host "🔎 Recherche de patterns de secrets dans les fichiers..." -ForegroundColor Yellow
Write-Host ""

$secretPatterns = @(
    @{ Pattern = "password\s*=\s*['\`"][^'\`"]{3,}"; Description = "Mot de passe en clair" },
    @{ Pattern = "connectionstring.*password="; Description = "Connection string avec mot de passe" },
    @{ Pattern = "secretkey\s*=\s*['\`"][^'\`"]{10,}"; Description = "Clé secrète JWT" },
    @{ Pattern = "accesskey\s*=\s*[^;]{20,}"; Description = "Clé d'accès Azure" },
    @{ Pattern = "Server=.*;.*Password=.*[^;]{5,}"; Description = "SQL Server password" }
)

$filesToCheck = Get-ChildItem -Path . -Include "*.cs","*.json","*.config","*.xml" -Recurse -ErrorAction SilentlyContinue | Where-Object { 
    $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\.git\\" 
}

foreach ($file in $filesToCheck) {
    $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
    
    if ($content) {
        foreach ($secretPattern in $secretPatterns) {
            if ($content -imatch $secretPattern.Pattern) {
                Write-Host "   ⚠️  Pattern suspect trouvé : $($secretPattern.Description)" -ForegroundColor Yellow
                Write-Host "      Fichier : $($file.FullName)" -ForegroundColor Gray
                Write-Host "      Vérifiez manuellement ce fichier" -ForegroundColor Gray
                $issuesFound++
            }
        }
    }
}

if ($issuesFound -eq 0) {
    Write-Host "   ✅ Aucun pattern de secret détecté" -ForegroundColor Green
}

Write-Host ""

# ==========================================
# 3. Vérifier l'historique Git
# ==========================================

Write-Host "📜 Vérification de l'historique Git..." -ForegroundColor Yellow
Write-Host ""

$gitInstalled = Get-Command git -ErrorAction SilentlyContinue

if ($gitInstalled) {
    # Vérifier les commits avec des fichiers sensibles
    $sensitiveCommits = git log --all --full-history --source -- "*secrets*" "*appsettings*.local.json" "*.env" 2>&1
    
    if ($sensitiveCommits -and $sensitiveCommits.Count -gt 0) {
        Write-Host "   ⚠️  Des fichiers sensibles ont été trouvés dans l'historique Git" -ForegroundColor Yellow
        Write-Host "      Vous devriez nettoyer l'historique avec BFG Repo-Cleaner" -ForegroundColor Gray
        $issuesFound++
    }
    else {
        Write-Host "   ✅ Aucun fichier sensible dans l'historique Git" -ForegroundColor Green
    }
}
else {
    Write-Host "   ⚠️  Git n'est pas installé, impossible de vérifier l'historique" -ForegroundColor Yellow
}

Write-Host ""

# ==========================================
# 4. Vérifier le .gitignore
# ==========================================

Write-Host "📝 Vérification du .gitignore..." -ForegroundColor Yellow
Write-Host ""

if (Test-Path ".gitignore") {
    $gitignoreContent = Get-Content ".gitignore" -Raw
    
    $requiredPatterns = @(
        ".env",
        "*.user.secrets.json",
        "secrets.json",
        "appsettings.*.local.json"
    )
    
    $missingPatterns = @()
    
    foreach ($pattern in $requiredPatterns) {
        if ($gitignoreContent -notmatch [regex]::Escape($pattern)) {
            $missingPatterns += $pattern
        }
    }
    
    if ($missingPatterns.Count -gt 0) {
        Write-Host "   ⚠️  Patterns manquants dans .gitignore :" -ForegroundColor Yellow
        foreach ($missing in $missingPatterns) {
            Write-Host "      - $missing" -ForegroundColor Gray
        }
        $issuesFound++
    }
    else {
        Write-Host "   ✅ .gitignore correctement configuré" -ForegroundColor Green
    }
}
else {
    Write-Host "   ❌ .gitignore introuvable !" -ForegroundColor Red
    $issuesFound++
}

Write-Host ""

# ==========================================
# 5. Vérifier les appsettings.json
# ==========================================

Write-Host "⚙️  Vérification des appsettings.json..." -ForegroundColor Yellow
Write-Host ""

$appsettingsFiles = Get-ChildItem -Path . -Filter "appsettings*.json" -Recurse -ErrorAction SilentlyContinue | Where-Object { 
    $_.FullName -notmatch "\\bin\\|\\obj\\" -and $_.Name -notmatch "\.local\." 
}

foreach ($appsettings in $appsettingsFiles) {
    $content = Get-Content -Path $appsettings.FullName -Raw -ErrorAction SilentlyContinue
    
    if ($content) {
        # Vérifier qu'il n'y a pas de vraies valeurs de connexion
        if ($content -imatch "Password=(?!YOUR_|<|{|\*)[^;]{5,}") {
            Write-Host "   ⚠️  Mot de passe potentiel dans : $($appsettings.Name)" -ForegroundColor Yellow
            Write-Host "      Chemin : $($appsettings.FullName)" -ForegroundColor Gray
            $issuesFound++
        }
        
        if ($content -imatch "SecretKey.*['\`"]\w{32,}['\`"]") {
            Write-Host "   ⚠️  Clé secrète potentielle dans : $($appsettings.Name)" -ForegroundColor Yellow
            Write-Host "      Chemin : $($appsettings.FullName)" -ForegroundColor Gray
            $issuesFound++
        }
    }
}

if ($issuesFound -eq 0) {
    Write-Host "   ✅ appsettings.json semblent propres" -ForegroundColor Green
}

Write-Host ""

# ==========================================
# 6. Résumé et recommandations
# ==========================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  📊 Résumé de l'Audit" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

if ($issuesFound -eq 0) {
    Write-Host "✅ Aucun problème détecté !" -ForegroundColor Green
    Write-Host ""
    Write-Host "Votre repository semble prêt à être rendu public." -ForegroundColor Green
    Write-Host ""
    Write-Host "Dernières vérifications recommandées :" -ForegroundColor Yellow
    Write-Host "  1. Relire manuellement les appsettings.json" -ForegroundColor White
    Write-Host "  2. Vérifier que User Secrets est configuré localement" -ForegroundColor White
    Write-Host "  3. Tester le clone du repo sur une autre machine" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Host "⚠️  $issuesFound problème(s) potentiel(s) détecté(s)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Actions recommandées :" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Corriger les problèmes listés ci-dessus" -ForegroundColor White
    Write-Host "2. Si des secrets ont été committés par erreur :" -ForegroundColor White
    Write-Host "   - Utiliser BFG Repo-Cleaner pour nettoyer l'historique" -ForegroundColor Gray
    Write-Host "   - Ou recréer le repository depuis un état propre" -ForegroundColor Gray
    Write-Host "3. Re-exécuter ce script pour vérifier" -ForegroundColor White
    Write-Host ""
    Write-Host "⚠️  NE PAS rendre le repo public tant que ces problèmes ne sont pas résolus !" -ForegroundColor Red
    Write-Host ""
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Retourner un code d'erreur si des problèmes ont été trouvés
if ($issuesFound -gt 0) {
    exit 1
}
