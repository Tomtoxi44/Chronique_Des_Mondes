# Spécification Fonctionnelle — Chronique des Mondes

> **Version** : 2.0 — Mise à jour post-implémentation MVP

**Chronique des Mondes** est une plateforme web de jeu de rôle (JDR) : Blazor Server frontend + API REST .NET backend.

## Hiérarchie : Monde → Campagne → Chapitre

## Systèmes de Jeu (enum GameType)

| Type | Statut |
|---|---|
| Empty, Generic, Custom | Implémentés |
| DnD5e | Données personnage basiques implémentées |
| Pathfinder, CallOfCthulhu, Warhammer, Cyberpunk, Skyrim | Futurs |

## Fonctionnalités Implémentées

### Auth
- Inscription, connexion JWT (access + refresh token), déconnexion
- Redirection `/login` si non authentifié

### Profil
- Modification pseudo, avatar, mot de passe, langue (fr/en)

### Mondes
- CRUD complet (nom, description, GameType, visibilité)
- Invitation joueurs : lien URL + QR code
- Vue détail : aperçu, personnages du monde, campagnes

### Personnages
- Personnage de base : CRUD, statut disponible/verrouillé
- Personnage de monde : copie adaptée au monde, données JSON par GameType
- D&D 5e : classe, race, background, 6 caractéristiques (Force/Dex/Con/Int/Sag/Cha), inventaire

### Campagnes
- CRUD, visibilité, lien invitation, statut (EnPréparation/Active/Terminée)

### Chapitres
- CRUD avec contenu texte, numérotation séquentielle, navigation arborescente

### Sessions
- Lancement par le MJ → notifications aux participants
- Vue MJ : fiches personnages cliquables, stats D&D
- Vue Joueur : 3 onglets (Personnage / Inventaire / D&D), gestion inventaire

### Notifications
- In-app, badges non-lus, invitation monde + lancement session

### Interface
- Thèmes CSS par GameType, mode sombre/clair, responsive mobile

## Planifié (Court Terme)

- **PNJ** : Création, association chapitre, comportement (Amical/Neutre/Hostile)
- **SignalR** : Temps réel session MJ/Joueurs
- **Reset password** et validation email (Azure Communication Services)
- **Invitation par email**
- **Historique sessions**

## Futur (Long Terme)

- Combat assisté (mode générique) + lanceur de dés intégré
- D&D 5e avancé (CA, PV calculés, jets automatiques)
- Autres types de jeu (Pathfinder, CoC, Warhammer, Cyberpunk, Skyrim)
- Sorts et équipements + échanges d'équipements entre joueurs
- Système d'achievements (succès) à 3 niveaux (Monde/Campagne/Chapitre)
- Statistiques de dés et de participation + rapports automatiques mensuels/annuels
- Salon vocal intégré aux sessions
- Fonctionnalités sociales (amis, messagerie)

## Contraintes Métier

1. Un personnage de base ne rejoint qu'un seul monde à la fois (verrouillage)
2. Le MJ est le créateur du monde — non délégable
3. Les joueurs ne voient PAS le contenu des chapitres (anti-spoil)
4. `GameSpecificData` JSON sur `WorldCharacter` = données par monde-personnage
5. Toutes les pages nécessitent un JWT valide
