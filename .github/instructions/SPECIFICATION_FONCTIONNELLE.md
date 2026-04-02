# Spécification Fonctionnelle - Chronique des Mondes

## 📋 Vue d'Ensemble

**Chronique des Mondes** est une application d'assistance aux jeux de rôle (JDR) avec deux interfaces complémentaires :

- 🖥️ **Desktop (Maître de Jeu)** - Interface complète pour créer et gérer des mondes, campagnes et sessions
- 📱 **Mobile (Joueurs)** - Interface optimisée pour participer aux sessions et gérer ses personnages

### Hiérarchie Principale

L'application suit une organisation hiérarchique en trois niveaux :

**Monde** → **Campagne** → **Chapitre**

- **Monde** : Univers de jeu avec règles spécifiques
  - Le créateur devient automatiquement **Maître du Jeu**
  - Peut être créé vide (**Empty**) pour servir de salon d'invitation
  - Contient plusieurs campagnes
  
- **Campagne** : Histoire/aventure dans un monde
  - Peut impacter le monde
  - Contient plusieurs chapitres
  
- **Chapitre** : Séances/épisodes d'une campagne
  - Contenu organisé avec événements et PNJ
  - Sessions en ligne avec salon vocal

### Systèmes de Jeu Supportés

1. **Empty** : Monde vide servant uniquement de salon d'invitation
2. **Custom** : Mode libre où le MJ gère manuellement toutes les règles
3. **D&D 5e** : Dungeons & Dragons avec règles automatisées
4. **Skyrim** : Elder Scrolls avec mécaniques spécifiques

### Concept Principal

Un utilisateur peut endosser **plusieurs rôles simultanément** :
- **Joueur** dans certaines campagnes
- **Maître du Jeu (MJ)** dans d'autres campagnes
- Même jouer dans des campagnes créées par d'autres tout en étant MJ de ses propres campagnes

---

## 🌍 Système de Mondes

### Création d'un Monde

#### Paramètres de Base
- **Nom** du monde
- **Description** de l'univers
- **Système de règles** (Empty, Custom, D&D, Skyrim)
- **Visibilité** : Public ou privé

#### Règles du Monde
- Le **créateur** devient automatiquement **Maître du Jeu** du monde
- Un monde peut contenir **plusieurs campagnes**
- Les règles du monde s'appliquent à toutes les campagnes qu'il contient
- Un monde **Empty** sert uniquement de salon d'invitation (pas de règles de jeu)

### Gestion d'un Monde

#### Campagnes du Monde
- Liste de toutes les campagnes du monde
- Création de nouvelles campagnes dans le monde
- Suppression de campagnes (par le MJ uniquement)

#### Personnages du Monde
- Liste de tous les personnages ayant intégré le monde
- Consultation des caractéristiques selon les règles du monde

### Types de Mondes

#### Monde Empty
- **Usage** : Salon d'invitation, lieu de rencontre
- **Règles** : Aucune
- **Personnages** : Peuvent rejoindre sans adaptation
- **But** : Socialisation, recrutement pour d'autres mondes

#### Monde Custom
- **Usage** : Règles personnalisées par le MJ
- **Règles** : Définies librement par le MJ
- **Personnages** : Adaptés manuellement par le MJ

#### Monde D&D 5e
- **Usage** : Campagnes Dungeons & Dragons
- **Règles** : D&D 5ème édition officielles
- **Personnages** : Caractéristiques D&D automatiques (classes, races, etc.)
- **Calculs** : Jets de dés, CA, sauvegardes automatisés

#### Monde Skyrim
- **Usage** : Campagnes Elder Scrolls
- **Règles** : Système Skyrim
- **Personnages** : Compétences, perks, races de Skyrim
- **Calculs** : Formules Skyrim pour combat et magie

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

### Deux Niveaux de Personnages

