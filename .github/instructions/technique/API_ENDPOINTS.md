# API Endpoints - Chronique des Mondes

## Vue d'ensemble

L'API REST est construite avec **ASP.NET Core Minimal API** et organisée par domaines fonctionnels. Chaque groupe d'endpoints est défini dans un fichier dédié (ex: `CharacterEndpoints.cs`).

### Principes de conception

1. **Minimal API** : Endpoints groupés par domaine avec extension methods
2. **Header X-Game-Type** : Détermine la couche business à utiliser (Generic/Dnd/Skyrim)
3. **Authorization policies** : Validation fine des permissions (IsCharacterOwner, IsGameMaster, etc.)
4. **Result<T> pattern** : Retour standardisé avec gestion d'erreurs
5. **Validation multi-couches** : FluentValidation + Business Layer + EF Query Filters

---

## 1. Authentification

### 1.1 POST /api/auth/register

Création d'un nouveau compte utilisateur.

**Endpoint :** `POST /api/auth/register`  
**Authorization :** Aucune (public)

**Request Body :**
```json
{
  "username": "DragonMaster42",
  "email": "player@example.com",
  "password": "SecureP@ssw0rd!"
}
```

**Response 201 Created :**
```json
{
  "userId": 123,
  "username": "DragonMaster42",
  "email": "player@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response 400 Bad Request :**
```json
{
  "error": "Email already exists",
  "details": ["Un compte avec cet email existe déjà"]
}
```

**Implémentation :**
```csharp
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");
        
        group.MapPost("/register", async (
            RegisterRequest request,
            IUserService userService,
            IPasswordService passwordService,
            IJwtService jwtService) =>
        {
            // 1. Valider le modèle
            var validator = new RegisterRequestValidator();
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors);
            
            // 2. Vérifier si l'email existe
            var existingUser = await userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return Results.BadRequest(new { error = "Email already exists" });
            
            // 3. Hash du mot de passe
            var passwordHash = passwordService.HashPassword(request.Password);
            
            // 4. Créer l'utilisateur
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
            };
            
            var result = await userService.CreateAsync(user);
            if (!result.IsSuccess)
                return Results.BadRequest(result.Error);
            
            // 5. Générer le token JWT
            var token = jwtService.GenerateToken(result.Data.Id, result.Data.Email);
            
            return Results.Created($"/api/users/{result.Data.Id}", new
            {
                userId = result.Data.Id,
                username = result.Data.Username,
                email = result.Data.Email,
                token
            });
        });
    }
}
```

---

### 1.2 POST /api/auth/login

Connexion utilisateur.

**Endpoint :** `POST /api/auth/login`  
**Authorization :** Aucune (public)

**Request Body :**
```json
{
  "email": "player@example.com",
  "password": "SecureP@ssw0rd!"
}
```

**Response 200 OK :**
```json
{
  "userId": 123,
  "username": "DragonMaster42",
  "email": "player@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-22T14:30:00Z"
}
```

**Response 401 Unauthorized :**
```json
{
  "error": "Invalid credentials"
}
```

---

### 1.3 POST /api/auth/refresh

Rafraîchissement du token JWT.

**Endpoint :** `POST /api/auth/refresh`  
**Authorization :** Bearer token (expiré mais valide)

**Response 200 OK :**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-22T14:30:00Z"
}
```

---

## 2. Profil Utilisateur

### 2.1 GET /api/users/profile

Récupération du profil de l'utilisateur connecté.

**Endpoint :** `GET /api/users/profile`  
**Authorization :** Bearer token (requis)

**Response 200 OK :**
```json
{
  "id": 123,
  "email": "player@example.com",
  "nickname": "DragonMaster42",
  "username": "dragonmaster",
  "avatarUrl": "/uploads/avatars/123_avatar.jpg",
  "preferences": "{\"theme\":\"dark\",\"notifications\":{\"email\":true,\"inApp\":true}}",
  "createdAt": "2025-01-15T10:00:00Z"
}
```

**Response 401 Unauthorized :**
```json
{
  "error": "Unauthorized",
  "details": "Token JWT invalide ou expiré"
}
```

**Response 404 Not Found :**
```json
{
  "error": "Profile not found",
  "details": "Aucun profil trouvé pour cet utilisateur"
}
```

