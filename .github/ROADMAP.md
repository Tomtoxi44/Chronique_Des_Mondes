# Roadmap — Chronique des Mondes

> **Dernière vérification : 20 juillet 2026**, par audit du code source (services,
> endpoints, composants Blazor) et des tests. La révision précédente (18/07) était
> **fortement en retard** : historique chat/dés, échanges d'équipement, succès
> automatiques, invitation par email, D&D avancé (compétences, jets de sauvegarde,
> CA/PV/maîtrise) et combat D&D automatisé étaient marqués « à faire » alors qu'ils
> sont livrés.
>
> **Convention :**
> - `[x]` implémenté et vérifié dans le code
> - `[~]` partiellement implémenté (le détail précise ce qui manque)
> - `[ ]` non commencé
>
> **➡️ Ce qui reste vraiment à faire est résumé en bas de ce document.**

---

## Phase 0 : Fondations MVP — COMPLÉTÉE

### Authentification
- [x] Inscription et authentification (JWT)
- [x] Connexion sécurisée (access + refresh token)
- [x] Déconnexion
- [x] Protection des routes (redirection automatique si non authentifié)
- [x] Réinitialisation de mot de passe (email Azure Communication Services) — pages `ForgotPassword`/`ResetPassword`
- [x] Validation d'email à l'inscription — page `ConfirmEmail`

**Statut** : Complété.

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
- [x] Liste des mondes de l'utilisateur (+ mondes rejoints)
- [x] Modification et suppression de monde
- [x] Vue détail d'un monde (aperçu, personnages, campagnes)
- [x] Système d'invitation : lien URL unique + QR code
- [x] Rejoindre un monde via lien/QR (sélection de personnage)

**Statut** : Complété.

---

### Personnages
- [x] Création de personnage de base (nom, prénom, description, avatar)
- [x] Modification et suppression de personnage de base
- [x] Liste des personnages avec statut (base / copie de monde)
- [x] Personnage de monde (copie adaptée au monde, données JSON GameType)
- [x] Données D&D 5e : classe, sous-classe, race, background, 6 caractéristiques, inventaire
- [x] Vue et modification du personnage de monde par le joueur

**Statut** : Complété.

---

### Campagnes / Chapitres / Sessions / Notifications / Interface
- [x] Campagnes : CRUD, liste, lien d'invitation
- [x] Chapitres : CRUD, numérotation, réorganisation drag-and-drop, arborescence
- [x] Sessions : lancement MJ, vues MJ/Joueur, inventaire en session
- [x] Notifications in-app : badge non-lus, invitation monde, lancement session, liens d'action
- [x] Design system, thèmes par GameType, clair/sombre, dual-sidebar, responsive, i18n fr/en

**Statut** : Complété.

---

## Phase 1 : Enrichissement MVP — COMPLÉTÉE

### PNJ (Personnages Non-Joueurs)
- [x] Création de PNJ dans un chapitre (nom, description, apparence, âge)
- [x] Association PNJ ↔ Chapitre, affichage en session MJ
- [x] Stats D&D 5e du PNJ (type, race, classe, indice de dangerosité, 6 stats, CA, PV, vitesse)
- [~] Attaques du PNJ avec modificateurs, jets de sauvegarde du PNJ — non implémentés

**Statut** : Complété (hors attaques/JdS avancés du PNJ).

---

### Temps Réel (SignalR)
- [x] Hubs `SessionHub`, `CombatHub`, `NotificationHub`
- [x] Synchronisation temps réel MJ ↔ joueurs, push sans rechargement
- [x] Statut de connexion des participants
- [x] Autorisation d'appartenance à la campagne vérifiée avant de rejoindre un groupe

**Statut** : Complété.

---

### Communication Email
- [x] Réinitialisation de mot de passe par email
- [x] Validation d'email à l'inscription
- [x] Invitation d'un joueur par email depuis le monde (`InvitePlayerByEmailAsync`)

**Statut** : Complété.

---

### Historique
- [x] **Rechargement de l'historique du chat et des dés à l'ouverture d'une session**
      (`GetSessionHistoryAsync` + reconstruction de la timeline avant connexion au hub, MJ et Joueur)
- [~] Historique des sessions par campagne — page `/sessions` (liste), pas de vue par campagne détaillée
- [x] Liste des mondes rejoints (section « Mondes rejoints » de `/worlds`)

---

## Phase 3 : Combat et Dés — COMPLÉTÉE

> Livrée dès avril 2026.

### Lanceur de Dés
- [x] d4, d6, d8, d10, d12, d20, d100, XdY + modificateur
- [x] Diffusion SignalR à la table
- [x] Historique des lancers persisté **et rechargé** à l'ouverture

### Combat Générique
- [x] Déclenchement MJ, sélection des combattants, initiative, gestion des tours, suivi PV, résumé

