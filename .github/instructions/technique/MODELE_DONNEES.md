# Modèle de Données - Chronique des Mondes

## Vue d'ensemble

La base de données est conçue pour supporter **plusieurs systèmes de jeu** (générique, D&D 5e, Skyrim, etc.) tout en maintenant une structure cohérente et extensible.

### Principes de conception

1. **Héritage TPH (Table Per Hierarchy)** : Une seule table `Characters` avec discriminateur `CharacterType`
2. **Attributs flexibles JSON** : Colonnes `Attributes` pour stocker les caractéristiques spécifiques à chaque système
3. **Tag GameType** : Chaque entité est marquée avec son système de jeu pour garantir la compatibilité
4. **IsOfficial flag** : Distingue le contenu officiel (bibliothèque publique) du contenu utilisateur

---

## 1. Tables principales

### 1.1 Users (Utilisateurs)

Gère l'authentification et les informations utilisateur.

```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    INDEX IX_Users_Email (Email),
    INDEX IX_Users_Username (Username),
    INDEX IX_Users_IsActive (IsActive)
);
```

**Colonnes clés :**
- `PasswordHash` : Hash BCrypt du mot de passe
- `IsActive` : Permet de désactiver un compte sans le supprimer
- `LastLoginAt` : Suivi des connexions pour statistiques

---

### 1.2 Characters (Personnages)

Table principale avec héritage TPH pour supporter tous les types de personnages.

```sql
CREATE TABLE Characters (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CharacterType NVARCHAR(50) NOT NULL, -- Discriminateur: 'Generic', 'Dnd5e', 'Skyrim'
    GameType INT NOT NULL, -- Enum: 0=Generic, 1=Dnd5e, 2=Skyrim
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    
    -- Attributs génériques
    Level INT NOT NULL DEFAULT 1,
    Experience INT NOT NULL DEFAULT 0,
    MaxHealth INT NOT NULL,
    CurrentHealth INT NOT NULL,
    
    -- Attributs flexibles (JSON)
    Attributes NVARCHAR(MAX) NULL, -- Stockage JSON pour caractéristiques spécifiques
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Characters_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    INDEX IX_Characters_UserId (UserId),
    INDEX IX_Characters_GameType (GameType),
    INDEX IX_Characters_CharacterType (CharacterType),
    INDEX IX_Characters_IsDeleted (IsDeleted)
);
```

**Exemples de JSON dans `Attributes` :**

**Générique :**
```json
{
    "customStats": {
        "Force": 12,
        "Agilité": 15,
        "Intelligence": 10
    }
}
```

**D&D 5e :**
```json
{
    "race": "Elf",
    "class": "Wizard",
    "subclass": "School of Evocation",
    "abilityScores": {
        "STR": 8,
        "DEX": 14,
        "CON": 12,
        "INT": 18,
        "WIS": 13,
        "CHA": 10
    },
    "proficiencyBonus": 3,
    "armorClass": 13,
    "speed": 30,
    "hitDice": "1d6",
    "savingThrows": ["INT", "WIS"],
    "skills": ["Arcana", "History", "Investigation", "Perception"]
}
```

**Skyrim (futur) :**
```json
{
    "race": "Nord",
    "standing stone": "The Warrior",
    "skills": {
        "OneHanded": 45,
        "Block": 30,
        "HeavyArmor": 50
    },
    "shouts": ["Fus Ro Dah", "Whirlwind Sprint"]
}
```

---

### 1.3 Campaigns (Campagnes)

Regroupe plusieurs sessions de jeu.

```sql
CREATE TABLE Campaigns (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GameMasterId INT NOT NULL, -- MJ
    GameType INT NOT NULL, -- Doit correspondre aux personnages participants
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Campaigns_Users FOREIGN KEY (GameMasterId) REFERENCES Users(Id),
    
    INDEX IX_Campaigns_GameMasterId (GameMasterId),
    INDEX IX_Campaigns_GameType (GameType),
    INDEX IX_Campaigns_IsActive (IsActive)
);
```

---

### 1.4 CampaignCharacters (Participants)

Table de liaison entre campagnes et personnages.

