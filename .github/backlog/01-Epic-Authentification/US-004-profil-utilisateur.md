# US-004 - Gestion du Profil Utilisateur

## 📝 Description

**En tant qu'** utilisateur connecté  
**Je veux** consulter et modifier mon profil (nom, avatar, préférences)  
**Afin de** personnaliser mon expérience sur la plateforme

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [x] Page "Mon Profil" accessible depuis le menu utilisateur (`/profile`)
- [x] Affichage des informations : email, nom d'utilisateur, avatar, date d'inscription
- [x] Modification possible : nom d'utilisateur, avatar, préférences
- [x] L'email ne peut pas être modifié (pour MVP)
- [x] Validation : nom d'utilisateur unique, 3-30 caractères
- [x] Upload d'avatar (formats: jpg, png, max 2MB)
- [x] Aperçu de l'avatar avant sauvegarde
- [x] Message de confirmation après sauvegarde
- [x] Préférences : thème (clair/sombre), notifications (email/in-app)

### Techniques
- [x] Endpoint : `GET /api/users/profile`
- [x] Response : `{ "id": int, "email": "user@example.com", "nickname": "Pseudo", "username": "username", "avatarUrl": "/uploads/avatars/123_avatar.jpg", "createdAt": "2025-10-15", "preferences": "{\"theme\":\"dark\",\"notifications\":{\"email\":true,\"inApp\":true}}" }`
- [x] Endpoint : `PUT /api/users/profile`
- [x] Body : `{ "username": "NouveauNom", "preferences": "{...}" }`
- [x] Endpoint : `POST /api/users/avatar` (multipart/form-data)
- [x] Response 200 : Profile mis à jour
- [x] Response 400 : Validation échouée

---

## 🧪 Tests

### Tests Unitaires
- [x] `UserProfileServiceTests.GetProfileAsync_UserExists_ReturnsProfileResponse()`
- [x] `UserProfileServiceTests.UpdateProfileAsync_ValidRequest_UpdatesProfile()`
- [x] `UserProfileServiceTests.UpdateProfileAsync_UsernameAlreadyTaken_ReturnsNull()`
- [x] `UserProfileServiceTests.IsUsernameAvailableAsync_UsernameAvailable_ReturnsTrue()`
- [x] `UserProfileServiceTests.IsUsernameAvailableAsync_UsernameTaken_ReturnsFalse()`
- [x] `AvatarServiceTests.ValidateAvatarFile_ValidJpg_ReturnsTrue()`
- [x] `AvatarServiceTests.ValidateAvatarFile_ValidPng_ReturnsTrue()`
- [x] `AvatarServiceTests.ValidateAvatarFile_FileTooLarge_ReturnsFalse()`
- [x] `AvatarServiceTests.ValidateAvatarFile_InvalidExtension_ReturnsFalse()`
- [x] `AvatarServiceTests.UploadAvatarAsync_ValidFile_CreatesFileWithCorrectName()`
- [x] `AvatarServiceTests.DeleteAvatarAsync_FileExists_RemovesFile()`
- [x] **22 tests unitaires passent avec succès**

