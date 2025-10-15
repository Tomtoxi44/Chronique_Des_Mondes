# Epic 2 - Gestion des Parties, Campagnes & Sessions

## 📋 Informations Générales

- **Phase** : Phase 0 (MVP Core)
- **Priorité** : P0 - Critique
- **Statut** : 📝 Planifié
- **Estimation totale** : 48 Story Points

---

## 🎯 Objectif

Permettre aux Maîtres du Jeu (MJ) de créer et gérer des campagnes complètes, d'inviter des joueurs, de structurer le contenu en chapitres, et de lancer des sessions de jeu en temps réel.

---

## 📊 Critères d'Acceptation Globaux

- [ ] Un MJ peut créer une campagne générique
- [ ] Un MJ peut structurer sa campagne en chapitres
- [ ] Un MJ peut inviter des joueurs à rejoindre sa campagne
- [ ] Les joueurs reçoivent des notifications d'invitation (email + in-app)
- [ ] Les joueurs peuvent accepter/refuser les invitations
- [ ] Un MJ peut lancer une session de jeu
- [ ] Les sessions supportent la progression par chapitres
- [ ] Les sessions sont sauvegardées automatiquement
- [ ] Un historique des sessions est disponible

---

## 📝 User Stories

| ID | Titre | Statut | Story Points | Priorité |
|----|-------|--------|--------------|----------|
| [US-011](./US-011-creation-campagne.md) | Création de campagne | 📝 Planifié | 5 | P0 |
| [US-012](./US-012-modification-campagne.md) | Modification de campagne | 📝 Planifié | 3 | P0 |
| [US-013](./US-013-liste-campagnes.md) | Liste des campagnes | 📝 Planifié | 3 | P0 |
| [US-014](./US-014-creation-chapitres.md) | Création et gestion des chapitres | 📝 Planifié | 5 | P0 |
| [US-015](./US-015-invitation-joueurs.md) | Invitation de joueurs | 📝 Planifié | 5 | P0 |
| [US-016](./US-016-acceptation-invitation.md) | Acceptation d'invitation | 📝 Planifié | 3 | P0 |
| [US-017](./US-017-selection-personnage.md) | Sélection personnage pour campagne | 📝 Planifié | 3 | P0 |
| [US-018](./US-018-lancement-session.md) | Lancement de session | 📝 Planifié | 8 | P0 |
| [US-019](./US-019-progression-chapitres.md) | Progression par chapitres | 📝 Planifié | 5 | P1 |
| [US-020](./US-020-auto-sauvegarde.md) | Auto-sauvegarde de session | 📝 Planifié | 3 | P1 |
| [US-021](./US-021-historique-sessions.md) | Historique des sessions | 📝 Planifié | 3 | P2 |
| [US-022](./US-022-gestion-joueurs.md) | Gestion des joueurs | 📝 Planifié | 2 | P1 |

---

## 🏗️ Architecture Technique

### Backend (.NET 10)
- **Endpoints** : `CampaignEndpoints.cs`, `SessionEndpoints.cs`, `ChapterEndpoints.cs`
- **Services** : `CampaignService`, `SessionService`, `InvitationService`, `EmailService`
- **Models** : `Campaign`, `Chapter`, `Session`, `CampaignPlayer`, `Invitation`
- **Hubs SignalR** : `SessionHub` (notifications temps réel)

### Frontend (Blazor Server)
- **Pages** : Campaigns.razor, CampaignDetail.razor, SessionLive.razor
- **Components** : CampaignForm, ChapterEditor, InvitationManager, SessionControls
- **Services** : CampaignService, SessionService, SignalRService

### Base de Données
- **Tables** : 
  - `Campaigns` (1-N `Chapters`)
  - `Chapters` (N-N `NPCs`)
  - `Sessions` (1-N `SessionEvents`)
  - `CampaignPlayers` (relation N-N `Campaigns` ↔ `Users`)
  - `Invitations`

---

## 🔄 Flux Utilisateur Clé

### Création de Campagne
1. MJ crée une campagne (nom, description, système de jeu)
2. MJ ajoute des chapitres (structure narrative)
3. MJ invite des joueurs (email)
4. Joueurs acceptent et sélectionnent leur personnage
5. MJ lance une session
6. Progression chapitre par chapitre
7. Sauvegarde automatique

### Notifications Temps Réel (SignalR)
- Invitations reçues
- Session lancée
- Joueur rejoint/quitte
- Changement de chapitre
- Sauvegarde effectuée

---

## 🧪 Tests

### Tests Unitaires
- Création/modification de campagnes
- Gestion des chapitres
- Système d'invitations
- Validation des données

### Tests d'Intégration
- Flux complet : Création → Invitation → Acceptation → Session
- Notifications SignalR
- Sauvegarde automatique

### Tests E2E
- Parcours MJ complet
- Parcours Joueur complet
- Session multi-joueurs

---

## 🔗 Dépendances

### Dépend de
- [Epic 1](../01-Epic-Authentification/) - Authentification
- [Epic 3](../03-Epic-Personnages-PNJ/) - Personnages (pour sélection)

### Bloque
- [Epic 4](../04-Epic-Combat-Des/) - Combat (nécessite sessions actives)
- [Epic 5](../05-Epic-Sorts-Equipements/) - Sorts/Équipements

---

## 📈 Progression

```
Complété : [░░░░░░░░░░] 0% (0/12 US)
Planifié  : 12 US
```

---

## 📚 Documentation Associée

- [SPECIFICATION_FONCTIONNELLE.md](../../instructions/SPECIFICATION_FONCTIONNELLE.md) - Gestion des campagnes
- [SIGNALR_TEMPS_REEL.md](../../instructions/technique/SIGNALR_TEMPS_REEL.md) - SessionHub
- [MODELE_DONNEES.md](../../instructions/technique/MODELE_DONNEES.md) - Tables Campaign, Session

---

**Dernière mise à jour** : 15 octobre 2025