**Implémentation :**
```csharp
group.MapGet("/profile", async (
    ClaimsPrincipal user,
    IUserProfileService profileService) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        return Results.Unauthorized();
    
    var profile = await profileService.GetProfileAsync(userId);
    if (profile == null)
        return Results.NotFound(new { error = "Profile not found" });
    
    return Results.Ok(profile);
})
.RequireAuthorization()
.WithName("GetUserProfile")
.WithOpenApi();
```

---

### 2.2 PUT /api/users/profile

Mise à jour du profil de l'utilisateur connecté.

**Endpoint :** `PUT /api/users/profile`  
**Authorization :** Bearer token (requis)

**Request Body :**
```json
{
  "username": "newusername",
  "preferences": "{\"theme\":\"light\",\"notifications\":{\"email\":false,\"inApp\":true}}"
}
```

**Response 200 OK :**
```json
{
  "id": 123,
  "email": "player@example.com",
  "nickname": "DragonMaster42",
  "username": "newusername",
  "avatarUrl": "/uploads/avatars/123_avatar.jpg",
  "preferences": "{\"theme\":\"light\",\"notifications\":{\"email\":false,\"inApp\":true}}",
  "createdAt": "2025-01-15T10:00:00Z"
}
```

**Response 400 Bad Request :**
```json
{
  "error": "Username already taken",
  "details": "Ce nom d'utilisateur est déjà utilisé par un autre compte"
}
```

**Response 401 Unauthorized :** (voir section 2.1)

**Validation :**
- `username` : 3-30 caractères, optionnel, unique
- `preferences` : JSON valide, optionnel

---

### 2.3 POST /api/users/avatar

Upload d'un avatar pour l'utilisateur connecté.

**Endpoint :** `POST /api/users/avatar`  
**Authorization :** Bearer token (requis)  
**Content-Type :** `multipart/form-data`

**Request Body (Form Data) :**
- `avatar` : Fichier image (JPG, JPEG, PNG)

**Response 200 OK :**
```json
{
  "avatarUrl": "/uploads/avatars/123_avatar.jpg",
  "message": "Avatar uploaded successfully"
}
```

**Response 400 Bad Request :**
```json
{
  "error": "Invalid file format",
  "details": "Formats acceptés: JPG, PNG. Taille maximale: 2 MB"
}
```

**Response 401 Unauthorized :** (voir section 2.1)

**Validation :**
- Formats acceptés : `.jpg`, `.jpeg`, `.png`
- Taille maximale : 2 MB
- Stockage : `/wwwroot/uploads/avatars/{userId}_avatar.{ext}`

**Implémentation :**
```csharp
group.MapPost("/avatar", async (
    HttpContext httpContext,
    ClaimsPrincipal user,
    IAvatarService avatarService,
    AppDbContext dbContext) =>
{
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        return Results.Unauthorized();

    var form = await httpContext.Request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    
    if (file == null)
        return Results.BadRequest(new { error = "No file provided" });

    if (!avatarService.ValidateAvatarFile(file, out var errorMessage))
        return Results.BadRequest(new { error = errorMessage });

    var avatarUrl = await avatarService.UploadAvatarAsync(userId, file);
    if (avatarUrl == null)
        return Results.BadRequest(new { error = "Failed to upload avatar" });

    var userEntity = await dbContext.Users.FindAsync(userId);
    if (userEntity != null)
    {
        userEntity.AvatarUrl = avatarUrl;
        await dbContext.SaveChangesAsync();
    }

    return Results.Ok(new { avatarUrl, message = "Avatar uploaded successfully" });
})
.RequireAuthorization()
.WithName("UploadUserAvatar")
.WithOpenApi();
```

---

## 3. Personnages (Characters)

### 2.1 GET /api/characters

Liste les personnages de l'utilisateur connecté.

