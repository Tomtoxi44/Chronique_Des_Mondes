# Spécification Fonctionnelle — Chronique des Mondes

> **Version** : 3.0 — Mise à jour post-implémentation D&D 5e  
> **Fichier canonique** : `.github/copilot/instructions/SPECIFICATION_FONCTIONNELLE.md`

---

## Vue d'ensemble

**Chronique des Mondes** est une plateforme web de jeu de rôle (JDR) permettant à des utilisateurs de créer, gérer et participer à des campagnes de jeu. L'application est construite avec Blazor Server (.NET) côté frontend et une API REST .NET côté backend.

### Interfaces
- **Desktop (Maître de Jeu)** — Interface complète pour créer et gérer des mondes, campagnes, chapitres et sessions
- **Mobile (Joueurs)** — Interface prioritairement optimisée pour participer aux sessions et gérer ses personnages

### Hiérarchie Principale

```
Monde → Campagne → Chapitre
```

- **Monde** : Univers de jeu avec son type de règles. Le créateur devient automatiquement Maître du Jeu (MJ). Peut contenir plusieurs campagnes.
- **Campagne** : Histoire/aventure dans un monde. Contient plusieurs chapitres.
- **Chapitre** : Segment narratif d'une campagne. Peut contenir des PNJ associés.

### Multi-Rôles
Un utilisateur peut simultanément être :
- **Joueur** dans certaines campagnes (via invitation)
- **Maître du Jeu** dans ses propres mondes et campagnes

---

## Systèmes de Jeu Supportés

L'application utilise un enum `GameType` centralisé. Le thème visuel et les données spécifiques aux personnages s'adaptent automatiquement selon ce type.

**Logique fondamentale** : tout est **générique par défaut**. Quand un monde a un type de jeu précis (ex: DnD5e), le système de règles de ce type s'applique à tout ce qui passe par ce monde : personnages joueurs, PNJ, sessions.

| GameType | Statut | Description |
|---|---|---|
| **Empty** | Implémenté | Monde sans règles, usage libre |
| **Generic** | Implémenté | Mode libre, MJ gère manuellement |
| **DnD5e** | Implémenté (wizard + fiche complète) | Dungeons & Dragons 5e — module complet |
| **Pathfinder** | Futur | Pathfinder — à définir |
| **CallOfCthulhu** | Futur | L'Appel de Cthulhu — à définir |
| **Warhammer** | Futur | Warhammer Fantasy — à définir |
| **Cyberpunk** | Futur | Cyberpunk RPG — à définir |
| **Skyrim** | Futur | Elder Scrolls Skyrim — à définir |
| **Custom** | Implémenté | Mode personnalisé par le MJ |

---

## Fonctionnalités Implémentées

### Authentification

- Inscription avec email et mot de passe
- Connexion sécurisée avec JWT (access token + refresh token)
- Déconnexion
- Protection des routes : redirection automatique vers `/login` si non authentifié

---

### Profil Utilisateur

