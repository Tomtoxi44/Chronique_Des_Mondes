---
mode: agent
tools: ['githubRepo']
---

# Agent de Développement US (User Story) - Chronique des Mondes

## Configuration du Projet

**Chronique des Mondes** - Plateforme de gestion de campagnes de jeux de rôle avec système de combat et support D&D 5e.

### Paramètres Projet
- **Repository** : `Tomtoxi44/Chronique_Des_Mondes`
- **Projet** : Chronique des Mondes (Cdm)
- **Structure US** : `.github/backlog/epic-XX-XXXX/US-XXX-XXXX.md`
- **Technologies** :
  - Frontend : Blazor Server (.NET 10)
  - Backend : .NET 10 + Aspire
  - Database : SQL Server (dev), PostgreSQL (prod planned)
  - Real-time : SignalR
  - Auth : JWT + BCrypt (MVP)
- **Commandes** :
  - Build : `dotnet build Cdm/Cdm.slnx`
  - Test : `dotnet test Cdm/Cdm.slnx`
  - Run : `dotnet run --project Cdm/Cdm.AppHost`
- **Conventions** :
  - Branches : `feature/US-XXX-description`
  - Nommage : PascalCase classes, camelCase fields avec `this.` prefix
  - Tests IHM : Playwright
  - Rôles : `Cdm-Users`, `Cdm-GameMasters`, `Cdm-Admins`

## Objectif

Développer une User Story (US) en suivant un processus structuré et en maintenant la synchronisation avec GitHub, tout en respectant les bonnes pratiques .NET 10 + Aspire + Blazor Server.

## Méthodologie de Travail

- **Création de liste de tâches** : Créer une liste de tâches détaillée basée sur l'US à développer
- **Progression étape par étape** : S'arrêter après chaque étape pour validation avant de continuer
- **Validation utilisateur** : Demander confirmation avant de passer à l'étape suivante
- **Tests IHM automatisés** : Utiliser Playwright pour les US touchant à l'interface Blazor Server
- **Documentation visuelle** : Générer des captures d'écran de validation pour chaque modification IHM
- **Respect des standards** : Suivre StyleCop, conventions .NET 10, et bonnes pratiques Aspire

## Standards de Développement

### Conventions de Code
- **Indentation** : 4 espaces, no tabs (SA1027)
- **Braces** : Allman style - braces on new lines (SA1500)
- **this. prefix** : Obligatoire pour instance members (SA1101)
- **Nommage** :
  - Classes : PascalCase (`UserAccount`, `CampaignService`)
  - Méthodes : PascalCase (`RegisterAsync`, `CreateCampaign`)
  - Champs privés : camelCase avec `this.` (`this.userId`, `this.campaignId`)
  - Propriétés : PascalCase (`DisplayName`, `CreatedAt`)
  - Paramètres : camelCase (`userId`, `campaignName`)

### Documentation
- **Langue** : Tous les commentaires en ANGLAIS
- **XML Documentation** : Obligatoire pour tous les membres publics
- **Tags requis** : `<summary>`, `<param>`, `<returns>`, `<exception>`

### Sécurité
- **Passwords** : BCrypt avec work factor 12
- **Secrets** : Variables d'environnement uniquement, jamais en dur
- **Validation** : Server-side validation obligatoire avec Data Annotations
- **Logging** : Jamais de mots de passe, tokens ou données sensibles dans les logs

### Architecture
- **Separation of Concerns** : Services abstraits + implémentation
- **Dependency Injection** : Utiliser le DI natif .NET
- **Logging structuré** : `logger.LogInformation("User {UserId} action {Action}", userId, action)`
- **Aspire** : `AddServiceDefaults()` et `MapDefaultEndpoints()` dans tous les projets

## Processus

### 0. Planification initiale
- **Créer une liste de tâches** détaillée basée sur l'US fournie
- Présenter la liste de tâches à l'utilisateur pour validation
- **ARRÊT** : Attendre validation utilisateur avant de continuer

### 1. Identification de l'US
- Demander à l'utilisateur le numéro de l'US à développer (format : US-XXX)
- Valider que l'US existe dans le backlog local
- **ARRÊT** : Confirmer l'US trouvée avec l'utilisateur avant de continuer

### 2. Localisation et analyse
- Rechercher le fichier US dans : `.github/backlog/epic-XX-XXXX/US-XXX-XXXX.md`
- Si non trouvé, explorer récursivement le dossier `.github/backlog/` pour localiser l'US
- Analyser le contenu de l'US (critères d'acceptation, definition of done, etc.)
- Identifier les dépendances avec d'autres US ou Epics
- **ARRÊT** : Présenter l'analyse de l'US à l'utilisateur et demander validation

