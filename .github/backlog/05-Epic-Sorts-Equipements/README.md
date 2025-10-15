# Epic 5 - Sorts & Équipements

## 📋 Informations Générales

- **Phase** : Phase 2 (Contenu de Jeu)
- **Priorité** : P2 - Moyenne
- **Statut** : 📝 Planifié
- **Estimation totale** : 38 Story Points

---

## 🎯 Objectif

Créer un système complet de sorts et d'équipements avec une architecture à deux niveaux (officiels + personnalisés), permettant l'apprentissage, la gestion d'inventaire et les échanges entre joueurs.

---

## 📊 Critères d'Acceptation Globaux

### Sorts
- [ ] Architecture à deux niveaux (officiels + privés)
- [ ] Création de sorts génériques avec titre, description, image
- [ ] Bibliothèque de sorts consultable et filtrable
- [ ] Apprentissage de sorts par personnages
- [ ] Une seule instance par sort (pas de doublons)
- [ ] Utilisation en combat (lancement manuel)

### Équipements
- [ ] Architecture à deux niveaux (officiels + privés)
- [ ] Création d'équipements génériques
- [ ] Inventaire des personnages (quantités multiples)
- [ ] Équipement actif vs stocké
- [ ] Catalogue d'équipements avec recherche

### Échanges
- [ ] MJ peut proposer équipement à joueur
- [ ] Joueurs peuvent échanger entre eux
- [ ] Notifications d'échanges temps réel
- [ ] Historique des échanges

---

## 📝 User Stories

| ID | Titre | Statut | Story Points | Priorité |
|----|-------|--------|--------------|----------|
| [US-040](./US-040-creation-sorts.md) | Création de sorts génériques | 📝 Planifié | 5 | P2 |
| [US-041](./US-041-bibliotheque-sorts.md) | Bibliothèque de sorts | 📝 Planifié | 5 | P2 |
| [US-042](./US-042-apprentissage-sorts.md) | Apprentissage de sorts | 📝 Planifié | 5 | P2 |
| [US-043](./US-043-utilisation-sorts-combat.md) | Utilisation sorts en combat | 📝 Planifié | 5 | P2 |
| [US-044](./US-044-creation-equipements.md) | Création d'équipements | 📝 Planifié | 5 | P2 |
| [US-045](./US-045-inventaire-personnage.md) | Inventaire des personnages | 📝 Planifié | 5 | P2 |
| [US-046](./US-046-catalogue-equipements.md) | Catalogue d'équipements | 📝 Planifié | 3 | P2 |
| [US-047](./US-047-echange-mj-joueur.md) | Échange MJ → Joueur | 📝 Planifié | 3 | P2 |
| [US-048](./US-048-echange-joueur-joueur.md) | Échange Joueur ↔ Joueur | 📝 Planifié | 5 | P2 |

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `SpellEndpoints.cs`, `EquipmentEndpoints.cs`, `TradeEndpoints.cs`
- **Services** : 
  - `SpellService`, `SpellLibraryService`
  - `EquipmentService`, `InventoryService`
  - `TradeService`
- **Models** : 
  - `Spell` (IsOfficial, CreatedBy)
  - `Equipment` (IsOfficial, CreatedBy)
  - `CharacterSpell` (relation N-N)
  - `CharacterEquipment` (relation N-N avec Quantity)
  - `TradeProposal`
- **Hubs SignalR** : `TradeHub` (notifications échanges)

### Frontend (Blazor Server)
- **Pages** : 
  - SpellLibrary.razor, CreateSpell.razor
  - EquipmentCatalog.razor, Inventory.razor
  - TradeCenter.razor
- **Components** : 
  - SpellCard, SpellEditor, SpellFilter
  - EquipmentCard, InventoryManager
  - TradeProposal, TradeHistory
- **Services** : SpellService, EquipmentService, TradeService

### Base de Données
- **Tables** : 
  - `Spells` (IsOfficial, CreatedBy nullable)
  - `Equipment` (IsOfficial, CreatedBy nullable)
  - `CharacterSpells` (N-N relation)
  - `CharacterEquipment` (N-N avec Quantity, IsEquipped)
  - `TradeProposals` (FromUser, ToUser, Items, Status)

---

## 🪄 Architecture à Deux Niveaux

### Sorts Officiels
- Créés par l'équipe de développement
- `IsOfficial = true`, `CreatedBy = null`
- Disponibles pour tous les utilisateurs
- Ne peuvent pas être modifiés/supprimés

### Sorts Privés
- Créés par les utilisateurs
- `IsOfficial = false`, `CreatedBy = userId`
- Visibles uniquement par le créateur
- Peuvent être modifiés/supprimés

### Exemple de Filtre
```
Vue Globale : [Tous mes sorts] = Officiels + Mes créations
Vue Officiels : [Sorts officiels] = IsOfficial == true
Vue Personnels : [Mes créations] = CreatedBy == currentUserId
```

---

## 🎒 Système d'Inventaire

### Gestion des Quantités
- Un équipement peut être possédé en plusieurs exemplaires
- `CharacterEquipment.Quantity` pour gérer le nombre
- Exemple : 10 Potions de Soin

### État d'Équipement
- **Équipé** : `IsEquipped = true` (bonus actifs)
- **Stocké** : `IsEquipped = false` (inventaire)

### Limite d'Inventaire (optionnel)
- Poids total (encumbrance)
- Nombre maximum d'objets
- Slots d'équipement (tête, corps, arme, etc.)

---

## 🔄 Système d'Échanges

### Flux MJ → Joueur
1. MJ sélectionne un équipement (officiel ou perso)
2. MJ propose au joueur (notification temps réel)
3. Joueur accepte/refuse
4. Si accepté : copie dans inventaire du joueur

### Flux Joueur ↔ Joueur
1. Joueur A initie échange avec Joueur B (même campagne)
2. Joueur A sélectionne objets à donner (quantités)
3. Joueur B sélectionne objets à donner (quantités)
4. Les deux joueurs valident
5. Transfert de propriété

### Notifications SignalR
- Nouvelle proposition reçue
- Échange accepté/refusé
- Échange complété

---

## 🧪 Tests

### Tests Unitaires
- Création sorts/équipements
- Validation IsOfficial
- Calcul quantités inventaire
- Validation échanges

### Tests d'Intégration
- CRUD sorts/équipements
- Apprentissage sorts
- Flux complet d'échange

### Tests E2E
- Parcours création sort → Apprentissage → Utilisation
- Parcours échange MJ → Joueur
- Parcours échange Joueur ↔ Joueur

---

## 🔗 Dépendances

### Dépend de
- [Epic 3](../03-Epic-Personnages-PNJ/) - Personnages
- [Epic 4](../04-Epic-Combat-Des/) - Combat (pour utilisation sorts)

### Bloque
- [Epic 6](../06-Epic-Mode-DnD/) - Sorts D&D avec calculs automatiques

---

## 📈 Progression

```
Complété : [░░░░░░░░░░] 0% (0/9 US)
Planifié  : 9 US
```

---

## 📚 Documentation Associée

- [SPECIFICATION_FONCTIONNELLE.md](../../instructions/SPECIFICATION_FONCTIONNELLE.md) - Sorts & Équipements
- [MODELE_DONNEES.md](../../instructions/technique/MODELE_DONNEES.md) - Tables Spells, Equipment
- [SIGNALR_TEMPS_REEL.md](../../instructions/technique/SIGNALR_TEMPS_REEL.md) - TradeHub

---

**Dernière mise à jour** : 15 octobre 2025
