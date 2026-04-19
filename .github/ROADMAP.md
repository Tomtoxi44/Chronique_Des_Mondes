# Roadmap — Chronique des Mondes

> **Mise à jour** : Roadmap révisée post-implémentation MVP  
> Les items `[x]` sont implémentés et déployés. Les items `[ ]` sont planifiés.

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

## Phase 1 : Enrichissement MVP — EN COURS

### PNJ (Personnages Non-Joueurs)
- [ ] Création de PNJ générique dans un chapitre (nom, description, comportement, PV)
- [ ] Association PNJ ↔ Chapitre
- [ ] Affichage PNJ dans la session MJ
- [ ] Monstres comme sous-type de PNJ

**Critère de succès** : Le MJ peut créer et placer des PNJ dans ses chapitres et les consulter en session.

---

### Temps Réel (SignalR)
- [ ] Hub SignalR pour les sessions actives
- [ ] Synchronisation en temps réel entre MJ et joueurs
- [ ] Notifications push sans rechargement de page
- [ ] Statut de connexion des participants

**Critère de succès** : Les actions du MJ (ex: changement de chapitre) sont reflétées instantanément chez les joueurs.

---

### Communication Email
- [ ] Réinitialisation de mot de passe par email (Azure Communication Services)
- [ ] Validation d'email à l'inscription
- [ ] Invitation d'un joueur par email directement depuis le monde

**Critère de succès** : Un utilisateur peut récupérer son accès via email.

---

### Historique
- [ ] Historique des sessions par campagne (date, participants, chapitres)
- [ ] Historique des mondes rejoints par l'utilisateur

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

## Phase 3 : Combat et Dés — FUTUR

> Dépend de la Phase 2.

### Lanceur de Dés
- [ ] Support d4, d6, d8, d10, d12, d20, d100
- [ ] Nombre de dés variable (XdY + modificateur)
- [ ] Affichage et transmission au MJ
- [ ] Historique des lancers dans la session

### Combat Générique
- [ ] Déclenchement par le MJ, sélection combattants
- [ ] Initiative manuelle, gestion des tours
- [ ] Suivi PV en temps réel
- [ ] Résumé de combat

### Combat D&D Automatisé
- [ ] Initiative 1d20 + modificateur Dextérité
- [ ] Jets d'attaque avec bonus automatiques
- [ ] Calcul dégâts + résistances/vulnérabilités
- [ ] Jets de sauvegarde automatiques

---

## Phase 4 : Sorts et Équipements — FUTUR

### Sorts
- [ ] Création de sorts génériques (titre, description)
- [ ] Sorts D&D spécialisés (niveau, école, composantes, formule de dégâts)
- [ ] Bibliothèque de sorts (officielle + personnelle)
- [ ] Sorts appris par personnage

### Équipements
- [ ] Catalogue d'équipements (officiel + personnel)
- [ ] Inventaire avancé (équipé/non-équipé, poids)
- [ ] Équipements D&D (type, bonus toucher, formule dégâts, propriétés)

### Échanges d'Équipements
- [ ] MJ → Joueur : proposition d'équipement, acceptation/refus
- [ ] Joueur → Joueur : échange direct entre joueurs de la même campagne
- [ ] Notifications de proposition
- [ ] Liste des propositions en attente

---

## Phase 5 : Engagement et Progression — FUTUR

### Système d'Achievements (Succès)
- [ ] Succès à 3 niveaux : Monde, Campagne, Chapitre
- [ ] Rareté : Commun, Rare, Épique, Légendaire
- [ ] Attribution automatique (conditions) + manuelle (MJ)
- [ ] Notifications de déblocage selon la rareté
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

