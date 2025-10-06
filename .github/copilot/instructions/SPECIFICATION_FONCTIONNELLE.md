# Spécification Fonctionnelle - Chronique des Mondes

## 📋 Vue d'Ensemble

**Chronique des Mondes** est une plateforme de jeu de rôle (JDR) permettant à des utilisateurs de créer, gérer et participer à des campagnes de jeu.

### Concept Principal
Un utilisateur peut endosser **plusieurs rôles simultanément** :
- **Joueur** dans certaines campagnes
- **Maître du Jeu (MJ)** dans d'autres campagnes
- Même jouer dans des campagnes créées par d'autres tout en étant MJ de ses propres campagnes

### Systèmes de Jeu Supportés
La plateforme supporte deux modes de jeu :
1. **Générique** : Mode libre où le MJ gère manuellement toutes les règles
2. **Spécialisé** : Systèmes avec règles automatisées (D&D, Skyrim à venir)

---

## 👥 Gestion des Utilisateurs

### Inscription et Authentification
- Création de compte utilisateur
- Connexion sécurisée
- Réinitialisation de mot de passe

### Profils Multi-Rôles
- Un utilisateur peut être MJ et joueur simultanément
- Participation à plusieurs campagnes en parallèle
- Historique personnel de toutes les activités

---

## 🎭 Système de Personnages

### Création de Personnages

#### Mode Générique
- **Nom** du personnage
- **Points de vie** (HP)
- **Champs personnalisables** selon les besoins du MJ
- **Gestion manuelle** des caractéristiques

#### Mode Spécialisé (Exemple : D&D)
- **Race** (Elfe, Nain, Humain, etc.)
- **Classe** (Guerrier, Mage, Clerc, etc.)
- **Caractéristiques** (Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme)
- **Compétences** et maîtrises
- **Niveau** et progression
- **Classe d'armure** (CA)
- **Bonus de maîtrise** automatique selon le niveau
- **Calculs automatiques** pour attaques et défense

### Règles de Compatibilité
- Un personnage D&D ne peut rejoindre qu'une campagne D&D
- Un personnage générique peut rejoindre une campagne générique
- Impossibilité de mélanger les systèmes de jeu

### Gestion des Personnages
- Modification des caractéristiques
- Suivi de la progression
- Duplication possible pour créer des variantes
- Suppression de personnages

---

## 🏰 Système de Campagnes

### Création et Configuration

#### Paramètres de Base
- **Nom** de la campagne
- **Description** narrative
- **Système de jeu** (Générique, D&D, etc.)
- **Visibilité** : Publique ou privée

#### Organisation
- **Campagnes privées** : Accessibles uniquement sur invitation
- **Campagnes publiques** : Visibles et rejoignables par tous
- **Un seul MJ** par campagne (le créateur)

### Structure en Chapitres

#### Chapitres
- **Numérotation séquentielle** (Chapitre 1, 2, 3...)
- **Titre** du chapitre
- **Contenu narratif** organisé en blocs de texte
- **Progression linéaire** entre les chapitres

#### Blocs Narratifs
- **Ordre défini** pour structurer l'histoire
- **Texte descriptif** pour le contexte et l'ambiance
- **Liens vers des PNJ** pour interactions

### Gestion des Personnages Non-Joueurs (PNJ)

#### Types de PNJ
1. **PNJ standard** : Personnages d'interaction (marchands, alliés, etc.)
2. **Monstres** : Créatures hostiles pour les combats

#### Comportements PNJ (3 attitudes)
- **🟢 Amical** : Réaction positive aux joueurs
- **🟡 Neutre** : Attitude professionnelle et distante
- **🔴 Hostile** : Réaction agressive ou méfiante

Chaque comportement définit :
- **Réponse du PNJ** selon l'attitude des joueurs
- **Contexte** expliquant la situation
- **Évolution possible** selon les actions des joueurs

#### Caractéristiques des PNJ Génériques
- Nom et surnom
- Description physique et personnalité
- Historique et motivations

#### Caractéristiques des Monstres D&D
- Statistiques de combat complètes (CA, PV, vitesse)
- Caractéristiques (Force, Dextérité, etc.)
- Attaques avec modificateurs et dégâts
- Compétences spéciales
- Sens et langues
- Facteur de puissance (Challenge Rating)
- **Quantité** (ex: groupe de 3 gobelins)

### Invitations et Participation

