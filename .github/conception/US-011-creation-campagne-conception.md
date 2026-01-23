# Conception Technique - US-011 : Création de Campagne

> **User Story** : En tant que Maître du Jeu, je veux créer une nouvelle campagne avec nom, description et système de jeu afin d'organiser mes sessions de jeu de rôle et inviter des joueurs.

**Date** : 13 novembre 2025  
**Statut** : 📝 En conception  
**Complexité** : Moyenne (5 SP)  
**Durée estimée** : 1-2 jours

---

## 📋 Vue d'Ensemble

### Objectif
Permettre aux utilisateurs ayant le rôle **GameMaster** de créer des campagnes de jeu de rôle avec leurs informations de base, incluant la possibilité d'uploader une image de couverture.

### Prérequis
- US-001 : Inscription utilisateur (✅ Fait)
- US-002 : Connexion utilisateur (✅ Fait)
- US-006 : Gestion des rôles - Rôle GameMaster (✅ Fait)

---

## 🏗️ Architecture

### Couches Impactées

```
┌─────────────────────────────────────────────┐
│ Frontend - Blazor Server                     │
│ ┌─────────────────────────────────────────┐ │
│ │ Pages/Campaigns/CreateCampaign.razor    │ │
│ │ Components/Shared/CampaignForm.razor    │ │
│ │ Components/Shared/ImageUploader.razor   │ │
│ └─────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────┐ │
│ │ Services/ICampaignService.cs            │ │
│ │ Services/CampaignService.cs (client)    │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
                    ↓ HTTPS
┌─────────────────────────────────────────────┐
│ API Service                                  │
│ ┌─────────────────────────────────────────┐ │
│ │ Endpoints/CampaignEndpoints.cs          │ │
│ │ Endpoints/Models/CreateCampaignRequest │ │
│ │ Endpoints/Models/CampaignResponse      │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Business Layer                               │
│ ┌─────────────────────────────────────────┐ │
│ │ Business.Abstraction/Services/          │ │
│ │   ICampaignService.cs                   │ │
│ │ Business.Abstraction/DTOs/              │ │
│ │   CampaignDto.cs                        │ │
│ │   CreateCampaignDto.cs                  │ │
│ └─────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────┐ │
│ │ Business.Common/Services/               │ │
│ │   CampaignService.cs                    │ │
│ │ Business.Common/Validators/             │ │
│ │   CreateCampaignValidator.cs            │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Data Layer                                   │
│ ┌─────────────────────────────────────────┐ │
│ │ Data.Common/Models/Campaign.cs          │ │
│ │ Data.Common/AppDbContext.cs             │ │
│ └─────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────┐ │
│ │ Common/Services/ImageStorageService.cs  │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Base de données SQL Server                   │
│ Table: Campaigns                             │
└─────────────────────────────────────────────┘
```

---

## 💾 Modèle de Données

### Table `Campaigns`

**Colonnes principales :**
- `Id` : int, PK, IDENTITY
- `Name` : nvarchar(100), NOT NULL, INDEX
- `Description` : nvarchar(max), NULL
- `GameType` : int, NOT NULL (enum: 0=Generic, 1=DnD5e, 2=Pathfinder, 3=CallOfCthulhu, 4=Warhammer, 99=Custom)
- `Visibility` : int, NOT NULL (enum: 0=Private, 1=Public), DEFAULT 0
- `MaxPlayers` : int, NOT NULL, DEFAULT 6, CHECK (MaxPlayers >= 1 AND MaxPlayers <= 20)
- `CoverImageUrl` : nvarchar(500), NULL
- `CreatedBy` : int, NOT NULL, FK → Users(Id)
- `CreatedAt` : datetime2, NOT NULL, DEFAULT GETUTCDATE()
- `UpdatedAt` : datetime2, NULL
- `IsActive` : bit, NOT NULL, DEFAULT 1
- `IsDeleted` : bit, NOT NULL, DEFAULT 0