- Consultation du profil (nom, email, date d'inscription)
- Modification du pseudo (nickname)
- Changement de mot de passe
- Upload d'avatar (image de profil)
- Changement de langue de l'interface (fr/en via cookie de culture)

---

### Mondes

#### Création et gestion
- Création d'un monde avec : nom, description, type de jeu (GameType), visibilité (public/privé)
- Liste de tous les mondes de l'utilisateur (en tant que MJ)
- Modification et suppression d'un monde
- Vue détail d'un monde : aperçu, personnages associés, campagnes

#### Système d'invitation des joueurs
Le MJ peut inviter des joueurs à rejoindre son monde par plusieurs moyens :
- **Lien d'invitation** : URL unique générée par le backend, copiable en un clic
- **QR Code** : Généré automatiquement à partir du lien d'invitation, scannable directement

Quand un joueur clique sur le lien (ou scanne le QR) :
1. Il est redirigé vers la page de rejoindre le monde
2. Il sélectionne un de ses personnages disponibles
3. Une copie adaptée du personnage est créée dans le monde (personnage de monde)
4. **Si le monde est de type DnD5e**, il est automatiquement redirigé vers l'assistant de création D&D

---

### Personnages

#### Personnage de Base
Créé par l'utilisateur dans son espace personnel :
- Nom, prénom
- Description générale
- Image/avatar
- Statut : Disponible ou Verrouillé (verrouillé quand intégré dans un monde)

Un personnage de base ne peut rejoindre qu'un seul monde à la fois (règle de verrouillage).

#### Personnage de Monde
Créé automatiquement lors de l'intégration dans un monde :
- Copie du personnage de base liée au monde
- Données spécifiques au type de jeu du monde stockées en JSON (`GameSpecificData`)

#### Module D&D 5e — Assistant de Création (Implémenté)

Pour les mondes de type `DnD5e`, un wizard 4 étapes guide le joueur :

**Étape 1 — Identité** : Nom, prénom, âge, description (pré-rempli depuis le personnage de base)

**Étape 2 — Stats D&D** :
- Niveau (1-20) avec calcul automatique du bonus de maîtrise
- Classe (Guerrier, Barbare, Paladin, Rôdeur, Roublard, Moine, Magicien, Ensorceleur, Occultiste, Clerc, Druide, Barde)
- Sous-classe
- Race (Humain, Haut-Elfe, Elfe des Bois, Nain des Collines, Nain des Montagnes, Halfelin Pied-Léger, Demi-Orque, Tieffelin, Drakéide, Demi-Elfe, Gnome des Roches) avec bonus automatiques
- Sous-race (avec bonus additionnels)
- Background (Acolyte, Charlatan, Criminel, Artiste, Héros du Peuple, Artisan de Guilde, Ermite, Noble, Baroudeur, Sage, Marin, Soldat, Gamin des Rues)
- Alignement
- 6 Caractéristiques (Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme) — modificateurs calculés auto
- PV (avec suggestion auto : dé de vie + Con × niveau), CA, Vitesse

**Étape 3 — Inventaire** : Ajout d'objets depuis la bibliothèque D&D (armes, armures, potions) ou création personnalisée

**Étape 4 — Sorts & Magie** : Sorts connus/préparés depuis la bibliothèque, groupés par niveau

#### Module D&D 5e — Vue Détail Fiche Personnage (Implémenté)

Route : `/world-character/{worldCharacterId}/dnd`

Accessible après le wizard et depuis la liste des personnages du monde. 4 onglets :

1. **Fiche** : Bloc 6 caractéristiques avec modificateurs, PV/CA/Vitesse/Initiative/Bonus de maîtrise, Classe/Race/Background/Alignement/Niveau — tout éditable
2. **Compétences** : 18 compétences avec cases de maîtrise, valeur calculée = mod + prof bonus si maîtrisé
3. **Inventaire** : Liste des objets avec ajout (depuis bibliothèque ou personnalisé) et suppression
4. **Sorts** : Sorts groupés par niveau, préparation, ajout (depuis bibliothèque ou personnalisé)

#### Créateur d'objets/sorts personnalisés (dans la vue détail)

- **Arme custom** : nom, jet de touche (car. + maîtrise), dés de dégâts (XdY), type, prévisualisation du calcul
- **Sort custom** : nom, niveau, école, description, composants, concentration, rituel
- **Objet custom** : nom, catégorie, quantité, notes

#### Tables de référence D&D 5e en base (Seedées)
- `DndRaces` : 11 races avec bonus de statistiques et sous-races
- `DndClasses` : 12 classes avec dés de vie, magie, sous-classes et compétences disponibles
- `DndBackgrounds` : 13 backgrounds avec compétences accordées
- `DndSkills` : 18 compétences liées aux caractéristiques
- `DndSpells` : 40+ sorts par école et classe
- `DndItems` : 30+ armes, armures, potions
- `DndMonsterTemplates` : 18+ modèles de monstres pour le MJ

#### Gestion CRUD Personnages
- Liste de ses personnages avec statut
- Création de personnage générique
- Modification (avant verrouillage pour le personnage de base)
- Suppression (si non verrouillé)

---

### Campagnes

- Création d'une campagne dans un monde : nom, description, visibilité (public/privé)
- Modification et suppression d'une campagne
- Liste des campagnes (les siennes + campagnes publiques)
- Lien d'invitation spécifique à la campagne (token)
- Système de statut : En préparation / Active / Terminée

---

### Chapitres

- Création de chapitres dans une campagne (titre, contenu texte)
- Numérotation séquentielle (ordre)
- Modification du contenu d'un chapitre
- Suppression d'un chapitre
- Navigation arborescente entre chapitres dans la vue campagne

---

### PNJ (Personnages Non-Joueurs)

- Création de PNJ dans un chapitre : nom, description, comportement (Amical/Neutre/Hostile), points de vie
- Association PNJ ↔ Chapitre
- Vue PNJ dans la session pour le MJ (cartes expandables)
- Système de @mention pour référencer PNJ et personnages joueurs dans les chapitres
- **Si monde DnD5e** : champs stats D&D (Force/Dex/Con/Int/Sag/Cha, CA, HP, classe, race) via `DndNpcService`
- Option de création depuis un `DndMonsterTemplate`

---

### Sessions

#### Lancement (MJ)
- Le MJ sélectionne une campagne à lancer
- La session est créée avec le statut `Starting`
- Chaque personnage de monde participant reçoit une notification dans l'application

#### Vue MJ (SessionGm)
- Visualisation des participants (nom, statut)
- Accès aux fiches personnages des joueurs (clic pour développer)
- Navigation entre les chapitres de la campagne
- Pour les personnages D&D : affichage des stats (classe, race, 6 caractéristiques)

#### Vue Joueur (SessionPlayer)
- Notification d'invitation → bouton "Rejoindre la session"
- Vue en 3 onglets :
  - **Personnage** : Nom, description, informations de base
  - **Inventaire** : Gestion des objets (ajouter/supprimer avec quantités)
  - **D&D** (si applicable) : Grille des 6 caractéristiques avec modificateurs calculés
- Priorité mobile : ergonomie conçue d'abord pour téléphone

---

### Notifications

- Système de notifications in-app
- Badges de compteur non lu
- Notification lors d'une invitation à un monde
- Notification lors du lancement d'une session
- Lien d'action dans chaque notification pour naviguer vers la bonne page

---

### Interface et Thèmes

- Mode sombre par défaut, mode clair disponible
- Système de thèmes CSS adaptatif selon le `GameType` du monde/personnage/campagne
- Navigation double sidebar (principale + contextuelle selon la section)
- Responsive : sidebar hamburger sur mobile

---

## Fonctionnalités Planifiées (Court Terme)

### SignalR — Temps Réel
- Synchronisation en temps réel de la session entre MJ et joueurs
- Notifications push sans rechargement de page
- Statut de connexion des participants en direct

### Réinitialisation de Mot de Passe
- Formulaire de demande de reset
- Envoi d'email avec lien sécurisé (Azure Communication Services)

### Validation d'Email
- Email de confirmation à l'inscription

### Invitation par Email
- Le MJ peut saisir un email pour inviter directement un joueur

### Historique des Sessions
- Liste des sessions passées par campagne

---

## Fonctionnalités Futures (Long Terme)

### Combat Assisté (Mode Générique d'abord, puis D&D)
- Déclenchement de combat par le MJ
- Gestion de l'initiative manuelle
- Suivi des PV des combattants
- Lanceur de dés intégré (d4, d6, d8, d10, d12, d20, d100)
- **Note** : La partie combat D&D (automatisée) sera implémentée après le combat générique

### D&D 5e Avancé (après combat générique)
- Calculs automatiques de CA, bonus de maîtrise, PV par niveau
- Jets d'attaque et de sauvegarde automatisés
- Gestion des slots de sorts
- Repos court/long
- Conditions et états (Prone, Restrained, Frightened...)
- Feats (dons)

### Autres Types de Jeu
Chaque type de jeu sera ajouté selon la même architecture (`GameSpecificData` JSON) :
- Pathfinder, Call of Cthulhu, Warhammer, Cyberpunk, Skyrim

### Sorts et Équipements Avancés
- Échanges d'équipements entre joueurs (MJ → Joueur, Joueur → Joueur)
- Bibliothèque de sorts avancée

### Système d'Achievements (Succès)
- Succès créés par le MJ à trois niveaux : Monde, Campagne, Chapitre
- Rareté : Commun, Rare, Épique, Légendaire
- Attribution manuelle ou automatique

### Statistiques
- Statistiques de dés (lancers, coups critiques, facteur de chance)
- Statistiques de participation (sessions jouées, heures, distribution MJ/Joueur)
- Rapports automatiques mensuels/annuels

### Salon Vocal en Session
- Intégration d'un salon audio pendant les sessions actives

---

## Architecture Technique (Référence)

- **Frontend** : Blazor Server (.NET 9), CSS custom (variables + thèmes), Bootstrap Icons
- **Backend** : API REST .NET 9, Entity Framework Core, Azure SQL
- **Auth** : JWT (access + refresh tokens), stockage localStorage
- **Notifications** : Système in-app via table `Notification` en base
- **Temps réel** : SignalR (prévu)
- **Emails** : Azure Communication Services (prévu)
- **Données spécifiques JDR** : Champ JSON `GameSpecificData` sur `WorldCharacter` et `NonPlayerCharacter`
- **Thèmes** : CSS custom properties + classes `.theme-{gametype}` sur conteneur
- **Module D&D** : Projet séparé `Cdm.Business.DnD5e` avec services `DndCharacterService`, `DndNpcService`, `DndReferenceService`

---

## Contraintes et Règles Métier

1. **Un personnage de base ne peut rejoindre qu'un seul monde à la fois** (verrouillage)
2. **Le MJ d'un monde est son créateur** — ne peut pas être délégué
3. **Les joueurs d'un monde ne peuvent voir** que la description du monde et les titres/descriptions des campagnes — pas le contenu des chapitres (anti-spoil)
4. **Les données de jeu spécifiques** (D&D, etc.) sont stockées par monde-personnage, pas par personnage de base
5. **Toutes les pages applicatives** nécessitent une authentification JWT valide
6. **La visibilité** d'un monde ou d'une campagne (Public/Privé) est indépendante : une campagne peut être privée dans un monde public
7. **Le joueur est libre** de créer des armes, sorts, objets personnalisés qui ne sont pas dans les listes de référence