```sql
CREATE TABLE CampaignCharacters (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CampaignId INT NOT NULL,
    CharacterId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1, -- Permet de retirer un personnage sans supprimer l'historique
    
    CONSTRAINT FK_CampaignCharacters_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignCharacters_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id),
    CONSTRAINT UQ_CampaignCharacters UNIQUE (CampaignId, CharacterId),
    
    INDEX IX_CampaignCharacters_CampaignId (CampaignId),
    INDEX IX_CampaignCharacters_CharacterId (CharacterId)
);
```

**Règle de validation (business layer) :**
```csharp
// Campaign.GameType DOIT correspondre à Character.GameType
if (campaign.GameType != character.GameType)
    return Result<T>.Failure("Incompatibilité: le personnage n'est pas compatible avec cette campagne");
```

---

### 1.5 Sessions (Sessions de jeu)

Une session de jeu dans une campagne.

```sql
CREATE TABLE Sessions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CampaignId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    ScheduledDate DATETIME2 NULL,
    StartedAt DATETIME2 NULL,
    EndedAt DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Planned, 1=Active, 2=Completed, 3=Cancelled
    Summary NVARCHAR(MAX) NULL, -- Résumé après session
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Sessions_Campaigns FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id) ON DELETE CASCADE,
    
    INDEX IX_Sessions_CampaignId (CampaignId),
    INDEX IX_Sessions_Status (Status),
    INDEX IX_Sessions_ScheduledDate (ScheduledDate)
);
```

---

### 1.6 Combats

Gère les combats dans une session.

```sql
CREATE TABLE Combats (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId INT NOT NULL,
    Name NVARCHAR(200) NULL, -- Ex: "Combat contre les gobelins"
    CurrentRound INT NOT NULL DEFAULT 1,
    CurrentTurn INT NOT NULL DEFAULT 0, -- Index du combattant actif
    Status INT NOT NULL DEFAULT 0, -- 0=NotStarted, 1=InProgress, 2=Completed
    StartedAt DATETIME2 NULL,
    EndedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Combats_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE,
    
    INDEX IX_Combats_SessionId (SessionId),
    INDEX IX_Combats_Status (Status)
);
```

---

### 1.7 CombatParticipants (Ordre d'initiative)

Gère l'ordre des tours de combat.

```sql
CREATE TABLE CombatParticipants (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CombatId INT NOT NULL,
    CharacterId INT NULL, -- NULL si c'est un PNJ temporaire
    NpcName NVARCHAR(100) NULL, -- Nom du PNJ si pas lié à un Character
    InitiativeRoll INT NOT NULL, -- Résultat du jet d'initiative
    InitiativeOrder INT NOT NULL, -- Ordre calculé (avec bonus DEX pour D&D)
    CurrentHealth INT NOT NULL,
    MaxHealth INT NOT NULL,
    IsNpc BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1, -- false si KO/mort
    
    CONSTRAINT FK_CombatParticipants_Combats FOREIGN KEY (CombatId) REFERENCES Combats(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CombatParticipants_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id),
    
    INDEX IX_CombatParticipants_CombatId_InitiativeOrder (CombatId, InitiativeOrder)
);
```

**Calcul de l'ordre d'initiative (D&D 5e) :**
```csharp
// InitiativeOrder = InitiativeRoll + modificateur DEX
int dexModifier = (character.Attributes.abilityScores.DEX - 10) / 2;
participant.InitiativeOrder = participant.InitiativeRoll + dexModifier;
```

---

## 2. Tables de contenu de jeu

### 2.1 Spells (Sorts)

Bibliothèque de sorts pour tous les systèmes.

```sql
CREATE TABLE Spells (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GameType INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Level INT NOT NULL, -- 0 pour cantrip, 1-9 pour D&D
    School NVARCHAR(50) NULL, -- "Evocation", "Necromancy", etc. (D&D)
    CastingTime NVARCHAR(100) NULL, -- "1 action", "1 bonus action", etc.
    Range NVARCHAR(100) NULL, -- "30 feet", "Self", "Touch"
    Components NVARCHAR(200) NULL, -- "V, S, M (un morceau de charbon)"
    Duration NVARCHAR(100) NULL, -- "Instantané", "Concentration, jusqu'à 1 minute"
    
    -- Attributs spécifiques (JSON)
    GameSpecificData NVARCHAR(MAX) NULL,
    
    IsOfficial BIT NOT NULL DEFAULT 0, -- true = contenu officiel, false = homebrew
    CreatedBy INT NULL, -- UserId si homebrew
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Spells_Users FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    
    INDEX IX_Spells_GameType (GameType),
    INDEX IX_Spells_IsOfficial (IsOfficial),
    INDEX IX_Spells_Level (Level)
);
```