**Endpoint :** `GET /api/characters`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 0` (0=Generic, 1=Dnd5e, 2=Skyrim)

**Query Parameters :**
- `gameType` (int, optional) : Filtrer par type de jeu
- `includeDeleted` (bool, default=false) : Inclure les personnages supprimés

**Response 200 OK :**
```json
{
  "characters": [
    {
      "id": 42,
      "name": "Gandalf le Gris",
      "gameType": 1,
      "characterType": "Dnd5e",
      "level": 15,
      "currentHealth": 78,
      "maxHealth": 95,
      "imageUrl": "https://example.com/gandalf.jpg",
      "createdAt": "2025-10-01T10:00:00Z"
    },
    {
      "id": 43,
      "name": "Aragorn",
      "gameType": 1,
      "characterType": "Dnd5e",
      "level": 12,
      "currentHealth": 105,
      "maxHealth": 105,
      "imageUrl": "https://example.com/aragorn.jpg",
      "createdAt": "2025-10-05T14:30:00Z"
    }
  ],
  "total": 2
}
```

---

### 2.2 GET /api/characters/{id}

Récupère un personnage spécifique.

**Endpoint :** `GET /api/characters/{id}`  
**Authorization :** Bearer token + Policy `IsCharacterOwner`  
**Headers :** `X-Game-Type: 1` (Dnd5e)

**Response 200 OK :**
```json
{
  "id": 42,
  "name": "Gandalf le Gris",
  "gameType": 1,
  "characterType": "Dnd5e",
  "level": 15,
  "experience": 45000,
  "currentHealth": 78,
  "maxHealth": 95,
  "description": "Un vieux magicien sage et puissant",
  "imageUrl": "https://example.com/gandalf.jpg",
  "attributes": {
    "race": "Human",
    "class": "Wizard",
    "subclass": "School of Evocation",
    "abilityScores": {
      "STR": 10,
      "DEX": 12,
      "CON": 14,
      "INT": 20,
      "WIS": 18,
      "CHA": 16
    },
    "proficiencyBonus": 5,
    "armorClass": 15,
    "speed": 30,
    "hitDice": "1d6",
    "savingThrows": ["INT", "WIS"],
    "skills": ["Arcana", "History", "Insight", "Investigation"]
  },
  "createdAt": "2025-10-01T10:00:00Z",
  "updatedAt": "2025-10-15T08:20:00Z"
}
```

**Response 403 Forbidden :**
```json
{
  "error": "You do not have permission to access this character"
}
```

---

### 2.3 POST /api/characters

Création d'un nouveau personnage.

**Endpoint :** `POST /api/characters`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 1` (Dnd5e)

**Request Body (Generic) :**
```json
{
  "name": "Héros Mystérieux",
  "gameType": 0,
  "maxHealth": 100,
  "description": "Un héros générique",
  "attributes": {
    "customStats": {
      "Force": 15,
      "Agilité": 12,
      "Intelligence": 10
    }
  }
}
```

**Request Body (D&D 5e) :**
```json
{
  "name": "Legolas",
  "gameType": 1,
  "maxHealth": 65,
  "description": "Archer elfe agile",
  "attributes": {
    "race": "Elf",
    "class": "Ranger",
    "subclass": "Hunter",
    "abilityScores": {
      "STR": 12,
      "DEX": 20,
      "CON": 14,
      "INT": 10,
      "WIS": 16,
      "CHA": 12
    },
    "proficiencyBonus": 3,
    "armorClass": 17,
    "speed": 35,
    "hitDice": "1d10"
  }
}
```

**Response 201 Created :**
```json
{
  "id": 44,
  "name": "Legolas",
  "gameType": 1,
  "characterType": "Dnd5e",
  "level": 1,
  "currentHealth": 65,
  "maxHealth": 65,
  "createdAt": "2025-10-15T09:00:00Z"
}
```

**Implémentation avec routing multi-type :**
```csharp
group.MapPost("/", async (
    HttpContext httpContext,
    CreateCharacterRequest request,
    ICharacterServiceFactory characterServiceFactory) =>
{
    // 1. Récupérer le GameType depuis le header
    var gameType = httpContext.Items["GameType"] as GameType?;
    if (gameType == null)
        return Results.BadRequest(new { error = "X-Game-Type header is required" });
    
    // 2. Sélectionner le bon service
    var characterService = characterServiceFactory.GetService(gameType.Value);
    
    // 3. Créer le personnage avec la logique spécifique
    var result = await characterService.CreateAsync(request);
    
    if (!result.IsSuccess)
        return Results.BadRequest(result.Error);
    
    return Results.Created($"/api/characters/{result.Data.Id}", result.Data);
})
.RequireAuthorization();
```

---

### 2.4 PUT /api/characters/{id}

Mise à jour d'un personnage.

**Endpoint :** `PUT /api/characters/{id}`  
**Authorization :** Bearer token + Policy `IsCharacterOwner`  
**Headers :** `X-Game-Type: 1`

**Request Body :**
```json
{
  "name": "Gandalf le Blanc",
  "description": "Revenu plus puissant que jamais",
  "currentHealth": 95,
  "maxHealth": 110,
  "level": 16,
  "attributes": {
    "subclass": "School of Evocation",
    "abilityScores": {
      "INT": 22,
      "WIS": 20
    }
  }
}
```