#### Système d'Invitations
- Le MJ invite des joueurs à sa campagne
- Les joueurs reçoivent des notifications
- Acceptation ou refus de l'invitation
- Sélection d'un personnage compatible pour rejoindre

#### Gestion des Joueurs
- Liste des joueurs invités et leur statut
- Liste des joueurs ayant accepté
- Personnages associés à chaque joueur
- Possibilité de retirer des joueurs

---

## 🪄 Système de Sorts

### Architecture en Deux Niveaux

#### 1. Sorts Officiels 🌟
- **Source** : Bibliothèque de sorts prédéfinie
- **Visibilité** : Accessibles à tous les utilisateurs
- **Exemples** : Sorts D&D officiels (Boule de Feu, Projectile Magique, etc.)
- **Maintenance** : Gérés par l'équipe du jeu

#### 2. Sorts Privés 👤
- **Création** : Par les utilisateurs eux-mêmes
- **Visibilité** : Privés, visibles uniquement par leur créateur
- **Usage** : Pour personnaliser ses campagnes
- **Restriction** : Aucun échange possible entre utilisateurs

### Types de Sorts

#### Sorts Génériques
- **Titre** du sort
- **Description** libre
- **Image** optionnelle
- **Gestion manuelle** par le MJ/joueur

#### Sorts Spécialisés D&D
- **Niveau** de sort (1 à 9)
- **École de magie** (Évocation, Abjuration, etc.)
- **Temps d'incantation** (action, action bonus, réaction)
- **Portée** et **durée**
- **Composantes** (Verbales, Somatiques, Matérielles)
- **Formule de dégâts** (ex: 8d6 feu)
- **Jet de sauvegarde** requis (Dextérité, Sagesse, etc.)
- **Calculs automatiques** selon la classe du lanceur

### Modificateurs de Lancer D&D

#### Calculs Automatiques par Classe
- **Magicien/Occultiste** → Modificateur basé sur Intelligence
- **Clerc/Druide/Rôdeur** → Modificateur basé sur Sagesse
- **Barde/Ensorceleur/Paladin** → Modificateur basé sur Charisme

#### Formules de Calcul
- **Bonus d'attaque de sort** = Modificateur de caractéristique + Bonus de maîtrise
- **Difficulté de sauvegarde (DD)** = 8 + Modificateur de caractéristique + Bonus de maîtrise

#### Exemples
- Mage niveau 5 (Intelligence 18) :
  - Modificateur Intelligence : +4
  - Bonus de maîtrise : +3
  - Bonus d'attaque : +7
  - DD de sauvegarde : 15

### Apprentissage de Sorts
- Un personnage peut apprendre des sorts officiels
- Un personnage peut apprendre ses propres sorts privés
- Un personnage ne peut avoir qu'une seule instance de chaque sort
- Impossible d'apprendre les sorts privés d'un autre utilisateur
- Validation de compatibilité avec le système de jeu

### Consultation des Sorts
- Vue globale : tous les sorts (officiels + personnels)
- Vue filtrée : uniquement les sorts officiels
- Recherche par système de jeu (D&D, Skyrim, etc.)
- Filtrage par niveau, école, etc.

---

## ⚔️ Système d'Équipements

### Architecture en Deux Niveaux

#### 1. Équipements Officiels 🌟
- **Source** : Catalogue d'équipements prédéfini
- **Visibilité** : Accessibles à tous les utilisateurs
- **Exemples** : Épée longue, armure de plates, potion de soins
- **Maintenance** : Gérés par l'équipe du jeu

#### 2. Équipements Privés 👤
- **Création** : Par les utilisateurs
- **Visibilité** : Privés à leur créateur
- **Usage** : Pour équipements uniques et personnalisés
- **Échangeables** : Oui, contrairement aux sorts

### Types d'Équipements

#### Équipements Génériques
- **Titre** de l'objet
- **Description**
- **Image** optionnelle
- **Tags** pour la recherche
- **Gestion manuelle** des effets

#### Équipements Spécialisés D&D
- **Type** d'arme (mêlée, à distance)
- **Bonus de toucher** (+1, +2, etc.)
- **Formule de dégâts** (1d8 + modificateur Force)
- **Type de dégâts** (tranchant, contondant, perforant)
- **Propriétés spéciales** (finesse, polyvalente, etc.)
- **Bonus à la CA** pour les armures
- **Calculs automatiques** en combat