---

### 2.2 Equipment (Équipements)

Armes, armures, objets.

```sql
CREATE TABLE Equipment (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GameType INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Type INT NOT NULL, -- 0=Weapon, 1=Armor, 2=Item, 3=Tool, etc.
    Rarity INT NULL, -- 0=Common, 1=Uncommon, 2=Rare, 3=VeryRare, 4=Legendary (D&D)
    Weight DECIMAL(10,2) NULL,
    Value DECIMAL(10,2) NULL, -- En pièces d'or (D&D) ou autre monnaie
    
    -- Attributs spécifiques (JSON)
    GameSpecificData NVARCHAR(MAX) NULL, -- {"damage": "1d8", "damageType": "slashing", "properties": ["versatile"]}
    
    IsOfficial BIT NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Equipment_Users FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    
    INDEX IX_Equipment_GameType (GameType),
    INDEX IX_Equipment_Type (Type),
    INDEX IX_Equipment_IsOfficial (IsOfficial)
);
```

**Exemple de `GameSpecificData` pour une épée longue D&D :**
```json
{
    "damage": "1d8",
    "damageType": "slashing",
    "properties": ["versatile"],
    "versatileDamage": "1d10",
    "proficiencyType": "martial"
}
```

---

### 2.3 CharacterSpells (Sorts connus)

Table de liaison entre personnages et sorts.

```sql
CREATE TABLE CharacterSpells (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CharacterId INT NOT NULL,
    SpellId INT NOT NULL,
    IsPrepared BIT NOT NULL DEFAULT 0, -- Pour les classes qui préparent leurs sorts
    LearnedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_CharacterSpells_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CharacterSpells_Spells FOREIGN KEY (SpellId) REFERENCES Spells(Id),
    CONSTRAINT UQ_CharacterSpells UNIQUE (CharacterId, SpellId),
    
    INDEX IX_CharacterSpells_CharacterId (CharacterId),
    INDEX IX_CharacterSpells_SpellId (SpellId)
);
```

---

### 2.4 CharacterEquipment (Inventaire)

Équipements possédés par un personnage.

```sql
CREATE TABLE CharacterEquipment (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CharacterId INT NOT NULL,
    EquipmentId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    IsEquipped BIT NOT NULL DEFAULT 0, -- Équipé ou dans le sac
    AcquiredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Notes NVARCHAR(500) NULL, -- Ex: "Épée magique +1"
    
    CONSTRAINT FK_CharacterEquipment_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CharacterEquipment_Equipment FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id),
    
    INDEX IX_CharacterEquipment_CharacterId (CharacterId),
    INDEX IX_CharacterEquipment_EquipmentId (EquipmentId)
);
```

---

## 3. Système de transactions (Échanges d'équipements)

### 3.1 EquipmentProposals (Propositions d'échange)

```sql
CREATE TABLE EquipmentProposals (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId INT NOT NULL,
    FromCharacterId INT NOT NULL,
    ToCharacterId INT NOT NULL,
    ProposedItems NVARCHAR(MAX) NOT NULL, -- JSON: [{equipmentId, quantity}]
    RequestedItems NVARCHAR(MAX) NULL, -- JSON: [{equipmentId, quantity}] (NULL si don)
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Accepted, 2=Declined, 3=Cancelled
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt DATETIME2 NULL,
    
    CONSTRAINT FK_EquipmentProposals_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
    CONSTRAINT FK_EquipmentProposals_FromCharacter FOREIGN KEY (FromCharacterId) REFERENCES Characters(Id),
    CONSTRAINT FK_EquipmentProposals_ToCharacter FOREIGN KEY (ToCharacterId) REFERENCES Characters(Id),
    
    INDEX IX_EquipmentProposals_SessionId (SessionId),
    INDEX IX_EquipmentProposals_Status (Status)
);
```

**Exemple de JSON `ProposedItems` :**
```json
[
    {"equipmentId": 42, "quantity": 1}, // Une épée longue
    {"equipmentId": 15, "quantity": 50}  // 50 pièces d'or
]
```

