# US-009 - Validation d'Email

## 📝 Description

**En tant qu'** utilisateur nouvellement inscrit  
**Je veux** valider mon adresse email via un lien reçu par email  
**Afin de** prouver que je suis propriétaire de l'adresse email et activer pleinement mon compte

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Email de validation envoyé automatiquement après inscription
- [ ] Email contient lien de validation unique et sécurisé
- [ ] Lien valide pendant 24 heures
- [ ] Clic sur lien → page de confirmation
- [ ] Compte marqué comme "EmailVerified = true"
- [ ] Message de succès affiché
- [ ] Option de renvoyer email de validation si expiré
- [ ] Restrictions sur compte non validé (optionnel) :
  - [ ] Limite nombre de parties rejointes (ex: 1 max)
  - [ ] Impossibilité de créer campagne
  - [ ] Badge "Email non validé" sur profil
- [ ] Notification rappel après 3 jours si email non validé

### Techniques
- [ ] Colonne `Users.EmailVerified` (bool, défaut: false)
- [ ] Table `EmailVerificationTokens` similaire à `PasswordResetTokens`
- [ ] Endpoint : `POST /api/auth/verify-email?token=xxx`
- [ ] Response 200 : `{ "message": "Email validé avec succès" }`
- [ ] Response 400 : Token invalide/expiré
- [ ] Endpoint : `POST /api/auth/resend-verification` [Authorize]
- [ ] Response 200 : Email renvoyé

---

## 🧪 Tests

### Tests Unitaires
- [ ] `AuthService.SendVerificationEmail_WithNewUser_SendsEmail()`
- [ ] `AuthService.VerifyEmail_WithValidToken_MarksEmailVerified()`
- [ ] `AuthService.VerifyEmail_WithExpiredToken_ThrowsException()`
- [ ] `AuthService.ResendVerification_WithUnverifiedUser_SendsNewEmail()`
- [ ] `AuthService.ResendVerification_WithAlreadyVerifiedUser_ThrowsException()`

### Tests d'Intégration
- [ ] `RegisterEndpoint_SendsVerificationEmail()`
- [ ] `VerifyEmailEndpoint_WithValidToken_UpdatesDatabase()`
- [ ] `VerifyEmailEndpoint_WithExpiredToken_Returns400()`
- [ ] `ResendVerificationEndpoint_SendsNewEmail()`

### Tests E2E
- [ ] Inscription → Email reçu → Clic lien → Validation → Badge disparaît
- [ ] Token expiré → Renvoyer email → Nouveau lien → Validation OK
- [ ] Compte non validé → Restriction création campagne

---

## 🔧 Tâches Techniques

### Backend
- [ ] Ajouter colonne `Users.EmailVerified` (bool, défaut: false)
- [ ] Créer table `EmailVerificationTokens`
  - [ ] Id (PK, Guid)
  - [ ] UserId (FK → Users)
  - [ ] Token (string, unique, index)
  - [ ] ExpiresAt (DateTime, +24h)
  - [ ] VerifiedAt (DateTime, nullable)
  - [ ] CreatedAt (DateTime)
- [ ] Modifier `AuthService.RegisterAsync()` :
  - [ ] Créer token de validation
  - [ ] Envoyer email de validation
- [ ] Créer `AuthService.VerifyEmailAsync(token)` :
  - [ ] Valider token (existe, non expiré, non utilisé)
  - [ ] Marquer `Users.EmailVerified = true`
  - [ ] Marquer token comme utilisé
  - [ ] Logger validation
- [ ] Créer `AuthService.ResendVerificationEmailAsync(userId)` :
  - [ ] Vérifier email pas déjà validé
  - [ ] Invalider anciens tokens
  - [ ] Générer nouveau token
  - [ ] Envoyer email
  - [ ] Rate limiting (1/heure max)
- [ ] Créer endpoints :
  - [ ] `POST /api/auth/verify-email?token=xxx`
  - [ ] `POST /api/auth/resend-verification` [Authorize]
- [ ] (Optionnel) Middleware pour restreindre accès si email non validé :
  ```csharp
  [Authorize]
  [RequireEmailVerified] // Attribut custom
  public async Task<IResult> CreateCampaign(...)
  ```
- [ ] Job background : Notification rappel après 3 jours

### Frontend (Blazor)
- [ ] Créer page `VerifyEmail.razor` (/verify-email?token=xxx)
- [ ] Afficher spinner pendant validation
- [ ] Afficher message succès/erreur
- [ ] Bouton "Renvoyer l'email" si échec
- [ ] Banner "Email non validé" sur pages principales :
  ```razor
  @if (!currentUser.EmailVerified)
  {
      <div class="alert alert-warning">
          Votre email n'est pas validé. 
          <button @onclick="ResendVerification">Renvoyer l'email</button>
      </div>
  }
  ```
- [ ] Badge "✓ Email validé" sur profil
- [ ] Implémenter `AuthenticationService.VerifyEmailAsync(token)`
- [ ] Implémenter `AuthenticationService.ResendVerificationAsync()`

### Email Template
- [ ] Créer template HTML pour email de validation
- [ ] Sujet : "Validez votre adresse email - Chronique des Mondes"
- [ ] Contenu :
  ```
  Bonjour,
  
  Bienvenue sur Chronique des Mondes !
  
  Pour finaliser votre inscription, veuillez valider votre adresse email 
  en cliquant sur le lien ci-dessous :
  
  [Valider mon email]
  
  Ce lien expirera dans 24 heures.
  
  Si vous n'avez pas créé de compte, ignorez cet email.
  
  L'équipe Chronique des Mondes
  ```

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription
- Configuration Azure Communication Services

### Bloque
- Aucune (feature optionnelle pour MVP)

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (similaire reset password)
- Effort : 1 jour
- Risques : Délai email, taux ouverture faible

---

## 📝 Notes Techniques

### Génération Token
```csharp
var token = Guid.NewGuid().ToString("N"); // 32 caractères
```

### Restriction Basée sur Email Non Validé
```csharp
// Attribut custom
public class RequireEmailVerifiedAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var emailVerified = user.FindFirst("EmailVerified")?.Value == "true";
        
        if (!emailVerified)
        {
            context.Result = new ForbidResult();
        }
    }
}

// Ajouter claim au JWT
claims.Add(new Claim("EmailVerified", user.EmailVerified.ToString()));
```

### Job de Rappel
```csharp
// Tous les jours à 10h
public class EmailVerificationReminderJob : IHostedService
{
    public async Task ExecuteAsync()
    {
        var unverifiedUsers = await _db.Users
            .Where(u => !u.EmailVerified && 
                        u.CreatedAt < DateTime.UtcNow.AddDays(-3))
            .ToListAsync();
        
        foreach (var user in unverifiedUsers)
        {
            await _emailService.SendReminderAsync(user.Email);
        }
    }
}
```

### Sécurité
- ⚠️ Rate limiting sur resend (1/heure)
- ⚠️ Token usage unique
- ⚠️ Expiration 24h (plus long que reset password)
- ⚠️ Logger toutes validations

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Email envoyé à l'inscription
- [ ] Validation fonctionnelle
- [ ] Renvoyer email fonctionnel
- [ ] Banner affiché si non validé
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 3
