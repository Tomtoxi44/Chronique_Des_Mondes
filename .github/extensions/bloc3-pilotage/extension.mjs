// Extension: bloc3-pilotage
// BLOC 3 — Coordonner et piloter (Certification Expert en Développement Logiciel YNOV)
// Compétences : C3.1, C3.2.1, C3.2.2, C3.3.1, C3.3.2, C3.4.1, C3.4.2
// Modalité : Présentation orale + démonstration

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC3_CONTEXT = `
## 📊 BLOC 3 — Coordonner et Piloter (Certification YNOV Expert Dev Logiciel)
### Titre : Expert en développement logiciel — RNCP 39583 — Niveau 7

Tu assistes un étudiant en certification "Expert(e) en Développement Logiciel" YNOV.
Projet : **Chronique des Mondes** — Plateforme JDR multi-systèmes.
Méthode : Agile/Kanban (GitHub Projects) · Backlog dans .github/backlog/ · US-XXX format

### ⏰ Modalité & Deadline BLOC 3
- **Épreuve** : ORAL **45 minutes** — 30' présentation + 15' échange avec le jury — Individuel
- **Date** : **20 au 24 juillet 2026** (S30)
- **Dépôt du support** : avant l'oral sur **DigiformaCertif** → https://ynov.mycertif.app/selection-connexion
- ⚠️ Sans dépôt dans le délai imparti → bloc **automatiquement invalidé**

### ✅ Règles de validation (TOUJOURS rappeler)
- Bloc validé si **≥ 50%** des compétences sont « acquises »
- **Aucune** compétence éliminatoire ne doit être « non acquise »
- La certification nécessite la validation de **TOUS** les blocs

### 💡 Conseils pour l'oral 45 minutes
- **30' présentation** : planifier ~4 min par compétence C3.X (7 compétences)
- **15' échange** : préparer les questions sur vos arbitrages projet, votre gestion du retard, etc.
- Inclure une démonstration live de l'application (5-8 min recommandées)
- Documenter les décisions prises avec leur impact (logigrammes)
- Prouver l'usage réel d'outils de pilotage (GitHub Projects, KPIs chiffrés)

### Compétences évaluées (présentation + démonstration) :
- **C3.1**   Planification (Gantt/PERT, Agile/Scrum/Kanban, RACI)
- **C3.2.1** Piloter l'avancement (tableaux de bord, KPIs)
- **C3.2.2** Prendre des arbitrages (logigrammes de décision)
- **C3.3.1** Manager une équipe (styles managériaux)
- **C3.3.2** Évaluer les besoins en compétences (plan de développement)
- **C3.4.1** Rédiger des comptes rendus client (indicateurs de satisfaction)
- **C3.4.2** Démontrer les fonctionnalités devant un jury

### Livrables attendus par le jury :
1. Tableau de planification (Kanban GitHub Projects + sprints)
2. RACI (Responsible, Accountable, Consulted, Informed)
3. Tableau de bord KPIs (vélocité, taux de complétion, bugs)
4. Logigramme d'arbitrage pour les décisions clés
5. Comptes rendus des sprints (rétrospectives)
6. Plan de démonstration pour le jury (scénario de démo — 45 min chrono !)

Indique toujours la compétence C3.X visée et produis des livrables prêts pour la présentation.
`;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => ({ additionalContext: BLOC3_CONTEXT }),
        onUserPromptSubmitted: async (input) => {
            const p = input.prompt.toLowerCase();
            const relevant = ["planning","gantt","kanban","sprint","agile","scrum","kpi","tableau de bord",
                "raci","arbitrage","management","équipe","compétence","compte rendu","démo","démonstration",
                "c3.","bloc 3","vélocité","burndown","retrospective","rétrospective"].some(k => p.includes(k));
            if (!relevant) return;
            return {
                additionalContext: `⚡ BLOC 3 actif — Mentionne la compétence C3.X visée, structure le livrable pour une présentation jury, ancre dans le contexte Chronique des Mondes.`,
            };
        },
    },
    tools: [
        {
            name: "bloc3_raci",
            description: "Génère la matrice RACI du projet Chronique des Mondes (C3.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Matrice RACI — Chronique des Mondes [C3.1]

## Légende
- **R** = Responsible (réalise)
- **A** = Accountable (responsable final)
- **C** = Consulted (consulté avant)
- **I** = Informed (informé après)

## Tableau RACI
| Activité | Tommy (Dev/PO) | Jury YNOV | MJ (beta tester) | Joueurs (beta) |
|----------|---------------|-----------|------------------|----------------|
| Cadrage et spécifications | **A/R** | C | C | I |
| Développement fonctionnalités | **A/R** | I | C | I |
| Tests unitaires xUnit | **A/R** | I | - | - |
| Tests d'acceptation | R | A | **R** | C |
| Revue de code (Copilot) | **A** | I | - | - |
| Déploiement staging | **A/R** | I | I | - |
| Déploiement production | **A/R** | I | I | I |
| Documentation technique | **A/R** | C | I | - |
| Démonstration jury | **R** | **A** | C | - |
| Soutenance orale | **R** | **A** | - | - |
| Retours utilisateurs | I | I | **R** | **R** |
| Correctifs post-déploiement | **A/R** | I | C | I |

## Notes
- Tommy est seul développeur → R et A souvent cumulés
- Le jury YNOV est Accountable sur l'évaluation finale
- Les MJ (Maîtres de Jeu) sont les beta testeurs principaux pour la logique métier

---
> **Livrable C3.1** — À présenter lors de la présentation jury.`,
        },
        {
            name: "bloc3_kpi_dashboard",
            description: "Génère le tableau de bord KPIs pour le suivi d'avancement du projet (C3.2.1)",
            parameters: {
                type: "object",
                properties: {
                    sprint_number: { type: "number", description: "Numéro du sprint (défaut : 1)" },
                    us_total: { type: "number", description: "Nombre total d'US (défaut : 30)" },
                    us_done: { type: "number", description: "US terminées (défaut : 0)" },
                    bugs_open: { type: "number", description: "Bugs ouverts (défaut : 0)" },
                },
            },
            handler: async (args) => {
                const sprint = args.sprint_number || 1;
                const total = args.us_total || 30;
                const done = args.us_done || 0;
                const bugs = args.bugs_open || 0;
                const pct = Math.round((done / total) * 100);
                return `# Tableau de Bord KPIs — Chronique des Mondes [C3.2.1]
## Sprint ${sprint} — ${new Date().toLocaleDateString("fr-FR")}

## 📊 Indicateurs Clés

### Avancement global
| KPI | Valeur | Cible | Statut |
|-----|--------|-------|--------|
| US complétées | ${done}/${total} | ${total} | ${pct < 50 ? "🔴" : pct < 80 ? "🟡" : "✅"} ${pct}% |
| Bugs ouverts | ${bugs} | 0 | ${bugs > 5 ? "🔴" : bugs > 2 ? "🟡" : "✅"} |
| Couverture tests | - | ≥ 70% | ⬜ À mesurer |
| Build CI | - | ✅ | ⬜ À vérifier |
| Score sécurité OWASP | - | 0 critique | ⬜ À auditer |

### Vélocité par Sprint
| Sprint | US planifiées | US livrées | Vélocité |
|--------|--------------|------------|----------|
| Sprint 1 | ? | ? | ? SP |
| Sprint ${sprint} (actuel) | ? | ${done} | ? SP |
| **Projection fin** | ${total} | — | — |

### Qualité Code
| Métrique | Valeur | Seuil alerte |
|----------|--------|-------------|
| Couverture tests unitaires | ⬜ | < 70% |
| Tests Playwright passants | ⬜ | < 100% |
| Warnings build (0 requis) | ⬜ | > 0 |
| Vulnérabilités NuGet | ⬜ | > 0 critique |

### Satisfaction utilisateurs (MJ beta)
| Critère | Note /5 | Commentaire |
|---------|---------|-------------|
| Facilité de prise en main | ⬜/5 | |
| Performance interface | ⬜/5 | |
| Stabilité (0 crash) | ⬜/5 | |

## 🚦 Indicateurs de pilotage
\`\`\`
Progression : [${"█".repeat(Math.floor(pct / 10))}${"░".repeat(10 - Math.floor(pct / 10))}] ${pct}%

État général : ${pct < 30 ? "🔴 RETARD" : pct < 70 ? "🟡 EN COURS" : "✅ ON TRACK"}
\`\`\`

---
> **Livrable C3.2.1** — Tableau de bord à mettre à jour à chaque sprint.`;
            },
        },
        {
            name: "bloc3_sprint_retrospective",
            description: "Génère un template de compte-rendu de sprint / rétrospective (C3.4.1)",
            parameters: {
                type: "object",
                properties: {
                    sprint_number: { type: "number", description: "Numéro du sprint" },
                },
            },
            handler: async (args) => {
                const sprint = args.sprint_number || 1;
                return `# Compte-rendu Sprint ${sprint} — Chronique des Mondes [C3.4.1]
## Date : ${new Date().toLocaleDateString("fr-FR")}

## 📋 Résumé du Sprint

### US livrées
| US | Titre | Statut | Remarques |
|----|-------|--------|-----------|
| US-XXX | [Titre] | ✅ Done | |
| US-XXX | [Titre] | ⚠️ Partiel | |

### US non livrées (et pourquoi)
| US | Titre | Raison | Action corrective |
|----|-------|--------|------------------|
| US-XXX | [Titre] | [Raison] | [Action] |

## 🌟 Rétrospective (Format Start/Stop/Continue)

### ✅ Continue (ce qui a bien fonctionné)
- [ ] Exemple : Tests xUnit écrits en TDD
- [ ] Exemple : CI pipeline stable

### 🛑 Stop (ce qui ne fonctionne pas)
- [ ] Exemple : Estimation trop optimiste des US de combat

### 🚀 Start (à améliorer)
- [ ] Exemple : Ajouter tests Playwright dès le développement
- [ ] Exemple : Démo staging chaque vendredi

## 📊 Métriques Sprint
| KPI | Valeur |
|-----|--------|
| US planifiées | |
| US livrées | |
| Bugs découverts | |
| Bugs corrigés | |
| Couverture tests | |

## 📅 Objectifs Sprint ${sprint + 1}
1. [ ] [Objectif 1]
2. [ ] [Objectif 2]
3. [ ] [Objectif 3]

## ✉️ Communication client / jury
- Avancement communiqué via : GitHub Projects + rapport sprint
- Prochain point de synchronisation : [Date]

---
> **Livrable C3.4.1** — Compte-rendu à archiver dans \`.github/workflows/\` ou \`conception/\`.`;
            },
        },
        {
            name: "bloc3_demo_script",
            description: "Génère le script de démonstration pour le jury YNOV (C3.4.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Script de Démonstration Jury — Chronique des Mondes [C3.4.2]

## 🎯 Objectif
Démontrer les fonctionnalités clés de la plateforme JDR en 15-20 minutes devant le jury YNOV.

## 📋 Prérequis
- [ ] Application démarrée : \`dotnet run --project Cdm/Cdm.AppHost\`
- [ ] Base de données avec données de démo chargées
- [ ] 2 navigateurs ouverts (MJ + Joueur)
- [ ] URL : https://localhost:5001

## 🎬 Déroulé de la Démo (15 min)

### Partie 1 — Présentation architecture (3 min)
1. Montrer l'Aspire Dashboard (https://localhost:17223)
   - Services actifs : API, Web, MigrationsManager
   - Métriques temps réel
2. Montrer la structure des projets dans VS Code

### Partie 2 — Authentification & Sécurité (2 min)
1. Connexion MJ (rôle : Cdm-GameMasters)
2. Montrer le JWT dans les DevTools → header Authorization
3. Tenter accès non autorisé → 403 Forbidden
*→ Compétences illustrées : C2.2.3 (sécurité), C2.1.1 (environnements)*

### Partie 3 — Gestion de campagne (3 min)
1. Créer une campagne D&D 5e
2. Inviter un joueur (2e navigateur)
3. Joueur rejoint la campagne via le lien
4. Afficher la liste des personnages compatibles D&D
*→ Compétences illustrées : C2.2.1 (prototype), C2.2.4 (versioning)*

### Partie 4 — Session de jeu en temps réel (5 min)
1. MJ lance une session
2. Joueur rejoint via SignalR → notification instantanée
3. Lancer un dé (1d20+5) → résultat calculé côté serveur
4. Combat : initiative, tour de jeu, attaque
5. Couper la connexion joueur → état préservé, reconnexion OK
*→ Compétences illustrées : C2.2.2 (tests), C4.1.2 (monitoring)*

### Partie 5 — CI/CD & Qualité (2 min)
1. Montrer le pipeline GitHub Actions (dernier run)
2. Rapport de couverture des tests
3. Rapport OWASP Dependency Check
*→ Compétences illustrées : C2.1.2 (CI), C2.2.3 (sécurité), C4.1.1 (dépendances)*

## ⚠️ Plan de secours
| Problème | Solution |
|---------|---------|
| DB inaccessible | Données mockées en mémoire (InMemory EF Core) |
| SignalR ne se connecte pas | Démo en local uniquement, pas staging |
| Build cassé | Montrer dernier build réussi sur GitHub Actions |

---
> **Livrable C3.4.2** — Script à répéter 2× avant la soutenance.`,
        },
    ],
});

await session.log("✅ Bloc 3 — Pilotage chargé (C3.1→C3.4.2). Outils : bloc3_raci · bloc3_kpi_dashboard · bloc3_sprint_retrospective · bloc3_demo_script");