---

### 3.2 EquipmentTrades (Historique des échanges)

```sql
CREATE TABLE EquipmentTrades (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProposalId INT NOT NULL,
    ExecutedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TransactionDetails NVARCHAR(MAX) NOT NULL, -- JSON complet de la transaction
    
    CONSTRAINT FK_EquipmentTrades_Proposals FOREIGN KEY (ProposalId) REFERENCES EquipmentProposals(Id),
    
    INDEX IX_EquipmentTrades_ProposalId (ProposalId),
    INDEX IX_EquipmentTrades_ExecutedAt (ExecutedAt)
);
```

**Pattern de transaction EF Core :**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try {
    // 1. Retirer les items du donneur
    // 2. Ajouter les items au receveur
    // 3. Créer l'historique EquipmentTrade
    // 4. Mettre à jour le statut de la proposition
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}
```

---

## 4. Système de lancers de dés

### 4.1 DiceRolls (Historique des jets)

Tous les jets de dés sont enregistrés côté serveur pour éviter la triche.

```sql
CREATE TABLE DiceRolls (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId INT NULL,
    CombatId INT NULL,
    CharacterId INT NULL,
    UserId INT NOT NULL, -- Qui a lancé le dé
    DiceNotation NVARCHAR(50) NOT NULL, -- "1d20", "2d6+3", "1d8+5"
    Results NVARCHAR(MAX) NOT NULL, -- JSON: [12, 8] pour 2d6
    Modifiers INT NOT NULL DEFAULT 0, -- +3, -2, etc.
    TotalResult INT NOT NULL, -- Somme + modifiers
    RollType INT NOT NULL, -- 0=Generic, 1=Initiative, 2=Attack, 3=Damage, 4=SavingThrow, 5=SkillCheck
    Context NVARCHAR(500) NULL, -- "Attaque avec épée longue", "Jet de Perception"
    RolledAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_DiceRolls_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
    CONSTRAINT FK_DiceRolls_Combats FOREIGN KEY (CombatId) REFERENCES Combats(Id),
    CONSTRAINT FK_DiceRolls_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id),
    CONSTRAINT FK_DiceRolls_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    
    INDEX IX_DiceRolls_SessionId (SessionId),
    INDEX IX_DiceRolls_CharacterId (CharacterId),
    INDEX IX_DiceRolls_RolledAt (RolledAt)
);
```

**Exemple de JSON `Results` :**
```json
[18]  // Pour 1d20
[3, 5, 2, 6]  // Pour 4d6
```

**Calcul côté serveur :**
```csharp
public class DiceService {
    public DiceRollResult Roll(string notation) {
        // Parse "2d6+3"
        var (diceCount, diceType, modifier) = ParseNotation(notation);
        
        var results = new List<int>();
        for (int i = 0; i < diceCount; i++) {
            results.Add(Random.Shared.Next(1, diceType + 1));
        }
        
        var total = results.Sum() + modifier;
        
        return new DiceRollResult {
            Results = results,
            Modifier = modifier,
            Total = total
        };
    }
}
```

---

## 5. Notifications et messages

### 5.1 Notifications

```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Type INT NOT NULL, -- 0=SessionInvite, 1=CombatTurn, 2=TradeProposal, 3=System
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    RelatedEntityId INT NULL, -- SessionId, ProposalId, etc.
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReadAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    INDEX IX_Notifications_UserId_IsRead (UserId, IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt)
);
```

---

### 5.2 SessionInvitations (Invitations par email)

```sql
CREATE TABLE SessionInvitations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId INT NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE, -- Token unique pour acceptation
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Accepted, 2=Declined, 3=Expired
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    AcceptedAt DATETIME2 NULL,
    
    CONSTRAINT FK_SessionInvitations_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE,
    
    INDEX IX_SessionInvitations_Token (Token),
    INDEX IX_SessionInvitations_Email (Email),
    INDEX IX_SessionInvitations_Status (Status)
);
```

---

## 6. Statistiques et métriques (Phase 4)

### 6.1 DiceStatistics (Vue matérialisée ou table calculée)

```sql
CREATE TABLE DiceStatistics (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CharacterId INT NOT NULL,
    Period INT NOT NULL, -- 0=AllTime, 1=LastMonth, 2=LastWeek
    TotalRolls INT NOT NULL,
    AverageResult DECIMAL(10,2) NOT NULL,
    CriticalSuccesses INT NOT NULL, -- Nat 20 en D&D
    CriticalFailures INT NOT NULL, -- Nat 1 en D&D
    HighestRoll INT NOT NULL,
    LowestRoll INT NOT NULL,
    LastCalculatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_DiceStatistics_Characters FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_DiceStatistics UNIQUE (CharacterId, Period),
    
    INDEX IX_DiceStatistics_CharacterId (CharacterId)
);
```

**Calcul via Hosted Service :**
```csharp
public class DiceStatisticsHostedService : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            // Recalculer les statistiques toutes les heures
            await RecalculateStatistics();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

