# Roadmap — Chronique des Mondes

> **Dernière vérification : 18 juillet 2026**, par relecture du code source et tests
> fonctionnels réels (dont une session multi-joueurs). La roadmap précédente sous-estimait
> fortement l'avancement : PNJ, temps réel SignalR, lanceur de dés et combat générique
> étaient marqués « à faire » alors qu'ils étaient livrés depuis avril 2026.
>
> **Convention :**
> - `[x]` implémenté et vérifié
> - `[~]` partiellement implémenté (le détail précise ce qui manque)
> - `[ ]` non commencé
>
> État des lieux complet et priorités : `docs/ETAT_DU_PROJET.md`.

---

## Phase 0 : Fondations MVP — COMPLÉTÉE

### Authentification
- [x] Inscription et authentification (JWT)
- [x] Connexion sécurisée (access + refresh token)
- [x] Déconnexion
- [x] Protection des routes (redirection automatique si non authentifié)
- [ ] Réinitialisation de mot de passe (email Azure Communication Services)
- [ ] Validation d'email à l'inscription

**Statut** : Core complété. Reset password et validation email à planifier.

---

### Profil Utilisateur
- [x] Consultation du profil (nom, email, date inscription)
- [x] Modification du pseudo
- [x] Upload d'avatar
- [x] Changement de mot de passe
- [x] Changement de langue (fr/en via cookie)

**Statut** : Complété.

---

### Mondes
- [x] Création de monde (nom, description, GameType, visibilité)
- [x] Liste des mondes de l'utilisateur
- [x] Modification et suppression de monde
- [x] Vue détail d'un monde (aperçu, personnages, campagnes)
- [x] Système d'invitation : lien URL unique + QR code
- [x] Rejoindre un monde via lien/QR (sélection de personnage)

**Statut** : Complété.

---

### Personnages
- [x] Création de personnage de base (nom, prénom, description, avatar)
- [x] Modification et suppression de personnage de base
- [x] Liste des personnages avec statut (disponible/verrouillé)
- [x] Personnage de monde (copie adaptée au monde, données JSON GameType)
- [x] Données D&D 5e basiques : classe, race, background, 6 caractéristiques, inventaire
- [x] Vue et modification du personnage de monde par le joueur

**Statut** : Basique complété. Voir "Planifié" pour D&D avancé.

---

### Campagnes
- [x] Création de campagne (nom, description, monde parent, visibilité)
- [x] Modification et suppression de campagne
- [x] Liste des campagnes (les siennes + publiques)
- [x] Lien d'invitation de campagne

**Statut** : Complété.

---

### Chapitres
- [x] Création de chapitre (titre, contenu texte)
- [x] Modification et suppression de chapitre
- [x] Numérotation séquentielle et ordre
- [x] Navigation arborescente dans la vue campagne

**Statut** : Complété.

---