#### 1. Personnage de Base (Original)
Créé par l'utilisateur dans son profil :
- **Nom** et **Prénom**
- **Description** générale
- **Image/Avatar**
- **Caractéristiques de base** (points de vie, attributs génériques)
- **Statut** : Disponible ou Verrouillé

#### 2. Personnage de Monde (Copie Adaptée)
Créé automatiquement lors de l'intégration dans un monde :
- **Copie** du personnage de base
- **Adaptation** selon les règles du monde
- **Caractéristiques spécifiques** au système de jeu (D&D, Skyrim, etc.)
- **Lien** vers le personnage original

### Processus d'Intégration

#### Rejoindre un Monde
1. Le joueur sélectionne un **personnage de base disponible**
2. Le joueur demande à rejoindre un **monde**
3. Le système crée une **copie** du personnage
4. La copie est **adaptée** aux règles du monde :
   - **Empty** : Copie simple sans modification
   - **Custom** : Adaptation manuelle par le MJ
   - **D&D** : Ajout de race, classe, caractéristiques D&D
   - **Skyrim** : Ajout de race, compétences, perks Skyrim
5. Le personnage de base devient **verrouillé**
6. Le personnage de monde est **actif** dans le monde

#### Règle de Verrouillage
- Un personnage de base ayant intégré un monde devient **verrouillé**
- Un personnage verrouillé **ne peut plus rejoindre** d'autres mondes
- Cette règle évite les incohérences (un même personnage ne peut pas avoir des stats D&D et Skyrim simultanément)
- Le personnage original **reste visible** dans le profil mais est marqué comme "Intégré dans [Nom du Monde]"

#### Créer des Copies pour Plusieurs Mondes
- Pour jouer dans plusieurs mondes, l'utilisateur doit créer **plusieurs personnages de base**
- Chaque personnage de base peut rejoindre **un seul monde**
- Exemple :
  - **Aragorn** (personnage de base) → rejoint **Monde D&D** → devient **Aragorn (D&D)** avec classe Rôdeur
  - **Dovahkiin** (personnage de base) → rejoint **Monde Skyrim** → devient **Dovahkiin (Skyrim)** avec compétences Skyrim

### Création de Personnages

#### Mode Empty
- **Nom** et **Prénom**
- **Description** libre
- **Points de vie** (optionnel)
- Aucune caractéristique spécifique

#### Mode Custom
- **Nom** et **Prénom**
- **Points de vie** (HP)
- **Champs personnalisables** selon les besoins du MJ
- **Gestion manuelle** des caractéristiques par le MJ

#### Mode D&D 5e
- **Nom** et **Prénom**
- **Race** (Elfe, Nain, Humain, Demi-Elfe, etc.)
- **Classe** (Guerrier, Mage, Clerc, Rôdeur, etc.)
- **Sous-classe** (Archétype)
- **Caractéristiques** (Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme)
- **Compétences** et maîtrises
- **Niveau** et progression (1-20)
- **Classe d'armure** (CA) calculée automatiquement
- **Bonus de maîtrise** automatique selon le niveau
- **Points de vie maximum** calculés selon classe et Constitution
- **Calculs automatiques** pour attaques et défense

#### Mode Skyrim
- **Nom** et **Prénom**
- **Race** (Nord, Elfe noir, Khajiit, Argonien, etc.)
- **Compétences** (Combat, Magie, Furtivité)
- **Perks** débloqués
- **Santé**, **Magie**, **Endurance**
- **Niveau** général et par compétence
- **Calculs automatiques** selon les formules Skyrim

### Règles de Compatibilité
- Un personnage **D&D** ne peut rejoindre qu'un monde **D&D**
- Un personnage **Skyrim** ne peut rejoindre qu'un monde **Skyrim**
- Un personnage **Custom** peut rejoindre un monde **Custom**
- Un personnage **Empty** peut rejoindre n'importe quel monde
- Impossibilité de mélanger les systèmes de jeu

