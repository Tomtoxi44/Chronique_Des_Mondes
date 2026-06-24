// Extension: bloc3-pilotage
// BLOC 3 — Coordonner et piloter (Certification Expert en Développement Logiciel YNOV)
// Compétences : C3.1, C3.2.1, C3.2.2, C3.3.1, C3.3.2, C3.4.1, C3.4.2
// Modalité : Présentation orale + démonstration

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC3_CONTEXT = `
BLOC 3 – Coordonner et piloter (Certification YNOV Expert en Développement Logiciel)
Titre : Expert en développement logiciel – RNCP 39583 – Niveau 7

Tu assistes un étudiant en certification « Expert(e) en Développement Logiciel » YNOV.
Projet : Chronique des Mondes – Plateforme de gestion de campagnes de jeu de rôle multi-systèmes.
Méthode : Agile/Kanban (GitHub Projects), backlog dans .github/backlog/, format US-XXX

Modalité et deadline BLOC 3 :
- Epreuve : oral 45 minutes – 30 min présentation + 15 min échange avec le jury – Individuel
- Date : 17 septembre 2026
- Dépôt du support : avant l'oral sur DigiformaCertif – https://ynov.mycertif.app/selection-connexion
- Attention : sans dépôt dans le délai imparti, le bloc est automatiquement invalidé

Règles de validation :
- Bloc validé si au moins 50 % des compétences sont « acquises »
- Aucune compétence éliminatoire ne doit être « non acquise »
- La certification nécessite la validation de tous les blocs

Conseils pour l'oral 45 minutes :
- 30 min de présentation : prévoir environ 4 min par compétence C3.X (7 compétences)
- 15 min d'échange : préparer les questions sur les arbitrages projet et la gestion des retards
- Inclure une démonstration live de l'application (5 à 8 min recommandées)
- Documenter les décisions prises avec leur impact (logigrammes)
- Prouver l'usage réel d'outils de pilotage (GitHub Projects, KPIs chiffrés)

Compétences évaluées (présentation + démonstration) :
- C3.1   : Planification (Gantt/PERT, Agile/Scrum/Kanban, RACI)
- C3.2.1 : Piloter l'avancement (tableaux de bord, KPIs)
- C3.2.2 : Prendre des arbitrages (logigrammes de décision)
- C3.3.1 : Manager une équipe (styles managériaux)
- C3.3.2 : Evaluer les besoins en compétences (plan de développement)
- C3.4.1 : Rédiger des comptes rendus client (indicateurs de satisfaction)
- C3.4.2 : Démontrer les fonctionnalités devant un jury

Livrables attendus par le jury :
1. Tableau de planification (Kanban GitHub Projects + sprints)
2. Matrice RACI (Responsible, Accountable, Consulted, Informed)
3. Tableau de bord KPIs (vélocité, taux de complétion, bugs)
4. Logigramme d'arbitrage pour les décisions clés
5. Comptes rendus des sprints (rétrospectives)
6. Plan de démonstration pour le jury (45 min chrono)

Règles de génération des livrables :
- Aucun émoji dans les réponses
- Guillemets français : « » pour les citations et termes techniques
- Tiret demi-cadratin : – (U+2013) pour les listes et séparations, jamais le tiret cadratin —
- Contenu concis et factuel, adapté à un document professionnel

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
                additionalContext: `BLOC 3 actif – Mentionne la compétence C3.X visée, structure le livrable pour une présentation jury, ancre dans le contexte Chronique des Mondes.`,
            };
        },
    },
    tools: [
        {
            name: "bloc3_raci",
            description: "Génère la matrice RACI du projet Chronique des Mondes (C3.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Matrice RACI – Chronique des Mondes [C3.1]

Légende :
- R = Responsible (réalise)
- A = Accountable (responsable final)
- C = Consulted (consulté avant)
- I = Informed (informé après)

| Activité                         | Tommy (Dev/PO) | Jury YNOV | MJ (beta tester) | Joueurs (beta) |
|----------------------------------|----------------|-----------|------------------|----------------|
| Cadrage et spécifications        | A/R            | C         | C                | I              |
| Développement fonctionnalités    | A/R            | I         | C                | I              |
| Tests unitaires xUnit            | A/R            | I         | –                | –              |
| Tests d'acceptation              | R              | A         | R                | C              |
| Revue de code (Copilot)          | A              | I         | –                | –              |
| Déploiement staging              | A/R            | I         | I                | –              |
| Déploiement production           | A/R            | I         | I                | I              |
| Documentation technique          | A/R            | C         | I                | –              |
| Démonstration jury               | R              | A         | C                | –              |
| Soutenance orale                 | R              | A         | –                | –              |
| Retours utilisateurs             | I              | I         | R                | R              |
| Correctifs post-déploiement      | A/R            | I         | C                | I              |

Notes :
- Tommy est seul développeur, donc R et A sont souvent cumulés.
- Le jury YNOV est Accountable sur l'évaluation finale.
- Les Maîtres de Jeu sont les beta testeurs principaux pour la logique métier.

Compétence visée : C3.1 – A présenter lors de la présentation jury.`,
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
                return `# Tableau de Bord KPIs – Chronique des Mondes [C3.2.1]
Sprint ${sprint} – ${new Date().toLocaleDateString("fr-FR")}

Indicateurs clés :

Avancement global :

| KPI                   | Valeur        | Cible   | Statut       |
|-----------------------|---------------|---------|--------------|
| US completees         | ${done}/${total} | ${total} | ${pct} %  |
| Bugs ouverts          | ${bugs}       | 0       | ${bugs > 5 ? "Critique" : bugs > 2 ? "Attention" : "OK"} |
| Couverture tests      | –             | ≥ 70 %  | A mesurer    |
| Build CI              | –             | Stable  | A verifier   |
| Score securite OWASP  | –             | 0 critique | A auditer |

Velocite par sprint :

| Sprint              | US planifiees | US livrees | Velocite |
|---------------------|---------------|------------|----------|
| Sprint 1            | ?             | ?          | ? SP     |
| Sprint ${sprint} (actuel) | ?       | ${done}    | ? SP     |
| Projection fin      | ${total}      | –          | –        |

Qualite du code :

| Metrique                   | Valeur | Seuil d'alerte |
|----------------------------|--------|----------------|
| Couverture tests unitaires | –      | < 70 %         |
| Tests Playwright passants  | –      | < 100 %        |
| Warnings de build          | –      | > 0            |
| Vulnerabilites NuGet       | –      | > 0 critique   |

Satisfaction utilisateurs (MJ beta) :

| Critere                  | Note /5 | Commentaire |
|--------------------------|---------|-------------|
| Facilite de prise en main | – /5   |             |
| Performance de l'interface | – /5  |             |
| Stabilite (0 crash)      | – /5    |             |

Progression : ${pct} % – Etat : ${pct < 30 ? "Retard" : pct < 70 ? "En cours" : "Dans les delais"}

Compétence visée : C3.2.1 – Tableau de bord à mettre à jour à chaque sprint.`;
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
                return `# Compte rendu Sprint ${sprint} – Chronique des Mondes [C3.4.1]
Date : ${new Date().toLocaleDateString("fr-FR")}

Résumé du sprint :

US livrées :

| US     | Titre   | Statut   | Remarques |
|--------|---------|----------|-----------|
| US-XXX | [Titre] | Terminée |           |
| US-XXX | [Titre] | Partielle|           |

US non livrées :

| US     | Titre   | Raison    | Action corrective |
|--------|---------|-----------|-------------------|
| US-XXX | [Titre] | [Raison]  | [Action]          |

Rétrospective (format Start / Stop / Continue) :

Continue – ce qui a bien fonctionné :
- [ ] Exemple : Tests xUnit écrits en TDD
- [ ] Exemple : Pipeline CI stable

Stop – ce qui ne fonctionne pas :
- [ ] Exemple : Estimation trop optimiste des US de combat

Start – à améliorer :
- [ ] Exemple : Ajouter tests Playwright dès le développement
- [ ] Exemple : Démo staging chaque vendredi

Métriques du sprint :

| KPI               | Valeur |
|-------------------|--------|
| US planifiées     |        |
| US livrées        |        |
| Bugs découverts   |        |
| Bugs corrigés     |        |
| Couverture tests  |        |

Objectifs Sprint ${sprint + 1} :
1. [ ] [Objectif 1]
2. [ ] [Objectif 2]
3. [ ] [Objectif 3]

Communication :
- Avancement communiqué via : GitHub Projects + rapport sprint
- Prochain point de synchronisation : [Date]

Compétence visée : C3.4.1 – Compte rendu à archiver dans conception/ ou .github/sprints/.`;
            },
        },
        {
            name: "bloc3_demo_script",
            description: "Génère le script de démonstration pour le jury YNOV (C3.4.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Script de Démonstration Jury – Chronique des Mondes [C3.4.2]

Objectif :
Démontrer les fonctionnalités clés de la plateforme JDR en 15 à 20 minutes devant le jury YNOV.

Prérequis :
- [ ] Application démarrée : dotnet run --project Cdm/Cdm.AppHost
- [ ] Base de données avec données de démonstration chargées
- [ ] Deux navigateurs ouverts (MJ + Joueur)
- [ ] URL : https://localhost:5001

Déroulé de la démonstration (15 min) :

Partie 1 – Présentation architecture (3 min) :
1. Montrer l'Aspire Dashboard (https://localhost:17223)
   – Services actifs : API, Web, MigrationsManager
   – Métriques temps réel
2. Montrer la structure des projets dans VS Code

Partie 2 – Authentification & Sécurité (2 min) :
1. Connexion MJ (rôle : Cdm-GameMasters)
2. Montrer le JWT dans les DevTools, en-tête Authorization
3. Tenter un accès non autorisé – 403 Forbidden
Compétences illustrées : C2.2.3 (sécurité), C2.1.1 (environnements)

Partie 3 – Gestion de campagne (3 min) :
1. Créer une campagne D&D 5e
2. Inviter un joueur (2e navigateur)
3. Le joueur rejoint la campagne via le lien
4. Afficher la liste des personnages compatibles D&D
Compétences illustrées : C2.2.1 (prototype), C2.2.4 (versioning)

Partie 4 – Session de jeu en temps réel (5 min) :
1. Le MJ lance une session
2. Le joueur rejoint via SignalR – notification instantanée
3. Lancer un dé (1d20+5) – résultat calculé côté serveur
4. Combat : initiative, tour de jeu, attaque
5. Couper la connexion joueur – état préservé, reconnexion fonctionnelle
Compétences illustrées : C2.2.2 (tests), C4.1.2 (monitoring)

Partie 5 – CI/CD & Qualité (2 min) :
1. Montrer le pipeline GitHub Actions (dernier run)
2. Rapport de couverture des tests
3. Rapport OWASP Dependency Check
Compétences illustrées : C2.1.2 (CI), C2.2.3 (sécurité), C4.1.1 (dépendances)

Plan de secours :

| Problème                    | Solution                                          |
|-----------------------------|---------------------------------------------------|
| Base de données inaccessible | Données mockées en mémoire (InMemory EF Core)    |
| SignalR ne se connecte pas  | Démonstration en local uniquement, pas staging    |
| Build cassé                 | Montrer le dernier build réussi sur GitHub Actions |

Compétence visée : C3.4.2 – Script à répéter deux fois avant la soutenance.`,
        },
    ],
});

await session.log("Bloc 3 – Pilotage charge (C3.1 a C3.4.2). Outils : bloc3_raci · bloc3_kpi_dashboard · bloc3_sprint_retrospective · bloc3_demo_script");
