# Epic 6 - Mode D&D (Règles Automatisées)

## 📋 Informations Générales

- **Phase** : Phase 3 (Mode Spécialisé D&D 5e)
- **Priorité** : P3 - Basse
- **Statut** : 📝 Planifié
- **Estimation totale** : 54 Story Points

---

## 🎯 Objectif

Implémenter les règles complètes de **Dungeons & Dragons 5e** avec calculs automatiques pour personnages, PNJ/monstres, combat, sorts et équipements spécifiques à D&D.

---

## 📊 Critères d'Acceptation Globaux

### Personnages D&D
- [x] Données basiques : classe, race, background, 6 caractéristiques (via GameSpecificData JSON)
- [x] Calcul des modificateurs `(val-10)/2` affiché dans les vues MJ et Joueur
- [ ] Calculs automatiques avancés : PV calculés, CA, bonus de maîtrise selon niveau
- [ ] Compétences et maîtrises D&D
- [ ] Sous-classe (archétype)

> **Note** : La montée de niveau (XP) est hors scope actuel — sera traitée lors de l'implémentation D&D avancée.

### PNJ/Monstres D&D
- [ ] Création monstres D&D avec statistiques complètes
- [ ] Bibliothèque de monstres officiels D&D
- [ ] Challenge Rating (CR) et calcul XP
- [ ] Attaques spéciales et résistances

### Combat D&D
- [ ] Calcul d'initiative automatique (1d20 + Dex)
- [ ] Jets d'attaque automatiques avec comparaison CA
- [ ] Calcul dégâts automatiques (résistances, vulnérabilités)
- [ ] Coups critiques (dégâts doublés)
- [ ] Jets de sauvegarde automatiques

### Sorts D&D
- [ ] Bibliothèque de sorts D&D officiels
- [ ] Emplacements de sorts par niveau
- [ ] Calcul DD de sauvegarde (8 + maîtrise + mod incantation)
- [ ] Jets d'attaque de sort automatiques

### Équipements D&D
- [ ] Armes avec propriétés D&D (finesse, polyvalent, etc.)
- [ ] Armures avec calcul CA automatique
- [ ] Objets magiques officiels

---

## 📝 User Stories

| ID | Titre | Statut | Priorité |
|----|-------|--------|----------|
| [US-049](./US-049-fiche-personnage-dnd.md) | Fiche personnage D&D complète | ⏳ Futur | P3 |
| [US-050](./US-050-races-dnd.md) | Races D&D | ⏳ Futur | P3 |
| [US-051](./US-051-classes-dnd.md) | Classes D&D | ⏳ Futur | P3 |
| [US-052](./US-052-jets-competences.md) | Jets de compétences | ⏳ Futur | P3 |
| [US-053](./US-053-avantage-desavantage.md) | Avantage / Désavantage | ⏳ Futur | P3 |
| [US-055](./US-055-actions-bonus-reactions.md) | Actions bonus et réactions | ⏳ Futur | P3 |
| [US-056](./US-056-conditions-etats.md) | Conditions et états | ⏳ Futur | P3 |
| [US-057](./US-057-repos-court-long.md) | Repos court / long | ⏳ Futur | P3 |

> **US-054 (XP / Montée de niveau)** : Retiré du scope — fonctionnalité spécifique D&D, sera reconsidérée lors de l'implémentation D&D avancée.

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `DnDCharacterEndpoints.cs`, `DnDCombatEndpoints.cs`, `DnDSpellEndpoints.cs`
- **Services** : 
  - `DnDCharacterService` (calculs D&D)
  - `DnDCombatService` (règles combat D&D)
  - `DnDSpellService` (emplacements, DD)
  - `DnDMonsterService` (CR, XP)
- **Models** : 
  - `DnDCharacter` (hérite de `BaseCharacter`)
  - `DnDMonster` (hérite de `BaseCharacter`)
  - `DnDSpell` (hérite de `Spell`)
  - `DnDEquipment` (hérite de `Equipment`)
- **Règles D&D** : Services de calcul dédiés

### Frontend (Blazor Server)
- **Pages** : 
  - CreateDnDCharacter.razor
  - DnDCharacterSheet.razor
  - DnDCombatView.razor
- **Components** : 
  - DnDCharacterForm (races, classes, caractéristiques)
  - AbilityScoreCalculator
  - SpellSlotTracker
  - DnDMonsterCard
- **Services** : DnDCharacterService, DnDCombatService

### Base de Données
- **Extension de** : `BaseCharacters` avec colonnes D&D
  - Race, Class, Level (1-20)
  - Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma
  - ArmorClass, ProficiencyBonus, HitDice
  - Skills (JSON), Proficiencies (JSON)
- **Nouvelles tables** :
  - `DnDSpellSlots` (emplacements par niveau)
  - `DnDMonsters` (bibliothèque officielle)

---

## 🎲 Règles D&D Automatisées

### Calcul des Modificateurs
```
Modificateur = (Caractéristique - 10) / 2 (arrondi inf)

Exemples :
Force 16 → +3
Dextérité 14 → +2
Intelligence 8 → -1
```

