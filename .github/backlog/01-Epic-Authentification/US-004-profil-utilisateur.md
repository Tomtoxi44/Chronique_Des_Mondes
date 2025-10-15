# US-004 - Gestion du Profil Utilisateur

## 📝 Description

**En tant qu'** utilisateur connecté  
**Je veux** consulter et modifier mon profil (nom, avatar, préférences)  
**Afin de** personnaliser mon expérience sur la plateforme

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mon Profil" accessible depuis le menu utilisateur
- [ ] Affichage des informations : email, nom d'utilisateur, avatar, date d'inscription
- [ ] Modification possible : nom d'utilisateur, avatar
- [ ] L'email ne peut pas être modifié (pour MVP)
- [ ] Validation : nom d'utilisateur unique, 3-30 caractères
- [ ] Upload d'avatar (formats: jpg, png, max 2MB)
- [ ] Aperçu de l'avatar avant sauvegarde
- [ ] Message de confirmation après sauvegarde
- [ ] Préférences : thème (clair/sombre), notifications

### Techniques
- [ ] Endpoint : `GET /api/users/profile`
- [ ] Response : `{ "id": "guid", "email": "user@example.com", "username": "Joueur123", "avatarUrl": "/uploads/avatars/guid.jpg", "createdAt": "2025-10-15", "preferences": { "theme": "dark", "notifications": true } }`
- [ ] Endpoint : `PUT /api/users/profile`
- [ ] Body : `{ "username": "NouveauNom", "avatar": "base64_image", "preferences": {...} }`
- [ ] Response 200 : Profile mis à jour
- [ ] Response 400 : Validation échouée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `UserService.GetProfile_WithValidUser_ReturnsProfile()`
- [ ] `UserService.UpdateProfile_WithValidData_UpdatesDatabase()`
- [ ] `UserService.UpdateProfile_WithDuplicateUsername_ThrowsException()`
- [ ] `AvatarService.ValidateImage_WithValidFormat_ReturnsTrue()`
- [ ] `AvatarService.ValidateImage_WithTooBigFile_ThrowsFalse()`

### Tests d'Intégration
- [ ] `ProfileEndpoint_GetProfile_ReturnsUserData()`
- [ ] `ProfileEndpoint_UpdateProfile_SavesChanges()`
- [ ] `ProfileEndpoint_UploadAvatar_SavesFile()`

### Tests E2E
- [ ] Consultation profil → Modification nom → Sauvegarde → Vérification affichage
- [ ] Upload avatar → Aperçu → Sauvegarde → Vérification affichage

---

## 🔧 Tâches Techniques

### Backend
- [ ] Ajouter colonnes à `Users` : `Username`, `AvatarUrl`, `Preferences` (JSON)
- [ ] Créer `ProfileResponse` DTO
- [ ] Créer `UpdateProfileRequest` DTO
- [ ] Implémenter `UserService.GetProfileAsync()`
- [ ] Implémenter `UserService.UpdateProfileAsync()`
- [ ] Implémenter `AvatarService.UploadAvatarAsync()`
  - [ ] Validation format/taille
  - [ ] Génération nom unique
  - [ ] Sauvegarde dans `/uploads/avatars/`
  - [ ] Suppression ancien avatar
- [ ] Créer endpoints :
  - [ ] `GET /api/users/profile`
  - [ ] `PUT /api/users/profile`
  - [ ] `POST /api/users/avatar`
- [ ] Autorisation : JWT requis

### Frontend
- [ ] Créer page `Profile.razor` (/profile)
- [ ] Créer composant `ProfileEditor.razor`
- [ ] Créer composant `AvatarUploader.razor`
- [ ] Implémenter `UserService.GetProfileAsync()`
- [ ] Implémenter `UserService.UpdateProfileAsync()`
- [ ] Gestion upload fichier avec preview
- [ ] Validation côté client

### Base de Données
- [ ] Migration : Ajouter colonnes `Username`, `AvatarUrl`, `Preferences`
- [ ] Index sur `Username` (unique)

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

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Upload d'avatar fonctionnel
- [ ] Documentation API mise à jour
- [ ] Déployé en staging
- [ ] Mergé dans main

---

**Statut** : 🔄 En cours  
**Assigné à** : Tommy ANGIBAUD  
**Date de début** : 15 octobre 2025