### Inventaire des Personnages
- Chaque personnage possède un inventaire
- **Quantités multiples** possibles pour un même objet
- Ajout et retrait d'équipements
- Équipement actif vs non-équipé
- Poids et encombrement (optionnel selon le système)

### Système d'Échanges d'Équipements 🔄

#### MJ → Joueur (Proposition)
**Fonctionnement :**
1. Le MJ sélectionne un équipement (officiel ou personnel)
2. Le MJ propose cet équipement à un joueur de sa campagne
3. Le joueur reçoit une notification
4. Le joueur accepte ou refuse la proposition

**Résultat si accepté :**
- L'équipement est **copié** dans l'inventaire du joueur
- L'équipement **reste disponible** chez le MJ
- Le MJ peut **reproposer** le même équipement à d'autres joueurs

**Cas d'usage :**
- Récompenses de quêtes
- Équipements de départ
- Objets magiques uniques créés par le MJ

#### Joueur → Joueur (Échange Direct)
**Fonctionnement :**
1. Deux joueurs doivent être dans la **même campagne**
2. Un joueur propose un échange
3. Sélection des objets de chaque côté
4. Validation des quantités
5. L'autre joueur accepte ou refuse

**Résultat si accepté :**
- **Transfert de propriété** complet
- Le joueur donneur **perd** l'objet de son inventaire
- Le joueur receveur **gagne** l'objet dans son inventaire

**Validations :**
- Vérification que les objets existent
- Contrôle des quantités suffisantes
- Confirmation de la même campagne
- Prévention des abus (duplication)

**Cas d'usage :**
- Échange entre compagnons d'aventure
- Don d'objets inutilisés
- Optimisation d'équipe (donner l'armure au tank)

### Gestion des Propositions
- Liste des propositions en attente pour un joueur
- Historique des échanges effectués
- Possibilité d'annuler une proposition non acceptée
- Notifications lors de nouvelles propositions

---

## ⚔️ Système de Combat

### Déclenchement d'un Combat

#### Par le MJ
1. Le MJ consulte un chapitre de sa campagne
2. Visualisation des PNJ et monstres du chapitre
3. Sélection des créatures qui participent au combat
4. Sélection des joueurs participants
5. Lancement du combat

### Calcul d'Initiative
- Chaque participant lance un jet d'initiative
- **Mode D&D** : 1d20 + modificateur de Dextérité
- **Mode générique** : Gestion manuelle par le MJ
- Ordre des tours établi du plus haut au plus bas

### Déroulement des Tours

#### Structure d'un Tour
1. **À votre tour** : Le participant actif est notifié
2. **Actions possibles** :
   - Attaque (avec arme ou sort)
   - Déplacement
   - Action spéciale
   - Passer son tour
3. **Résolution** des actions avec calculs automatiques (D&D) ou manuels
4. **Passage** au participant suivant

#### Notifications de Tour
- Notification visuelle pour le joueur dont c'est le tour
- Cadre coloré autour de son personnage
- Pop-up "À votre tour !"
- Signal sonore optionnel
- Timer optionnel par tour (ex: 2 minutes)

### Calculs Automatiques D&D

#### Jets d'Attaque
- **Attaque au corps à corps** : 1d20 + modificateur Force/Dextérité + bonus de maîtrise
- **Attaque à distance** : 1d20 + modificateur Dextérité + bonus de maîtrise
- **Attaque de sort** : 1d20 + modificateur d'incantation + bonus de maîtrise
- Comparaison avec la Classe d'Armure (CA) de la cible

#### Calcul des Dégâts
- Application de la formule de dégâts de l'arme/sort
- Ajout des modificateurs appropriés
- Gestion des coups critiques (20 naturel = dégâts doublés)
- Résistances et vulnérabilités des créatures

#### Jets de Sauvegarde
- Jet 1d20 + modificateur de caractéristique appropriée
- Comparaison avec le DD (Difficulté de Sauvegarde)
- Demi-dégâts sur sauvegarde réussie (selon le sort)

### Invitations Dynamiques en Combat

#### Ajout de Joueurs en Cours de Combat
1. Le MJ peut inviter un joueur supplémentaire pendant le combat
2. Le joueur reçoit une notification immédiate
3. Le joueur rejoint le combat en cours
4. **Jet d'initiative automatique** pour le nouveau participant
5. **Intégration dans l'ordre des tours** selon le résultat
6. Le joueur joue dès que son tour arrive