**Response 200 OK :**
```json
{
  "id": 42,
  "name": "Gandalf le Blanc",
  "level": 16,
  "updatedAt": "2025-10-15T10:00:00Z"
}
```

---

### 2.5 DELETE /api/characters/{id}

Suppression logique d'un personnage (soft delete).

**Endpoint :** `DELETE /api/characters/{id}`  
**Authorization :** Bearer token + Policy `IsCharacterOwner`

**Response 204 No Content**

---

## 4. Campagnes (Campaigns)

### 4.1 GET /api/campaigns

Liste les campagnes accessibles (créées ou participant).

**Endpoint :** `GET /api/campaigns`  
**Authorization :** Bearer token

**Query Parameters :**
- `gameType` (int, optional) : Filtrer par type de jeu
- `isActive` (bool, default=true) : Inclure seulement les campagnes actives

**Response 200 OK :**
```json
{
  "campaigns": [
    {
      "id": 10,
      "name": "La Quête de l'Anneau",
      "gameType": 1,
      "gameMaster": {
        "id": 5,
        "username": "DungeonMaster"
      },
      "description": "Une épopée en Terre du Milieu",
      "isActive": true,
      "participantCount": 4,
      "createdAt": "2025-09-01T12:00:00Z"
    }
  ],
  "total": 1
}
```

---

### 3.2 POST /api/campaigns

Création d'une nouvelle campagne.

**Endpoint :** `POST /api/campaigns`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 1`

**Request Body :**
```json
{
  "name": "Les Mines de la Moria",
  "gameType": 1,
  "description": "Exploration des profondeurs dangereuses",
  "imageUrl": "https://example.com/moria.jpg"
}
```

**Response 201 Created :**
```json
{
  "id": 11,
  "name": "Les Mines de la Moria",
  "gameType": 1,
  "gameMasterId": 123,
  "isActive": true,
  "createdAt": "2025-10-15T10:30:00Z"
}
```

---

### 3.3 POST /api/campaigns/{id}/characters

Ajouter un personnage à une campagne.

**Endpoint :** `POST /api/campaigns/{id}/characters`  
**Authorization :** Bearer token + Policy `IsGameMaster` OR `IsCharacterOwner`

**Request Body :**
```json
{
  "characterId": 42
}
```

**Response 200 OK :**
```json
{
  "campaignId": 10,
  "characterId": 42,
  "joinedAt": "2025-10-15T11:00:00Z"
}
```

**Response 400 Bad Request :**
```json
{
  "error": "GameType mismatch",
  "details": ["Le personnage (Dnd5e) n'est pas compatible avec cette campagne (Generic)"]
}
```

**Validation métier :**
```csharp
var campaign = await _campaignRepository.GetByIdAsync(campaignId);
var character = await _characterRepository.GetByIdAsync(characterId);