### Gestion des Personnages
- **Modification** des caractéristiques (avant verrouillage uniquement pour le personnage de base)
- **Modification** du personnage de monde par le MJ ou le joueur selon les règles
- **Suivi de la progression** (niveaux, expérience)
- **Suppression** de personnages de base (uniquement si non verrouillés)
- **Consultation** de l'historique (combats, sessions, événements)

---

## 🏰 Système de Campagnes

### Création et Configuration

#### Paramètres de Base
- **Nom** de la campagne
- **Description** narrative
- **Monde parent** : La campagne appartient obligatoirement à un monde
- **Visibilité** : Publique ou privée (héritée du monde ou différente)

#### Organisation
- Une campagne **appartient à un monde** spécifique
- **Campagnes privées** : Accessibles uniquement sur invitation
- **Campagnes publiques** : Visibles et rejoignables par tous
- Le **MJ du monde** peut créer des campagnes dans son monde
- Le système de jeu est **hérité du monde** (pas de choix à la création)

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

#### Caractéristiques des PNJ/Monstres Génériques
- **Nom** et surnom
- **Description** physique et personnalité
- **Historique** et motivations
- **Points de vie** (si applicable)
- **Capacités** et compétences (définies librement par le MJ)
- **Équipement** possédé
- **Quantité** (pour les groupes, ex: 3 bandits)

#### Caractéristiques Spécifiques D&D
En plus des caractéristiques génériques, les PNJ/Monstres D&D possèdent :
- **Statistiques de combat** complètes (CA, PV, vitesse)
- **Caractéristiques** (Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme)
- **Attaques** avec modificateurs et formules de dégâts
- **Compétences spéciales** et capacités
- **Jets de sauvegarde** et résistances
- **Sens** (vision dans le noir, perception passive) et langues
- **Facteur de puissance** (Challenge Rating) pour l'équilibrage des combats
- **Calculs automatiques** basés sur les règles D&D 5e

### Invitations et Participation

#### Système d'Invitations
- Le MJ invite des joueurs à sa campagne
- Les joueurs reçoivent des notifications
- Acceptation ou refus de l'invitation
- Sélection d'un personnage compatible pour rejoindre

#### Gestion des Joueurs
- Liste des joueurs invités et leur statut
- Liste des joueurs ayant accepté
- Personnages associés à chaque joueur (personnages de monde)
- Possibilité de retirer des joueurs

---

## 🎯 Système d'Événements

### Concept

Les **événements** sont des modificateurs créés par le MJ qui affectent les personnages et l'ambiance du jeu. Ils peuvent être appliqués à trois niveaux de la hiérarchie.

### Niveaux d'Application

#### 1. Événement de Monde
- **Portée** : Affecte tous les personnages de toutes les campagnes du monde
- **Durée** : Permanent ou temporaire
- **Exemples** :
  - "Éclipse Éternelle" : -2 en Sagesse pour tous les personnages du monde
  - "Bénédiction Divine" : +1 en Charisme pour tous
  - "Famine Globale" : -10 PV maximum pour tous

#### 2. Événement de Campagne
- **Portée** : Affecte tous les personnages participant à la campagne
- **Durée** : Lié à la campagne (disparaît si la campagne se termine)
- **Exemples** :
  - "Malédiction du Dragon" : -3 en Force pour tous les participants
  - "Alliance des Mages" : +5 bonus aux sorts pour tous
  - "Blessure de Guerre" : Réduction de 20% des PV max

#### 3. Événement de Chapitre
- **Portée** : Affecte les personnages uniquement pendant ce chapitre
- **Durée** : Disparaît à la fin du chapitre
- **Exemples** :
  - "Brouillard Empoisonné" : -1 en Constitution pendant le chapitre
  - "Inspiration Héroïque" : +2 à tous les jets d'attaque
  - "Fatigue Extrême" : -5 en Initiative

### Création d'Événements