### 3. Synchronisation GitHub
- Chercher l'issue GitHub correspondante dans le dépôt `https://github.com/Tomtoxi44/Chronique_Des_Mondes/issues/`
- Si l'issue n'est pas trouvée automatiquement :
  - Demander à l'utilisateur de fournir le numéro d'issue existante, OU
  - Proposer de créer une nouvelle issue basée sur l'US analysée
  - **ARRÊT** : Attendre validation utilisateur pour créer l'issue si nécessaire
- Si création d'issue requise :
  - Extraire titre et description depuis le fichier US
  - Créer l'issue avec les labels appropriés (epic, user-story, enhancement, etc.)
  - Assigner l'issue si demandé par l'utilisateur
- Mettre à jour le statut sur GitHub vers "In Progress"
- **ARRÊT** : Confirmer la synchronisation GitHub réussie avant de continuer

### 4. Gestion Git
- Vérifier la branche actuelle avec `git branch --show-current`
- S'assurer d'être sur la branche `dev` avant la création
- Créer une nouvelle branche git avec la convention : `feature/US-XXX-nom-descriptif`
- Faire un checkout sur la nouvelle branche
- **ARRÊT** : Confirmer la création de branche et demander validation du nom avant de continuer

### 5. Développement
- Analyser les exigences techniques de l'US
- Consulter la documentation technique dans `.github/instructions/technique/`
- Proposer une architecture/approche de développement conforme aux standards
- **ARRÊT** : Présenter le plan technique et attendre validation utilisateur
- Implémenter le code nécessaire en respectant :
  - StyleCop rules (SA1xxx)
  - Conventions de nommage avec `this.` prefix
  - XML documentation en anglais
  - Logging structuré avec contexte métier
  - Validation server-side
- Créer/mettre à jour les tests unitaires avec xUnit
- **ARRÊT** : Présenter le code développé et demander validation avant tests

### 6. Tests IHM avec Playwright (si applicable)
Si l'US concerne l'interface utilisateur Blazor Server :
- **Tests fonctionnels** : 
  - Navigation entre pages et composants Blazor
  - Interactions utilisateur (clics, saisies, sélections)
  - Validation des formulaires et messages d'erreur
  - Tests des parcours définis dans les critères d'acceptation
  - Tests SignalR pour les fonctionnalités temps réel
- **Tests visuels** :
  - Captures d'écran avant/après modification
  - Comparaison visuelle avec les maquettes/wireframes
  - Vérification de la cohérence graphique (couleurs, polices, espacements)
- **Tests responsive** :
  - Affichage desktop (1920x1080, 1366x768)
  - Affichage tablette (768x1024)
  - Affichage mobile (375x667, 390x844)
- **Tests d'authentification** :
  - Connexion/déconnexion JWT + BCrypt
  - Gestion des rôles (Cdm-Users, Cdm-GameMasters, Cdm-Admins)
  - Redirections sécurisées
  - Gestion des sessions
- **Tests de performance IHM** :
  - Temps de chargement des pages Blazor
  - Réactivité des interactions SignalR
  - Performance du rendering des composants
- **Tests d'accessibilité** : Validation WCAG si applicable
- **Tests de régression** : Valider que les fonctionnalités existantes ne sont pas cassées
- **ARRÊT** : Présenter les résultats des tests IHM et captures d'écran pour validation

### 7. Vérification des Bonnes Pratiques
Après implémentation, valider automatiquement :
- **Standards de code** : 
  - .editorconfig respecté (4 espaces, Allman braces)
  - StyleCop rules (SA1xxx) validées
  - Nommage : PascalCase classes, camelCase fields avec `this.`
  - XML documentation en anglais sur tous les membres publics
- **Architecture** : 
  - Interfaces + implémentation séparées
  - Injection de dépendances utilisée
  - Services dans Cdm.Business.Common/Cdm.Business.Abstraction
  - DTOs dans Cdm.Common
  - Entities dans Cdm.Data.Common
- **Logging** : 
  - Logging structuré avec paramètres nommés
  - Pas de string interpolation dans les logs
  - Contexte métier (UserId, CampaignId, Action) inclus
  - Pas de données sensibles loggées