### Combat D&D Automatisé
- [x] Initiative auto 1d20 + modificateur de Dextérité (résolue serveur)
- [x] Jets d'attaque d20 + détection du critique (nat 20 / nat 1)
- [~] Calcul des dégâts avec résistances/vulnérabilités — dégâts oui, résistances/vulnérabilités non
- [~] Jets de sauvegarde automatiques en combat — partiels

---

## Phase 2 / 4 : Systèmes de Jeu — D&D COMPLÉTÉ

### D&D 5e Avancé (personnage)
- [x] Sous-classe (archétype)
- [x] Compétences et maîtrises (onglet Compétences, sauvegarde des maîtrises)
- [x] Classe d'armure, points de vie, bonus de maîtrise (`DndRules` : `ProficiencyBonus`, `UnarmoredArmorClass`, `AverageHitPoints`)
- [x] Jets de sauvegarde (onglet dédié)

### Sorts & Équipements
- [x] Sorts D&D (`DndSpell`), bibliothèque officielle (seeder), sorts appris par personnage
- [~] Sorts génériques (titre/description) — base présente, peu développée
- [x] Catalogue d'équipements officiel (seeder), équipements D&D
- [~] Inventaire — gestion en session OK, **pas de notion de poids**

### Échanges d'Équipements
- [x] MJ → Joueur : proposition, acceptation/refus (UI en session MJ et Joueur)
- [x] Joueur → Joueur : échange dans la même campagne
- [x] Notifications de proposition, liste des propositions en attente

### Autres Types de Jeu (systèmes de règles spécifiques)
- [ ] Skyrim, Pathfinder, Call of Cthulhu, Warhammer, Cyberpunk : **thème visuel présent, mais pas de bloc de stats/règles dédié** (profil générique utilisé). Seul D&D 5e a un système de règles complet.

---

## Phase 5 : Engagement et Progression — PARTIELLEMENT COMPLÉTÉE

### Système d'Achievements (Succès)
- [x] Modèle à 3 niveaux : Monde, Campagne, Chapitre (`AchievementLevel`)
- [x] Rareté : Commun, Rare, Épique, Légendaire
- [x] Attribution manuelle (MJ)
- [x] Attribution automatique par conditions (`AchievementEvaluationService` : critiques, fumbles, nb de jets, sessions…)
- [x] Notification `AchievementUnlocked` déclenchée à l'attribution
- [x] Affichage dans le profil joueur (section « Mes succès »)
- [~] UI de création aux niveaux Campagne/Chapitre (le modèle le permet ; création surtout exposée au niveau Monde)

### Statistiques de Dés
- [x] Total de lancers, moyenne, analyse d20 (nat 20 / nat 1, taux de critique/échec), par type de dé
- [x] Affichage dans le profil
- [ ] Tendances temporelles (par mois, par campagne)
- [ ] Segmentation par système de jeu

### Statistiques de Participation — **NON COMMENCÉ**
- [ ] Sessions jouées, heures de jeu (mensuel / annuel / total)
- [ ] Distribution MJ/Joueur, taille moyenne des groupes
- [ ] Historique par campagne

### Rapports Automatiques — **NON COMMENCÉ**
- [ ] Rapport mensuel (activité, records, moments forts)
- [ ] Rapport annuel (rétrospective)
- [ ] Envoi optionnel par email

---

## Phase 6 : Expérience Immersive — NON COMMENCÉE

### Salon Vocal en Session
- [ ] Audio pendant les sessions, contrôle MJ (muet/actif), partage d'écran

### Autres Outils MJ
- [ ] Générateurs (PNJ aléatoires, lieux, trésors)
- [ ] Carte de monde interactive
- [ ] Ambiance sonore (bibliothèque musicale)

### Fonctionnalités Sociales
- [ ] Système d'amis
- [ ] Messagerie privée
- [ ] Partage de contenu (campagnes, personnages)

---

## ➡️ Ce qui reste vraiment à faire (par effort croissant)

**Petit / bounded**
1. **Poids de l'inventaire** (D&D) — champ + affichage + capacité de charge.
2. **PNJ D&D : attaques + jets de sauvegarde** (compléter le bloc de stats existant).
3. **Combat D&D : résistances/vulnérabilités + JdS automatiques** (compléter l'auto existant).
4. **UI de création de succès aux niveaux Campagne/Chapitre** (le back le permet déjà).

**Moyen**
5. **Statistiques de participation** (sessions jouées, heures, distribution MJ/Joueur) — nouveau service + page.
6. **Stats de dés : tendances temporelles + segmentation par système** (étendre `DiceStatsDto`/`StatisticsService`).
7. **Sorts génériques** (hors D&D) — enrichir la création.

**Gros**
8. **Rapports automatiques** mensuel/annuel + envoi email.
9. **Systèmes de règles des autres jeux** (Skyrim, Pathfinder, Cthulhu, Warhammer, Cyberpunk) — chacun = un bloc de stats/règles dédié.
10. **Phase 6** : vocal, générateurs, carte interactive, ambiance sonore, amis, messagerie, partage.
