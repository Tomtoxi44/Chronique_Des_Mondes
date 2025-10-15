# US-010 - Gestion des Sessions Actives

## 📝 Description

**En tant qu'** utilisateur  
**Je veux** voir la liste de mes sessions actives (appareils connectés)  
**Afin de** surveiller l'accès à mon compte et révoquer les sessions suspectes

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mes Sessions" accessible depuis le profil utilisateur
- [ ] Liste des sessions actives avec informations :
  - [ ] Type d'appareil (PC, Mobile, Tablette)
  - [ ] Navigateur (Chrome, Firefox, Edge, Safari)
  - [ ] Système d'exploitation (Windows, macOS, Linux, Android, iOS)
  - [ ] Adresse IP (optionnel, anonymisée)
  - [ ] Localisation approximative (pays, ville)
  - [ ] Date dernière activité
  - [ ] Indicateur "Session actuelle" pour l'appareil en cours
- [ ] Bouton "Révoquer" sur chaque session (sauf actuelle)
- [ ] Bouton "Révoquer toutes les autres sessions"
- [ ] Confirmation avant révocation
- [ ] Message de succès après révocation
- [ ] Session révoquée → déconnexion immédiate sur l'appareil concerné
- [ ] Limite maximum de sessions simultanées (ex: 5 appareils)
- [ ] Notification email si nouvelle connexion depuis appareil inconnu

### Techniques
- [ ] Extension table `RefreshTokens` avec colonnes :
  - [ ] DeviceType (enum : Desktop, Mobile, Tablet)
  - [ ] Browser (string)
  - [ ] OperatingSystem (string)
  - [ ] IpAddress (string, anonymisée)
  - [ ] Location (string, optionnel)
  - [ ] LastActivityAt (DateTime)
- [ ] Endpoint : `GET /api/auth/sessions` [Authorize]
- [ ] Response : `[{ "id": "guid", "deviceType": "Desktop", "browser": "Chrome", "os": "Windows", "lastActivity": "...", "isCurrent": true }, ...]`
- [ ] Endpoint : `DELETE /api/auth/sessions/{sessionId}` [Authorize]
- [ ] Response 200 : Session révoquée
- [ ] Endpoint : `DELETE /api/auth/sessions/revoke-all` [Authorize]
- [ ] Response 200 : Toutes sessions révoquées sauf actuelle

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SessionService.GetActiveSessions_WithUser_ReturnsSessionsList()`
- [ ] `SessionService.RevokeSession_WithValidSessionId_RevokesToken()`
- [ ] `SessionService.RevokeSession_CannotRevokeCurrentSession_ThrowsException()`
- [ ] `SessionService.RevokeAllOtherSessions_RevokesAllExceptCurrent()`
- [ ] `SessionService.ParseUserAgent_WithValidUserAgent_ReturnsDeviceInfo()`

### Tests d'Intégration
- [ ] `SessionsEndpoint_GetSessions_ReturnsUserSessions()`
- [ ] `SessionsEndpoint_RevokeSession_InvalidatesRefreshToken()`
- [ ] `SessionsEndpoint_RevokeAllOther_KeepsCurrentSession()`

### Tests E2E
- [ ] Login depuis 2 appareils → Liste affiche 2 sessions
- [ ] Révoquer session B depuis appareil A → Appareil B déconnecté
- [ ] Révoquer toutes autres → Seule session actuelle reste

---

## 🔧 Tâches Techniques

### Backend
- [ ] Migration : Ajouter colonnes à `RefreshTokens`
  ```sql
  ALTER TABLE RefreshTokens ADD COLUMN DeviceType INT;
  ALTER TABLE RefreshTokens ADD COLUMN Browser VARCHAR(100);
  ALTER TABLE RefreshTokens ADD COLUMN OperatingSystem VARCHAR(100);
  ALTER TABLE RefreshTokens ADD COLUMN IpAddress VARCHAR(50);
  ALTER TABLE RefreshTokens ADD COLUMN Location VARCHAR(200);
  ALTER TABLE RefreshTokens ADD COLUMN LastActivityAt DATETIME2;
  ```
- [ ] Créer `DeviceInfoService` :
  - [ ] `ParseUserAgent(userAgent)` → DeviceInfo
  - [ ] Utiliser library `UAParser` (NuGet : UAParser)
  ```csharp
  var uaParser = Parser.GetDefault();
  var clientInfo = uaParser.Parse(userAgent);
  
  return new DeviceInfo
  {
      Browser = clientInfo.UA.Family,
      OS = clientInfo.OS.Family,
      DeviceType = DetermineDeviceType(clientInfo)
  };
  ```
- [ ] Créer `IpGeolocationService` (optionnel) :
  - [ ] API gratuite : ipapi.co, ip-api.com
  - [ ] `GetLocationFromIp(ipAddress)` → Location
- [ ] Modifier `AuthService.LoginAsync()` et `RefreshTokenAsync()` :
  - [ ] Extraire User-Agent du header
  - [ ] Extraire IP du contexte HTTP
  - [ ] Parser infos appareil
  - [ ] Stocker dans RefreshToken
  - [ ] Mettre à jour `LastActivityAt`
- [ ] Créer `SessionService.GetActiveSessionsAsync(userId)` :
  - [ ] Récupérer refresh tokens non révoqués et non expirés
  - [ ] Mapper vers DTO SessionInfo
  - [ ] Marquer session actuelle (comparer jti du JWT)
- [ ] Créer `SessionService.RevokeSessionAsync(userId, sessionId)` :
  - [ ] Vérifier session appartient à l'utilisateur
  - [ ] Interdire révocation session actuelle
  - [ ] Révoquer refresh token
- [ ] Créer `SessionService.RevokeAllOtherSessionsAsync(userId, currentSessionId)` :
  - [ ] Révoquer tous sauf session actuelle
- [ ] Créer endpoints :
  - [ ] `GET /api/auth/sessions` [Authorize]
  - [ ] `DELETE /api/auth/sessions/{sessionId}` [Authorize]
  - [ ] `DELETE /api/auth/sessions/revoke-all` [Authorize]
- [ ] (Optionnel) Notification email nouvelle connexion :
  ```csharp
  if (IsNewDevice(userId, deviceInfo))
  {
      await _emailService.SendNewDeviceAlertAsync(user.Email, deviceInfo);
  }
  ```

### Frontend (Blazor)
- [ ] Créer page `ActiveSessions.razor` (/profile/sessions)
- [ ] Créer composant `SessionCard.razor` :
  ```razor
  <div class="session-card @(Session.IsCurrent ? "current" : "")">
      <div class="device-icon">
          @GetDeviceIcon(Session.DeviceType)
      </div>
      <div class="session-info">
          <h4>@Session.Browser sur @Session.OS</h4>
          <p>Dernière activité : @Session.LastActivity.Humanize()</p>
          <p>@Session.Location</p>
          @if (Session.IsCurrent)
          {
              <span class="badge-current">Session actuelle</span>
          }
      </div>
      @if (!Session.IsCurrent)
      {
          <button @onclick="OnRevoke">Révoquer</button>
      }
  </div>
  ```
- [ ] Implémenter `SessionService.GetActiveSessionsAsync()`
- [ ] Implémenter `SessionService.RevokeSessionAsync(sessionId)`
- [ ] Implémenter `SessionService.RevokeAllOtherSessionsAsync()`
- [ ] Modal de confirmation avant révocation
- [ ] Rafraîchir liste après révocation
- [ ] Package NuGet : `Humanizer` pour "Il y a 2 heures"

---

## 🔗 Dépendances

### Dépend de
- [US-007](./US-007-refresh-token.md) - Refresh tokens
- [US-002](./US-002-connexion-utilisateur.md) - Connexion

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible-Moyenne (parsing User-Agent)
- Effort : 1 jour
- Risques : Parsing User-Agent imprécis

---

## 📝 Notes Techniques

### Parsing User-Agent
```csharp
// Installer : dotnet add package UAParser
using UAParser;

