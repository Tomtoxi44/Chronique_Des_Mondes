# État du projet — Chronique des Mondes

**Date du point :** 18 juillet 2026
**Méthode :** reconstitution depuis le code source réel, l'historique git, la roadmap et le backlog — pas depuis la documentation seule.

---

## 1. Où en est le projet, en une phrase

L'application est **fonctionnelle de bout en bout** sur son cœur de métier (authentification → monde → campagne → chapitre → personnage → session temps réel → combat). Le MVP est dépassé : la Phase 1 et une bonne partie des Phases 3 et 5 de la roadmap sont en réalité déjà livrées. Ce qui manque relève surtout de **l'email, des systèmes de jeu autres que D&D, des statistiques, et de la dette technique**.

> ⚠️ **Point important : `.github/ROADMAP.md` est nettement en retard sur la réalité.** Elle liste comme « à faire » des choses qui tournent déjà (PNJ, SignalR temps réel, lanceur de dés, combat générique). Le §4 rétablit la vérité. C'est le premier document à corriger.

---

## 2. Chronologie réelle (git)

| Période | Commits | Ce qui s'est passé |
|---------|---------|--------------------|
| Oct. 2025 | 45 | Démarrage : socle, auth, mondes, personnages |
| Nov. 2025 → Mars 2026 | 27 | Itérations : campagnes, chapitres, sessions, UI |
| **Avr. 2026** | **91** | **Le gros du produit** : D&D 5e complet, combat SignalR, PNJ, sessions temps réel |
| Juin 2026 | 8 | CI/CD (xUnit, OWASP, couverture), skills de certification |
| Juil. 2026 | 29 | Dossier de certification Bloc 2, erreurs typées + ToastService, **audit de code + correctifs** |

**Lecture :** le dernier vrai développement de *fonctionnalités* date d'**avril 2026**. Depuis, le travail a porté sur la **qualité** (CI, sécurité, documentation de certification, audit). Le projet est donc en phase de consolidation, pas de construction.

**Dernier jalon :** PR #55 (`fix/audit-14-points`) mergée sur `dev` le 18/07/2026 — 12 des 14 points d'audit + 4 bugs découverts en test.

---

## 3. Ce qui est fait et vérifié

Statut établi par lecture du code **et** test fonctionnel réel dans le navigateur.

### Socle technique
- Architecture .NET 10 en couches (Abstraction / Business / Data / Api / Web) orchestrée par **.NET Aspire**
- **Blazor Server** + Fluent UI, design system CSS à tokens, thème par type de jeu, mode clair/sombre
- **SQL Server + EF Core**, 28 migrations
- **CI/CD** : build `warnaserror`, tests xUnit + couverture, OWASP Dependency Check, E2E Playwright, déploiement Azure
- Catalogue d'erreurs typées (`CdmErrorCode`) + `ToastService` + localisation fr/en (`.resx`, 225 entrées)

### Fonctionnel — testé et opérationnel ✅
| Domaine | État |
|---------|------|
| Inscription / connexion / déconnexion (JWT + refresh token) | ✅ + auto-connexion après inscription |
| Protection des routes, rôles (Player / GameMaster) | ✅ |
| Profil (pseudo, avatar, mot de passe) | ✅ |
| Mondes : CRUD, types de jeu, invitation par lien + QR | ✅ |
| Personnages : CRUD, personnage de monde, wizard D&D 5e | ✅ |
| Campagnes : CRUD, invitation, statut | ✅ |
| Chapitres : CRUD, ordre, éditeur narratif avec mentions `@PNJ/@PJ` | ✅ |
| **PNJ** : création, association au chapitre, stat block D&D | ✅ *(roadmap dit « à faire » — faux)* |
| **Sessions temps réel SignalR** : présence, chat, dés partagés | ✅ *(roadmap dit « à faire » — faux)* |
| **Combat** : initiative, tours, PV, journal, vues MJ/joueur | ✅ *(roadmap dit « à faire » — faux)* |
| **Lanceur de dés** d4→d100 avec historique | ✅ *(roadmap dit « à faire » — faux)* |
| Notifications in-app + badge + actions | ✅ *(actions réparées le 18/07)* |
| Événements de monde (narratif, modif. stat/PV/dé) | ✅ |
| Succès / achievements (création, rareté) | ✅ partiel — création OK, attribution automatique non vérifiée |
| D&D 5e : races, classes, backgrounds, sorts, objets, inventaire | ✅ base complète (seeder) |

---

## 4. Écarts entre la roadmap et la réalité

À corriger dans `.github/ROADMAP.md` :

| Item roadmap | Statut affiché | Statut réel |
|---|---|---|
| PNJ (Phase 1) | `[ ]` | ✅ **fait** |
| Temps réel SignalR (Phase 1) | `[ ]` | ✅ **fait** |
| Lanceur de dés (Phase 3) | `[ ]` | ✅ **fait** |
| Combat générique (Phase 3) | `[ ]` | ✅ **fait** |
| Sorts / équipements D&D (Phase 4) | `[ ]` | ✅ **largement fait** (seeder, inventaire, sorts par personnage) |
| Achievements (Phase 5) | `[ ]` | 🟡 **partiel** (création oui, attribution auto à vérifier) |
| Historique de sessions (Phase 1) | `[ ]` | 🟡 **partiel** (page Sessions existe, historique limité) |

