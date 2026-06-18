// Extension: bloc1-cadrage
// BLOC 1 — Cadrer un projet (Certification Expert en Développement Logiciel YNOV)
// Compétences : C1.1.1, C1.1.2, C1.2.1, C1.2.2, C1.2.3, C1.3.1, C1.3.2, C1.4.1, C1.4.2, C1.5, C1.6
// Modalité : Oral soutenance devant jury

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC1_CONTEXT = `
## 🎯 BLOC 1 — Cadrage de Projet (Certification YNOV Expert Dev Logiciel)

Tu assistes un étudiant en certification "Expert(e) en Développement Logiciel" YNOV.
Projet : **Chronique des Mondes** — Plateforme multi-systèmes de gestion de campagnes de JDR.
Stack : .NET 10 · Aspire · Blazor Server · SignalR · EF Core · SQL Server · JWT/BCrypt

### Compétences évaluées (oral soutenance) :
- **C1.1.1** Cartographier les parties prenantes (stakeholders, matrice influence/intérêt)
- **C1.1.2** Analyser la demande via entretien d'explicitation
- **C1.2.1** Cartographier opportunités/menaces (analyse SWOT)
- **C1.2.2** Évaluer la faisabilité technique (audit technique)
- **C1.2.3** Cartographier les risques techniques et fonctionnels
- **C1.3.1** Assurer une veille technique/technologique/réglementaire
- **C1.3.2** Sélectionner l'architecture technique (étude comparative)
- **C1.4.1** Évaluer la charge de travail en jours-hommes
- **C1.4.2** Estimer le coût et le budget prévisionnel
- **C1.5**   Modéliser l'architecture logicielle (UML, Merise, C4...)
- **C1.6**   Présenter le cadrage au client (argumentaire structuré)

### Livrables attendus par le jury :
1. Cartographie parties prenantes (tableau + matrice influence/intérêt)
2. Analyse SWOT formalisée
3. Audit de faisabilité technique avec justifications
4. Matrice des risques (probabilité × impact)
5. Rapport de veille technologique (justifiant les choix .NET 10/Aspire/Blazor)
6. Étude comparative des architectures
7. Planning en jours-hommes (WBS + estimation)
8. Budget prévisionnel
9. Diagrammes UML/C4
10. Support de présentation client

Indique toujours la compétence C1.X.X concernée et génère des livrables prêts pour la soutenance.
`;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => ({ additionalContext: BLOC1_CONTEXT }),
        onUserPromptSubmitted: async (input) => {
            const p = input.prompt.toLowerCase();
            const relevant = ["swot","partie prenante","stakeholder","risque","faisabilité",
                "architecture","veille","charge","budget","coût","uml","c4","planning",
                "cadrage","bloc 1","c1.","oral","soutenance","faisabilite"].some(k => p.includes(k));
            if (!relevant) return;
            return {
                additionalContext: `⚡ BLOC 1 actif — Mentionne la compétence C1.X.X visée, produis un livrable structuré et ancre ta réponse dans le projet Chronique des Mondes.`,
            };
        },
    },
    tools: [
        {
            name: "bloc1_swot",
            description: "Génère une analyse SWOT complète pour Chronique des Mondes (C1.2.1). Focus : global | technique | marché | concurrentiel",
            parameters: {
                type: "object",
                properties: {
                    focus: { type: "string", enum: ["global", "technique", "marché", "concurrentiel"] },
                },
            },
            handler: async (args) => {
                const focus = args.focus || "global";
                return `# Analyse SWOT — Chronique des Mondes [C1.2.1] · Focus : ${focus}

## Forces (Strengths)
| # | Force | Impact |
|---|-------|--------|
| 1 | Stack .NET 10 + Aspire — orchestration microservices intégrée | Élevé |
| 2 | Blazor Server : temps réel natif via SignalR sans framework JS | Élevé |
| 3 | Architecture multi-systèmes JDR (générique + D&D 5e) — différenciation | Moyen |
| 4 | Modèle de données flexible (JSON polymorphique pour attributs spécifiques) | Moyen |
| 5 | CI/CD GitHub Actions + déploiement progressif + versioning | Élevé |
| 6 | Séparation stricte Business / Data / API → maintenabilité | Élevé |

## Faiblesses (Weaknesses)
| # | Faiblesse | Mitigation |
|---|-----------|------------|
| 1 | Complexité architecturale élevée pour un seul développeur | MVP strict + priorisation US |
| 2 | Double BDD : SQL Server (dev) / PostgreSQL (prod) | Tests intégration sur les deux |
| 3 | Blazor Server : état serveur — latence si nombreuses connexions | Load tests + Aspire scaling |
| 4 | Scope fonctionnel large (combat, inventaire, sorts, sessions) | Découpage Epics/US + Kanban |

## Opportunités (Opportunities)
| # | Opportunité | Priorité |
|---|-------------|----------|
| 1 | Marché JDR numérique en croissance (Roll20, Foundry VTT) | Haute |
| 2 | API REST ouverte → modules communautaires, extensions tierces | Moyenne |
| 3 | Conformité RGAA/OPQUAST → secteur associatif et scolaire | Moyenne |
| 4 | SRD 5.1 OGL — contenus D&D légalement exploitables | Haute |

## Menaces (Threats)
| # | Menace | Contre-mesure |
|---|--------|---------------|
| 1 | Concurrence Roll20 (leader) + Foundry VTT (open source) | Spécialisation multi-système + UX |
| 2 | Droits OGL D&D 5e (Wizards of the Coast) | Utiliser uniquement SRD 5.1 |
| 3 | Failles OWASP — données perso joueurs | Audit OWASP Top 10 (C2.2.3) |
| 4 | Breaking changes .NET 10 RC | Lock NuGet + tests régression |

---
> **Livrable C1.2.1** — À présenter lors de l'oral de soutenance YNOV.`;
            },
        },
        {
            name: "bloc1_stakeholders",
            description: "Génère la cartographie des parties prenantes + matrice influence/intérêt pour Chronique des Mondes (C1.1.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Cartographie Parties Prenantes — Chronique des Mondes [C1.1.1]

## Tableau des Stakeholders
| Partie prenante | Rôle | Intérêt | Influence | Stratégie |
|-----------------|------|---------|-----------|-----------|
| Étudiant (Tommy) | Développeur / Product Owner | Très élevé | Très élevée | Gérer de près |
| Jury YNOV | Évaluateur certification | Élevé | Élevée | Satisfaire & documenter |
| Maîtres de Jeu | Utilisateurs avancés — gestion campagnes/sessions | Très élevé | Élevée | Impliquer & consulter |
| Joueurs (Users) | Utilisateurs finaux — gestion personnages | Élevé | Moyenne | Tenir informés |
| Administrateurs | Gestion plateforme, modération | Moyen | Élevée | Gérer de près |
| Communauté JDR | Utilisateurs potentiels, feedback | Moyen | Faible | Surveiller |
| Hébergeur (OVH) | Infrastructure serveur | Faible | Élevée | Surveiller |
| Wizards of the Coast | Propriétaires droits D&D 5e | Faible | Élevée | Surveiller (licences SRD) |

## Matrice Influence / Intérêt
\`\`\`
     Influence
       ↑ Élevée
       │  Jury YNOV · Hébergeur · WotC     │  Tommy · MJ · Admins
       │          [Satisfaire]              │   [Gérer de près]
       ├────────────────────────────────────┼──────────────────→ Intérêt
       │  Communauté JDR                    │  Joueurs
       │     [Surveiller]                   │  [Tenir informés]
       ↓ Faible
\`\`\`

## Plan d'engagement
- **Jury YNOV** : Livrables soignés, oral préparé, démonstration fonctionnelle
- **Maîtres de Jeu** : Interviews utilisateurs, tests d'acceptation (C2.3.1)
- **Joueurs** : Beta tests, recueil feedback (C4.3.1, C4.3.3)

---
> **Livrable C1.1.1** — À présenter lors de l'oral de soutenance YNOV.`,
        },
        {
            name: "bloc1_risk_matrix",
            description: "Génère la matrice des risques techniques et fonctionnels (C1.2.3)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Matrice des Risques — Chronique des Mondes [C1.2.3]

## Légende : Probabilité (1→5) × Impact (1→5) = Score
- 🔴 Critique (>12) · 🟠 Élevé (8-12) · 🟡 Moyen (4-7) · 🟢 Faible (<4)

## Risques Techniques
| ID | Risque | P | I | Score | Niveau | Plan d'action |
|----|--------|---|---|-------|--------|---------------|
| RT-01 | Performance Blazor Server 50+ connexions | 3 | 4 | 12 | 🟠 Élevé | Load tests + Aspire scaling |
| RT-02 | Migration SQL Server → PostgreSQL (prod) | 3 | 4 | 12 | 🟠 Élevé | Tests intégration double BDD dès MVP |
| RT-03 | Désync SignalR (reconnexions combat) | 3 | 4 | 12 | 🟠 Élevé | Circuit breaker + tests E2E SignalR |
| RT-04 | Failles OWASP (injection, XSS) | 2 | 5 | 10 | 🟠 Élevé | Audit OWASP Top 10 (C2.2.3) |
| RT-05 | Complexité migrations EF Core (2 contextes) | 3 | 3 | 9 | 🟠 Élevé | CI migration auto + snapshots |
| RT-06 | Breaking changes .NET 10 | 2 | 3 | 6 | 🟡 Moyen | Lock NuGet + tests régression |

## Risques Fonctionnels
| ID | Risque | P | I | Score | Niveau | Plan d'action |
|----|--------|---|---|-------|--------|---------------|
| RF-01 | Scope creep (trop de systèmes JDR) | 4 | 3 | 12 | 🟠 Élevé | MVP strict : D&D 5e + générique |
| RF-02 | Droits intellectuels SRD D&D 5e (OGL) | 2 | 5 | 10 | 🟠 Élevé | Utiliser uniquement SRD 5.1 OGL |
| RF-03 | UX trop complexe pour les joueurs | 3 | 3 | 9 | 🟠 Élevé | Tests utilisateurs MJ itératifs |
| RF-04 | Non-conformité RGPD (données joueurs) | 2 | 4 | 8 | 🟠 Élevé | Politique vie privée + chiffrement |

---
> **Livrable C1.2.3** — Mettre à jour à chaque sprint.`,
        },
        {
            name: "bloc1_architecture_comparison",
            description: "Génère l'étude comparative d'architectures justifiant les choix du projet (C1.3.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Étude Comparative Architectures Techniques [C1.3.2]
## Projet : Chronique des Mondes

## Critères pondérés
| Critère | Poids |
|---------|-------|
| Scalabilité | 25% |
| Complexité développement | 20% |
| Performance temps réel | 20% |
| Maintenabilité | 15% |
| Coût infrastructure | 10% |
| Maturité écosystème | 10% |

## Tableau comparatif (note /5)
| Critère | Microservices Aspire ✅ | Monolithe .NET | API + SPA React |
|---------|----------------------|----------------|-----------------|
| Scalabilité | **5** | 2 | 4 |
| Complexité dev | 3 | **5** | 2 |
| Performance RT | **4** | 3 | 3 |
| Maintenabilité | **5** | 2 | 3 |
| Coût infra | 3 | **5** | 3 |
| Maturité | **5** | 4 | 5 |
| **Score pondéré** | **4.25/5** | 3.1/5 | 3.2/5 |

## ✅ Décision : Microservices .NET 10 + Aspire
**Justification** : Score le plus élevé (4.25/5). Blazor Server avec SignalR permet le temps réel
sans framework JS supplémentaire, réduisant la complexité pour un développeur solo tout en conservant
une architecture scalable prête pour la production. Aspire simplifie l'orchestration locale et le monitoring.

---
> **Livrable C1.3.2** — À présenter lors de l'oral de soutenance YNOV.`,
        },
        {
            name: "bloc1_charge_budget",
            description: "Génère l'estimation de charge (jours-hommes) et le budget prévisionnel (C1.4.1 + C1.4.2)",
            parameters: {
                type: "object",
                properties: {
                    tjm: { type: "number", description: "Taux Journalier Moyen en euros (défaut : 400)" },
                },
            },
            handler: async (args) => {
                const tjm = args.tjm || 400;
                const phases = [
                    ["Phase 1 — Architecture & Setup", 11],
                    ["Phase 2 — Backend Core (MVP)", 23],
                    ["Phase 3 — Frontend Blazor Server", 24],
                    ["Phase 4 — Qualité & Documentation", 13],
                ];
                const total = phases.reduce((s, [, d]) => s + d, 0);
                const aleasPct = 0.15;
                const totalAvecAleas = Math.round(total * (1 + aleasPct));
                return `# Estimation Charge & Budget — Chronique des Mondes [C1.4.1 + C1.4.2]
TJM : ${tjm}€/jour

## WBS par Phase
| Phase | Jours-hommes | Coût |
|-------|-------------|------|
${phases.map(([n, d]) => `| ${n} | ${d}j | ${d * tjm}€ |`).join("\n")}
| **TOTAL** | **${total}j** | **${total * tjm}€** |
| + Aléas (${aleasPct * 100}%) | ${Math.round(total * aleasPct)}j | ${Math.round(total * tjm * aleasPct)}€ |
| **BUDGET FINAL** | **${totalAvecAleas}j** | **${Math.round(total * tjm * (1 + aleasPct))}€** |

## Détail Phase 1 — Architecture & Setup (11j)
- Cadrage + documentation + SWOT : 2j
- Setup Aspire AppHost + projets : 1j
- CI/CD GitHub Actions + environnements : 2j
- Base de données + migrations EF Core : 3j
- Authentification JWT + BCrypt : 3j

## Détail Phase 2 — Backend Core (23j)
- CRUD Utilisateurs + Campagnes : 4j
- CRUD Personnages + attributs D&D : 5j
- Système de combat + dés côté serveur : 6j
- API Sessions JDR + états : 4j
- Tests unitaires xUnit (couverture 80%) : 4j

## Détail Phase 3 — Frontend Blazor Server (24j)
- Layout principal + navigation : 2j
- Pages Campagnes + Personnages : 6j
- Interface combat temps réel SignalR : 8j
- Tableau de bord MJ : 4j
- Tests Playwright + accessibilité RGAA : 4j

## Détail Phase 4 — Qualité & Documentation (13j)
- Documentation technique complète : 3j
- Cahier de recettes + scénarios test : 2j
- Audit sécurité OWASP : 2j
- Déploiement production + monitoring : 3j
- Préparation soutenance : 3j

## Budget Infrastructure (annuel)
| Poste | /mois | /an |
|-------|-------|-----|
| Serveur dédié OVH (dev+prod) | 30€ | 360€ |
| Domaine + SSL | 2€ | 24€ |
| GitHub Teams (CI/CD) | 4€ | 48€ |
| **Total infra** | **36€** | **432€** |

---
> **Livrables C1.4.1 + C1.4.2** — À présenter lors de l'oral de soutenance YNOV.`;
            },
        },
    ],
});

await session.log("✅ Bloc 1 — Cadrage chargé (C1.1.1→C1.6). Outils : bloc1_swot · bloc1_stakeholders · bloc1_risk_matrix · bloc1_architecture_comparison · bloc1_charge_budget");
