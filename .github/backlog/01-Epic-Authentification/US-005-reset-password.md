# US-005 - Réinitialisation de Mot de Passe

## 📝 Description

**En tant qu'** utilisateur ayant oublié son mot de passe  
**Je veux** pouvoir réinitialiser mon mot de passe via email  
**Afin de** retrouver l'accès à mon compte

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mot de passe oublié" accessible depuis la page de connexion
- [ ] Formulaire avec champ email
- [ ] Email de réinitialisation envoyé avec lien sécurisé
- [ ] Lien valide pendant 1 heure uniquement
- [ ] Page de réinitialisation avec formulaire nouveau mot de passe
- [ ] Validation du token de réinitialisation
- [ ] Nouveau mot de passe doit respecter les critères de sécurité
- [ ] Hash du nouveau mot de passe avec BCrypt
- [ ] Invalidation du token après utilisation
- [ ] Email de confirmation de changement de mot de passe
- [ ] Message d'erreur si token expiré ou invalide

### Techniques
- [ ] Endpoint : `POST /api/auth/forgot-password`
- [ ] Body : `{ "email": "user@example.com" }`
- [ ] Response 200 : `{ "message": "Email envoyé si compte existe" }` (sécurité : ne pas révéler existence compte)
- [ ] Endpoint : `POST /api/auth/reset-password`
- [ ] Body : `{ "token": "reset-token", "newPassword": "NewPass123!", "confirmPassword": "NewPass123!" }`
- [ ] Response 200 : `{ "message": "Mot de passe réinitialisé" }`
- [ ] Response 400 : Token invalide/expiré ou validation échouée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `AuthService.ForgotPassword_WithValidEmail_SendsEmail()`
- [ ] `AuthService.ForgotPassword_WithInvalidEmail_DoesNotRevealExistence()`
- [ ] `AuthService.ResetPassword_WithValidToken_UpdatesPassword()`
- [ ] `AuthService.ResetPassword_WithExpiredToken_ThrowsException()`
- [ ] `AuthService.ResetPassword_TokenUsedOnce_CannotReuseToken()`

### Tests d'Intégration
- [ ] `ForgotPasswordEndpoint_SendsEmailWithValidToken()`
- [ ] `ResetPasswordEndpoint_UpdatesPasswordInDatabase()`
- [ ] `ResetPasswordEndpoint_InvalidatesOldTokens()`

### Tests E2E
- [ ] Flux complet : Oubli → Email reçu → Clic lien → Nouveau mot de passe → Connexion
- [ ] Token expiré après 1h → Message d'erreur
- [ ] Réutilisation token → Erreur

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer table `PasswordResetTokens`
  - [ ] Id (PK, Guid)
  - [ ] UserId (FK → Users)
  - [ ] Token (string, unique, index)
  - [ ] ExpiresAt (DateTime)
  - [ ] UsedAt (DateTime, nullable)
  - [ ] CreatedAt (DateTime)
- [ ] Créer `ForgotPasswordRequest` DTO
- [ ] Créer `ResetPasswordRequest` DTO
- [ ] Implémenter `AuthService.ForgotPasswordAsync()`
  - [ ] Vérifier email existe (sans révéler si inexistant)
  - [ ] Générer token sécurisé (GUID ou crypto random)
  - [ ] Sauvegarder token avec expiration 1h
  - [ ] Envoyer email avec lien de réinitialisation
- [ ] Implémenter `AuthService.ResetPasswordAsync()`
  - [ ] Valider token (existe, non expiré, non utilisé)
  - [ ] Valider nouveau mot de passe
  - [ ] Hash nouveau mot de passe BCrypt
  - [ ] Mettre à jour utilisateur
  - [ ] Marquer token comme utilisé
  - [ ] Envoyer email de confirmation
- [ ] Créer endpoints :
  - [ ] `POST /api/auth/forgot-password`
  - [ ] `POST /api/auth/reset-password`
  - [ ] `GET /api/auth/verify-reset-token?token=xxx` (optionnel, valider token avant formulaire)
- [ ] Configurer Azure Communication Services pour email

### Frontend
- [ ] Créer page `ForgotPassword.razor` (/forgot-password)
- [ ] Créer page `ResetPassword.razor` (/reset-password?token=xxx)
- [ ] Créer composant `ForgotPasswordForm.razor`
- [ ] Créer composant `ResetPasswordForm.razor`
- [ ] Implémenter `AuthenticationService.ForgotPasswordAsync()`
- [ ] Implémenter `AuthenticationService.ResetPasswordAsync()`
- [ ] Afficher messages de succès/erreur
- [ ] Lien "Mot de passe oublié ?" sur page de connexion

### Email Template
- [ ] Créer template HTML pour email de réinitialisation
- [ ] Sujet : "Réinitialisation de votre mot de passe - Chronique des Mondes"
- [ ] Contenu :
  - [ ] Message explicatif
  - [ ] Bouton/lien de réinitialisation
  - [ ] Durée de validité (1h)
  - [ ] Message sécurité (ignorer si non demandé)

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription
- Configuration Azure Communication Services

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (sécurité, emails, tokens)
- Effort : 1-2 jours
- Risques : Délai email, sécurité tokens

---

## 📝 Notes Techniques

### Génération Token Sécurisé
```csharp
// Option 1 : GUID
var token = Guid.NewGuid().ToString("N");

// Option 2 : Crypto Random (plus sécurisé)
var bytes = new byte[32];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(bytes);
}
var token = Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
```

### Email de Réinitialisation
```
Bonjour,

Vous avez demandé la réinitialisation de votre mot de passe sur Chronique des Mondes.

Cliquez sur le lien ci-dessous pour créer un nouveau mot de passe :
{ResetLink}

Ce lien expirera dans 1 heure.

Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.

L'équipe Chronique des Mondes
```

### Sécurité
- ⚠️ Ne jamais révéler si email existe ou non (réponse identique)
- ⚠️ Token usage unique obligatoire
- ⚠️ Expiration stricte 1 heure
- ⚠️ Limiter nombre de demandes (rate limiting : 3/heure)
- ⚠️ Logger toutes tentatives de réinitialisation

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Emails envoyés correctement
- [ ] Token sécurisé et expire correctement
- [ ] Documentation API mise à jour
- [ ] Déployé en staging
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