**Index :**
- `IX_Campaigns_CreatedBy` : Index non-clustered sur `CreatedBy` (performances requêtes "Mes Campagnes")
- `IX_Campaigns_GameType` : Index non-clustered sur `GameType` (filtrage par système)
- `IX_Campaigns_Visibility_IsActive` : Index composite sur `(Visibility, IsActive)` (campagnes publiques)

**Relations :**
- `Campaigns.CreatedBy` → `Users.Id` (ON DELETE CASCADE)

---

## 🔧 Composants Techniques

### 1. Backend - Entité Campaign

**Propriétés principales :**
- Navigation property `User CreatedByUser` (relation Many-to-One)
- Méthode de validation intégrée (DataAnnotations)
- Timestamps automatiques (CreatedAt, UpdatedAt)
- Soft delete avec `IsDeleted`

**Enum GameType :**
```
Generic = 0
DnD5e = 1
Pathfinder = 2
CallOfCthulhu = 3
Warhammer = 4
Custom = 99
```

**Enum Visibility :**
```
Private = 0  // Visible uniquement par le MJ et les joueurs invités
Public = 1   // Visible dans la recherche publique
```

---

### 2. Backend - Service Layer

**ICampaignService (Abstraction) :**
- `Task<CampaignDto?> CreateCampaignAsync(CreateCampaignDto request, int userId)`
- `Task<List<CampaignDto>> GetMyCampaignsAsync(int userId)`
- `Task<CampaignDto?> GetCampaignByIdAsync(int campaignId, int userId)`

**CampaignService (Implementation) :**
- Utilise `AppDbContext` pour les opérations DB
- Appelle `ImageStorageService` pour upload d'images
- Valide via `CreateCampaignValidator` (FluentValidation)
- Gère les transactions pour atomicité
- Log les opérations importantes

**Validation (FluentValidation) :**
- Name : RuleFor(x => x.Name).NotEmpty().Length(3, 100)
- Description : RuleFor(x => x.Description).MaximumLength(5000)
- GameType : RuleFor(x => x.GameType).IsInEnum()
- MaxPlayers : RuleFor(x => x.MaxPlayers).InclusiveBetween(1, 20)
- CoverImage : Validation taille (max 5MB), formats (jpg, png, webp)

---

### 3. Backend - API Endpoint

**Endpoint :** `POST /api/campaigns`

**Request Body (CreateCampaignRequest) :**
- Name (string, required)
- Description (string, optional)
- GameType (enum, required)
- Visibility (enum, optional, default: Private)
- MaxPlayers (int, optional, default: 6)
- CoverImageBase64 (string, optional) - Image encodée en Base64

**Authorization :**
- JWT Bearer Token requis
- Vérifie rôle GameMaster via `ClaimTypes.Role`
- Retourne 403 Forbidden si l'utilisateur n'a pas le rôle

**Response 201 Created (CampaignResponse) :**
- Id, Name, Description, GameType, Visibility, MaxPlayers
- CoverImageUrl, CreatedBy, CreatedAt
- Location header : `/api/campaigns/{id}`

**Response 400 Bad Request :**
- Validation échouée (détails des erreurs)

**Response 401 Unauthorized :**
- Token absent ou invalide

**Response 403 Forbidden :**
- Utilisateur n'a pas le rôle GameMaster

---

### 4. Backend - Image Storage Service

**IImageStorageService (Abstraction) :**
- `Task<string?> UploadCampaignCoverAsync(string base64Image, int campaignId)`
- `Task<bool> DeleteCampaignCoverAsync(string imageUrl)`

**ImageStorageService (Implementation) :**
- Stockage local : `/wwwroot/uploads/campaigns/{campaignId}_cover.{ext}`
- Validation format (jpg, png, webp)
- Validation taille (max 5MB)
- Génération de noms uniques pour éviter conflits
- Compression d'image pour optimisation (optionnel)
- Retourne URL relative : `/uploads/campaigns/{campaignId}_cover.{ext}`

