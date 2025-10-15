# US-045 - Objets Magiques

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer des objets magiques avec propriétés spéciales  
**Afin de** récompenser les joueurs avec loot intéressant

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page `/magic-items/new` (MJ uniquement)
- [ ] Formulaire objet magique :
  - [ ] Nom
  - [ ] Type d'objet (Arme, Armure, Anneau, Bâton, etc.)
  - [ ] Rareté (Commune, Peu commune, Rare, Très rare, Légendaire)
  - [ ] Description (Markdown)
  - [ ] Propriétés magiques (texte libre)
  - [ ] Charges (nombre + régénération)
  - [ ] Sorts stockés (multi-select sorts campagne)
  - [ ] Attunement requis (oui/non + classe)
  - [ ] Modificateurs bonus (+1 CA, +2 Force, etc.)
- [ ] Sauvegarde → Disponible pour ajout inventaire
- [ ] Bibliothèque objets magiques campagne

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/magic-items`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `MagicItemService.CreateMagicItem_ValidData_CreatesMagicItem()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `MagicItem` :
```csharp
public class MagicItem
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    
    public string Name { get; set; }
    public ItemType ItemType { get; set; }
    public MagicItemRarity Rarity { get; set; }
    public string Description { get; set; }
    public string MagicalProperties { get; set; }
    
    public int? MaxCharges { get; set; }
    public string? ChargeRegeneration { get; set; } // "1d6 à l'aube"
    
    public bool RequiresAttunement { get; set; }
    public string? AttunementRequirements { get; set; }
    
    public string? BonusModifiers { get; set; } // JSON: { "AC": 2, "Strength": 1 }
    
    public ICollection<Spell> StoredSpells { get; set; } // N-N
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public enum MagicItemRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    VeryRare = 3,
    Legendary = 4,
    Artifact = 5
}
```
- [ ] Créer `MagicItemService`

### Frontend
- [ ] Page `CreateMagicItem.razor`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Création objets magiques
- [ ] Propriétés complètes
- [ ] Bibliothèque campagne
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 12
