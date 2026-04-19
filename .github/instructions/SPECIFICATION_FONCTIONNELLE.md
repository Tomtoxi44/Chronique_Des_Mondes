# Spécification Fonctionnelle — Chronique des Mondes

> **Version** : 2.0 — Mise à jour post-implémentation MVP  
> **Dernière mise à jour** : Refonte complète pour aligner sur l'état réel du projet

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
- **Chapitre** : Segment narratif d'une campagne. Peut contenir des PNJ associés (à venir).

### Multi-Rôles
Un utilisateur peut simultanément être :
- **Joueur** dans certaines campagnes (via invitation)
- **Maître du Jeu** dans ses propres mondes et campagnes

---

## Systèmes de Jeu Supportés

L'application utilise un enum `GameType` centralisé. Le thème visuel et les données spécifiques aux personnages s'adaptent automatiquement selon ce type.

| GameType | Statut | Description |
|---|---|---|
| **Empty** | Implémenté | Monde sans règles, usage libre |
| **Generic** | Implémenté | Mode libre, MJ gère manuellement |
| **DnD5e** | Implémenté (basique) | Dungeons & Dragons 5e — données de personnage |
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

**Limites actuelles** : Pas de réinitialisation de mot de passe (prévu), pas de validation par email (prévu).

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
4. Si le monde est de type DnD5e, des champs spécifiques D&D lui sont demandés

#### Personnages du monde
- Le MJ peut voir la liste des personnages qui ont rejoint son monde
- Les personnages de monde sont distincts des personnages originaux

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

#### Données Spécifiques D&D 5e (Implémenté)
Pour les mondes de type `DnD5e`, le personnage de monde peut renseigner :
- **Classe** (Guerrier, Mage, Rôdeur, etc.)
- **Race** (Humain, Elfe, Nain, etc.)
- **Background** (Contexte narratif)
- **6 Caractéristiques** : Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme
- Les modificateurs sont calculés automatiquement `((val - 10) / 2)`
- **Inventaire** de base (nom d'objet + quantité), stocké dans le JSON

**Données non encore implémentées pour D&D** : Sous-classe, compétences/maîtrises, points de vie calculés, classe d'armure, bonus de maîtrise automatique, jets de sauvegarde.

#### Gestion CRUD
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

### Sessions

#### Lancement (MJ)
- Le MJ sélectionne une campagne à lancer
- Configuration de la session
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

### PNJ (Personnages Non-Joueurs)
- Création de PNJ dans un chapitre : nom, description, comportement (Amical/Neutre/Hostile), points de vie
- Association PNJ ↔ Chapitre
- Vue PNJ dans la session pour le MJ
- Monstres comme sous-type de PNJ

### SignalR — Temps Réel
- Synchronisation en temps réel de la session entre MJ et joueurs
- Notifications push sans rechargement de page
- Statut de connexion des participants en direct

### Réinitialisation de Mot de Passe
- Formulaire de demande de reset
- Envoi d'email avec lien sécurisé (Azure Communication Services)

### Validation d'Email
- Email de confirmation à l'inscription
- Vérification avant d'autoriser certaines actions sensibles

### Invitation par Email
- Le MJ peut saisir un email pour inviter directement un joueur
- Email envoyé avec le lien d'invitation au monde

### Historique des Sessions
- Liste des sessions passées par campagne
- Informations : date, participants, chapitres joués

---

## Fonctionnalités Futures (Long Terme)

> Ces fonctionnalités sont prévues mais pas encore planifiées dans la roadmap immédiate.

### Combat Assisté (Mode Générique)
- Déclenchement de combat par le MJ
- Gestion de l'initiative manuelle
- Suivi des PV des combattants
- Transmission des résultats de dés
- Historique des actions en combat

### Lanceur de Dés
- Dés standard : d4, d6, d8, d10, d12, d20, d100
- Affichage du résultat
- Historique des lancers dans la session

### D&D 5e Avancé
- Calculs automatiques de CA, bonus de maîtrise, PV par niveau
- Jets d'attaque et de sauvegarde automatisés
- Compétences et maîtrises
- Intégration combat D&D

### Autres Types de Jeu
Chaque type de jeu sera ajouté selon la même architecture (`GameSpecificData` JSON) :
- Pathfinder (proche D&D 5e)
- Call of Cthulhu
- Warhammer Fantasy
- Cyberpunk
- Skyrim

### Sorts et Équipements
- Bibliothèque de sorts (officielle + personnelle)
- Inventaire avancé avec catalogue d'équipements
- Compatibilité avec le système de combat

### Échanges d'Équipements entre Joueurs
- **MJ → Joueur** : Le MJ propose un équipement à un joueur (copié dans son inventaire si accepté)
- **Joueur → Joueur** : Échange direct entre deux joueurs de la même campagne (transfert de propriété)
- Notifications lors de chaque proposition
- Liste des propositions en attente

### Système d'Achievements (Succès)
- Succès créés par le MJ à trois niveaux : Monde, Campagne, Chapitre
- Rareté : Commun, Rare, Épique, Légendaire
- Attribution manuelle par le MJ ou automatique (basée sur des conditions)
- Notifications de déblocage selon la rareté
- Affichage dans le profil du joueur

### Statistiques de Dés
- Statistiques globales : total de lancers, moyenne, facteur de chance
- Analyse par type de dé (d4 à d100)
- Analyse spécifique d20 : coups critiques (20 naturels), échecs critiques (1 naturels)
- Tendances temporelles par mois/campagne
- Segmentation par système de jeu (Générique, D&D, etc.)

### Statistiques de Participation
- Métriques de sessions jouées, heures de jeu (mensuel, annuel, total)
- Distribution MJ/Joueur, taille moyenne des groupes
- Historique par campagne

### Rapports Automatiques
- Rapport mensuel : résumé d'activité, moments forts, records
- Rapport annuel : rétrospective complète, comparaison année par année
- Envoi optionnel par email (Azure Communication Services)

### Salon Vocal en Session
- Intégration d'un salon audio pendant les sessions actives
- Contrôle du MJ (muet/actif par joueur)
- Option de partage d'écran

---

## Architecture Technique (Référence)

- **Frontend** : Blazor Server (.NET 9), CSS custom (variables + thèmes), Bootstrap Icons
- **Backend** : API REST .NET 9, Entity Framework Core, PostgreSQL
- **Auth** : JWT (access + refresh tokens), stockage localStorage
- **Notifications** : Système in-app via table `Notification` en base
- **Temps réel** : SignalR (prévu)
- **Emails** : Azure Communication Services (prévu)
- **Données spécifiques JDR** : Champ JSON `GameSpecificData` sur `WorldCharacter`
- **Thèmes** : CSS custom properties + classes `.theme-{gametype}` sur conteneur

---

## Contraintes et Règles Métier

1. **Un personnage de base ne peut rejoindre qu'un seul monde à la fois** (verrouillage)
2. **Le MJ d'un monde est son créateur** — ne peut pas être délégué
3. **Les joueurs d'un monde ne peuvent voir** que la description du monde et les titres/descriptions des campagnes — pas le contenu des chapitres (anti-spoil)
4. **Les données de jeu spécifiques** (D&D, etc.) sont stockées par monde-personnage, pas par personnage de base
5. **Toutes les pages applicatives** nécessitent une authentification JWT valide
6. **La visibilité** d'un monde ou d'une campagne (Public/Privé) est indépendante : une campagne peut être privée dans un monde public