#### Cas d'Usage
- Renfort inattendu
- Joueur arrivant en retard à la session
- PNJ allié rejoignant le combat
- Embuscade avec nouveaux ennemis

### État du Combat
- Affichage en temps réel pour tous les participants
- Points de vie actuels de chaque combattant
- Ordre d'initiative visible
- Indicateur de qui joue actuellement
- Historique des actions du combat

### Fin de Combat
- Victoire : Tous les ennemis vaincus
- Défaite : Tous les alliés vaincus
- Fuite : Déclenchée par le MJ
- Résumé du combat (dégâts infligés/reçus, sorts utilisés, etc.)

---

## 🎮 Système de Sessions

### Concept de Session
Une **session** représente une séance de jeu active où le MJ et les joueurs se retrouvent pour faire progresser une campagne.

### Lancement de Session

#### Consultation des Campagnes Disponibles
Un utilisateur peut lancer une session depuis :
1. **Ses propres campagnes** (en tant que MJ)
2. **Campagnes publiques** qu'il a rejointes

#### Processus de Lancement
1. Le lanceur sélectionne une campagne
2. Le lanceur devient automatiquement **MJ de la session**
3. Configuration des paramètres de session :
   - Autoriser les arrivées tardives
   - Notifier les joueurs absents
   - Intervalle de sauvegarde automatique
   - Durée maximale de session
   - Limite de temps par tour (optionnelle)
4. Message de bienvenue optionnel
5. Validation du lancement

#### Notifications de Session
**Joueurs connectés :**
- Notification en temps réel dans l'application
- Pop-up avec options :
  - "Rejoindre maintenant"
  - "Rejoindre plus tard"
- Signal sonore

**Joueurs déconnectés :**
- Email automatique avec :
  - Nom de la campagne
  - Nom du MJ
  - Heure de début
  - Message de bienvenue
  - Lien direct pour rejoindre

### Invitations Pré-Session
- Le MJ peut inviter des joueurs avant de lancer la session
- Les joueurs acceptent ou refusent l'invitation
- Sélection du personnage à utiliser
- Liste des participants confirmés visible avant le lancement

### Session Active

#### Progression par Chapitres
- La session démarre au chapitre actuel de la campagne
- Le MJ lit le contenu narratif aux joueurs
- Les joueurs interagissent avec les PNJ
- Les combats sont lancés selon le scénario
- Transition au chapitre suivant une fois terminé

#### Sauvegarde Automatique
- **Intervalles réguliers** (ex: toutes les 5 minutes)
- **Points critiques** :
  - Fin de combat
  - Fin de chapitre
  - Actions importantes
- **État sauvegardé** :
  - Chapitre actuel
  - État des personnages (HP, sorts utilisés, etc.)
  - Progression dans le chapitre
  - Inventaires
  - Actions effectuées

#### Barre de Progression
- Visualisation du chapitre actuel
- Nombre total de chapitres
- Pourcentage de complétion de la campagne
- Estimation du temps restant

### Fin de Session
- Le MJ clôture la session
- Sauvegarde finale automatique
- Résumé de la session :
  - Chapitres complétés
  - Combats gagnés
  - Objets obtenus
  - Expérience gagnée
  - Temps de jeu
- Notifications de fin envoyées aux participants

### Historique des Sessions
- Liste de toutes les sessions passées
- Date et durée de chaque session
- Participants présents
- Chapitres joués
- Possibilité de restaurer un état précédent

---

## 📊 Système de Statistiques

### Statistiques de Participation

#### Métriques de Sessions
**Mensuel :**
- Nombre de sessions jouées
- Total d'heures de jeu
- Durée moyenne des sessions
- Jours actifs dans le mois
- Jour préféré de la semaine
- Créneau horaire favori

**Annuel :**
- Sessions totales de l'année
- Heures de jeu totales
- Nombre de campagnes participées
- Campagnes complétées
- Période la plus active

**Depuis le début :**
- Nombre total de sessions
- Heures de jeu cumulées
- Date de la première session
- Session la plus longue
- Session la plus courte

#### Tendances
- Croissance mensuelle de l'activité
- Score de régularité
- Risque d'épuisement
- Suggestions pour équilibrer le rythme

### Statistiques de Rôles

