# US-003 - Gestion des Tokens JWT

## 📝 Description

**En tant que** développeur backend  
**Je veux** un système complet de génération et validation des tokens JWT  
**Afin de** sécuriser l'accès aux ressources protégées de l'API

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [x] Génération de JWT lors de la connexion réussie
- [x] Token contient : userId, email, roles, expiration
- [x] Signature HMAC-SHA256 avec clé secrète
- [x] Expiration configurable (défaut : 7 jours)
- [x] Validation automatique du token sur endpoints protégés
- [x] Vérification de la signature et de l'expiration
- [x] Middleware d'authentification JWT global
- [x] Extraction des claims utilisateur depuis le token
- [x] Gestion des erreurs : token expiré, invalide, manquant

### Techniques
- [x] Configuration JWT dans appsettings.json (SecretKey, Issuer, Audience, ExpiryDays)
- [x] Service `JwtService` avec méthodes :
  - [x] `GenerateToken(User user)` → string
  - [x] `ValidateToken(string token)` → ClaimsPrincipal
  - [x] `GetUserIdFromToken(string token)` → Guid
- [x] Middleware `UseAuthentication()` et `UseAuthorization()`
- [x] Attribut `[Authorize]` sur endpoints protégés
- [x] Response 401 : Token manquant ou invalide
- [x] Response 403 : Token valide mais rôle insuffisant

---

## 🧪 Tests

### Tests Unitaires
- [x] `JwtService.GenerateToken_WithValidUser_ReturnsValidToken()`
- [x] `JwtService.ValidateToken_WithValidToken_ReturnsClaimsPrincipal()`
- [x] `JwtService.ValidateToken_WithExpiredToken_ThrowsException()`
- [x] `JwtService.ValidateToken_WithInvalidSignature_ThrowsException()`
- [x] `JwtService.GetUserIdFromToken_WithValidToken_ReturnsUserId()`

### Tests d'Intégration
- [x] `ProtectedEndpoint_WithValidToken_Returns200()`
- [x] `ProtectedEndpoint_WithoutToken_Returns401()`
- [x] `ProtectedEndpoint_WithExpiredToken_Returns401()`
- [x] `ProtectedEndpoint_WithInvalidToken_Returns401()`

### Tests E2E
- [x] Login → Récupération token → Accès endpoint protégé → Succès
- [x] Accès endpoint protégé sans token → Erreur 401

---

## 🔧 Tâches Techniques

### Backend
- [x] Installer package `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] Installer package `System.IdentityModel.Tokens.Jwt`
- [x] Créer `JwtService` dans `Cdm.Common/Services/`
  - [x] Méthode `GenerateToken(User user)`
  - [x] Méthode `ValidateToken(string token)`
  - [x] Méthode `GetUserIdFromToken(string token)`
- [x] Configurer JWT dans `Program.cs` :
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
          options.TokenValidationParameters = new TokenValidationParameters {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = config["JWT:Issuer"],
              ValidAudience = config["JWT:Audience"],
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(config["JWT:SecretKey"])
              )
          };
      });
  ```
- [x] Ajouter `app.UseAuthentication()` et `app.UseAuthorization()`
- [x] Ajouter `[Authorize]` sur endpoints protégés
- [x] Gérer erreurs 401/403 avec messages clairs

### Configuration User Secrets
- [x] Ajouter clé JWT dans User Secrets :
  ```json
  {
    "Authentication": {
      "JWT": {
        "SecretKey": "votre-clé-secrète-minimum-64-caractères-pour-sécurité",
        "Issuer": "ChroniqueMonde",
        "Audience": "ChroniqueMonde-Users",
        "ExpiryDays": 7
      }
    }
  }
  ```

### Frontend
- [x] Stocker token dans localStorage après login
- [x] Ajouter token dans header `Authorization: Bearer {token}`
- [x] Intercepteur HTTP pour ajout automatique du header
- [x] Gestion erreur 401 : redirection vers login

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription
- [US-002](./US-002-connexion-utilisateur.md) - Connexion

### Bloque
- [US-004](./US-004-profil-utilisateur.md) - Profil
- [US-007](./US-007-refresh-token.md) - Refresh token
- Tous les endpoints protégés du projet

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne-Haute (sécurité critique)
- Effort : 1-2 jours
- Risques : Configuration sécurité, gestion expiration

---

## 📝 Notes Techniques

### Structure du JWT
```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "userId-guid",
  "email": "user@example.com",
  "role": "Player",
  "exp": 1729522800,
  "iss": "ChroniqueMonde",
  "aud": "ChroniqueMonde-Users"
}

Signature:
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

### Génération Clé Secrète
```powershell
# PowerShell - Générer clé 64 caractères
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

### Sécurité
- ⚠️ Clé secrète minimum 64 caractères
- ⚠️ Ne jamais committer la clé secrète
- ⚠️ Rotation périodique de la clé (tous les 6 mois)
- ⚠️ HTTPS obligatoire en production

---

## ✅ Definition of Done

- [x] Code implémenté et testé
- [x] Tests unitaires passent (couverture > 80%)
- [x] Tests d'intégration passent
- [x] Middleware JWT configuré
- [x] Endpoints protégés fonctionnels
- [x] Documentation API mise à jour
- [x] Mergé dans main

---

**Statut** : ✅ Terminé  
**Assigné à** : Tommy ANGIBAUD  
**Date de complétion** : 15 octobre 2025