#### Paramètres
- **Nom** de l'événement
- **Description** narrative (pourquoi cet événement se produit)
- **Niveau d'application** (Monde, Campagne, Chapitre)
- **Type d'effet** :
  - Modificateur de caractéristique (Force, Dextérité, etc.)
  - Modificateur de PV (maximum ou actuels)
  - Modificateur de jets de dés (attaque, sauvegarde, etc.)
  - Effet narratif (pas de mécanique, juste RP)
- **Valeur** du modificateur (+/- X)
- **Durée** : Permanent, temporaire (X jours), jusqu'à condition

#### Permissions
- Seul le **MJ du monde** peut créer des événements de Monde
- Le **MJ** d'une campagne peut créer des événements de Campagne
- Le **MJ** peut créer des événements de Chapitre pour le chapitre actif

### Impact sur les Personnages

#### Application Automatique
- Les événements s'appliquent **automatiquement** aux personnages concernés
- Les modificateurs sont **cumulatifs** (plusieurs événements peuvent s'additionner)
- Les personnages voient leurs stats ajustées en temps réel
- Exemple : Un personnage avec 16 en Dextérité sous "Éclipse Éternelle" (-2 Dex) aura 14

#### Affichage pour les Joueurs
- Liste des événements actifs affectant le personnage
- Origine de chaque événement (Monde, Campagne, Chapitre)
- Modificateurs totaux appliqués
- Icône/indicateur visuel sur la fiche de personnage

#### Gestion par le MJ
- Création, modification, suppression d'événements
- Activation/désactivation temporaire
- Historique des événements passés
- Aperçu de l'impact global sur tous les personnages

---

## 🏆 Système de Succès (Achievements)

### Concept

Les **succès** (achievements) récompensent les joueurs pour leurs accomplissements dans un monde, une campagne ou un chapitre spécifique. Ils sont créés par le MJ et peuvent être attribués automatiquement ou manuellement.

### Niveaux de Succès

#### 1. Succès de Monde
- **Portée** : Disponibles pour tous les joueurs du monde
- **Attribution** : Conservés même après la fin des campagnes
- **Exemples** :
  - "Explorateur du Monde" : Visiter 10 lieux différents du monde
  - "Légende du Monde" : Atteindre le niveau 20
  - "Héros du Monde" : Vaincre le boss final du monde

#### 2. Succès de Campagne
- **Portée** : Spécifiques à une campagne
- **Attribution** : Liés à la progression dans la campagne
- **Exemples** :
  - "Vainqueur du Dragon Noir" : Vaincre le dragon de la campagne
  - "Sauveur de la Ville" : Compléter la quête principale
  - "Compagnon Fidèle" : Participer à 10 sessions de la campagne

#### 3. Succès de Chapitre
- **Portée** : Spécifiques à un chapitre
- **Attribution** : Débloqués pendant ou à la fin du chapitre
- **Exemples** :
  - "Découvreur de Secrets" : Trouver la salle cachée du chapitre 3
  - "Pacifiste" : Terminer le chapitre sans combat
  - "Speedrunner" : Terminer le chapitre en moins de 2 heures

### Création de Succès

#### Paramètres
- **Nom** du succès
- **Description** (comment l'obtenir)
- **Niveau** (Monde, Campagne, Chapitre)
- **Rareté** :
  - Commun (facile à obtenir)
  - Rare (nécessite effort)
  - Épique (accomplissement notable)
  - Légendaire (exploit exceptionnel)
- **Icône** ou image du succès
- **Récompense** (optionnelle) :
  - Points d'expérience bonus
  - Titre honorifique
  - Objet unique
- **Condition de déblocage** :
  - Automatique (basée sur une statistique)
  - Manuelle (attribué par le MJ)

#### Permissions
- Le **MJ du monde** peut créer des succès de Monde
- Le **MJ** d'une campagne peut créer des succès de Campagne
- Le **MJ** peut créer des succès de Chapitre

### Modes d'Attribution

#### Attribution Automatique
Le système détecte automatiquement quand un joueur remplit les conditions :
- **Conditions statistiques** :
  - "Atteindre niveau X"
  - "Infliger X dégâts en un coup"
  - "Vaincre X ennemis"
  - "Participer à X sessions"
- **Vérification continue** après chaque action significative
- **Notification immédiate** au déblocage
- **Bannière de célébration** selon la rareté

#### Attribution Manuelle par le MJ
Le MJ décide manuellement d'attribuer le succès :
- **Succès narratifs** (actions créatives, roleplay exceptionnel)
- **Succès secrets** (conditions non révélées aux joueurs)
- **Succès événementiels** (participation à un événement spécial)
- Le MJ sélectionne un ou plusieurs joueurs et leur attribue le succès
- Notification envoyée aux joueurs concernés

### Interface pour les Joueurs

#### Liste des Succès
- **Tous les succès** du monde/campagne/chapitre actuel
- Succès **débloqués** (avec date et heure)
- Succès **verrouillés** (avec indice ou description complète selon le choix du MJ)
- **Progression** vers les succès automatiques (ex: "15/50 sorts appris")
- **Filtrage** par niveau (Monde, Campagne, Chapitre) et rareté

#### Célébrations
**Commun :**
- Notification discrète
- Son de succès simple
- Badge ajouté au profil

**Rare :**
- Notification plus visible
- Animation de l'icône
- Message de félicitations

**Épique :**
- Bannière plein écran
- Animation élaborée
- Annonce aux autres joueurs de la session

**Légendaire :**
- Célébration maximale avec effets visuels
- Annonce à tous les joueurs du monde
- Badge permanent ultra-visible
- Titre honorifique déblocable

### Gestion par le MJ

#### Tableau de Bord
- Liste de tous les succès créés (par niveau)
- Statistiques de déblocage (combien de joueurs ont obtenu chaque succès)
- Succès les plus rares / les plus communs
- Historique des attributions manuelles

#### Attribution Manuelle
- Sélection d'un succès
- Sélection des joueurs concernés
- Confirmation avec message optionnel
- Log de l'attribution pour traçabilité

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

#### Lancé Uniquement par le MJ
1. Le MJ consulte un chapitre de sa campagne
2. Visualisation des PNJ et monstres disponibles
3. Sélection des créatures qui participent au combat
4. Sélection des joueurs participants (personnages de la campagne en cours)
5. Lancement du combat

#### Types de Combat
- **Joueurs vs Créatures** : Combat classique contre des PNJ hostiles ou monstres
- **Joueurs vs Joueurs** (PvP) : Combat entre personnages joueurs (arène, duel, etc.)
- **Mixte** : Joueurs + PNJ alliés vs Joueurs + PNJ ennemis

### Calcul d'Initiative

#### Mode Empty/Custom
- Le MJ définit l'ordre des tours selon sa méthode préférée :
  - **Ordre manuel** : Le MJ décide de l'ordre des participants
  - **Jets de dés libres** : Le MJ demande des jets (ex: 1d6, 1d10) et ordonne les résultats
  - Le joueur peut lancer physiquement ou utiliser l'application
  - Les résultats sont transmis au MJ qui établit l'ordre

#### Mode D&D/Skyrim (systèmes à règles automatisées)
- Chaque participant lance automatiquement un **jet d'initiative**
- **Formule D&D** : 1d20 + modificateur de Dextérité
- **Formule Skyrim** : Basée sur les compétences de combat
- Calcul automatique par l'application
- Ordre des tours établi automatiquement du plus haut au plus bas
- Les **événements actifs** sont pris en compte dans les modificateurs

**Note** : En mode D&D/Skyrim, le MJ peut désactiver les calculs automatiques pour gérer l'initiative manuellement (mode hybride).

### Déroulement des Tours

#### Structure d'un Tour
1. **À votre tour** : Le participant actif est notifié
2. **Actions possibles** :
   - Attaque (avec arme ou sort)
   - Déplacement
   - Action spéciale
   - Passer son tour
3. **Résolution** des actions :
   - **Mode générique** : Gestion manuelle par le MJ
   - **Mode D&D/Skyrim** : Calculs automatiques selon les règles
4. **Passage** au participant suivant

#### Notifications de Tour
- Notification visuelle pour le joueur dont c'est le tour
- Cadre coloré autour de son personnage
- Pop-up "À votre tour !"
- Signal sonore optionnel
- Timer optionnel par tour (ex: 2 minutes)

### Résolution des Actions

#### Mode Empty/Custom - Gestion Manuelle

**Principe** : Le MJ demande des jets de dés et interprète les résultats selon sa propre logique.

**Déroulement d'une attaque** :
1. Le MJ demande un jet (ex: "Lance 2d6 pour attaquer")
2. Le joueur peut :
   - Lancer physiquement et entrer la valeur
   - Utiliser le lanceur de dés de l'application
3. Le résultat est transmis au MJ
4. Le MJ interprète et applique les conséquences :
   - Détermine si l'attaque touche
   - Calcule ou décide des dégâts
   - Réduit les PV du monstre/PNJ manuellement

**Flexibilité** :
- Types de dés libres (d4, d6, d8, d10, d12, d20, d100)
- Nombre de dés variable selon la situation
- Interprétation narrative des résultats
- Pas de formules fixes

#### Mode D&D/Skyrim - Calculs Automatiques avec Statistiques

**Principe** : L'application connaît les règles et ajoute automatiquement les statistiques du personnage aux jets de dés.

**Jets d'Attaque D&D** :
- **Le joueur lance le dé** (ou utilise le lanceur automatique)
- **Le système ajoute automatiquement** :
  - Modificateur de Force/Dextérité (selon l'arme)
  - Bonus de maîtrise
  - Modificateurs d'événements actifs
- **Formule complète** : Résultat du dé + modificateurs automatiques
- Exemple : Le joueur lance 1d20 et obtient 14 → le système affiche "14 + 5 (Force) + 3 (Maîtrise) = 22"
- Comparaison automatique avec la Classe d'Armure (CA) de la cible
- Résultat affiché : "Touché !" ou "Raté !"

**Calcul des Dégâts D&D** :
- **Le joueur lance les dés de dégâts** de l'arme/sort
- **Le système ajoute automatiquement** :
  - Modificateur de Force/Dextérité (pour les armes)
  - Bonus d'arme magique (+1, +2, etc.)
  - Modificateurs d'événements actifs
- **Gestion automatique** :
  - Coups critiques (20 naturel = dégâts doublés)
  - Résistances et vulnérabilités des créatures
  - Mise à jour automatique des PV de la cible
- Exemple : Le joueur lance 1d8 et obtient 6 → le système affiche "6 + 4 (Force) = 10 dégâts"

**Jets de Sauvegarde D&D** :
- Jet 1d20 + modificateur de caractéristique appropriée (ajouté automatiquement)
- Comparaison automatique avec le DD (Difficulté de Sauvegarde)
- Application automatique des effets (demi-dégâts, immunité, etc.)
- Les événements actifs modifient les jets de sauvegarde

**Skyrim** :
- Formules de dégâts basées sur les compétences
- Bonus de perks appliqués automatiquement
- Calculs d'armure et de résistance Skyrim

**Vue Spécifique pour MJ et Joueurs** :
- **Vue MJ** : Voit tous les jets, PV de toutes les créatures, peut modifier manuellement
- **Vue Joueur** : Voit uniquement ses jets et les informations publiques (PV visibles, etc.)
- **Synchronisation temps réel** via SignalR

**Mode Hybride** :
- Le MJ peut désactiver les calculs automatiques même en mode D&D/Skyrim
- Permet de gérer manuellement certaines situations spéciales
- Utile pour des règles maison ou des scénarios narratifs

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

**Organisation des statistiques** : Les données sont segmentées par système de jeu (Générique, D&D, Skyrim) et par campagne.

#### Performance Globale
- **Total de lancers** effectués (tous types de dés confondus)
- **Moyenne générale** de tous les lancers
- **Facteur de chance** : Comparaison avec la moyenne théorique
- **Niveau de confiance** : Basé sur le nombre de lancers
- **Par système de jeu** : Statistiques séparées pour Générique, D&D, Skyrim
- **Par campagne** : "Dans cette campagne, vous avez lancé X dés avec une moyenne de Y"

#### Analyse par Type de Dé
**Pour chaque type (d4, d6, d8, d10, d12, d20, d100) :**
- Nombre total de lancers
- Moyenne obtenue
- Distribution des résultats
- Résultat le plus fréquent
- Résultat le plus rare
- Écart par rapport à la moyenne théorique

**Note** : En mode générique, les types de dés utilisés varient selon les campagnes. En D&D, le d20 est prépondérant.

#### Analyse Spécifique d20 (principalement D&D)
Cette section s'applique uniquement aux campagnes utilisant le système d20 (D&D principalement) :
- **20 naturels** (coups critiques) : Nombre et taux
- **1 naturels** (échecs critiques) : Nombre et taux
- **Taux de réussite critique** vs échecs critiques
- **Plus longue série** de jets chanceux
- **Plus longue série** de jets malchanceux
- **Comparaison** avec la probabilité théorique (5% pour chaque)

#### Tendances Temporelles
**Par mois :**
- Moyenne du mois (tous dés / par type de dé)
- Évaluation de la chance (chanceux/normal/malchanceux)
- Amélioration vs mois précédent
- Meilleur mois de l'année
- Pire mois de l'année
- Détail par système de jeu utilisé

#### Analyse par Contexte

**Jets de combat (tous systèmes)** :
- Nombre total de jets en combat
- Jets d'attaque : taux de réussite (si applicable en D&D)
- Jets de dégâts : moyenne par type d'arme/sort
- Jets de sauvegarde : taux de réussite (D&D)

**Tests de compétences (D&D)** :
- Taux de réussite global
- Compétence la plus réussie
- Compétence à améliorer
- Jets d'initiative : moyenne

**Jets narratifs (Générique)** :
- Distribution des types de dés utilisés
- Contextes les plus fréquents (combat, exploration, social)

#### Superstitions et Patterns
- **Chiffres porte-bonheur** identifiés (par type de dé)
- **Chiffres porte-malheur** (par type de dé)
- **Meilleur jour** de la semaine pour lancer les dés
- **Meilleure heure** de la journée
- **Personnage le plus chanceux** (par système de jeu)
- **Campagne la plus chanceuse** (statistiques par campagne)

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

### Termes Généraux (tous systèmes)

**Monde** : Univers de jeu avec règles spécifiques (Empty, Custom, D&D, Skyrim). Contient plusieurs campagnes. Le créateur devient automatiquement Maître du Jeu.

**MJ (Maître du Jeu)** : Utilisateur qui crée et anime un monde/campagne, gère le scénario et contrôle les PNJ.

**Joueur** : Utilisateur qui participe à une campagne en incarnant un personnage.

**Campagne** : Histoire et aventure dans un monde, composée de chapitres. Hérite du système de règles du monde.

**Chapitre** : Section d'une campagne avec son propre contenu narratif, PNJ et événements.

**Personnage de Base** : Personnage original créé par l'utilisateur dans son profil. Peut rejoindre un seul monde.

**Personnage de Monde** : Copie d'un personnage de base adaptée aux règles d'un monde spécifique (D&D, Skyrim, etc.).

**Verrouillage** : État d'un personnage de base ayant rejoint un monde. Il ne peut plus rejoindre d'autres mondes.

**PNJ (Personnage Non-Joueur)** : Personnage contrôlé par le MJ, peut être un allié ou un ennemi.

**Session** : Séance de jeu active où MJ et joueurs se retrouvent pour faire progresser une campagne.

**Événement** : Modificateur créé par le MJ applicable à un Monde, une Campagne ou un Chapitre. Affecte les personnages automatiquement.

**Succès (Achievement)** : Récompense débloquée pour accomplissement dans un Monde, une Campagne ou un Chapitre. Peut être automatique ou attribué manuellement par le MJ.

**Sort** : Capacité magique ou spéciale qu'un personnage peut apprendre et utiliser. En mode Custom, défini librement. En D&D/Skyrim, suit les règles officielles.

**Équipement** : Objet (arme, armure, item) qu'un personnage possède dans son inventaire.

**Initiative** : Ordre dans lequel les participants jouent pendant un combat. Déterminé manuellement (Custom) ou automatiquement (D&D/Skyrim).

**Points de Vie (PV/HP)** : Représentation de la santé du personnage. Gestion manuelle en Custom, automatique en D&D/Skyrim.

**Mode Hybride** : En mode D&D/Skyrim, possibilité pour le MJ de désactiver les calculs automatiques pour gérer manuellement certaines situations.

### Termes Spécifiques aux Systèmes de Jeu

**Empty** : Type de monde vide servant uniquement de salon d'invitation. Aucune règle de jeu.

**Custom** : Type de monde avec règles personnalisées définies librement par le MJ.

### Termes Spécifiques D&D

**d20** : Dé à 20 faces, base du système D&D pour les tests d'action.

**Classe d'Armure (CA)** : Valeur représentant la difficulté pour toucher un personnage/créature. Plus elle est élevée, plus c'est difficile.

**Jet de Sauvegarde** : Jet de dé pour résister à un effet (sort, poison, etc.). Formule : 1d20 + modificateur de caractéristique.

**Coup Critique** : Résultat de 20 naturel sur un d20, entraîne le doublement des dégâts de l'attaque.

**Échec Critique** : Résultat de 1 naturel sur un d20, entraîne un échec automatique de l'action.

**Modificateur** : Bonus ou malus appliqué à un jet de dé selon une caractéristique (Force, Dextérité, etc.). Calculé comme : (Valeur - 10) / 2.

**Bonus de Maîtrise** : Bonus basé sur le niveau du personnage, ajouté aux jets pour lesquels le personnage est compétent. Augmente tous les 4 niveaux.

**Caractéristiques** : Six attributs principaux en D&D : Force, Dextérité, Constitution, Intelligence, Sagesse, Charisme.

**DD (Difficulté de Sauvegarde)** : Valeur cible qu'un jet de sauvegarde doit atteindre pour réussir. Formule : 8 + modificateur + bonus de maîtrise.

**Challenge Rating (CR)** : Indicateur de difficulté d'un monstre, utilisé pour équilibrer les combats.

---

## 🎯 Conclusion

Cette spécification fonctionnelle définit l'ensemble des fonctionnalités de **Chronique des Mondes** du point de vue utilisateur, sans référence aux technologies d'implémentation.

Le système offre une plateforme complète pour :
- ✅ Créer et gérer des mondes avec règles spécifiques (Empty, Custom, D&D, Skyrim)
- ✅ Organiser des campagnes et chapitres dans une hiérarchie claire
- ✅ Gérer des personnages avec duplication et verrouillage par monde
- ✅ Créer des événements affectant Monde/Campagne/Chapitre
- ✅ Attribuer des succès automatiques ou manuels aux joueurs
- ✅ Lancer des combats avec jets de dés automatiques (stats ajoutées automatiquement)
- ✅ Jouer avec des systèmes de règles automatisés ou libres
- ✅ Collaborer avec d'autres joueurs
- ✅ Suivre sa progression et ses statistiques

La flexibilité du système permet à chaque groupe de jouer selon ses préférences, que ce soit avec des règles strictes (D&D, Skyrim) ou en mode narratif libre (Custom), ou simplement se retrouver dans un salon (Empty).
