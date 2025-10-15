# US-057 - Repos Court et Long

## 📝 Description

**En tant que** Joueur D&D 5e  
**Je veux** effectuer repos court et long  
**Afin de** récupérer HP, dés de vie et emplacements sorts

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bouton "Prendre un Repos" dans fiche personnage
- [ ] **Repos Court** (1 heure RP) :
  - [ ] Lancer dés de vie pour récupérer HP
  - [ ] Modal : Choisir nombre dés (max = niveau)
  - [ ] Formule : XdY + (X × Mod Con)
  - [ ] Capacités "Repos Court" restaurées (Action Surge, Ki points, etc.)
- [ ] **Repos Long** (8 heures RP) :
  - [ ] Restauration HP complets
  - [ ] Restauration dés de vie (max moitié niveau, minimum 1)
  - [ ] Restauration tous emplacements sorts
  - [ ] Restauration toutes capacités
  - [ ] Réduction niveau épuisement de 1
- [ ] MJ peut forcer repos pour groupe
- [ ] Validation : Maximum 1 repos long par 24h RP

### Techniques
- [ ] Endpoint : `POST /api/characters/{characterId}/rest`
- [ ] Body : `{ "restType": "Short" | "Long", "hitDiceSpent": 2 }`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DndService.ShortRest_RestoresHP()`
- [ ] `DndService.LongRest_RestoresAll()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `DndService.TakeRestAsync(characterId, restType, options)`
- [ ] Créer `Character.HitDiceRemaining` (int)
- [ ] Validation 24h cooldown

### Frontend
- [ ] Modal `RestModal.razor`
- [ ] Lancer dés de vie interactif

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Repos court fonctionnel
- [ ] Repos long complet
- [ ] Dés de vie gérés
- [ ] Cooldown 24h
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 15