---

## 5. Ce qui reste à faire

### 5.1 Fonctionnalités non commencées

| Fonctionnalité | Détail | Effort |
|---|---|---|
| **Email (Azure Communication Services)** | `AzureEmailService` existe mais est **commenté** dans `Program.cs`. Aucune page « mot de passe oublié ». Bloque : reset password, validation d'email, invitation par mail | Moyen |
| **Échanges d'objets** | Seule la méthode hub `ProposeTradeTheory` existe — **aucune interface** | Moyen |
| **Statistiques & rapports** | Stats de dés, participation, rapports mensuels/annuels — rien de commencé | Élevé |
| **Autres systèmes de jeu** | Skyrim, Pathfinder, Cthulhu, Warhammer, Cyberpunk : les **thèmes visuels existent**, mais aucune logique de règles | Élevé (par système) |
| **D&D 5e avancé** | Sous-classes, CA calculée, PV auto, bonus de maîtrise, jets de sauvegarde | Moyen |

### 5.2 Dette technique (issue de l'audit)

| # | Sujet | Criticité |
|---|---|---|
| 1 | **`SessionMessages.ChapterId` stocke en réalité un `SessionId`** (FK vers `Chapters`) — incohérence pouvant casser le chat selon les ids en base. Correction = migration | 🔴 |
| 2 | **Couverture de tests partielle** — non couverts : Combat, Session, Npc, Notification, Character, Dnd*. Aucun test d'intégration endpoints/Hubs | 🟠 |
| 3 | **~150 `catch (Exception)`** qui avalent les erreurs (seuls ceux causant des bugs visibles ont été traités) | 🟡 |
| 4 | **`WorldDetail.razor` = 2 574 lignes** (1 416 + 1 158) — composant monolithe hébergeant campagnes, chapitres, événements, succès, invitations. À découper | 🟡 |
| 5 | Migrations à consolider (28, dont plusieurs « AddMissing… ») | 🟢 |
| 6 | JWT en `localStorage` → cookie `HttpOnly` (mitigé par CSP) | 🟡 |
| 7 | 2 fichiers à supprimer : `AuthTokenHandler.cs` (code mort), `~$ssier-bloc2.docx` | 🟢 |

### 5.3 Points d'UI/UX identifiés
- Palette « Grimoire » proposée (`docs/proposition-design.html`) — **non implémentée**, à ton choix
- Traductions EN : le mécanisme fonctionne, mais les libellés codés en dur dans les `.razor` restent en français
- Contraste du texte secondaire à vérifier (WCAG AA)

---

## 6. Idées de fonctionnalités supplémentaires

Non présentes dans la roadmap, cohérentes avec le produit :

1. **Export de campagne** (PDF/Markdown) — une « chronique » imprimable des chapitres joués. Très aligné avec le nom du produit, et excellent pour une démo de certification.
2. **Journal de session automatique** — les messages, jets et combats existent déjà en base : en faire un récit consultable après la partie est surtout un travail d'affichage.
3. **Import de personnage** depuis un JSON D&D Beyond / une fiche standard.
4. **Mode « écran de MJ »** — panneau latéral avec tables de référence, jets rapides, PNJ à portée de main.
5. **Recherche globale** (mondes, personnages, chapitres, PNJ) — l'app commence à contenir beaucoup d'entités.
6. **Gestion des absences / remplacement de personnage** en cours de campagne.

---

## 7. Priorisation recommandée

**Si l'objectif est la certification YNOV** (le projet est en phase dossier depuis juin) :
1. Dette #2 — **couverture de tests** : c'est le critère le plus regardé, et le plus faible aujourd'hui
2. Dette #1 — le bug `ChapterId/SessionId` : un bug de données documenté *et corrigé* est une excellente pièce pour le Bloc 2 (analyse de bug)
3. Mettre la **roadmap à jour** (§4) : un écart doc/réalité se remarque en soutenance
4. Export de campagne (#6.1) : fonctionnalité démontrable à fort impact visuel

**Si l'objectif est le produit** :
1. **Email** (reset password) — c'est le manque le plus visible pour un vrai utilisateur qui perd son mot de passe
2. Dette #1 (bug de données)
3. Échanges d'objets (interface manquante sur une base déjà posée)
4. D&D 5e avancé (CA/PV/maîtrise automatiques)

**Dans les deux cas**, la dette #1 est à traiter en premier parmi les corrections : c'est la seule qui peut casser une fonctionnalité en production selon les données.

---

## 8. Documents de référence

| Document | Contenu |
|---|---|
| `docs/ETAT_DU_PROJET.md` | Ce point de situation |
| `docs/AUDIT_CODE_ET_TESTS.md` | Audit initial : 14 points, criticité, axes d'amélioration |
| `docs/SUIVI_CORRECTIONS_AUDIT.md` | Suivi point par point des correctifs |
| `docs/RAPPORT_TEST_FONCTIONNEL.md` | 3 passes de tests + multi-joueurs + bugs trouvés + UX |
| `docs/proposition-design.html` | Proposition de palette « Grimoire » (avant/après) |
| `.github/ROADMAP.md` | ⚠️ à mettre à jour (voir §4) |
