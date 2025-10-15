# US-044 - Inventaire et Équipement

## 📝 Description

**En tant que** Joueur  
**Je veux** gérer l'inventaire de mon personnage  
**Afin de** suivre mes objets, armes et armures

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Onglet "Inventaire" dans fiche personnage
- [ ] Liste objets avec colonnes :
  - [ ] Nom
  - [ ] Type (Arme, Armure, Consommable, Objet Magique, Autre)
  - [ ] Quantité
  - [ ] Poids
  - [ ] Valeur (or)
  - [ ] Équipé (checkbox pour armes/armures)
- [ ] Bouton "Ajouter Objet" → Modal création
- [ ] Recherche et filtres par type
- [ ] Calcul automatique poids total
- [ ] Limite de charge (optionnel, selon Force)
- [ ] Drag & Drop pour organiser
- [ ] MJ peut ajouter objets à n'importe quel personnage

### Techniques
- [ ] Endpoint : `GET /api/characters/{characterId}/inventory`
- [ ] Endpoint : `POST /api/characters/{characterId}/inventory`
- [ ] Endpoint : `PUT /api/inventory/{itemId}/equip`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `InventoryService.AddItem_ValidItem_AddsToInventory()`
- [ ] `InventoryService.EquipItem_ValidWeapon_EquipsItem()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `InventoryItem` :
```csharp
public class InventoryItem
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Character Character { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public int Quantity { get; set; }
    public decimal Weight { get; set; } // Kilos
    public int Value { get; set; } // Gold pieces
    
    public bool IsEquipped { get; set; }
    public bool IsMagical { get; set; }
    public string? MagicalProperties { get; set; }
    
    // Weapon stats
    public string? DamageNotation { get; set; } // "1d8+2"
    public DamageType? DamageType { get; set; }
    public string? WeaponProperties { get; set; } // "Finesse, Light"
    
    // Armor stats
    public int? ArmorClass { get; set; }
    public ArmorType? ArmorType { get; set; }
    
    public DateTime AcquiredAt { get; set; }
}

public enum ItemType
{
    Weapon = 0,
    Armor = 1,
    Consumable = 2,
    MagicItem = 3,
    Tool = 4,
    Treasure = 5,
    Other = 6
}

public enum ArmorType
{
    Light = 0,
    Medium = 1,
    Heavy = 2,
    Shield = 3
}
```
- [ ] Créer `InventoryService`

### Frontend
- [ ] Composant `Inventory.razor`
- [ ] Modal `AddItemModal.razor`

### Base de Données
- [ ] Migration : Créer table `InventoryItems`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Inventaire fonctionnel
- [ ] Ajout/suppression objets
- [ ] Équipement armes/armures
- [ ] Calcul poids total
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 12
