// Extension: bloc1-cadrage
// BLOC 1 — Cadrer un projet (Certification Expert en Développement Logiciel YNOV)
// Compétences : C1.1.1, C1.1.2, C1.2.1, C1.2.2, C1.2.3, C1.3.1, C1.3.2, C1.4.1, C1.4.2, C1.5, C1.6
// Modalité : Oral soutenance devant jury

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC1_CONTEXT = `
BLOC 1 – Cadrage de Projet
Certification : Expert en développement logiciel – RNCP 39583 – Niveau 7

Tu assistes un étudiant en certification «  Expert(e) en Développement Logiciel  » YNOV.
Projet : Chronique des Mondes – Plateforme multi-systèmes de gestion de campagnes de JDR.
Stack : .NET 10, Aspire, Blazor Server, SignalR, EF Core, SQL Server, JWT/BCrypt

Modalité et deadline BLOC 1 :
- Epreuve : oral 30 minutes – 20 min présentation + 10 min échange avec le jury – Individuel
- Date : 15 juin 2026 (épreuve passée)
- Dépôt du support : avant l'oral sur DigiformaCertif (https://ynov.mycertif.app/selection-connexion)
- Attention : sans dépôt dans le délai imparti, le bloc est automatiquement invalidé

Règles de validation :
- Bloc validé si au moins 50 % des compétences sont « acquises »
- Aucune compétence éliminatoire ne doit être « non acquise »
- La certification nécessite la validation de tous les blocs
- Rattrapage possible si moins de 50 % des blocs sont invalidés

Compétences évaluées (oral soutenance) :
- C1.1.1 : Cartographier les parties prenantes (stakeholders, matrice influence/intérêt)
- C1.1.2 : Analyser la demande via entretien d'explicitation
- C1.2.1 : Cartographier opportunités/menaces (analyse SWOT)
- C1.2.2 : Evaluer la faisabilité technique (audit technique)
- C1.2.3 : Cartographier les risques techniques et fonctionnels
- C1.3.1 : Assurer une veille technique/technologique/réglementaire
- C1.3.2 : Sélectionner l'architecture technique (étude comparative)
- C1.4.1 : Evaluer la charge de travail en jours-hommes
- C1.4.2 : Estimer le coût et le budget prévisionnel
- C1.5   : Modéliser l'architecture logicielle (UML, Merise, C4)
- C1.6   : Présenter le cadrage au client (argumentaire structuré)

Conseils pour l'oral 30 minutes :
- 20 min de présentation : 10 à 12 slides maximum, 1 slide par compétence C1.X.X
- 10 min d'échange : préparer les choix technologiques (.NET 10 versus Java/Node, Aspire versus Docker Compose)
- Utiliser des visuels (SWOT, matrices, diagrammes) plutôt que du texte dense

Règles de génération des livrables :
- Aucun émoji dans les réponses
- Guillemets français : «  » pour les citations et termes techniques
- Tiret demi-cadratin : – (U+2013) pour les listes et séparations, jamais le tiret cadratin —
- Contenu concis et factuel, adapté à un document professionnel
- Indiquer la compétence C1.X.X en entête de chaque livrable
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
                additionalContext: `BLOC 1 actif – Indique la compétence C1.X.X visée, produis un livrable structuré sans émoji, avec guillemets français «  » et tiret –, ancré dans le projet Chronique des Mondes.`,
            };
        },
    },
    tools: [
        {
            name: "bloc1_planning_overview",
            description: "Affiche le planning complet de la certification 2025/26 avec les deadlines par bloc (RNCP 39583)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Planning de la Certification – Expert en Développement Logiciel RNCP 39583
Niveau 7 – Année 2025/2026

Calendrier des épreuves :

| Bloc   | Epreuve      | Format                          | Durée        | Date                 |
|--------|--------------|---------------------------------|--------------|----------------------|
| BLOC 1 | Rendu écrit  | Code source + dossier 30 p max  | –            | 15 juin 2026 (passé) |
| BLOC 2 | Rendu écrit  | Code source + dossier 30 p max  | –            | 23 juillet 2026      |
| BLOC 4 | Rendu écrit  | Dossier 20 p max                | –            | 20 août 2026         |
| BLOC 3 | Oral         | Présentation + échange jury     | 45 min       | 17 septembre 2026    |
| Jury   | Certification | Décision finale                | –            | Octobre 2026         |

Dépôt des livrables :
- Plateforme : DigiformaCertif
- URL : https://ynov.mycertif.app/selection-connexion
- Attention : tout dépôt hors délai entraîne l'invalidation automatique du bloc concerné

Règles de validation :

| Règle                        | Détail                                              |
|------------------------------|-----------------------------------------------------|
| Seuil par bloc               | Au moins 50 % des compétences doivent être acquises |
| Compétences éliminatoires    | Aucune ne doit être « non acquise »                 |
| Certification complète       | Validation des 4 blocs obligatoire                  |
| Rattrapage                   | Possible si moins de 50 % des blocs sont invalidés  |

Checklist avant chaque épreuve :
- Livrables déposés sur DigiformaCertif dans les délais
- Support de présentation déposé (pour les oraux)
- Diplôme/titre prérequis transmis au campus
- Toutes les compétences du bloc sont couvertes dans les livrables

Priorité de travail recommandée :
1. Juin 2026 : BLOC 1 rendu écrit (15 juin – passé)
2. Juin–juillet 2026 : finalisation dossier Bloc 2 (30 p) + dépôt le 23 juillet
3. Juillet–août 2026 : rédaction dossier Bloc 4 (20 p) + dépôt le 20 août
4. Août–septembre 2026 : préparation oral Bloc 3 (17 septembre)
5. Octobre 2026 : jury final de certification

Source : Modalités des épreuves certificatives RNCP 39583 YNOV (V1.0 – Rentrée 2025)`,
        },
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
                return `# Analyse SWOT – Chronique des Mondes [C1.2.1]
Focus : ${focus}

Forces

| N° | Force                                                                  | Impact |
|----|------------------------------------------------------------------------|--------|
| 1  | Stack .NET 10 + Aspire – orchestration microservices intégrée          | Elevé  |
| 2  | Blazor Server : temps réel natif via SignalR sans framework JS         | Elevé  |
| 3  | Architecture multi-systèmes JDR (générique + D&D 5e) – différenciation | Moyen  |
| 4  | Modèle de données flexible (JSON polymorphique pour attributs)         | Moyen  |
| 5  | CI/CD GitHub Actions + déploiement progressif + versioning SemVer      | Elevé  |
| 6  | Séparation stricte Business / Data / API – maintenabilité garantie     | Elevé  |

Faiblesses

| N° | Faiblesse                                               | Mitigation                         |
|----|---------------------------------------------------------|------------------------------------|
| 1  | Complexité architecturale pour un seul développeur      | MVP strict + priorisation US       |
| 2  | Double base de données : SQL Server (dev), PostgreSQL (prod) | Tests d'intégration sur les deux |
| 3  | Blazor Server : latence si nombreuses connexions        | Load tests + Aspire scaling        |
| 4  | Scope fonctionnel large (combat, inventaire, sorts)     | Découpage Epics/US + Kanban        |

Opportunités

| N° | Opportunité                                             | Priorité |
|----|---------------------------------------------------------|----------|
| 1  | Marché JDR numérique en croissance (Roll20, Foundry VTT) | Haute   |
| 2  | API REST ouverte – modules communautaires tiers         | Moyenne  |
| 3  | Conformité RGAA/OPQUAST – secteur associatif et scolaire | Moyenne |
| 4  | SRD 5.1 OGL – contenus D&D légalement exploitables     | Haute    |

Menaces

| N° | Menace                                                  | Contre-mesure                       |
|----|---------------------------------------------------------|-------------------------------------|
| 1  | Concurrence Roll20 (leader) + Foundry VTT (open source) | Spécialisation multi-système + UX  |
| 2  | Droits OGL D&D 5e (Wizards of the Coast)               | Utiliser uniquement le SRD 5.1      |
| 3  | Failles OWASP – données personnelles des joueurs        | Audit OWASP Top 10 (C2.2.3)        |
| 4  | Breaking changes .NET 10                               | Lock NuGet + tests de régression    |

Compétence visée : C1.2.1 – A présenter lors de l'oral de soutenance YNOV.`;
            },
        },
        {
            name: "bloc1_stakeholders",
            description: "Génère la cartographie des parties prenantes + matrice influence/intérêt pour Chronique des Mondes (C1.1.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Cartographie des Parties Prenantes – Chronique des Mondes [C1.1.1]

Tableau des parties prenantes :

| Partie prenante      | Rôle                                     | Intérêt     | Influence   | Stratégie          |
|----------------------|------------------------------------------|-------------|-------------|---------------------|
| Tommy Angibaud       | Développeur / Product Owner              | Très élevé  | Très élevée | Gérer de près       |
| Jury YNOV            | Evaluateur de la certification           | Elevé       | Elevée      | Satisfaire          |
| Maîtres de Jeu       | Utilisateurs avancés – gestion campagnes | Très élevé  | Elevée      | Impliquer           |
| Joueurs              | Utilisateurs finaux – gestion personnages | Elevé      | Moyenne     | Tenir informés      |
| Administrateurs      | Gestion de la plateforme, modération     | Moyen       | Elevée      | Gérer de près       |
| Communauté JDR       | Utilisateurs potentiels, retours         | Moyen       | Faible      | Surveiller          |
| Hébergeur (OVH)      | Infrastructure serveur                   | Faible      | Elevée      | Surveiller          |
| Wizards of the Coast | Propriétaires des droits D&D 5e          | Faible      | Elevée      | Surveiller (licences SRD) |

Matrice influence / intérêt :

                 Influence élevée
                        |
  Jury YNOV             |  Tommy – MJ – Admins
  Hébergeur – WotC      |
  [Satisfaire]          |  [Gérer de près]
  ----------------------+----------------------> Intérêt élevé
  Communauté JDR        |  Joueurs
  [Surveiller]          |  [Tenir informés]
                        |
                 Influence faible

Plan d'engagement :
- Jury YNOV : livrables soignés, oral préparé, démonstration fonctionnelle
- Maîtres de Jeu : interviews utilisateurs, tests d'acceptation (C2.3.1)
- Joueurs : beta tests, recueil de retours (C4.3.1, C4.3.3)

Compétence visée : C1.1.1 – A présenter lors de l'oral de soutenance YNOV.`,
        },
        {
            name: "bloc1_risk_matrix",
            description: "Génère la matrice des risques techniques et fonctionnels (C1.2.3)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Matrice des Risques – Chronique des Mondes [C1.2.3]

Légende : Probabilité (1 à 5) x Impact (1 à 5) = Score
- Score > 12 : Critique
- Score 8 à 12 : Elevé
- Score 4 à 7 : Moyen
- Score < 4 : Faible

Risques techniques :

| ID    | Risque                                              | P | I | Score | Niveau   | Plan d'action                           |
|-------|-----------------------------------------------------|---|---|-------|----------|-----------------------------------------|
| RT-01 | Performance Blazor Server 50+ connexions            | 3 | 4 | 12    | Elevé    | Load tests + Aspire scaling             |
| RT-02 | Migration SQL Server vers PostgreSQL (prod)         | 3 | 4 | 12    | Elevé    | Tests d'intégration double BDD dès MVP  |
| RT-03 | Désynchronisation SignalR (reconnexions combat)     | 3 | 4 | 12    | Elevé    | Circuit breaker + tests E2E SignalR     |
| RT-04 | Failles OWASP (injection, XSS)                     | 2 | 5 | 10    | Elevé    | Audit OWASP Top 10 (C2.2.3)            |
| RT-05 | Complexité migrations EF Core (2 contextes)         | 3 | 3 | 9     | Elevé    | CI migration automatique + snapshots    |
| RT-06 | Breaking changes .NET 10                            | 2 | 3 | 6     | Moyen    | Lock NuGet + tests de régression        |

Risques fonctionnels :

| ID    | Risque                                              | P | I | Score | Niveau   | Plan d'action                           |
|-------|-----------------------------------------------------|---|---|-------|----------|-----------------------------------------|
| RF-01 | Scope creep (trop de systèmes JDR à supporter)      | 4 | 3 | 12    | Elevé    | MVP strict : D&D 5e + générique         |
| RF-02 | Droits intellectuels SRD D&D 5e (OGL)              | 2 | 5 | 10    | Elevé    | Utiliser uniquement le SRD 5.1 OGL      |
| RF-03 | UX trop complexe pour les joueurs                   | 3 | 3 | 9     | Elevé    | Tests utilisateurs MJ itératifs         |
| RF-04 | Non-conformité RGPD (données joueurs)               | 2 | 4 | 8     | Elevé    | Politique vie privée + chiffrement      |

Compétence visée : C1.2.3 – A mettre à jour à chaque sprint.`,
        },
        {
            name: "bloc1_architecture_comparison",
            description: "Génère l'étude comparative d'architectures justifiant les choix du projet (C1.3.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Etude Comparative des Architectures Techniques [C1.3.2]
Projet : Chronique des Mondes

Critères d'évaluation pondérés :

| Critère                   | Poids |
|---------------------------|-------|
| Scalabilité               | 25 %  |
| Complexité développement  | 20 %  |
| Performance temps réel    | 20 %  |
| Maintenabilité            | 15 %  |
| Coût infrastructure       | 10 %  |
| Maturité de l'écosystème  | 10 %  |

Tableau comparatif (note sur 5) :

| Critère                | Microservices Aspire (retenu) | Monolithe .NET | API + SPA React |
|------------------------|-------------------------------|----------------|-----------------|
| Scalabilité            | 5                             | 2              | 4               |
| Complexité dev         | 3                             | 5              | 2               |
| Performance temps réel | 4                             | 3              | 3               |
| Maintenabilité         | 5                             | 2              | 3               |
| Coût infrastructure    | 3                             | 5              | 3               |
| Maturité               | 5                             | 4              | 5               |
| Score pondéré          | 4,25/5                        | 3,1/5          | 3,2/5           |

Décision retenue : Microservices .NET 10 + Aspire

Justification : score pondéré le plus élevé (4,25/5). Blazor Server avec SignalR permet le temps réel
sans framework JavaScript supplémentaire, réduisant la complexité pour un développeur solo tout en
conservant une architecture scalable adaptée à la production. Aspire simplifie l'orchestration locale
et le monitoring via son tableau de bord intégré.

Compétence visée : C1.3.2 – A présenter lors de l'oral de soutenance YNOV.`,
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
                return `# Estimation Charge & Budget – Chronique des Mondes [C1.4.1 + C1.4.2]
TJM : ${tjm} EUR/jour

WBS par phase :

| Phase                              | Jours-hommes | Cout        |
|------------------------------------|--------------|-------------|
${phases.map(([n, d]) => `| ${n} | ${d} j | ${d * tjm} EUR |`).join("\n")}
| TOTAL                              | ${total} j   | ${total * tjm} EUR |
| Aleas (${aleasPct * 100} %)        | ${Math.round(total * aleasPct)} j | ${Math.round(total * tjm * aleasPct)} EUR |
| BUDGET FINAL                       | ${totalAvecAleas} j | ${Math.round(total * tjm * (1 + aleasPct))} EUR |

Detail Phase 1 – Architecture & Setup (11 j) :
- Cadrage + documentation + SWOT : 2 j
- Setup Aspire AppHost + projets : 1 j
- CI/CD GitHub Actions + environnements : 2 j
- Base de donnees + migrations EF Core : 3 j
- Authentification JWT + BCrypt : 3 j

Detail Phase 2 – Backend Core (23 j) :
- CRUD Utilisateurs + Campagnes : 4 j
- CRUD Personnages + attributs D&D : 5 j
- Systeme de combat + des cote serveur : 6 j
- API Sessions JDR + etats : 4 j
- Tests unitaires xUnit (couverture 80 %) : 4 j

Detail Phase 3 – Frontend Blazor Server (24 j) :
- Layout principal + navigation : 2 j
- Pages Campagnes + Personnages : 6 j
- Interface combat temps reel SignalR : 8 j
- Tableau de bord MJ : 4 j
- Tests Playwright + accessibilite RGAA : 4 j

Detail Phase 4 – Qualite & Documentation (13 j) :
- Documentation technique complete : 3 j
- Cahier de recettes + scenarios test : 2 j
- Audit securite OWASP : 2 j
- Deploiement production + monitoring : 3 j
- Preparation soutenance : 3 j

Budget infrastructure (annuel) :

| Poste                        | Par mois | Par an |
|------------------------------|----------|--------|
| Serveur dedie OVH (dev+prod) | 30 EUR   | 360 EUR |
| Domaine + SSL                | 2 EUR    | 24 EUR  |
| GitHub Teams (CI/CD)         | 4 EUR    | 48 EUR  |
| Total infrastructure         | 36 EUR   | 432 EUR |

Competences visees : C1.4.1 + C1.4.2 – A presenter lors de l oral de soutenance YNOV.`;
            },
        },
    ],
});

await session.log("Bloc 1 – Cadrage charge (C1.1.1 a C1.6). Outils : bloc1_swot · bloc1_stakeholders · bloc1_risk_matrix · bloc1_architecture_comparison · bloc1_charge_budget");