### Bonus de Maîtrise
```
Niveau 1-4   → +2
Niveau 5-8   → +3
Niveau 9-12  → +4
Niveau 13-16 → +5
Niveau 17-20 → +6
```

### Classe d'Armure (CA)
```
Sans armure : 10 + mod Dex
Armure légère : Base armure + mod Dex
Armure moyenne : Base armure + mod Dex (max +2)
Armure lourde : Base armure
```

### Points de Vie (PV)
```
Niveau 1 : Dé de vie max + mod Con
Niveaux suivants : (Dé de vie moyen ou jet) + mod Con

Exemples de dés de vie :
- Guerrier : d10
- Clerc : d8
- Magicien : d6
```

### Jets d'Attaque
```
Corps à corps : 1d20 + mod Force + bonus maîtrise
Distance : 1d20 + mod Dex + bonus maîtrise
Sort : 1d20 + mod incantation + bonus maîtrise

Si résultat ≥ CA cible → Touché
Si résultat = 20 → Coup critique (dégâts doublés)
Si résultat = 1 → Échec critique
```

### Calcul des Dégâts
```
Dégâts = [Dé de dégâts] + mod caractéristique

Exemples :
- Épée longue : 1d8 + mod Force
- Arc long : 1d8 + mod Dex
- Boule de feu : 8d6 (pas de mod)

Coup critique : Doubler les dés de dégâts
Résistance : Diviser dégâts par 2
Vulnérabilité : Multiplier dégâts par 2
```

### DD de Sauvegarde (Sorts)
```
DD = 8 + bonus maîtrise + mod incantation

Exemples :
- Magicien niveau 5, Int 18 : DD = 8 + 3 + 4 = 15
- Clerc niveau 3, Sag 16 : DD = 8 + 2 + 3 = 13
```

### Emplacements de Sorts
```
Niveau 1 : 2 emplacements niveau 1
Niveau 2 : 3 emplacements niveau 1
Niveau 3 : 4 niveau 1 + 2 niveau 2
...
Niveau 20 : Tous les emplacements débloqués
```

---

## 📚 Bibliothèques Officielles D&D

### Races (SRD 5.1)
- Humain, Elfe (Haut, Sylvestre), Nain (Montagne, Colline)
- Halfelin, Drakéide, Gnome, Demi-Elfe, Demi-Orc, Tieffelin

### Classes (SRD 5.1)
- Barbare, Barde, Clerc, Druide
- Guerrier, Moine, Paladin, Rôdeur
- Roublard, Ensorceleur, Sorcier, Magicien

### Monstres (SRD 5.1)
- 300+ monstres avec statistiques complètes
- CR de 0 (Rat) à 30 (Tarrasque)
- Attaques, résistances, immunités, sens

### Sorts (SRD 5.1)
- 300+ sorts niveau 0 (cantrips) à 9
- Écoles de magie (Abjuration, Invocation, etc.)
- Composantes (V, S, M)

---

## 🧪 Tests

### Tests Unitaires
- Calculs de modificateurs
- Bonus de maîtrise par niveau
- Calcul CA selon armure
- PV par niveau et classe
- DD de sauvegarde

### Tests d'Intégration
- Création personnage D&D complet
- Combat avec calculs automatiques
- Progression de niveau

### Tests E2E
- Parcours complet : Création → Combat → Niveau up
- Utilisation de sorts avec emplacements
- Validation règles D&D

---

## 🔗 Dépendances

### Dépend de
- [Epic 3](../03-Epic-Personnages-PNJ/) - Base personnages
- [Epic 4](../04-Epic-Combat-Des/) - Système de combat générique
- [Epic 5](../05-Epic-Sorts-Equipements/) - Sorts et équipements

### Bloque
- Autres modes spécialisés (Pathfinder, Call of Cthulhu, etc.)

---

## ⚠️ Considérations Légales

### Licence OGL et SRD 5.1
- Utilisation du **System Reference Document (SRD) 5.1** sous licence **Open Gaming License (OGL)**
- Contenu gratuit et légalement utilisable
- Interdiction d'utiliser le logo "D&D" ou marques déposées Wizards of the Coast
- Mention obligatoire : "This work includes material taken from the System Reference Document 5.1 ("SRD 5.1") by Wizards of the Coast LLC and available at https://dnd.wizards.com/resources/systems-reference-document"

---

## 📈 Progression

```
Complété : [░░░░░░░░░░] 0% (0/9 US)
Planifié  : 9 US
```

---

## 📚 Documentation Associée

- [SPECIFICATION_FONCTIONNELLE.md](../../instructions/SPECIFICATION_FONCTIONNELLE.md) - Mode D&D
- [MODELE_DONNEES.md](../../instructions/technique/MODELE_DONNEES.md) - Extension D&D
- **SRD 5.1** : https://dnd.wizards.com/resources/systems-reference-document

---

**Dernière mise à jour** : 15 octobre 2025
