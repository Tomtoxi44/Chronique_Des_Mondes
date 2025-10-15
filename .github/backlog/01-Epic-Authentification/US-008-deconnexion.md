# US-008 - Déconnexion Sécurisée

## 📝 Description

**En tant qu'** utilisateur connecté  
**Je veux** pouvoir me déconnecter proprement de l'application  
**Afin de** sécuriser mon compte et invalider ma session

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bouton "Déconnexion" visible dans menu utilisateur
- [ ] Clic sur déconnexion → confirmation optionnelle
- [ ] Invalidation du token JWT côté serveur (si blacklist implémentée)
- [ ] Révocation du refresh token en base de données
- [ ] Suppression du token et refresh token côté client (localStorage)
- [ ] Redirection vers page de connexion
- [ ] Message de confirmation "Déconnexion réussie"
- [ ] État d'authentification mis à jour (Blazor AuthenticationStateProvider)
- [ ] Option "Déconnexion de tous les appareils" (révocation tous refresh tokens)

### Techniques
- [ ] Endpoint : `POST /api/auth/logout` [Authorize]
- [ ] Body : `{ "logoutAll": false }` (optionnel)
- [ ] Response 200 : `{ "message": "Déconnexion réussie" }`
- [ ] Révocation refresh token actuel
- [ ] Si `logoutAll = true` : révocation de tous les refresh tokens de l'utilisateur
- [ ] Suppression token côté client
- [ ] Mise à jour AuthenticationStateProvider

---

## 🧪 Tests

### Tests Unitaires
- [ ] `AuthService.Logout_WithValidRefreshToken_RevokesToken()`
- [ ] `AuthService.LogoutAll_RevokesAllUserTokens()`
- [ ] `AuthService.Logout_WithoutRefreshToken_Succeeds()` (déconnexion côté client uniquement)

### Tests d'Intégration
- [ ] `LogoutEndpoint_RevokesRefreshToken()`
- [ ] `LogoutEndpoint_WithLogoutAll_RevokesAllTokens()`
- [ ] `ProtectedEndpoint_AfterLogout_Returns401()`

### Tests E2E
- [ ] Login → Déconnexion → Token supprimé → Accès endpoint protégé → 401
- [ ] Déconnexion → Redirection login → Reconnexion OK
- [ ] Déconnexion tous appareils → Autres sessions invalidées

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `AuthService.LogoutAsync(userId, refreshToken)` :
  - [ ] Rechercher refresh token en DB
  - [ ] Marquer comme révoqué (`RevokedAt = DateTime.UtcNow`)
  - [ ] Logger déconnexion
- [ ] Créer `AuthService.LogoutAllAsync(userId)` :
  - [ ] Révoquer tous les refresh tokens de l'utilisateur
  - [ ] Logger déconnexion globale
- [ ] Créer endpoint `POST /api/auth/logout` [Authorize]
  - [ ] Extraire userId du JWT
  - [ ] Extraire refreshToken du body ou cookie
  - [ ] Appeler `LogoutAsync()` ou `LogoutAllAsync()` selon paramètre
- [ ] (Optionnel) Implémenter blacklist JWT si tokens de courte durée :
  - [ ] Table `RevokedJwts` avec jti (JWT ID) et expiration
  - [ ] Middleware pour vérifier si JWT dans blacklist
  - [ ] Job de nettoyage des JWT expirés

### Frontend (Blazor)
- [ ] Ajouter bouton "Déconnexion" dans menu utilisateur (NavMenu)
- [ ] Créer `AuthenticationService.LogoutAsync()` :
  ```csharp
  public async Task LogoutAsync(bool logoutAll = false)
  {
      var refreshToken = _localStorage.GetItem("refreshToken");
      
      try
      {
          await _httpClient.PostAsJsonAsync("/api/auth/logout", 
              new { refreshToken, logoutAll });
      }
      catch { /* Déconnexion côté client même si erreur serveur */ }
      
      // Suppression tokens côté client
      _localStorage.RemoveItem("token");
      _localStorage.RemoveItem("refreshToken");
      
      // Mise à jour état authentification
      await _authStateProvider.GetAuthenticationStateAsync();
      
      // Redirection
      _navigationManager.NavigateTo("/login");
  }
  ```
- [ ] Mettre à jour `CustomAuthStateProvider` :
  ```csharp
  public void MarkUserAsLoggedOut()
  {
      var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
      NotifyAuthenticationStateChanged(
          Task.FromResult(new AuthenticationState(anonymousUser)));
  }
  ```
- [ ] Option "Déconnexion de tous les appareils" sur page profil
- [ ] Message toast de confirmation après déconnexion

---

## 🔗 Dépendances

### Dépend de
- [US-002](./US-002-connexion-utilisateur.md) - Connexion
- [US-007](./US-007-refresh-token.md) - Refresh token (pour révocation)

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible (CRUD simple)
- Effort : 0.5 jour
- Risques : Gestion état Blazor

---

## 📝 Notes Techniques

### Blacklist JWT (Optionnel)
```csharp
// À la génération du JWT, ajouter claim jti (JWT ID)
var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    // ... autres claims
};

// Middleware de validation
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"]
        .FirstOrDefault()?.Replace("Bearer ", "");
    
    if (!string.IsNullOrEmpty(token))
    {
        var jti = GetJtiFromToken(token);
        if (await _blacklistService.IsBlacklistedAsync(jti))
        {
            context.Response.StatusCode = 401;
            return;
        }
    }
    
    await next();
});
```

### Révocation en Cascade
```
User A connecté sur 3 appareils :
- PC : RefreshToken1
- Mobile : RefreshToken2  
- Tablette : RefreshToken3

User A clique "Déconnexion tous appareils" sur PC :
→ Révocation RT1, RT2, RT3
→ Prochaine requête depuis Mobile/Tablette → 401
→ Redirection vers login
```

### Logging
```csharp
_logger.LogInformation(
    "User {UserId} logged out from device {DeviceInfo}. LogoutAll: {LogoutAll}",
    userId, deviceInfo, logoutAll);
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Bouton déconnexion fonctionnel
- [ ] Refresh token révoqué
- [ ] État authentification mis à jour
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