// Vérifier la compatibilité GameType
if (campaign.GameType != character.GameType)
{
    return Result<T>.Failure(
        "GameType mismatch", 
        $"Le personnage ({character.GameType}) n'est pas compatible avec cette campagne ({campaign.GameType})"
    );
}
```

---

## 4. Sessions

### 4.1 POST /api/sessions

Créer une session de jeu.

**Endpoint :** `POST /api/sessions`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Request Body :**
```json
{
  "campaignId": 10,
  "name": "Session 5 - La Bataille du Gouffre de Helm",
  "scheduledDate": "2025-10-20T19:00:00Z"
}
```

**Response 201 Created :**
```json
{
  "id": 50,
  "campaignId": 10,
  "name": "Session 5 - La Bataille du Gouffre de Helm",
  "status": 0,
  "scheduledDate": "2025-10-20T19:00:00Z",
  "createdAt": "2025-10-15T12:00:00Z"
}
```

---

### 4.2 POST /api/sessions/{id}/start

Démarrer une session.

**Endpoint :** `POST /api/sessions/{id}/start`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Response 200 OK :**
```json
{
  "id": 50,
  "status": 1,
  "startedAt": "2025-10-15T14:00:00Z"
}
```

**Effet secondaire :**
- Envoie une notification SignalR à tous les participants : `SessionHub.SessionStarted(sessionId)`

---

### 4.3 POST /api/sessions/{id}/invitations

Inviter un joueur par email.

**Endpoint :** `POST /api/sessions/{id}/invitations`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Request Body :**
```json
{
  "email": "newplayer@example.com"
}
```

**Response 201 Created :**
```json
{
  "invitationId": 100,
  "email": "newplayer@example.com",
  "token": "abc123xyz789",
  "expiresAt": "2025-10-22T12:00:00Z",
  "status": 0
}
```

**Effet secondaire :**
- Email envoyé via Azure Communication Services avec lien d'acceptation

---

## 5. Combats

### 5.1 POST /api/sessions/{sessionId}/combats

Créer un combat dans une session.

**Endpoint :** `POST /api/sessions/{sessionId}/combats`  
**Authorization :** Bearer token + Policy `IsGameMaster`  
**Headers :** `X-Game-Type: 1`

**Request Body :**
```json
{
  "name": "Attaque des Orcs",
  "participants": [
    {
      "characterId": 42,
      "initiativeRoll": 18
    },
    {
      "characterId": 43,
      "initiativeRoll": 15
    },
    {
      "npcName": "Orc Chef",
      "maxHealth": 45,
      "currentHealth": 45,
      "initiativeRoll": 12,
      "isNpc": true
    }
  ]
}
```

**Response 201 Created :**
```json
{
  "id": 75,
  "sessionId": 50,
  "name": "Attaque des Orcs",
  "status": 0,
  "currentRound": 1,
  "participants": [
    {
      "id": 200,
      "characterId": 42,
      "name": "Gandalf le Gris",
      "initiativeRoll": 18,
      "initiativeOrder": 20,
      "currentHealth": 78,
      "maxHealth": 95,
      "isActive": true
    },
    {
      "id": 201,
      "characterId": 43,
      "name": "Aragorn",
      "initiativeRoll": 15,
      "initiativeOrder": 17,
      "currentHealth": 105,
      "maxHealth": 105,
      "isActive": true
    },
    {
      "id": 202,
      "npcName": "Orc Chef",
      "initiativeRoll": 12,
      "initiativeOrder": 12,
      "currentHealth": 45,
      "maxHealth": 45,
      "isNpc": true,
      "isActive": true
    }
  ],
  "createdAt": "2025-10-15T15:00:00Z"
}
```

**Calcul de l'ordre d'initiative (D&D 5e) :**
```csharp
// Dans DndCombatService
foreach (var participant in participants)
{
    if (participant.CharacterId.HasValue)
    {
        var character = await _characterRepository.GetByIdAsync(participant.CharacterId.Value);
        var dexModifier = (character.Attributes.abilityScores.DEX - 10) / 2;
        participant.InitiativeOrder = participant.InitiativeRoll + dexModifier;
    }
    else
    {
        // PNJ : pas de bonus
        participant.InitiativeOrder = participant.InitiativeRoll;
    }
}

// Trier par InitiativeOrder décroissant
var sortedParticipants = participants.OrderByDescending(p => p.InitiativeOrder).ToList();
```

---

### 5.2 POST /api/combats/{id}/start

Démarrer un combat.

**Endpoint :** `POST /api/combats/{id}/start`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Response 200 OK :**
```json
{
  "id": 75,
  "status": 1,
  "currentRound": 1,
  "currentTurn": 0,
  "startedAt": "2025-10-15T15:05:00Z"
}
```

**Effet secondaire :**
- Notification SignalR : `CombatHub.CombatStarted(combatId, currentParticipantId)`

---

### 5.3 POST /api/combats/{id}/next-turn

Passer au tour suivant.

**Endpoint :** `POST /api/combats/{id}/next-turn`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Response 200 OK :**
```json
{
  "id": 75,
  "currentRound": 1,
  "currentTurn": 1,
  "currentParticipant": {
    "id": 201,
    "name": "Aragorn",
    "initiativeOrder": 17
  }
}
```

**Effet secondaire :**
- Notification SignalR : `CombatHub.TurnChanged(combatId, newParticipantId)`

---

### 5.4 POST /api/combats/{id}/damage

Infliger des dégâts à un participant.

**Endpoint :** `POST /api/combats/{id}/damage`  
**Authorization :** Bearer token + Policy `IsGameMaster`

**Request Body :**
```json
{
  "participantId": 202,
  "damage": 25,
  "damageType": "slashing"
}
```

**Response 200 OK :**
```json
{
  "participantId": 202,
  "previousHealth": 45,
  "currentHealth": 20,
  "maxHealth": 45,
  "isActive": true
}
```

**Effet secondaire :**
- Notification SignalR : `CombatHub.ParticipantDamaged(combatId, participantId, damage, currentHealth)`

---

## 6. Sorts (Spells)

### 6.1 GET /api/spells

Liste des sorts disponibles (bibliothèque officielle + homebrew).

**Endpoint :** `GET /api/spells`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 1`