- **Sécurité** : 
  - Pas de secrets en dur dans le code
  - BCrypt pour les mots de passe (work factor 12)
  - Validation server-side avec Data Annotations
  - Headers sécurité appropriés
- **Aspire** : 
  - `AddServiceDefaults()` appelé dans Program.cs
  - `MapDefaultEndpoints()` pour les health checks
  - Configuration externalisée
- **Configuration** : 
  - Secrets via User Secrets ou variables d'environnement
  - Validation au démarrage avec Options pattern
  - appsettings.json pour configuration non-sensible
- **Performance** : 
  - Health checks implémentés
  - Cache si applicable (IMemoryCache, IDistributedCache)
  - Async/await utilisé correctement
- **Tests** : 
  - Tests unitaires xUnit passent
  - Build sans warnings ni erreurs
  - Coverage acceptable sur le code métier
- **ARRÊT** : Présenter le rapport de vérification et attendre validation

### 8. Mise à jour des Tâches US
- Localiser et ouvrir le fichier US correspondant (`US-XXX-XXXX.md`)
- Cocher automatiquement les cases `- [ ]` devenues `- [x]` pour les tâches terminées
- Mettre à jour le statut d'avancement de l'US
- Vérifier les critères d'acceptation et cocher ceux validés
- Ajouter notes d'implémentation si nécessaire dans la section dédiée
- **Si tests IHM effectués** : Joindre les captures d'écran et résultats de tests {UI_TEST_FRAMEWORK}
- **ARRÊT** : Présenter la mise à jour du fichier US et demander validation

### 9. Documentation
- Mettre à jour la documentation technique dans `.github/instructions/technique/` si nécessaire
- Documenter les choix d'implémentation importants dans le code
- Mettre à jour `API_ENDPOINTS.md` si de nouveaux endpoints sont créés
- Mettre à jour `MODELE_DONNEES.md` si le schéma de base de données change
- Mettre à jour `FRONTEND_BLAZOR.md` pour les nouveaux composants Blazor
- **Si tests IHM** : Documenter les scénarios de test Playwright et captures d'écran de référence
- Ajouter des exemples d'utilisation si applicable
- **ARRÊT** : Présenter les mises à jour documentation et attendre validation

### 10. Finalisation et Pull Request
- Vérifier que tous les fichiers modifiés sont bien trackés
- Committer toutes les modifications avec un message descriptif selon la convention :
  - Format : `feat(US-XXX): Description courte en anglais`
  - Exemples : `feat(US-001): Add user registration`, `fix(US-042): Resolve combat calculation bug`
- Pousser la branche vers le repository distant GitHub
- **ARRÊT** : Confirmer le push et présenter le plan de PR avant création
- Créer une pull request vers la branche `dev`
- Ajouter Copilot comme reviewer automatiquement
- Inclure dans la PR :
  - Description détaillée des changements en français
  - Liste des fonctionnalités implémentées
  - Instructions de test pour les reviewers
  - Liens vers l'issue résolue (#XXX)
  - Captures d'écran si modifications IHM
  - Checklist Definition of Done validée
- Demander une review Copilot pour validation automatique
- **ARRÊT** : Présenter la PR créée et demander validation finale

## Questions de validation
Avant de commencer le développement :
- L'US est-elle claire et complète ?
- Y a-t-il des dépendances avec d'autres US ou Epics ?
- Les critères d'acceptation sont-ils testables ?
- L'environnement de développement est-il prêt (Aspire Dashboard accessible) ?
- Les documentation technique nécessaires sont-elles disponibles ?
- **Si IHM** : Les maquettes/wireframes sont-ils disponibles pour les tests visuels Blazor ?

Après le développement :
- Toutes les tâches de l'US sont-elles cochées ?
- Les critères d'acceptation sont-ils validés ?
- La Definition of Done (DoD) est-elle respectée ?
- Le code suit-il les bonnes pratiques StyleCop et .NET 10 ?
- Les tests unitaires xUnit passent-ils ?
- **Si IHM** : Les tests Playwright passent-ils et les captures correspondent-elles aux attentes ?
- La documentation technique est-elle à jour ?

## Bonnes pratiques Pull Request

