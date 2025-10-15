# US-001 - Inscription Utilisateur

## 📝 Description

**En tant que** visiteur du site  
**Je veux** créer un compte avec mon email et un mot de passe  
**Afin de** pouvoir accéder à la plateforme et créer des parties de jeu de rôle

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [x] Le formulaire d'inscription contient : email, mot de passe, confirmation mot de passe
- [x] L'email doit être valide (format email)
- [x] Le mot de passe doit respecter les critères de sécurité :
  - [x] Minimum 8 caractères
  - [x] Au moins 1 majuscule
  - [x] Au moins 1 minuscule
  - [x] Au moins 1 chiffre
  - [x] Au moins 1 caractère spécial
- [x] Les deux mots de passe doivent être identiques
- [x] L'email ne doit pas déjà être utilisé
- [x] Le mot de passe est hashé avec BCrypt (work factor 12)
- [x] Un compte utilisateur est créé dans la base de données
- [x] Un message de confirmation est affiché
- [x] L'utilisateur reçoit un email de bienvenue (optionnel pour MVP)

### Techniques
- [x] Endpoint : `POST /api/auth/register`
- [x] Body : `{ "email": "user@example.com", "password": "SecurePass123!", "confirmPassword": "SecurePass123!" }`
- [x] Response 201 : `{ "userId": "guid", "email": "user@example.com", "message": "Compte créé avec succès" }`
- [x] Response 400 : `{ "error": "Email déjà utilisé" }` ou validation échouée
- [x] Le mot de passe n'est jamais retourné dans la réponse

---

## 🧪 Tests

### Tests Unitaires
- [x] `AuthService.Register_WithValidData_ReturnsSuccess()`
- [x] `AuthService.Register_WithExistingEmail_ThrowsException()`
- [x] `AuthService.Register_WithWeakPassword_ThrowsValidationException()`
- [x] `BCryptService.HashPassword_WorksFactor12_ReturnsValidHash()`
- [x] `BCryptService.VerifyPassword_WithCorrectPassword_ReturnsTrue()`

### Tests d'Intégration
- [x] `RegisterEndpoint_WithValidData_CreatesUserInDatabase()`
- [x] `RegisterEndpoint_WithDuplicateEmail_Returns400()`
- [x] `RegisterEndpoint_PasswordMismatch_Returns400()`

### Tests E2E (Playwright)
- [x] Parcours complet : Remplir formulaire → Soumettre → Voir message de succès
- [x] Cas d'erreur : Email invalide → Message d'erreur affiché
- [x] Cas d'erreur : Mot de passe faible → Message d'erreur affiché

---

## 🔧 Tâches Techniques

### Backend
- [x] Créer `RegisterRequest` DTO avec validation
- [x] Créer `RegisterResponse` DTO
- [x] Implémenter `AuthService.RegisterAsync()`
  - [x] Validation des données
  - [x] Vérification email unique
  - [x] Hashage BCrypt du mot de passe
  - [x] Création de l'entité User
  - [x] Sauvegarde en base de données
- [x] Créer endpoint `POST /api/auth/register` dans `AuthEndpoints.cs`
- [x] Ajouter logs pour traçabilité
- [x] Configurer BCrypt work factor dans appsettings.json

### Frontend
- [x] Créer composant `RegisterForm.razor`
  - [x] Champs : Email, Password, ConfirmPassword
  - [x] Validation côté client (DataAnnotations)
  - [x] Indicateur de force du mot de passe
  - [x] Bouton "S'inscrire"
- [x] Créer page `Register.razor` (/register)
- [x] Implémenter `AuthenticationService.RegisterAsync()` (appel API)
- [x] Afficher messages d'erreur utilisateur
- [x] Redirection vers login après succès

### Base de Données
- [x] Créer migration pour table `Users`
  - [x] Id (PK, Guid)
  - [x] Email (Unique, Index)
  - [x] PasswordHash (string)
  - [x] CreatedAt (DateTime)
  - [x] UpdatedAt (DateTime)
- [x] Appliquer migration

---

## 🔗 Dépendances

### Dépend de
- Infrastructure de base (Entity Framework configuré)
- Configuration JWT et BCrypt dans User Secrets

### Bloque
- [US-002](./US-002-connexion-utilisateur.md) - Connexion utilisateur
- [US-004](./US-004-profil-utilisateur.md) - Gestion du profil

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (validation, sécurité)
- Effort : 1 jour
- Risques : BCrypt configuration, validation robuste

---

## 📝 Notes Techniques

### Configuration BCrypt
```json
{
  "Authentication": {
    "BCrypt": {
      "WorkFactor": 12
    }
  }
}
```

### Exemple d'utilisation
```csharp
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
```

### Sécurité
- ⚠️ Ne jamais logger les mots de passe en clair
- ⚠️ Toujours utiliser HTTPS en production
- ⚠️ Implémenter rate limiting pour éviter les attaques par force brute

---

## ✅ Definition of Done

- [x] Code implémenté et testé
- [x] Tests unitaires passent (couverture > 80%)
- [x] Tests d'intégration passent
- [x] Tests E2E passent
- [x] Code review validée
- [x] Documentation API mise à jour
- [x] Déployé en staging et validé
- [x] Mergé dans main

---

**Statut** : ✅ Terminé  
**Assigné à** : Tommy ANGIBAUD  
**Date de complétion** : 15 octobre 2025