**Query Parameters :**
- `level` (int, optional) : Filtrer par niveau de sort
- `school` (string, optional) : Filtrer par école (D&D)
- `isOfficial` (bool, optional) : true=officiel, false=homebrew
- `search` (string, optional) : Recherche par nom

**Response 200 OK :**
```json
{
  "spells": [
    {
      "id": 1,
      "name": "Fireball",
      "level": 3,
      "school": "Evocation",
      "castingTime": "1 action",
      "range": "150 feet",
      "components": "V, S, M (a tiny ball of bat guano and sulfur)",
      "duration": "Instantaneous",
      "description": "A bright streak flashes from your pointing finger...",
      "isOfficial": true
    }
  ],
  "total": 1
}
```

---

### 6.2 POST /api/spells

Créer un sort homebrew.

**Endpoint :** `POST /api/spells`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 1`

**Request Body :**
```json
{
  "name": "Invocation de Dragon",
  "level": 9,
  "school": "Conjuration",
  "castingTime": "1 action",
  "range": "60 feet",
  "components": "V, S, M (une écaille de dragon ancien)",
  "duration": "Concentration, jusqu'à 1 minute",
  "description": "Vous invoquez un dragon spectral qui obéit à vos ordres...",
  "gameSpecificData": {
    "ritual": false,
    "concentration": true
  }
}
```

**Response 201 Created :**
```json
{
  "id": 500,
  "name": "Invocation de Dragon",
  "isOfficial": false,
  "createdBy": 123,
  "createdAt": "2025-10-15T16:00:00Z"
}
```

---

### 6.3 POST /api/characters/{characterId}/spells

Ajouter un sort au personnage.

**Endpoint :** `POST /api/characters/{characterId}/spells`  
**Authorization :** Bearer token + Policy `IsCharacterOwner`

**Request Body :**
```json
{
  "spellId": 1,
  "isPrepared": true
}
```

**Response 201 Created :**
```json
{
  "characterId": 42,
  "spellId": 1,
  "isPrepared": true,
  "learnedAt": "2025-10-15T16:15:00Z"
}
```

---

## 7. Équipements (Equipment)

### 7.1 GET /api/equipment

Liste des équipements disponibles.

**Endpoint :** `GET /api/equipment`  
**Authorization :** Bearer token  
**Headers :** `X-Game-Type: 1`

**Query Parameters :**
- `type` (int, optional) : 0=Weapon, 1=Armor, 2=Item, 3=Tool
- `rarity` (int, optional) : 0=Common, 1=Uncommon, etc.
- `isOfficial` (bool, optional)

**Response 200 OK :**
```json
{
  "equipment": [
    {
      "id": 10,
      "name": "Épée longue",
      "type": 0,
      "rarity": 0,
      "weight": 3.0,
      "value": 15.0,
      "gameSpecificData": {
        "damage": "1d8",
        "damageType": "slashing",
        "properties": ["versatile"],
        "versatileDamage": "1d10"
      },
      "isOfficial": true
    }
  ],
  "total": 1
}
```

---

### 7.2 POST /api/characters/{characterId}/equipment

Ajouter un équipement à l'inventaire.

**Endpoint :** `POST /api/characters/{characterId}/equipment`  
**Authorization :** Bearer token + Policy `IsCharacterOwner`

**Request Body :**
```json
{
  "equipmentId": 10,
  "quantity": 1,
  "isEquipped": true,
  "notes": "Épée reçue du roi Théoden"
}
```

**Response 201 Created :**
```json
{
  "id": 300,
  "characterId": 42,
  "equipmentId": 10,
  "quantity": 1,
  "isEquipped": true,
  "acquiredAt": "2025-10-15T17:00:00Z"
}
```

---

### 7.3 POST /api/sessions/{sessionId}/equipment-proposals

Proposer un échange d'équipement.

**Endpoint :** `POST /api/sessions/{sessionId}/equipment-proposals`  
**Authorization :** Bearer token

**Request Body :**
```json
{
  "fromCharacterId": 42,
  "toCharacterId": 43,
  "proposedItems": [
    {"equipmentId": 10, "quantity": 1}
  ],
  "requestedItems": [
    {"equipmentId": 25, "quantity": 50}
  ]
}
```

**Response 201 Created :**
```json
{
  "id": 150,
  "status": 0,
  "createdAt": "2025-10-15T17:30:00Z"
}
```

**Effet secondaire :**
- Notification SignalR : `SessionHub.TradeProposalReceived(proposalId, toCharacterId)`

---

### 7.4 POST /api/equipment-proposals/{id}/accept

Accepter une proposition d'échange.

**Endpoint :** `POST /api/equipment-proposals/{id}/accept`  
**Authorization :** Bearer token + Policy `IsCharacterOwner` (du destinataire)

**Response 200 OK :**
```json
{
  "id": 150,
  "status": 1,
  "resolvedAt": "2025-10-15T17:35:00Z",
  "tradeId": 200
}
```

**Transaction atomique :**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // 1. Retirer les items du fromCharacter
    foreach (var item in proposal.ProposedItems)
        await _equipmentService.RemoveItemAsync(proposal.FromCharacterId, item);
    
    // 2. Ajouter les items au toCharacter
    foreach (var item in proposal.ProposedItems)
        await _equipmentService.AddItemAsync(proposal.ToCharacterId, item);
    
    // 3. Si échange (pas don), inverser
    if (proposal.RequestedItems != null)
    {
        foreach (var item in proposal.RequestedItems)
            await _equipmentService.RemoveItemAsync(proposal.ToCharacterId, item);
        
        foreach (var item in proposal.RequestedItems)
            await _equipmentService.AddItemAsync(proposal.FromCharacterId, item);
    }
    
    // 4. Créer l'historique
    var trade = new EquipmentTrade
    {
        ProposalId = proposal.Id,
        TransactionDetails = JsonSerializer.Serialize(proposal)
    };
    await _context.EquipmentTrades.AddAsync(trade);
    
    // 5. Mettre à jour le statut
    proposal.Status = ProposalStatus.Accepted;
    proposal.ResolvedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 8. Lancers de dés (Dice Rolls)

### 8.1 POST /api/dice/roll

Lancer des dés (serveur uniquement).

**Endpoint :** `POST /api/dice/roll`  
**Authorization :** Bearer token

**Request Body :**
```json
{
  "sessionId": 50,
  "characterId": 42,
  "diceNotation": "1d20+5",
  "rollType": 2,
  "context": "Attaque avec épée longue contre Orc"
}
```

**Response 200 OK :**
```json
{
  "id": 1000,
  "diceNotation": "1d20+5",
  "results": [18],
  "modifiers": 5,
  "totalResult": 23,
  "rollType": 2,
  "context": "Attaque avec épée longue contre Orc",
  "rolledAt": "2025-10-15T18:00:00Z"
}
```

**Effet secondaire :**
- Notification SignalR : `SessionHub.DiceRolled(sessionId, characterId, result)`

**Implémentation :**
```csharp
public class DiceService
{
    public DiceRollResult Roll(string notation)
    {
        // Parse "2d6+3"
        var match = Regex.Match(notation, @"(\d+)d(\d+)([\+\-]\d+)?");
        var diceCount = int.Parse(match.Groups[1].Value);
        var diceType = int.Parse(match.Groups[2].Value);
        var modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        
        var results = new List<int>();
        for (int i = 0; i < diceCount; i++)
        {
            results.Add(Random.Shared.Next(1, diceType + 1));
        }
        
        var total = results.Sum() + modifier;
        
        return new DiceRollResult
        {
            Results = results,
            Modifier = modifier,
            Total = total
        };
    }
}
```

---

### 8.2 GET /api/dice/statistics/{characterId}

Statistiques de jets pour un personnage.

**Endpoint :** `GET /api/dice/statistics/{characterId}`  
**Authorization :** Bearer token

**Query Parameters :**
- `period` (int) : 0=AllTime, 1=LastMonth, 2=LastWeek

**Response 200 OK :**
```json
{
  "characterId": 42,
  "period": 0,
  "totalRolls": 156,
  "averageResult": 12.5,
  "criticalSuccesses": 8,
  "criticalFailures": 7,
  "highestRoll": 20,
  "lowestRoll": 1,
  "lastCalculatedAt": "2025-10-15T12:00:00Z"
}
```

---

## 9. Notifications

### 9.1 GET /api/notifications

Liste des notifications de l'utilisateur.

**Endpoint :** `GET /api/notifications`  
**Authorization :** Bearer token

**Query Parameters :**
- `isRead` (bool, optional) : Filtrer par statut de lecture
- `type` (int, optional) : Filtrer par type

**Response 200 OK :**
```json
{
  "notifications": [
    {
      "id": 500,
      "type": 0,
      "title": "Nouvelle session planifiée",
      "message": "DungeonMaster a planifié 'Session 5' pour le 20 octobre",
      "relatedEntityId": 50,
      "isRead": false,
      "createdAt": "2025-10-15T12:00:00Z"
    },
    {
      "id": 501,
      "type": 2,
      "title": "Proposition d'échange",
      "message": "Aragorn vous propose un échange d'équipement",
      "relatedEntityId": 150,
      "isRead": false,
      "createdAt": "2025-10-15T17:30:00Z"
    }
  ],
  "unreadCount": 2,
  "total": 2
}
```

---

### 9.2 POST /api/notifications/{id}/read

Marquer une notification comme lue.

**Endpoint :** `POST /api/notifications/{id}/read`  
**Authorization :** Bearer token

**Response 204 No Content**

---

## 10. Middlewares et filtres

### 10.1 GameTypeMiddleware

Extrait le header `X-Game-Type` et le stocke dans `HttpContext.Items`.

```csharp
public class GameTypeMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Game-Type", out var gameTypeValue))
        {
            if (int.TryParse(gameTypeValue, out var gameTypeInt))
            {
                context.Items["GameType"] = (GameType)gameTypeInt;
            }
        }
        
        await _next(context);
    }
}
```

---

### 10.2 Authorization Handlers

**IsCharacterOwnerHandler :**
```csharp
public class IsCharacterOwnerHandler : AuthorizationHandler<IsCharacterOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsCharacterOwnerRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var characterId = httpContext.GetRouteValue("id") as int?;
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        
        var character = await _characterRepository.GetByIdAsync(characterId.Value);
        
        if (character?.UserId == userId)
        {
            context.Succeed(requirement);
        }
    }
}
```

**IsGameMasterHandler :**
```csharp
public class IsGameMasterHandler : AuthorizationHandler<IsGameMasterRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsGameMasterRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var sessionId = httpContext.GetRouteValue("sessionId") as int?;
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        
        var session = await _sessionRepository.GetByIdAsync(sessionId.Value);
        var campaign = await _campaignRepository.GetByIdAsync(session.CampaignId);
        
        if (campaign?.GameMasterId == userId)
        {
            context.Succeed(requirement);
        }
    }
}
```

---

## 11. Gestion des erreurs

### 11.1 Format de réponse standardisé

**Result<T> pattern :**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }
    public List<string> Details { get; set; }
    
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error, params string[] details) => new()
    {
        IsSuccess = false,
        Error = error,
        Details = details.ToList()
    };
}
```