#### Distribution Joueur/MJ
- Pourcentage de sessions en tant que joueur
- Pourcentage de sessions en tant que MJ
- Taille moyenne des groupes en tant que joueur
- Nombre de joueurs gérés en tant que MJ
- Durée moyenne des campagnes créées

#### Préférences de Systèmes de Jeu
- Répartition par système de jeu (D&D, générique, etc.)
- Système préféré
- Note de satisfaction moyenne par système

### Statistiques de Dés

#### Performance Globale
- **Total de lancers** effectués
- **Moyenne générale** de tous les lancers
- **Facteur de chance** : Comparaison avec la moyenne théorique
- **Niveau de confiance** : Basé sur le nombre de lancers

#### Analyse par Type de Dé
**Pour chaque type (d20, d6, d8, d10, d12) :**
- Nombre total de lancers
- Moyenne obtenue
- Distribution des résultats
- Résultat le plus fréquent
- Résultat le plus rare

#### Analyse Spécifique d20
- Nombre de 20 naturels (coups critiques)
- Nombre de 1 naturels (échecs critiques)
- Taux de réussite critique
- Plus longue série de jets chanceux
- Plus longue série de jets malchanceux

#### Tendances Temporelles
**Par mois :**
- Moyenne du mois
- Évaluation de la chance (chanceux/normal/malchanceux)
- Amélioration vs mois précédent
- Meilleur mois de l'année
- Pire mois de l'année

#### Analyse par Contexte
**Jets de combat :**
- Jets d'attaque : taux de réussite, coups critiques
- Jets de sauvegarde : taux de réussite
- Jets de dégâts : moyenne

**Tests de compétences :**
- Taux de réussite global
- Compétence la plus réussie
- Compétence à améliorer

#### Superstitions
- Chiffres porte-bonheur identifiés
- Chiffres porte-malheur
- Meilleur jour de la semaine
- Meilleure heure de la journée
- Personnage le plus chanceux

### Statistiques de Combat

#### Performance Globale
- Combats totaux participés
- Taux de victoire
- Combats sans être mis KO
- Ennemis vaincus au total
- Boss vaincus

#### Dégâts et Efficacité
- Dégâts totaux infligés
- Dégâts moyens par coup
- Record de dégâts en une attaque
- Dégâts totaux reçus
- Soins totaux reçus/donnés

#### Sorts et Capacités
- Sorts lancés au total
- Sort le plus utilisé
- Sort le plus efficace (dégâts moyens)
- Capacités spéciales activées

### Statistiques Sociales

#### Interactions
- Sessions jouées avec des amis
- Nouveaux joueurs aidés
- Équipements partagés
- Compliments reçus d'autres joueurs
- Note de travail d'équipe (sur 10)

#### Réseautage
- Nombre de compagnons d'aventure différents
- Joueurs préférés (le plus souvent ensemble)
- Campagnes co-créées

### Évolution des Personnages

#### Progression
- Niveaux gagnés
- Expérience totale obtenue
- Nouveau sorts appris
- Nouveaux équipements obtenus
- Compétences améliorées

#### Graphiques Temporels
- Évolution des points de vie max
- Évolution des caractéristiques
- Évolution de la puissance (calculée)
- Historique des acquisitions majeures

### Rapports Automatiques

#### Rapport Mensuel
Généré automatiquement à la fin de chaque mois contenant :
- Résumé du mois (sessions, heures, grade)
- Moments forts (exploits marquants)
- Records personnels battus
- Statistiques sociales
- Objectifs complétés
- Objectifs en cours
- Suggestions pour le mois prochain

#### Rapport Annuel
Synthèse de l'année complète avec :
- Rétrospective complète
- Évolution année par année
- Comparaison avec l'année précédente
- Classement parmi les joueurs actifs
- Récompenses de fin d'année

---

## 🏆 Système de Succès

### Concept
Les **succès** (achievements) récompensent les joueurs pour leurs accomplissements et les encouragent à explorer différents aspects du jeu.

### Catégories de Succès

#### 1. Combats et Victoires
**Exemples :**
- **Première Victoire** : Gagner son premier combat
- **Tueur de Dragons** : Vaincre un dragon ancien
- **Survivant** : Terminer 10 combats sans être mis KO
- **Bourreau** : Infliger plus de 100 dégâts en un coup
- **Critique Master** : Réaliser 50 coups critiques
- **Boss Slayer** : Vaincre 5 boss différents

