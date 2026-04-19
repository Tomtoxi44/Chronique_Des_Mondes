# Epic 1 - Authentification & Gestion Utilisateurs

## 📋 Informations Générales

- **Phase** : Phase 0 (MVP Core)
- **Priorité** : P0 - Critique
- **Statut** : ✅ Majorité implémentée
- **Estimation totale** : 34 Story Points

---

## 🎯 Objectif

Mettre en place un système d'authentification sécurisé et complet permettant aux utilisateurs de créer un compte, se connecter, et gérer leur profil. Ce système utilise JWT pour l'authentification et BCrypt pour le hashage des mots de passe.

---

## 📊 Critères d'Acceptation Globaux

- ✅ Un utilisateur peut créer un compte avec email et mot de passe
- ✅ Un utilisateur peut se connecter et recevoir un JWT valide
- ✅ Les mots de passe sont hashés avec BCrypt (work factor 12)
- [ ] Un utilisateur peut réinitialiser son mot de passe via email
- [ ] Un utilisateur peut gérer son profil (nom, avatar, préférences)
- [ ] Un utilisateur peut assumer plusieurs rôles (Joueur/MJ)
- [ ] Les tokens JWT expirent et peuvent être renouvelés
- [ ] Les sessions sont sécurisées et tracées

---

## 📝 User Stories

| ID | Titre | Statut | Priorité |
|----|-------|--------|----------|
| [US-001](./US-001-inscription-utilisateur.md) | Inscription utilisateur | ✅ Terminé | P0 |
| [US-002](./US-002-connexion-utilisateur.md) | Connexion utilisateur | ✅ Terminé | P0 |
| [US-003](./US-003-gestion-jwt.md) | Gestion des tokens JWT | ✅ Terminé | P0 |
| [US-004](./US-004-profil-utilisateur.md) | Gestion du profil utilisateur | ✅ Terminé | P0 |
| [US-005](./US-005-reset-password.md) | Réinitialisation mot de passe | ⏳ Planifié | P1 |
| [US-006](./US-006-gestion-roles.md) | Gestion multi-rôles (MJ/Joueur) | ✅ Terminé (implicite par contexte monde) | P0 |
| [US-007](./US-007-refresh-token.md) | Refresh token JWT | ✅ Terminé | P1 |
| [US-008](./US-008-deconnexion.md) | Déconnexion sécurisée | ✅ Terminé | P0 |
| [US-009](./US-009-validation-email.md) | Validation d'email | ⏳ Planifié | P2 |
| [US-010](./US-010-gestion-sessions.md) | Gestion des sessions actives | ✅ Terminé (in-app) | P2 |

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `AuthEndpoints.cs`
- **Services** : `AuthService`, `EmailService`
- **Models** : `User`, `UserRole`
- **Configuration** : JWT (SecretKey, Issuer, Audience, ExpiryDays)

### Frontend (Blazor Server)
- **Pages** : Login.razor, Register.razor, Profile.razor
- **Components** : LoginForm, RegisterForm, ProfileEditor
- **Services** : AuthenticationService, UserStateService

### Base de Données
- **Tables** : `Users`, `UserRoles`, `RefreshTokens`
- **Relations** : User 1-N UserRoles

---

## 🔐 Sécurité

- **Hashage** : BCrypt avec work factor 12
- **Tokens** : JWT avec signature HMAC-SHA256
- **Expiration** : Configurable (par défaut 7 jours)
- **Refresh tokens** : Rotation automatique
- **Rate limiting** : Protection contre brute force
- **HTTPS** : Obligatoire en production

---

## 🧪 Tests

### Tests Unitaires
- Hashage/vérification BCrypt
- Génération/validation JWT
- Validation des données d'inscription

### Tests d'Intégration
- Flux complet inscription → connexion
- Renouvellement de token
- Réinitialisation de mot de passe

### Tests E2E
- Parcours utilisateur complet
- Gestion des erreurs (email déjà utilisé, mot de passe incorrect)

---

## 🔗 Dépendances

### Packages NuGet
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `BCrypt.Net-Next`
- `System.IdentityModel.Tokens.Jwt`

### Services Externes
- **Azure Communication Services** : Envoi d'emails
- **Entity Framework Core** : Persistance des données

---

## 📈 Progression

```
Complété : [████████░░] 80% (8/10 US)
Planifié  : 2 US (reset password, validation email)
```

---

## 📚 Documentation Associée

- [SECURITE.md](../../instructions/technique/SECURITE.md) - Architecture de sécurité
- [API_ENDPOINTS.md](../../instructions/technique/API_ENDPOINTS.md) - Endpoints d'authentification
- [MODELE_DONNEES.md](../../instructions/technique/MODELE_DONNEES.md) - Schéma des tables User

---

**Dernière mise à jour** : 15 octobre 2025