---

## 7. Indexes et optimisations

### 7.1 Indexes composites essentiels

```sql
-- Recherche rapide des personnages actifs d'un utilisateur
CREATE INDEX IX_Characters_UserId_IsDeleted_GameType 
ON Characters(UserId, IsDeleted, GameType);

-- Recherche des sessions actives d'une campagne
CREATE INDEX IX_Sessions_CampaignId_Status 
ON Sessions(CampaignId, Status);

-- Ordre d'initiative dans un combat
CREATE INDEX IX_CombatParticipants_CombatId_InitiativeOrder_IsActive 
ON CombatParticipants(CombatId, InitiativeOrder, IsActive);

-- Historique de dés d'un personnage
CREATE INDEX IX_DiceRolls_CharacterId_RolledAt 
ON DiceRolls(CharacterId, RolledAt DESC);
```

---

### 7.2 Vues utiles

**Vue des personnages avec leur propriétaire :**
```sql
CREATE VIEW vw_CharactersWithOwners AS
SELECT 
    c.Id,
    c.Name,
    c.GameType,
    c.Level,
    c.CurrentHealth,
    c.MaxHealth,
    u.Username AS OwnerUsername,
    u.Email AS OwnerEmail
FROM Characters c
INNER JOIN Users u ON c.UserId = u.Id
WHERE c.IsDeleted = 0;
```

**Vue des combats en cours :**
```sql
CREATE VIEW vw_ActiveCombats AS
SELECT 
    cb.Id AS CombatId,
    cb.Name AS CombatName,
    cb.CurrentRound,
    s.Id AS SessionId,
    s.Name AS SessionName,
    cmp.Name AS CampaignName,
    u.Username AS GameMaster
FROM Combats cb
INNER JOIN Sessions s ON cb.SessionId = s.Id
INNER JOIN Campaigns cmp ON s.CampaignId = cmp.Id
INNER JOIN Users u ON cmp.GameMasterId = u.Id
WHERE cb.Status = 1; -- InProgress
```

---

## 8. Contraintes et règles métier

### 8.1 Contraintes CHECK

```sql
-- Un personnage ne peut pas avoir plus de PV actuels que de PV max
ALTER TABLE Characters
ADD CONSTRAINT CK_Characters_Health CHECK (CurrentHealth <= MaxHealth);

-- Un personnage doit avoir au moins 1 PV max
ALTER TABLE Characters
ADD CONSTRAINT CK_Characters_MaxHealth CHECK (MaxHealth > 0);

-- Le niveau doit être positif
ALTER TABLE Characters
ADD CONSTRAINT CK_Characters_Level CHECK (Level > 0);

-- La quantité d'équipement doit être positive
ALTER TABLE CharacterEquipment
ADD CONSTRAINT CK_CharacterEquipment_Quantity CHECK (Quantity > 0);
```

---

### 8.2 Triggers

**Mise à jour automatique de `UpdatedAt` :**
```sql
CREATE TRIGGER trg_Characters_UpdatedAt
ON Characters
AFTER UPDATE
AS
BEGIN
    UPDATE Characters
    SET UpdatedAt = GETUTCDATE()
    WHERE Id IN (SELECT Id FROM inserted);
END;
```