**Niveaux de rareté :**
- Commun (facile à obtenir)
- Rare (nécessite effort)
- Épique (accomplissement notable)
- Légendaire (exploit exceptionnel)

#### 2. Exploration et Progression
**Exemples :**
- **Explorateur** : Visiter 10 lieux différents
- **Aventurier Expérimenté** : Compléter 5 campagnes
- **Marathon Runner** : Jouer une session de plus de 6 heures
- **Régulier** : Jouer au moins une session par semaine pendant un mois
- **Nouveau Monde** : Découvrir un nouveau système de jeu

#### 3. Maîtrise des Sorts
**Exemples :**
- **Apprenti Sorcier** : Apprendre 10 sorts
- **Archimage** : Apprendre 50 sorts
- **Créateur de Magie** : Créer 5 sorts personnalisés
- **Maître Élémentaire** : Maîtriser tous les sorts d'une école de magie
- **Lance-Sorts Prolifique** : Lancer 100 sorts en combat

#### 4. Équipements et Richesses
**Exemples :**
- **Collectionneur** : Posséder 50 équipements différents
- **Armurier** : Créer 10 équipements personnalisés
- **Généreux** : Partager 20 équipements avec d'autres joueurs
- **Marchand** : Réaliser 50 échanges
- **Légendaire** : Obtenir un équipement légendaire

#### 5. Chance aux Dés
**Exemples :**
- **Chanceux** : Obtenir 10 jets naturels de 20
- **Série Dorée** : 5 jets consécutifs supérieurs à 15
- **Pas de Chance** : Obtenir 3 jets naturels de 1 d'affilée (succès humoristique)
- **Équilibré** : Maintenir une moyenne de 11+ pendant un mois
- **Mathématicien** : Réaliser 1000 jets de dés

#### 6. Social et Coopération
**Exemples :**
- **Nouveau Compagnon** : Jouer avec 10 joueurs différents
- **Ami Fidèle** : Jouer 20 sessions avec le même groupe
- **Mentor** : Aider 5 nouveaux joueurs
- **Leader** : Créer une campagne avec au moins 4 joueurs
- **Populaire** : Recevoir 50 compliments

#### 7. Maître du Jeu
**Exemples :**
- **Premier MJ** : Créer sa première campagne
- **Conteur** : Écrire 50 blocs narratifs
- **Créateur de Mondes** : Créer 20 PNJ uniques
- **Orchestrateur** : Gérer 10 combats épiques
- **MJ Dédié** : Animer 50 sessions

#### 8. Spéciaux et Événements
**Exemples :**
- **Survivant Héroïque** : Gagner un combat avec 1 PV restant
- **Solo Hero** : Vaincre un boss seul
- **Dernier Debout** : Être le seul survivant d'un combat et gagner
- **Pacifiste** : Terminer un chapitre sans combattre
- **Rapide** : Terminer une session en moins d'une heure

### Système de Déblocage

#### Vérification Automatique
- Les succès sont vérifiés après chaque action significative
- Combat terminé → vérification des succès de combat
- Sort appris → vérification des succès de maîtrise
- Session terminée → vérification des succès d'endurance

#### Déblocage Multiple
- Plusieurs succès peuvent être débloqués simultanément
- Un exploit majeur peut déclencher une cascade de succès
- Exemple : Vaincre un dragon seul → "Tueur de Dragons" + "Solo Hero" + "Survivant Héroïque"

### Célébrations de Succès

#### Niveaux de Célébration
**Commun :**
- Notification discrète
- Son de succès simple
- Icône du succès affichée

**Rare :**
- Notification plus visible
- Animation de l'icône
- Message de félicitations

**Épique :**
- Bannière plein écran
- Animation élaborée
- Musique de victoire
- Partage possible sur profil

**Légendaire :**
- Célébration maximale
- Effets visuels spectaculaires
- Annonce à tous les joueurs de la session
- Badge permanent sur le profil
- Titre déblocable

### Récompenses de Succès

#### Titres
- Déblocage de titres honorifiques
- Affichage sur le profil
- Exemples : "Tueur de Dragons", "Archimage", "Légende Vivante"

#### Bonus Cosmétiques
- Cadres de portrait spéciaux
- Effets visuels pour les personnages
- Emotes exclusives

#### Bonus de Progression
- Points d'expérience bonus
- Accès à du contenu spécial
- Réductions sur créations personnalisées

### Interface de Succès