var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...";
var uaParser = Parser.GetDefault();
var clientInfo = uaParser.Parse(userAgent);

Console.WriteLine($"Browser: {clientInfo.UA.Family} {clientInfo.UA.Major}");
// Browser: Chrome 118

Console.WriteLine($"OS: {clientInfo.OS.Family} {clientInfo.OS.Major}");
// OS: Windows 10

Console.WriteLine($"Device: {clientInfo.Device.Family}");
// Device: PC
```

### Géolocalisation IP (Optionnel)
```csharp
// API gratuite : ipapi.co (1000 requêtes/jour)
public async Task<string> GetLocationAsync(string ipAddress)
{
    var response = await _httpClient.GetAsync($"https://ipapi.co/{ipAddress}/json/");
    var json = await response.Content.ReadFromJsonAsync<IpapiResponse>();
    return $"{json.City}, {json.Country}";
}
```

### Détection Nouvel Appareil
```csharp
public bool IsNewDevice(Guid userId, DeviceInfo deviceInfo)
{
    var existingSessions = _db.RefreshTokens
        .Where(rt => rt.UserId == userId && 
                     rt.Browser == deviceInfo.Browser &&
                     rt.OperatingSystem == deviceInfo.OS)
        .Any();
    
    return !existingSessions;
}
```

### Email Nouvelle Connexion
```
Bonjour,

Une nouvelle connexion à votre compte a été détectée :

Appareil : Chrome sur Windows 10
Localisation : Paris, France
Date : 15 octobre 2025 à 14:30

Si vous êtes à l'origine de cette connexion, vous pouvez ignorer cet email.

Sinon, nous vous recommandons de :
1. Changer votre mot de passe immédiatement
2. Révoquer toutes les sessions actives
3. Activer la validation en deux étapes (bientôt disponible)

L'équipe Chronique des Mondes
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Page sessions actives fonctionnelle
- [ ] Révocation sessions opérationnelle
- [ ] Parsing User-Agent précis
- [ ] Notification nouvelle connexion (optionnel)
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 3