---

### 11.2 Exception Middleware

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Validation failed",
                details = ex.Errors.Select(e => e.ErrorMessage)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal server error",
                details = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                    ? ex.Message
                    : "An error occurred"
            });
        }
    }
}
```

---

## 12. Documentation Scalar

L'API est documentée avec **Scalar** (intégré à Aspire).

**Accès :** `https://localhost:7000/scalar/v1`

**Configuration :**
```csharp
builder.Services.AddOpenApi();

app.MapScalarApiReference(options =>
{
    options.Theme = ScalarTheme.Purple;
    options.Title = "Chronique des Mondes API";
});
```

---

## Résumé des patterns

| Pattern | Description |
|---------|-------------|
| **Minimal API** | Endpoints groupés par domaine avec extension methods |
| **X-Game-Type header** | Routing multi-type vers la bonne couche business |
| **Authorization policies** | IsCharacterOwner, IsGameMaster, IsSessionParticipant |
| **Result<T>** | Retour standardisé avec gestion d'erreurs |
| **Transaction pattern** | Échanges atomiques avec rollback |
| **Server-side dice** | Anti-triche + historique complet |
| **SignalR notifications** | Événements temps réel après actions API |

---

## Prochaines étapes

1. ✅ Implémenter tous les endpoints dans `Cdm.ApiService/Endpoints/`
2. ✅ Créer les Authorization Handlers dans `Cdm.ApiService/Authorization/`
3. ✅ Configurer Scalar pour la documentation
4. 🔄 Écrire les tests d'intégration (voir `STANDARDS_CODE.md`)
5. 🔄 Implémenter le rate limiting (Phase 6)

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0