**Validation de compatibilité GameType :**
```sql
CREATE TRIGGER trg_CampaignCharacters_GameType
ON CampaignCharacters
INSTEAD OF INSERT
AS
BEGIN
    -- Vérifier que Campaign.GameType = Character.GameType
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Campaigns cmp ON i.CampaignId = cmp.Id
        INNER JOIN Characters c ON i.CharacterId = c.Id
        WHERE cmp.GameType <> c.GameType
    )
    BEGIN
        RAISERROR('GameType incompatible entre la campagne et le personnage', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- Insertion si validation OK
    INSERT INTO CampaignCharacters (CampaignId, CharacterId, JoinedAt, IsActive)
    SELECT CampaignId, CharacterId, JoinedAt, IsActive
    FROM inserted;
END;
```

---

## 9. Stratégie de migration

### 9.1 Contexte séparé

Le projet utilise **deux DbContext** :

1. **MigrationsContext** : Contient TOUS les DbSet, utilisé uniquement pour les migrations
2. **AppDbContext** : Contexte d'exécution léger (peut être vide ou contenir seulement les tables fréquentes)

```csharp
// MigrationsContext.cs - TOUS les DbSet
public class MigrationsContext : DbContext {
    public DbSet<User> Users { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CampaignCharacter> CampaignCharacters { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Combat> Combats { get; set; }
    public DbSet<CombatParticipant> CombatParticipants { get; set; }
    public DbSet<Spell> Spells { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<CharacterSpell> CharacterSpells { get; set; }
    public DbSet<CharacterEquipment> CharacterEquipment { get; set; }
    public DbSet<EquipmentProposal> EquipmentProposals { get; set; }
    public DbSet<EquipmentTrade> EquipmentTrades { get; set; }
    public DbSet<DiceRoll> DiceRolls { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SessionInvitation> SessionInvitations { get; set; }
    public DbSet<DiceStatistic> DiceStatistics { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MigrationsContext).Assembly);
    }
}

// AppDbContext.cs - Runtime (peut être vide initialement)
public class AppDbContext : DbContext {
    // Sera rempli au besoin selon les fonctionnalités utilisées
}
```

---

### 9.2 Commandes de migration

```bash
# Créer une migration
dotnet ef migrations add InitialCreate --project Cdm.Migrations --startup-project Cdm.MigrationsManager --context MigrationsContext

# Appliquer les migrations
dotnet ef database update --project Cdm.Migrations --startup-project Cdm.MigrationsManager --context MigrationsContext

# Générer un script SQL
dotnet ef migrations script --project Cdm.Migrations --startup-project Cdm.MigrationsManager --context MigrationsContext --output migrations.sql
```

---

## 10. Connexion à la base de données

### 10.1 Chaînes de connexion

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER,PORT;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;",
    "DevelopmentConnection": "Server=YOUR_SERVER,PORT;Database=YOUR_DEV_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

⚠️ **Ne jamais committer les vraies valeurs de connexion !**  
Utilisez User Secrets en développement (voir `CONFIGURATION.md`).

---

### 10.2 Configuration dans Program.cs

```csharp
var connectionString = builder.Configuration.GetConnectionString(
    builder.Environment.IsDevelopment() ? "DevelopmentConnection" : "DefaultConnection"
);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Pour les migrations uniquement
builder.Services.AddDbContext<MigrationsContext>(options =>
    options.UseSqlServer(connectionString));
```

---

## Résumé des principes clés

| Principe | Description |
|----------|-------------|
| **TPH** | Une seule table Characters avec discriminateur CharacterType |
| **JSON flexible** | Colonnes `Attributes` pour données spécifiques au système |
| **GameType tagging** | Garantit la compatibilité entre entités (Campaign/Character) |
| **IsOfficial flag** | Sépare contenu officiel et homebrew |
| **Server-side dice** | Tous les jets enregistrés pour anti-triche |
| **Transaction pattern** | Échanges atomiques avec rollback en cas d'échec |
| **Split context** | MigrationsContext (complet) vs AppDbContext (runtime) |
| **Soft delete** | IsDeleted flag pour conserver l'historique |

---

## Prochaines étapes

1. ✅ Implémenter les modèles C# dans `Cdm.Data.Common/Models/`
2. ✅ Créer les configurations EF Core (Fluent API) dans `Cdm.Data.Common/Configurations/`
3. ✅ Générer la migration initiale
4. 🔄 Peupler avec des données de test (seed data)
5. 🔄 Créer les repositories (voir `API_ENDPOINTS.md`)

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0