### Vérification Automatique Avant PR
Avant de créer la pull request, exécuter ces vérifications :
```bash
# Build et tests
dotnet build Cdm/Cdm.slnx
dotnet test Cdm/Cdm.slnx

# Vérification de l'application Aspire
dotnet run --project Cdm/Cdm.AppHost
# Vérifier Dashboard : https://localhost:17223
# Vérifier Web : https://localhost:5001
# Vérifier API : https://localhost:5000

# Tests IHM si applicable (US touchant à l'interface Blazor)
# Les tests Playwright seront exécutés avec captures d'écran

# Vérification des standards
git diff --check  # Espaces en fin de ligne
grep -r "TODO\|FIXME" Cdm/  # Pas de TODO/FIXME oubliés
```

### Checklist Bonnes Pratiques Code
- [ ] **Indentation** : 4 espaces respectés, pas de tabs (SA1027)
- [ ] **Braces** : Allman style - braces sur nouvelles lignes (SA1500)
- [ ] **this. prefix** : Utilisé pour tous les membres d'instance (SA1101)
- [ ] **Nommage** : PascalCase classes, camelCase fields, conformité StyleCop
- [ ] **Documentation** : XML documentation en anglais sur tous les membres publics
- [ ] **Logging** : Logging structuré avec paramètres nommés, pas d'interpolation
- [ ] **Sécurité** : Pas de secrets en dur, BCrypt pour passwords, validation server-side
- [ ] **Aspire** : `AddServiceDefaults()` et `MapDefaultEndpoints()` présents
- [ ] **Architecture** : Séparation interfaces/implémentation, DI utilisée
- [ ] **Configuration** : Secrets via User Secrets/variables d'environnement
- [ ] **Exceptions** : Gestion d'erreur appropriée avec logs contextuels
- [ ] **Tests** : Tests unitaires xUnit créés/mis à jour et passent
- [ ] **Performance** : Health checks implémentés, async/await utilisé

### Structure de la PR
- **Titre** : Convention `feat(US-XXX): Description courte en anglais`
  - Exemples : `feat(US-001): Add user registration`, `fix(US-023): Fix campaign creation validation`
- **Description** : Résumé détaillé en français avec sections organisées
- **Labels** : Ajouter les labels appropriés (feature, enhancement, bug, epic-XX, etc.)
- **Reviewers** : Copilot automatiquement assigné
- **Target** : Branche `dev`

### Contenu requis de la PR
- 📋 **Résumé** : Description générale des fonctionnalités implémentées
- ✨ **Nouvelles fonctionnalités** : 
  - Liste détaillée avec détails techniques
  - Endpoints API ajoutés (si applicable)
  - Composants Blazor créés (si applicable)
  - Services et repositories implémentés
- 🔧 **Configuration et composants modifiés** :
  - Projets modifiés (Cdm.ApiService, Cdm.Web, etc.)
  - Fichiers de configuration (appsettings.json, etc.)
  - Dépendances ajoutées (NuGet packages)
- 🧪 **Tests et validation** :
  - Tests unitaires xUnit ajoutés/modifiés
  - Tests IHM Playwright (si applicable) avec captures
  - Résultats des tests (tous passent ✅)
- 📸 **Tests IHM** : Captures d'écran avant/après et résultats Playwright (si applicable)
- 🚀 **Instructions de test** :
  - Commandes pour lancer l'application
  - Parcours utilisateur à tester
  - URLs et identifiants de test
- 📊 **Métriques** :
  - Story points
  - Temps de développement
  - Lignes de code ajoutées/modifiées
  - Couverture de tests
- 🔗 **Liens** :
  - Issue fermée : `Closes #XXX`
  - Epic parent si applicable
  - Documentation technique mise à jour
- ✅ **Checklist Definition of Done** :
  - [ ] Code conforme StyleCop
  - [ ] Tests unitaires passent
  - [ ] Documentation technique à jour
  - [ ] Critères d'acceptation validés
  - [ ] Pas de secrets en dur
  - [ ] Logging structuré utilisé
  - [ ] Aspire defaults configurés

## Format de réponse attendu
1. **Présentation de la liste de tâches** avec demande de validation
2. Confirmation de l'US trouvée avec résumé
3. Statut de la synchronisation GitHub
4. Confirmation de la création de branche
5. Plan de développement proposé
6. Demande de validation avant implémentation
7. **Tests IHM avec Playwright** (si US touche à l'interface) avec captures d'écran
8. **Rapport de vérification des bonnes pratiques** (post-développement)
9. **Mise à jour fichier US** avec cases cochées et statut d'avancement
10. Création de la pull request avec review Copilot
11. Résumé final avec liens PR et issue fermée

**Important** : Chaque étape doit être validée par l'utilisateur avant de passer à la suivante. Ne jamais enchaîner plusieurs étapes sans validation explicite.