### Sessions
- [x] Lancement de session par le MJ
- [x] Notifications in-app aux participants lors du lancement
- [x] Vue MJ : liste participants, fiches personnages cliquables, stats D&D
- [x] Vue Joueur : rejoindre session, 3 onglets (Personnage / Inventaire / D&D)
- [x] Gestion inventaire dans la session (ajout/suppression d'objets)

**Statut** : Fonctionnel. Voir "Planifié" pour temps réel SignalR.

---

### Notifications
- [x] Système de notifications in-app
- [x] Badge compteur non-lus dans la topbar
- [x] Notification invitation à un monde
- [x] Notification lancement de session
- [x] Lien d'action dans chaque notification

**Statut** : Complété (in-app uniquement).

---

### Interface et Thèmes
- [x] Design system CSS avec variables et thèmes
- [x] Thème par GameType (.theme-dnd5e, .theme-cyberpunk, etc.)
- [x] Mode sombre par défaut, mode clair disponible
- [x] Navigation dual-sidebar (principale + contextuelle)
- [x] Responsive mobile (hamburger, tiroirs)
- [x] Localisation fr/en (fichiers resx)

**Statut** : Complété.

---

## Phase 1 : Enrichissement MVP — LARGEMENT COMPLÉTÉE

### PNJ (Personnages Non-Joueurs)
- [x] Création de PNJ générique dans un chapitre (nom, description, comportement, PV)
- [x] Association PNJ ↔ Chapitre
- [x] Affichage PNJ dans la session MJ
- [x] Monstres comme sous-type de PNJ (`DndMonsterTemplate`)

**Statut** : Complété. Le MJ crée ses PNJ depuis l'onglet PNJ du chapitre et les retrouve en session.

---

### Temps Réel (SignalR)
- [x] Hub SignalR pour les sessions actives (`SessionHub`, `CombatHub`, `NotificationHub`)
- [x] Synchronisation en temps réel entre MJ et joueurs
- [x] Notifications push sans rechargement de page
- [x] Statut de connexion des participants (badge « Connecté »)

**Statut** : Complété et validé par un test à deux comptes (18/07/2026).
Autorisation d'appartenance à la campagne vérifiée avant de rejoindre un groupe.

---

### Communication Email
- [ ] Réinitialisation de mot de passe par email (Azure Communication Services)
- [ ] Validation d'email à l'inscription
- [ ] Invitation d'un joueur par email directement depuis le monde

**Critère de succès** : Un utilisateur peut récupérer son accès via email.

---

### Historique
- [~] Historique des sessions par campagne — page `/sessions` existante, mais liste limitée
- [ ] Historique des mondes rejoints par l'utilisateur
- [ ] **Rechargement de l'historique du chat et des dés à l'ouverture d'une session**
      (les messages et jets sont bien persistés en base, mais jamais relus : le chat
      repart vide à chaque connexion)

---

## Phase 2 : Systèmes de Jeu Avancés — FUTUR

> Ces fonctionnalités dépendent de la validation du MVP et de la disponibilité.

### D&D 5e Avancé
- [ ] Sous-classe (archétype)
- [ ] Compétences et maîtrises
- [ ] Classe d'armure calculée automatiquement
- [ ] Points de vie calculés (selon classe + Constitution)
- [ ] Bonus de maîtrise automatique selon le niveau
- [ ] Jets de sauvegarde

**Note** : Le niveau et la montée de niveau sont spécifiques à D&D et seront implémentés dans cette phase.

---

### Autres Types de Jeu
- [ ] Skyrim : races, compétences, perks, statistiques Skyrim
- [ ] Pathfinder : adaptation proche D&D avec différences de règles
- [ ] Call of Cthulhu : système de santé mentale, compétences spécifiques
- [ ] Warhammer Fantasy : système de carrières, blessures critiques
- [ ] Cyberpunk : statistiques net-runner, équipement cybernétique

---

### PNJ D&D Avancé
- [ ] Statistiques complètes D&D (CA, vitesse, sens, langues, CR)
- [ ] Attaques avec modificateurs
- [ ] Jets de sauvegarde

---

## Phase 3 : Combat et Dés — LARGEMENT COMPLÉTÉE

> Livrée en avance (avril 2026), avant les Phases 2 et 4.

### Lanceur de Dés
- [x] Support d4, d6, d8, d10, d12, d20, d100
- [x] Nombre de dés variable (XdY + modificateur)
- [x] Affichage et transmission au MJ (diffusion SignalR à toute la table)
- [~] Historique des lancers — persisté en base, mais pas rechargé à l'ouverture

### Combat Générique
- [x] Déclenchement par le MJ, sélection des combattants (`CombatSetup`)
- [x] Initiative manuelle, gestion des tours (vues MJ et joueur)
- [x] Suivi PV en temps réel
- [x] Résumé de combat (camp vainqueur + notes)

**Statut** : Complété et validé en test à deux comptes.

### Combat D&D Automatisé
- [ ] Initiative 1d20 + modificateur Dextérité
- [ ] Jets d'attaque avec bonus automatiques
- [ ] Calcul dégâts + résistances/vulnérabilités
- [ ] Jets de sauvegarde automatiques

---

## Phase 4 : Sorts et Équipements — PARTIELLEMENT COMPLÉTÉE

### Sorts
- [~] Création de sorts génériques (titre, description)
- [x] Sorts D&D spécialisés (`DndSpell` : niveau, école, composantes, dégâts)
- [x] Bibliothèque de sorts officielle (seeder D&D 5e)
- [x] Sorts appris par personnage (`DndCharacterSpell`, sélection dans le wizard)

### Équipements
- [x] Catalogue d'équipements officiel (`DndItem`, seeder)
- [~] Inventaire (`DndInventoryItem` + gestion en session) — pas de notion de poids
- [x] Équipements D&D (type, bonus, dégâts, propriétés)

### Échanges d'Équipements — **NON COMMENCÉ**
- [ ] MJ → Joueur : proposition d'équipement, acceptation/refus
- [ ] Joueur → Joueur : échange direct entre joueurs de la même campagne
- [ ] Notifications de proposition
- [ ] Liste des propositions en attente

> ⚠️ Seule la méthode hub `ProposeTradeTheory` et le type de notification `TradeProposed`
> existent : **il n'y a aucune interface utilisateur**. La base est posée, tout le reste est à faire.

---

## Phase 5 : Engagement et Progression — FUTUR

### Système d'Achievements (Succès) — PARTIELLEMENT COMPLÉTÉ
- [~] Succès à 3 niveaux : Monde, Campagne, Chapitre — création au niveau Monde opérationnelle
- [x] Rareté : Commun, Rare, Épique, Légendaire (`AchievementRarity`)
- [~] Attribution manuelle (MJ) en place ; **attribution automatique par conditions à faire**
- [~] Type de notification `AchievementUnlocked` défini — déclenchement à vérifier
- [ ] Affichage dans le profil joueur

### Statistiques de Dés
- [ ] Total de lancers, moyenne, facteur de chance
- [ ] Analyse par type de dé (d4 à d100)
- [ ] Analyse d20 : coups critiques / échecs critiques
- [ ] Tendances temporelles par mois et par campagne
- [ ] Segmentation par système de jeu

### Statistiques de Participation
- [ ] Sessions jouées, heures de jeu (mensuel / annuel / total)
- [ ] Distribution MJ/Joueur, taille moyenne des groupes
- [ ] Historique par campagne

### Rapports Automatiques
- [ ] Rapport mensuel : activité, records, moments forts
- [ ] Rapport annuel : rétrospective complète
- [ ] Envoi optionnel par email

---

## Phase 6 : Expérience Immersive — VISION LONG TERME

> Extension de l'expérience utilisateur, sans planification immédiate.

### Salon Vocal en Session
- [ ] Intégration audio pendant les sessions actives
- [ ] Contrôle du MJ (muet/actif par joueur)
- [ ] Option partage d'écran

### Autres Outils MJ
- [ ] Générateurs : PNJ aléatoires, lieux, trésors
- [ ] Carte de monde interactive
- [ ] Ambiance sonore (bibliothèque musicale)

### Fonctionnalités Sociales
- [ ] Système d'amis
- [ ] Messagerie privée
- [ ] Partage de contenu (campagnes, personnages)