### Tests d'Intégration
- [ ] `ProfileEndpoint_GetProfile_ReturnsUserData()` - *Non implémenté (infrastructure de tests d'intégration non configurée)*
- [ ] `ProfileEndpoint_UpdateProfile_SavesChanges()` - *Non implémenté*
- [ ] `ProfileEndpoint_UploadAvatar_SavesFile()` - *Non implémenté*

### Tests E2E
- [ ] Consultation profil → Modification nom → Sauvegarde → Vérification affichage - *Non implémenté (Playwright non configuré)*
- [ ] Upload avatar → Aperçu → Sauvegarde → Vérification affichage - *Non implémenté*

---

## 🔧 Tâches Techniques

### Backend
- [x] Ajouter colonnes à `Users` : `Username` (30 chars, nullable), `AvatarUrl` (500 chars, nullable), `Preferences` (nvarchar(max), nullable)
- [x] Créer `ProfileResponse` DTO (Cdm.Business.Abstraction.DTOs.ViewModels)
- [x] Créer `UpdateProfileRequest` DTO (Cdm.Business.Abstraction.DTOs.Models) avec validation
- [x] Implémenter `IUserProfileService` et `UserProfileService` (Cdm.Business.Common)
  - [x] `GetProfileAsync(int userId)`
  - [x] `UpdateProfileAsync(int userId, UpdateProfileRequest request)`
  - [x] `IsUsernameAvailableAsync(string username, int currentUserId)`
- [x] Implémenter `IAvatarService` et `AvatarService` (Cdm.Business.Common)
  - [x] Validation format/taille (jpg/jpeg/png, max 2MB)
  - [x] Génération nom unique (`{userId}_avatar.{ext}`)
  - [x] Sauvegarde dans `/wwwroot/uploads/avatars/`
  - [x] Suppression ancien avatar
  - [x] `UploadAvatarAsync(int userId, IFormFile file)`
  - [x] `ValidateAvatarFile(IFormFile file, out string errorMessage)`
  - [x] `DeleteAvatarAsync(string avatarUrl)`
- [x] Créer `ProfileEndpoints.cs` (Cdm.ApiService.Endpoints) :
  - [x] `GET /api/users/profile` (RequireAuthorization)
  - [x] `PUT /api/users/profile` (RequireAuthorization)
  - [x] `POST /api/users/avatar` (RequireAuthorization, multipart/form-data)
- [x] Autorisation : JWT requis sur tous les endpoints
- [x] Enregistrement services dans `Program.cs`

### Frontend
- [x] Créer page `Profile.razor` (/profile) avec `@attribute [Authorize]`
- [x] Créer composant `ProfileEditor.razor` (Cdm.Web.Components.Shared)
  - [x] Affichage profil courant
  - [x] Formulaire EditForm avec validation
  - [x] Gestion préférences (thème, notifications)
  - [x] EventCallbacks pour sauvegarde et upload avatar
- [x] Créer composant `AvatarUploader.razor` (Cdm.Web.Components.Shared)
  - [x] InputFile avec validation client
  - [x] Messages d'erreur
  - [x] EventCallback pour fichier sélectionné
- [x] Intégration HttpClient pour appels API
- [x] Gestion upload fichier avec MultipartFormDataContent
- [x] Validation côté client (formats, taille)
- [x] Messages de succès/erreur

### Base de Données
- [x] Migration `20251017172036_AddUserProfileFields.cs` créée
- [x] Colonnes ajoutées : `Username`, `AvatarUrl`, `Preferences`
- [x] Index unique sur `Username` avec filtre `WHERE Username IS NOT NULL`
- [x] Migration appliquée via Aspire MigrationsManager

### Tests
- [x] Créer projet `Cdm.Business.Common.Tests` (xUnit)
- [x] Ajouter packages : Moq, Microsoft.EntityFrameworkCore.InMemory
- [x] 22 tests unitaires implémentés et passent
- [x] Tests UserProfileService (8 tests)
- [x] Tests AvatarService (14 tests)

### Documentation
- [x] Mettre à jour `API_ENDPOINTS.md` - Section 2 "Profil Utilisateur" ajoutée
- [x] Mettre à jour `MODELE_DONNEES.md` - Table Users mise à jour
- [x] Mettre à jour `FRONTEND_BLAZOR.md` - Section 3 "Profil Utilisateur" ajoutée avec 3 composants

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription
- [US-002](./US-002-connexion-utilisateur.md) - Connexion
- [US-003](./US-003-gestion-jwt.md) - JWT

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (upload fichier, validation)
- Effort : 1 jour
- Risques : Gestion stockage avatars, validation images

---

## 📝 Notes Techniques

### Stockage des Avatars
```
/wwwroot/uploads/avatars/
  ├── {userId}_avatar.jpg
  └── default-avatar.png
```

### Préférences JSON
```json
{
  "theme": "dark",
  "notifications": {
    "email": true,
    "inApp": true,
    "sessions": true
  },
  "language": "fr-FR"
}
```

---

## ✅ Definition of Done

- [x] Code implémenté et testé
- [x] Tests unitaires passent (22/22, couverture backend services 100%)
- [ ] Tests d'intégration passent (infrastructure non configurée)
- [ ] Tests E2E passent (Playwright non configuré)
- [x] Upload d'avatar fonctionnel (validation, stockage, suppression ancien)
- [x] Documentation technique mise à jour (API_ENDPOINTS.md, MODELE_DONNEES.md, FRONTEND_BLAZOR.md)
- [ ] Déployé en staging
- [ ] Mergé dans main

---

**Statut** : ✅ Prêt pour revue (Backend, Frontend, Tests unitaires, Documentation complétés)  
**Assigné à** : Tommy ANGIBAUD  
**Date de début** : 17 octobre 2025  
**Date de fin** : 17 octobre 2025

## 📦 Fichiers Créés/Modifiés

### Créés
- `Cdm.Data.Common/Models/User.cs` - Ajout de 3 propriétés (Username, AvatarUrl, Preferences)
- `Cdm.Migrations/Migrations/20251017172036_AddUserProfileFields.cs`
- `Cdm.Business.Abstraction/DTOs/ViewModels/ProfileResponse.cs`
- `Cdm.Business.Abstraction/DTOs/Models/UpdateProfileRequest.cs`
- `Cdm.Business.Abstraction/Services/IUserProfileService.cs`
- `Cdm.Business.Abstraction/Services/IAvatarService.cs`
- `Cdm.Business.Common/Services/UserProfileService.cs`
- `Cdm.Business.Common/Services/AvatarService.cs`
- `Cdm.ApiService/Endpoints/ProfileEndpoints.cs`
- `Cdm.Web/Components/Pages/Profile.razor`
- `Cdm.Web/Components/Shared/ProfileEditor.razor`
- `Cdm.Web/Components/Shared/AvatarUploader.razor`
- `Cdm.Web/wwwroot/uploads/avatars/` (directory)
- `Cdm.Business.Common.Tests/Services/UserProfileServiceTests.cs` (8 tests)
- `Cdm.Business.Common.Tests/Services/AvatarServiceTests.cs` (14 tests)

### Modifiés
- `Cdm.Business.Abstraction/Cdm.Business.Abstraction.csproj` - Ajout FrameworkReference
- `Cdm.Business.Common/Cdm.Business.Common.csproj` - Ajout FrameworkReference
- `Cdm.Web/Cdm.Web.csproj` - Ajout ProjectReference
- `Cdm.Web/Components/_Imports.razor` - Ajout using DTOs
- `Cdm.ApiService/Program.cs` - Enregistrement services et endpoints
- `.github/instructions/technique/API_ENDPOINTS.md` - Section 2 ajoutée
- `.github/instructions/technique/MODELE_DONNEES.md` - Table Users mise à jour
- `.github/instructions/technique/FRONTEND_BLAZOR.md` - Section 3 ajoutée