#### Profil Personnel
- Liste de tous les succès disponibles
- Succès débloqués vs non débloqués
- Pourcentage de complétion global
- Progression vers les succès en cours
- Succès les plus rares obtenus

#### Classements
- Classement par nombre de succès
- Classement par rareté (points de succès)
- Classement par catégorie
- Comparaison avec amis

#### Suggestions
- Succès proches d'être débloqués
- Recommandations d'actions pour débloquer
- Défis temporaires

---

## 📈 Objectifs et Défis

### Objectifs Personnalisés

#### Définition
Les joueurs peuvent se fixer des objectifs personnels :
- Court terme (pour la session)
- Moyen terme (pour le mois)
- Long terme (pour l'année)

#### Types d'Objectifs
- Progression (atteindre niveau X)
- Collection (obtenir X objets)
- Social (jouer avec X personnes)
- Maîtrise (apprendre X sorts)
- Performance (maintenir moyenne de dés > X)

#### Suivi
- Barre de progression pour chaque objectif
- Notifications de jalons importants
- Suggestions d'objectifs réalistes

### Défis Temporaires

#### Défis Hebdomadaires
Nouveaux défis chaque semaine :
- "Gagner 3 combats cette semaine"
- "Lancer 20 sorts de feu"
- "Réaliser 10 coups critiques"

#### Défis Mensuels
Défis plus ambitieux :
- "Compléter une campagne complète"
- "Créer 5 sorts personnalisés"
- "Maintenir moyenne de 12+ aux dés"

#### Récompenses
- Points bonus
- Succès exclusifs temporaires
- Items cosmétiques limités

---

## 🔔 Système de Notifications

### Types de Notifications

#### Notifications de Session
- Lancement de session par un MJ
- Invitation à rejoindre une session
- Session sur le point de commencer
- Tour de jeu imminent

#### Notifications de Combat
- Début de combat
- "À votre tour !"
- Joueur ajouté dynamiquement au combat
- Fin de combat

#### Notifications Sociales
- Invitation à une campagne
- Réponse à une invitation
- Proposition d'équipement
- Acceptation d'échange
- Nouveau compagnon ajouté

#### Notifications de Progression
- Niveau gagné
- Nouveau sort appris
- Équipement obtenu
- Chapitre complété
- Succès débloqué

### Canaux de Notification

#### Dans l'Application
- Notifications en temps réel
- Pop-ups non intrusives
- Badge de notification
- Centre de notifications

#### Par Email
- Notifications importantes
- Résumés quotidiens/hebdomadaires
- Invitation à des événements
- Rapports mensuels

#### Préférences Utilisateur
- Configuration des types de notifications souhaitées
- Choix des canaux (app, email, les deux)
- Fréquence des résumés
- Heures de silence (pas de notifications)

---

## 🎯 Parcours Utilisateur Types

### Parcours 1 : Nouveau Joueur

1. **Inscription** sur la plateforme
2. **Création** d'un premier personnage (mode guidé)
3. **Découverte** des campagnes publiques disponibles
4. **Demande** pour rejoindre une campagne
5. **Acceptation** et première session
6. **Apprentissage** des sorts et obtention d'équipements
7. **Progression** à travers les chapitres
8. **Déblocage** des premiers succès

### Parcours 2 : Joueur Devenant MJ

1. **Expérience** en tant que joueur dans plusieurs campagnes
2. **Décision** de créer sa propre campagne
3. **Création** de la campagne avec système de jeu
4. **Structuration** en chapitres avec contenu narratif
5. **Création** de PNJ et monstres
6. **Invitation** de joueurs
7. **Lancement** de la première session
8. **Animation** de la session avec combats
9. **Gestion** de la progression des joueurs

### Parcours 3 : Joueur Expérimenté Multi-Campagnes

1. **Participation** à plusieurs campagnes simultanément
2. **Gestion** de plusieurs personnages différents
3. **Échange** d'équipements avec autres joueurs
4. **Création** de sorts et équipements personnalisés
5. **Consultation** des statistiques et progression
6. **Déblocage** de succès avancés
7. **Aide** aux nouveaux joueurs
8. **Création** de sa propre campagne en parallèle

### Parcours 4 : MJ Créateur de Contenu

1. **Création** de campagnes élaborées
2. **Développement** de sorts personnalisés pour ses campagnes
3. **Création** d'équipements uniques et magiques
4. **Proposition** d'équipements aux joueurs
5. **Animation** de sessions régulières
6. **Adaptation** du contenu selon les actions des joueurs
7. **Invitations dynamiques** de joueurs en session
8. **Gestion** de multiples campagnes actives

---

## 🎨 Expérience Utilisateur

### Principes de Design

#### Clarté
- Interface intuitive et compréhensible
- Actions principales facilement accessibles
- Feedback visuel pour chaque action

#### Immersion
- Thème visuel adapté au jeu de rôle
- Animations fluides et contextuelles
- Sons d'ambiance et effets sonores

#### Accessibilité
- Support de différentes tailles d'écran
- Mode sombre/clair
- Options de personnalisation

### Feedback et Guidage

#### Tutoriels
- Guide de premiers pas pour nouveaux utilisateurs
- Astuces contextuelles
- Exemples et cas d'usage

#### Messages d'Erreur
- Messages clairs et compréhensibles
- Suggestions pour corriger l'erreur
- Prévention des erreurs courantes

#### Confirmations
- Confirmation pour actions importantes
- Prévention des suppressions accidentelles
- Possibilité d'annulation

---

## 🔒 Règles et Validations

### Sécurité et Permissions

#### Propriété
- Seul le créateur peut modifier sa campagne
- Seul le propriétaire peut modifier son personnage
- Les équipements privés ne sont visibles que par leur créateur

#### Validations de Compatibilité
- Un personnage D&D ne peut rejoindre qu'une campagne D&D
- Un sort D&D ne peut être appris que par un personnage D&D
- Vérification des systèmes de jeu à chaque action

#### Gestion des Échanges
- Vérification de la possession des objets
- Contrôle des quantités avant échange
- Validation que les joueurs sont dans la même campagne
- Prévention de la duplication d'objets

### Intégrité des Données

#### Cohérence
- Un personnage ne peut avoir qu'une instance de chaque sort
- Les équipements ont des quantités valides (>= 0)
- Les relations entre entités sont maintenues

#### Transactions
- Les échanges sont atomiques (tout ou rien)
- Pas de perte d'équipement en cas d'erreur
- Rollback automatique si problème

---

## 📚 Glossaire

**MJ (Maître du Jeu)** : Utilisateur qui crée et anime une campagne, gère le scénario et contrôle les PNJ.

**Joueur** : Utilisateur qui participe à une campagne en incarnant un personnage.

**Campagne** : Histoire et monde de jeu créés par un MJ, composés de chapitres.

**Chapitre** : Section d'une campagne avec son propre contenu narratif et ses PNJ.

**Personnage** : Avatar qu'un joueur incarne dans une campagne.

**PNJ (Personnage Non-Joueur)** : Personnage contrôlé par le MJ, peut être un allié ou un ennemi.

**Session** : Séance de jeu active où MJ et joueurs progressent dans une campagne.

**Sort** : Capacité magique qu'un personnage peut apprendre et utiliser.

**Équipement** : Objet (arme, armure, item) qu'un personnage possède.

**Initiative** : Ordre dans lequel les participants jouent pendant un combat.

**Classe d'Armure (CA)** : Difficulté pour toucher un personnage/créature (D&D).

**Jet de Sauvegarde** : Jet de dé pour résister à un effet (D&D).

**Coup Critique** : Résultat de 20 naturel sur un d20, dégâts doublés (D&D).

**Échec Critique** : Résultat de 1 naturel sur un d20, échec automatique (D&D).

**Modificateur** : Bonus ou malus appliqué à un jet de dé selon les caractéristiques.

**Bonus de Maîtrise** : Bonus basé sur le niveau du personnage (D&D).

**Succès** : Récompense débloquée pour accomplissement d'un exploit.

---

## 🎯 Conclusion

Cette spécification fonctionnelle définit l'ensemble des fonctionnalités de **Chronique des Mondes** du point de vue utilisateur, sans référence aux technologies d'implémentation.

Le système offre une plateforme complète pour :
- ✅ Créer et gérer des campagnes JDR
- ✅ Jouer avec des systèmes de règles automatisés ou libres
- ✅ Collaborer avec d'autres joueurs
- ✅ Suivre sa progression et ses statistiques
- ✅ Débloquer des succès et relever des défis

La flexibilité du système permet à chaque groupe de jouer selon ses préférences, que ce soit avec des règles strictes (D&D) ou en mode narratif libre (générique).