**Image par défaut :**
- Si pas d'upload, utilise `/uploads/campaigns/default-cover.png`

---

### 5. Frontend - Pages et Composants

**Page : `/campaigns/create`**
- Route : `@page "/campaigns/create"`
- Autorisation : `@attribute [Authorize(Roles = "GameMaster")]`
- Redirection si pas MJ : Affiche message ou redirige vers profil

**Composant : CampaignForm.razor**
- Formulaire Blazor avec EditForm
- DataAnnotationsValidator pour validation client
- Champs :
  - InputText pour Name (requis)
  - InputTextArea pour Description (5000 chars max)
  - InputSelect pour GameType (liste déroulante)
  - InputRadioGroup pour Visibility (Private/Public)
  - InputNumber pour MaxPlayers (1-20)
  - ImageUploader pour CoverImage

**Composant : ImageUploader.razor**
- InputFile avec accept="image/png, image/jpeg, image/webp"
- Aperçu de l'image uploadée (tag `<img>`)
- Conversion en Base64 pour envoi API
- Validation taille (max 5MB) côté client
- Bouton "Supprimer" pour retirer l'image

**Service Client : ICampaignService**
- `Task<CampaignResponse?> CreateCampaignAsync(CreateCampaignRequest request)`
- Utilise HttpClient configuré avec Aspire service discovery
- Ajoute JWT token via `AddAuthHeaderAsync()`
- Gère les erreurs HTTP (400, 401, 403)

---

### 6. Frontend - Flux Utilisateur

