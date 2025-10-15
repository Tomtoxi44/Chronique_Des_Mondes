# US-007 - Refresh Token JWT

## 📝 Description

**En tant qu'** utilisateur connecté  
**Je veux** que mon token soit renouvelé automatiquement avant expiration  
**Afin de** rester connecté sans avoir à me reconnecter fréquemment

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Token JWT a une durée de vie courte (15-60 minutes)
- [ ] Refresh token a une durée de vie longue (7-30 jours)
- [ ] Refresh token stocké de manière sécurisée (HttpOnly cookie ou DB)
- [ ] Endpoint de renouvellement accessible avec refresh token valide
- [ ] Nouveau JWT + nouveau refresh token générés (rotation)
- [ ] Ancien refresh token invalidé après utilisation
- [ ] Détection utilisation refresh token compromis (révocation en cascade)
- [ ] Limite nombre de refresh tokens actifs par utilisateur (max 5 appareils)
- [ ] Déconnexion automatique si refresh token expiré

### Techniques
- [ ] Endpoint : `POST /api/auth/refresh`
- [ ] Body : `{ "refreshToken": "long-token-here" }` OU cookie HttpOnly
- [ ] Response 200 : `{ "token": "new-jwt", "refreshToken": "new-refresh-token", "expiresAt": "..." }`
- [ ] Response 401 : Refresh token invalide/expiré
- [ ] Response 403 : Refresh token révoqué (activité suspecte)

---

## 🧪 Tests

### Tests Unitaires
- [ ] `JwtService.GenerateRefreshToken_ReturnsSecureToken()`
- [ ] `RefreshTokenService.ValidateRefreshToken_WithValidToken_ReturnsUser()`
- [ ] `RefreshTokenService.ValidateRefreshToken_WithExpiredToken_ThrowsException()`
- [ ] `RefreshTokenService.RevokeToken_WithValidToken_InvalidatesToken()`
- [ ] `RefreshTokenService.RevokeUserTokens_InvalidatesAllUserTokens()`

### Tests d'Intégration
- [ ] `RefreshEndpoint_WithValidRefreshToken_ReturnsNewTokens()`
- [ ] `RefreshEndpoint_WithExpiredRefreshToken_Returns401()`
- [ ] `RefreshEndpoint_ReuseOldRefreshToken_Returns403()`
- [ ] `RefreshEndpoint_TokenRotation_OldTokenInvalidated()`

### Tests E2E
- [ ] Login → Token expire → Refresh automatique → Accès maintenu
- [ ] Refresh token expiré → Redirection login
- [ ] Déconnexion → Tous refresh tokens révoqués

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer table `RefreshTokens`
  - [ ] Id (PK, Guid)
  - [ ] UserId (FK → Users)
  - [ ] Token (string, unique, hashed, index)
  - [ ] ExpiresAt (DateTime)
  - [ ] CreatedAt (DateTime)
  - [ ] RevokedAt (DateTime, nullable)
  - [ ] ReplacedByToken (string, nullable) - pour détection réutilisation
  - [ ] DeviceInfo (string, nullable) - User-Agent
- [ ] Créer `RefreshTokenService`
  - [ ] `GenerateRefreshTokenAsync(userId, deviceInfo)`
  - [ ] `ValidateRefreshTokenAsync(token)` → User
  - [ ] `RevokeTokenAsync(token, replacedByToken)`
  - [ ] `RevokeUserTokensAsync(userId)` - déconnexion globale
  - [ ] `CleanupExpiredTokensAsync()` - job de nettoyage
- [ ] Modifier `AuthService.LoginAsync()` :
  - [ ] Générer JWT (courte durée : 15min)
  - [ ] Générer refresh token (longue durée : 7j)
  - [ ] Retourner les deux
- [ ] Créer `AuthService.RefreshTokenAsync(refreshToken, deviceInfo)` :
  - [ ] Valider refresh token
  - [ ] Générer nouveau JWT
  - [ ] Générer nouveau refresh token (rotation)
  - [ ] Révoquer ancien refresh token
  - [ ] Si ancien token déjà révoqué → détection compromission → révoquer tous tokens utilisateur
- [ ] Créer endpoint `POST /api/auth/refresh`
- [ ] Créer endpoint `POST /api/auth/revoke` [Authorize] - déconnexion
- [ ] Job background pour nettoyer tokens expirés (quotidien)

### Frontend (Blazor)
- [ ] Stocker refresh token dans localStorage (ou cookie HttpOnly si possible)
- [ ] Intercepteur HTTP pour détecter 401
- [ ] Tentative refresh automatique si 401
- [ ] Si refresh échoue → déconnexion → redirection login
- [ ] Bouton "Déconnexion" → appel `/api/auth/revoke`
- [ ] Service `TokenRefreshService` :
  ```csharp
  public async Task<bool> TryRefreshTokenAsync()
  {
      var refreshToken = _localStorage.GetItem("refreshToken");
      var response = await _httpClient.PostAsync("/api/auth/refresh", 
          new { refreshToken });
      
      if (response.IsSuccessStatusCode)
      {
          var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
          _localStorage.SetItem("token", result.Token);
          _localStorage.SetItem("refreshToken", result.RefreshToken);
          return true;
      }
      
      return false;
  }
  ```

### Configuration
- [ ] Modifier durée JWT : 15 minutes (au lieu de 7 jours)
- [ ] Durée refresh token : 7 jours
- [ ] Max refresh tokens par user : 5

---

## 🔗 Dépendances

### Dépend de
- [US-002](./US-002-connexion-utilisateur.md) - Connexion
- [US-003](./US-003-gestion-jwt.md) - JWT

### Bloque
- Aucune (amélioration sécurité)

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (rotation tokens, sécurité)
- Effort : 1 jour
- Risques : Gestion révocation, détection compromission

---

## 📝 Notes Techniques

### Génération Refresh Token
```csharp
public string GenerateRefreshToken()
{
    var randomBytes = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
}
```

### Stockage Sécurisé
```csharp
// Hash du refresh token avant stockage DB
var tokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken, workFactor: 12);

// Vérification
var isValid = BCrypt.Net.BCrypt.Verify(providedToken, storedTokenHash);
```

### Détection Token Compromis
```
1. User A login → RT1 généré
2. Attaquant vole RT1
3. User A refresh → RT1 révoqué, RT2 généré
4. Attaquant tente refresh avec RT1
5. Système détecte RT1 déjà révoqué
6. → Compromission détectée
7. → Révocation de tous les tokens de User A
8. → Notification email User A
```

### Sécurité
- ⚠️ Refresh token doit être hashé en DB (comme mot de passe)
- ⚠️ Rotation obligatoire (nouveau refresh token à chaque refresh)
- ⚠️ Limite nombre de tokens actifs par user
- ⚠️ Logging de toutes les opérations refresh
- ⚠️ Notification email si révocation en cascade

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Rotation tokens fonctionnelle
- [ ] Détection compromission opérationnelle
- [ ] Job nettoyage configuré
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
