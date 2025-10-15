# US-041 - Grimoire du Personnage

## 📝 Description

**En tant que** Joueur avec personnage lanceur de sorts  
**Je veux** consulter mon grimoire et voir mes sorts connus/préparés  
**Afin de** gérer mon répertoire de sorts

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Onglet "Grimoire" dans fiche personnage
- [ ] Liste sorts connus avec filtres :
  - [ ] Par niveau
  - [ ] Par école
  - [ ] Par temps d'incantation
- [ ] Recherche par nom
- [ ] Bouton "Apprendre Sort" → Modal sélection sorts campagne
- [ ] Bouton "Préparer" / "Dépreparer" (si classe nécessite préparation)
- [ ] Indicateur slots disponibles par niveau (US-042)
- [ ] Clic sort → Modal détails complets
- [ ] MJ peut modifier grimoire n'importe quel personnage

### Techniques
- [ ] Endpoint : `GET /api/characters/{characterId}/spells`
- [ ] Endpoint : `POST /api/characters/{characterId}/spells/{spellId}/learn`
- [ ] Endpoint : `PUT /api/characters/{characterId}/spells/{spellId}/prepare`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterSpellService.LearnSpell_ValidSpell_AddsToGrimoire()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `CharacterSpell` :
```csharp
public class CharacterSpell
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Character Character { get; set; }
    
    public Guid SpellId { get; set; }
    public Spell Spell { get; set; }
    
    public bool IsPrepared { get; set; }
    public DateTime LearnedAt { get; set; }
}
```
- [ ] Créer services

### Frontend
- [ ] Composant `Spellbook.razor`
- [ ] Modal `LearnSpellModal.razor`

### Base de Données
- [ ] Migration : Créer table `CharacterSpells`
- [ ] Index unique : `IX_CharacterSpells_CharacterId_SpellId`

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Grimoire fonctionnel
- [ ] Apprendre/préparer sorts
- [ ] Filtres et recherche
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 11