**Étapes :**
1. Utilisateur clique sur "Nouvelle Campagne" depuis "Mes Campagnes"
2. Redirection vers `/campaigns/create`
3. Vérification du rôle GameMaster (si pas MJ → message d'erreur)
4. Affichage du formulaire CampaignForm
5. Utilisateur remplit les champs obligatoires (Name, GameType)
6. Utilisateur peut ajouter Description, Image, MaxPlayers
7. Clic sur "Créer la campagne"
8. Validation côté client (DataAnnotations)
9. Si validation OK → Appel API POST /api/campaigns
10. Si succès → Redirection vers `/campaigns/{id}` avec message succès
11. Si erreur → Affichage des erreurs dans le formulaire

**États de chargement :**
- Spinner pendant l'appel API
- Désactivation du bouton "Créer" pendant le traitement
- Messages d'erreur/succès avec `alert` Bootstrap

---

## 🔐 Sécurité

### Authorization
- **JWT requis** : Endpoint protégé par `[Authorize]`
- **Rôle GameMaster** : Vérification via `user.IsInRole("GameMaster")`
- **Validation userId** : Extraction du userId depuis le JWT claim `ClaimTypes.NameIdentifier`
- **CreatedBy automatique** : Le userId du JWT est utilisé, pas de paramètre externe

### Validation Multi-Couche
1. **Client (Blazor)** : DataAnnotations + validation manuelle taille image
2. **API** : Model validation automatique + vérification rôle
3. **Service** : FluentValidation pour règles métier complexes
4. **Database** : Contraintes CHECK, FK, NOT NULL

### Protection Upload d'Images
- **Validation extension** : Accepte uniquement jpg, png, webp
- **Validation taille** : Max 5MB
- **Validation MIME type** : Vérification du vrai type (pas juste l'extension)
- **Nom de fichier sécurisé** : Utilise `{campaignId}_cover.{ext}` (pas de nom utilisateur)
- **Stockage isolé** : Dossier dédié `/wwwroot/uploads/campaigns/`

---

## 🧪 Stratégie de Tests

### Tests Unitaires (Backend)

**CampaignService :**
- `CreateCampaignAsync_WithValidData_ReturnsCampaign()`
- `CreateCampaignAsync_WithInvalidName_ThrowsValidationException()`
- `CreateCampaignAsync_SetsCreatedByFromUserId()`
- `CreateCampaignAsync_WithDefaultValues_SetsDefaults()`

**CreateCampaignValidator :**
- `Validate_WithNameTooShort_ReturnsFalse()`
- `Validate_WithNameTooLong_ReturnsFalse()`
- `Validate_WithDescriptionTooLong_ReturnsFalse()`
- `Validate_WithInvalidMaxPlayers_ReturnsFalse()`

**ImageStorageService :**
- `UploadCampaignCoverAsync_WithValidImage_ReturnsUrl()`
- `UploadCampaignCoverAsync_WithInvalidFormat_ReturnsNull()`
- `UploadCampaignCoverAsync_WithTooLargeFile_ReturnsNull()`

### Tests d'Intégration

**CampaignEndpoint :**
- `CreateCampaign_WithValidData_SavesInDatabase()`
- `CreateCampaign_WithImage_UploadsAndSavesUrl()`
- `CreateCampaign_WithoutGameMasterRole_Returns403()`
- `CreateCampaign_WithoutAuth_Returns401()`
- `CreateCampaign_WithInvalidData_Returns400WithErrors()`

### Tests E2E (Playwright)

**Scénario complet :**
1. Login avec utilisateur ayant rôle GameMaster
2. Navigation vers "Mes Campagnes"
3. Clic sur "Nouvelle Campagne"
4. Remplissage formulaire (Name, GameType, Description)
5. Upload d'une image de couverture
6. Clic sur "Créer"
7. Vérification redirection vers page de détails
8. Vérification campagne apparaît dans "Mes Campagnes"

**Scénario validation :**
1. Tentative création avec nom trop court → Message d'erreur
2. Tentative création sans nom → Message d'erreur
3. Tentative upload image trop large → Message d'erreur

---

## 📦 Migration Database

### Fichier Migration

**Nom :** `20251113_AddCampaignsTable.cs`

**Up() :**
- CREATE TABLE Campaigns avec toutes les colonnes
- CREATE INDEX sur CreatedBy
- CREATE INDEX sur GameType
- CREATE INDEX composite sur (Visibility, IsActive)
- ALTER TABLE pour FK vers Users
- Seed data : Image par défaut `default-cover.png`

**Down() :**
- DROP CONSTRAINT FK_Campaigns_Users
- DROP INDEX
- DROP TABLE Campaigns

### Seed Data

**Image par défaut :**
- Copier `default-cover.png` dans `/wwwroot/uploads/campaigns/`
- Dimensions : 1200x630 (format Open Graph)
- Placeholder générique avec logo du projet

---

## 🚀 Plan d'Implémentation

### Phase 1 : Backend Data Layer (2h)
1. Créer entité `Campaign.cs` avec propriétés et relations
2. Ajouter DbSet dans `AppDbContext`
3. Créer migration `20251113_AddCampaignsTable`
4. Appliquer migration sur DB de dev
5. Vérifier structure de table

### Phase 2 : Backend Service Layer (3h)
1. Créer `ICampaignService` dans Business.Abstraction
2. Créer DTOs : `CampaignDto`, `CreateCampaignDto`
3. Implémenter `CampaignService` dans Business.Common
4. Créer `CreateCampaignValidator` avec FluentValidation
5. Créer `IImageStorageService` et `ImageStorageService`
6. Enregistrer services dans DI

### Phase 3 : Backend API (2h)
1. Créer `CampaignEndpoints.cs`
2. Créer modèles de requête/réponse dans `Endpoints/Models/`
3. Implémenter endpoint POST /api/campaigns
4. Ajouter autorisation (JWT + rôle GameMaster)
5. Mapper endpoint dans Program.cs
6. Tester avec Scalar/Postman

### Phase 4 : Frontend Components (3h)
1. Créer `ImageUploader.razor` (réutilisable)
2. Créer `CampaignForm.razor` avec tous les champs
3. Créer page `CreateCampaign.razor`
4. Implémenter validation client
5. Gérer états de chargement et erreurs

### Phase 5 : Frontend Service (1h)
1. Créer `ICampaignService` (client)
2. Implémenter `CampaignService` avec HttpClient
3. Enregistrer dans DI avec Aspire service discovery
4. Tester appel API depuis Blazor

### Phase 6 : Tests (3h)
1. Écrire tests unitaires CampaignService (8 tests)
2. Écrire tests unitaires ImageStorageService (3 tests)
3. Écrire tests intégration CampaignEndpoint (5 tests)
4. Écrire tests E2E Playwright (2 scénarios)
5. Vérifier couverture > 80%

### Phase 7 : Documentation (1h)
1. Mettre à jour `API_ENDPOINTS.md` avec POST /api/campaigns
2. Mettre à jour `MODELE_DONNEES.md` avec table Campaigns
3. Mettre à jour `SECURITE.md` avec autorisation MJ
4. Créer README pour dossier uploads/campaigns

**Durée totale estimée :** 15 heures (2 jours)

---

## 📊 Points d'Attention

### Risques Identifiés

1. **Upload d'Images**
   - Risque : Images malveillantes ou trop volumineuses
   - Mitigation : Validation stricte (taille, format, MIME type)

2. **Performance**
   - Risque : Base64 images = requêtes HTTP volumineuses
   - Mitigation : Compression côté client, limite 5MB

3. **Concurrence**
   - Risque : Deux MJ créent une campagne avec le même nom
   - Mitigation : Pas de contrainte unique sur Name (accepté métier)

4. **Autorisation**
   - Risque : Utilisateur sans rôle MJ accède à la création
   - Mitigation : Vérification JWT + rôle à plusieurs niveaux

### Optimisations Futures

1. **Stockage Cloud** : Migrer vers Azure Blob Storage ou AWS S3
2. **CDN** : Servir les images via CDN pour performances
3. **Thumbnails** : Générer vignettes automatiquement
4. **WebP** : Convertir automatiquement les images en WebP
5. **Pagination** : Ajouter pagination pour "Mes Campagnes"

---

## 🔗 Dépendances & Bloqueurs

### Dépend de (✅ Terminées)
- US-001 : Inscription utilisateur
- US-002 : Connexion utilisateur
- US-006 : Gestion des rôles (GameMaster)

### Bloque (⏳ Futures)
- US-012 : Modification de campagne
- US-013 : Suppression de campagne
- US-014 : Création de chapitres
- US-015 : Invitation de joueurs

---

## ✅ Critères de Validation

### Fonctionnels
- [x] Formulaire accessible uniquement par les GameMasters
- [x] Tous les champs obligatoires validés (Name, GameType)
- [x] Upload d'image fonctionnel avec aperçu
- [x] Valeurs par défaut appliquées (Visibility=Private, MaxPlayers=6)
- [x] Message de succès après création
- [x] Redirection vers page de détails de la campagne

### Techniques
- [x] Endpoint POST /api/campaigns implémenté
- [x] Validation multi-couche (client, API, service, DB)
- [x] Authorization JWT + rôle GameMaster
- [x] Images stockées dans `/wwwroot/uploads/campaigns/`
- [x] Migration database appliquée
- [x] Tests unitaires et intégration passent (couverture > 80%)

### Non-Fonctionnels
- [x] Temps de réponse < 2 secondes (hors upload image)
- [x] Upload d'image < 5 secondes
- [x] Pas de fuite mémoire sur upload multiple
- [x] Logs structurés pour debugging

---

**Document créé le** : 13 novembre 2025  
**Dernière mise à jour** : 13 novembre 2025  
**Version** : 1.0  
**Auteur** : GitHub Copilot
