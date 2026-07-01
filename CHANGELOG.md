# Journal des modifications – Chronique des Mondes

Format : [SemVer](https://semver.org/lang/fr/)
Types : feat (fonctionnalité), fix (correction), docs (documentation), style (UI), refactor, test, chore

---

## [0.9.0] – 2026-06-24

### Ajouté
- Pipeline CI/CD complet : tests xUnit, OWASP Dependency Check, couverture de code (`.github/workflows/ci.yml`)
- Skill Mermaid pour la génération de diagrammes de certification
- Agents de certification YNOV pour les 4 blocs (`.github/extensions/`)

### Modifié
- Pipeline `azure-deploy.yml` restructuré : séparation build/deploy
- Documentation README mise à jour

---

## [0.8.0] – 2026-06-10

### Corrigé
- Combat : authentification SignalR, modificateurs D&D, état d'erreur du wizard
- Notifications : comportement UX amélioré
- Combat : défilement vertical, bouton tour, bascule tous les participants

---

## [0.7.0] – 2026-05-28

### Ajouté
- Système de combat générique SignalR (tours, initiative, attaques, dégâts)
- Interface GM améliorée : phase d'initiative, lanceur de dés, fiche joueur
- NPC : support stat block D&D 5e

---

## [0.6.0] – 2026-05-10

### Ajouté
- D&D 5e : seeder complet (sorts, races, classes, backgrounds, équipements)
- Vue détaillée personnage D&D 5e
- Sélection sorts et background dans le wizard de création

---

## [0.5.0] – 2026-04-15

### Ajouté
- Logique métier D&D 5e complète (`Cdm.Business.DnD5e`)
- JWT : mécanisme de rafraîchissement des tokens (RefreshTokens)
- Session : persistance de l'activité utilisateur

---

## [0.4.0] – 2026-03-20

### Ajouté
- Chat temps réel dans les sessions (SignalR)
- Lancer de dés en session avec diffusion aux participants
- Notification de fin de session

---

## [0.3.0] – 2026-02-15

### Ajouté
- Gestion des sessions de jeu (création, démarrage, clôture)
- Participants de session liés aux WorldCharacters
- Hubs SignalR : `/hubs/session`, `/hubs/combat`, `/hubs/notifications`

### Corrigé
- Conflit de cascade sur `SessionParticipants`
- Schéma `Sessions` / `SessionParticipants` en production

---

## [0.2.0] – 2025-12-20

### Ajouté
- Modèle World / Campaign / Chapter / Event / Achievement
- WorldCharacter : personnage adapté à un univers avec données JSON système-spécifiques
- API REST complète (14 endpoints) documentée via Scalar

---

## [0.1.0] – 2025-10-15

### Ajouté
- Initialisation du projet .NET 10 + Aspire AppHost
- Authentification JWT HS256 + BCrypt work factor 12
- EF Core 10 + migrations vers SQL Server
- CI/CD initial : GitHub Actions + déploiement Azure App Service
- Gestion des rôles (Player, GameMaster, Admin)
