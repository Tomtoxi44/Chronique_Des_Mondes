# US-040 - Création de Sorts

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer des sorts personnalisés pour ma campagne  
**Afin de** enrichir les options tactiques des joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page `/spells/new` accessible MJ uniquement
- [ ] Formulaire création sort :
  - [ ] Nom (obligatoire)
  - [ ] Description (Markdown)
  - [ ] Niveau (0-9 pour D&D 5e, illimité pour Generic)
  - [ ] École de magie (Abjuration, Évocation, etc.)
  - [ ] Temps d'incantation (Action, Action Bonus, Réaction, etc.)
  - [ ] Portée (en mètres ou "Contact", "Personnelle")
  - [ ] Composantes (Verbale, Somatique, Matérielle + description)
  - [ ] Durée (Instantanée, Concentration, etc.)
  - [ ] Type de jet (Aucun, Jet d'Attaque, Jet de Sauvegarde)
  - [ ] Dégâts/Soins (notation dés : "2d6+3")
  - [ ] Effets spéciaux (texte libre)
  - [ ] Classe(s) autorisée(s) (Magicien, Clerc, etc.)
- [ ] Validation :
  - [ ] Nom unique par campagne
  - [ ] Niveau ≥ 0
- [ ] Prévisualisation Markdown description
- [ ] Enregistrement → Redirection liste sorts

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/spells`
- [ ] Body : SpellDto
- [ ] Response 201 : Spell créé

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SpellService.CreateSpell_ValidData_CreatesSpell()`
- [ ] `SpellService.CreateSpell_DuplicateName_ThrowsException()`

### Tests d'Intégration
- [ ] `SpellEndpoint_CreateSpell_SavesInDatabase()`

### Tests E2E
- [ ] MJ crée sort → Visible dans liste sorts

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Spell` :
```csharp
public class Spell
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    
    public string Name { get; set; } // Max 100
    public string Description { get; set; } // Markdown
    public int Level { get; set; } // 0-9 (cantrip = 0)
    public MagicSchool School { get; set; }
    public string CastingTime { get; set; } // "1 action", "1 action bonus"
    public string Range { get; set; } // "9 mètres", "Contact"
    public bool RequiresVerbal { get; set; }
    public bool RequiresSomatic { get; set; }
    public bool RequiresMaterial { get; set; }
    public string? MaterialComponents { get; set; }
    public string Duration { get; set; } // "Instantanée", "Concentration, 1 minute"
    public bool RequiresConcentration { get; set; }
    
    public SpellRollType RollType { get; set; } // None, AttackRoll, SavingThrow
    public string? AttackAbility { get; set; } // "Intelligence", "Sagesse"
    public string? SaveAbility { get; set; } // "Dextérité", "Constitution"
    public string? DamageNotation { get; set; } // "2d6+3"
    public DamageType? DamageType { get; set; } // Fire, Cold, Lightning, etc.
    public string? SpecialEffects { get; set; }
    
    public string AllowedClasses { get; set; } // JSON array ["Wizard", "Sorcerer"]
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; }
}

public enum MagicSchool
{
    Abjuration = 0,
    Conjuration = 1,
    Divination = 2,
    Enchantment = 3,
    Evocation = 4,
    Illusion = 5,
    Necromancy = 6,
    Transmutation = 7,
    Universal = 8 // For Generic mode
}

public enum SpellRollType
{
    None = 0,
    AttackRoll = 1,
    SavingThrow = 2
}

public enum DamageType
{
    Acid = 0,
    Bludgeoning = 1,
    Cold = 2,
    Fire = 3,
    Force = 4,
    Lightning = 5,
    Necrotic = 6,
    Piercing = 7,
    Poison = 8,
    Psychic = 9,
    Radiant = 10,
    Slashing = 11,
    Thunder = 12
}
```
- [ ] Créer `SpellService.CreateSpellAsync(campaignId, dto, userId)`
- [ ] Créer endpoints CRUD

### Frontend
- [ ] Créer page `CreateSpell.razor`
- [ ] Composant `SpellForm.razor`
- [ ] Prévisualisation Markdown (Markdig)

### Base de Données
- [ ] Migration : Créer table `Spells`
- [ ] Index : `IX_Spells_CampaignId_Name` (unique)

---

## 📊 Estimation

**Story Points** : 8

---

## ✅ Definition of Done

- [ ] Création sorts fonctionnelle
- [ ] Validation formulaire
- [ ] Prévisualisation Markdown
- [ ] Tests passent
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 11
