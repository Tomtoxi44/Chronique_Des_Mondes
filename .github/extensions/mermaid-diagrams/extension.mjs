// Extension: mermaid-diagrams
// Skill transversal – Génère des diagrammes Mermaid pour tous les blocs de certification
// Compétences couvertes : C1.5 (UML/Merise), C2.2.1 (prototype), C3.1 (planning), C3.2.2 (logigrammes)
// Les diagrammes sont rendus visuellement dans l'interface Copilot CLI.

import { joinSession } from "@github/copilot-sdk/extension";

const fence = "```";

function mermaidBlock(diagram) {
    return `${fence}mermaid\n${diagram}\n${fence}`;
}

const MERMAID_CONTEXT = `
Skill Mermaid – Diagrammes techniques pour la certification YNOV RNCP 39583

Ce skill génère des diagrammes Mermaid rendus visuellement dans l'interface Copilot CLI.
Disponible dans tous les agents de certification (Bloc 1, 2, 3, 4).

Outils disponibles :
- mermaid_architecture : diagramme C4 de l'architecture du projet (C1.5, C2.2.1)
- mermaid_sequence     : diagrammes de séquence – flux auth | combat | campaign_creation (C2.2.1, C2.2.2)
- mermaid_class        : diagramme de classes – domaine core | campaign | character | combat | all (C1.5)
- mermaid_gantt        : planning Gantt – certification 2025/26 ou phases de développement (C1.4.1, C3.1)
- mermaid_er           : schéma entité-relation de la base de données (C1.5)
- mermaid_flowchart    : logigrammes – bug_fix | deployment | ci_cd | user_feedback (C3.2.2, C4.2.2)
- mermaid_state        : machines d'états – combat | session | auth (C2.2.1, C2.2.2)
`;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => ({ additionalContext: MERMAID_CONTEXT }),
        onUserPromptSubmitted: async (input) => {
            const p = input.prompt.toLowerCase();
            const relevant = [
                "mermaid", "diagramme", "diagram", "graphe", "graph", "schema", "schéma",
                "sequence", "gantt", "entite", "entité", "classe", "etat", "état",
                "architecture", "flowchart", "logigramme", "c4", "uml", "er diagram",
                "visuel", "visualiser",
            ].some(k => p.includes(k));
            if (!relevant) return;
            return {
                additionalContext: `Skill Mermaid disponible – Propose les outils mermaid_* pour générer des diagrammes visuels rendus dans l'interface.`,
            };
        },
    },
    tools: [
        {
            name: "mermaid_architecture",
            description: "Génère le diagramme d'architecture C4 du projet Chronique des Mondes (C1.5, C2.2.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => {
                const diagram =
`graph TB
    Browser["Navigateur Web"]

    subgraph Aspire["Orchestration .NET 10 et Aspire AppHost"]
        Web["Cdm.Web – Blazor Server :5001"]
        API["Cdm.ApiService – REST + SignalR :5000"]
        Mig["Cdm.MigrationsManager – EF Core"]
    end

    subgraph Persistance["Persistance"]
        DB[("SQL Server")]
    end

    subgraph Infra["Infrastructure"]
        GH["GitHub Actions – CI/CD"]
        Sentry["Sentry.io – Monitoring prod"]
        ASPD["Aspire Dashboard – :17223"]
    end

    Browser -->|"HTTP + WebSocket"| Web
    Web -->|"HTTP REST"| API
    Web -->|"SignalR"| API
    API -->|"EF Core 10"| DB
    Mig -->|"migrations"| DB
    GH -->|"deploiement"| API
    API -->|"erreurs et traces"| Sentry
    API -.->|"metriques"| ASPD
    Web -.->|"logs"| ASPD`;

                return `# Architecture – Chronique des Mondes [C1.5 – C2.2.1]

${mermaidBlock(diagram)}

Légende :
- Flèches pleines : appels synchrones HTTP/REST
- Flèches WebSocket : connexions temps réel SignalR
- Flèches pointillées : flux de monitoring asynchrone

Compétences visées : C1.5 (modélisation C4), C2.2.1 (prototype applicatif)`;
            },
        },
        {
            name: "mermaid_sequence",
            description: "Génère un diagramme de séquence Mermaid (C2.2.1, C2.2.2) – flow : auth | combat | campaign_creation",
            parameters: {
                type: "object",
                properties: {
                    flow: {
                        type: "string",
                        enum: ["auth", "combat", "campaign_creation"],
                        description: "Flux à visualiser : 'auth' | 'combat' | 'campaign_creation'",
                    },
                },
            },
            handler: async (args) => {
                const flow = args.flow || "auth";

                const authDiagram =
`sequenceDiagram
    participant U as Utilisateur
    participant W as Blazor Server
    participant A as ApiService
    participant DB as SQL Server

    U->>W: Saisie email et mot de passe
    W->>A: POST /api/auth/login
    A->>DB: SELECT user WHERE email = ?
    DB-->>A: UserEntity
    Note over A: BCrypt.Verify – work factor 12
    A->>A: Generer JWT RS256 – expiration 1 h
    A-->>W: 200 OK – access_token + refresh_token
    W-->>U: Cookie securise et redirection dashboard
    Note over W,A: Requetes suivantes avec Authorization Bearer`;

                const combatDiagram =
`sequenceDiagram
    participant J as Joueur
    participant B as Blazor Server
    participant H as SignalR Hub
    participant S as CombatService
    participant DB as SQL Server

    J->>B: Clic – Lancer 1d20+5
    B->>H: RollDice(combatId, expression)
    H->>S: ProcessRoll(combatId, userId)
    Note over S: RNG cryptographique cote serveur
    S->>DB: INSERT CombatAction(result)
    S-->>H: BroadcastResult(combatId, result)
    H-->>B: ReceiveRollResult(result)
    B-->>J: Affichage du resultat
    H-->>B: NotifyAllPlayers(nextTurn)
    Note over J,H: Tous les joueurs informes en temps reel`;

                const campaignDiagram =
`sequenceDiagram
    participant MJ as Maitre de Jeu
    participant W as Blazor Server
    participant A as ApiService
    participant DB as SQL Server

    MJ->>W: Formulaire creer campagne D&D 5e
    W->>A: POST /api/campaigns
    Note over A: Valider GameType + autorisation MJ
    A->>DB: INSERT Campaign(name, gameType, ownerId)
    DB-->>A: CampaignEntity
    A-->>W: 201 Created
    W-->>MJ: Page campagne creee
    MJ->>W: Inviter un joueur par email
    W->>A: POST /api/campaigns/{id}/invite
    A->>DB: INSERT CampaignPlayer(campaignId, userId)
    A-->>W: 200 OK
    W-->>MJ: Invitation envoyee`;

                const diagrams = {
                    auth: authDiagram,
                    combat: combatDiagram,
                    campaign_creation: campaignDiagram,
                };
                const titles = {
                    auth: "Authentification JWT + BCrypt",
                    combat: "Système de Combat SignalR",
                    campaign_creation: "Création d'une Campagne",
                };

                return `# Diagramme de Séquence – ${titles[flow]} [C2.2.1 – C2.2.2]

${mermaidBlock(diagrams[flow])}

Compétences visées : C2.2.1 (prototype), C2.2.2 (ce flux doit être couvert par des tests d'intégration xUnit)`;
            },
        },
        {
            name: "mermaid_class",
            description: "Génère le diagramme de classes du modèle métier (C1.5, C2.2.1) – domain : core | campaign | character | combat | all",
            parameters: {
                type: "object",
                properties: {
                    domain: {
                        type: "string",
                        enum: ["core", "campaign", "character", "combat", "all"],
                        description: "Domaine : 'core' | 'campaign' | 'character' | 'combat' | 'all'",
                    },
                },
            },
            handler: async (args) => {
                const domain = args.domain || "all";

                const core =
`    class User {
        +int Id
        +string Email
        +string PasswordHash
        +UserRole Role
        +DateTime CreatedAt
    }
    class UserRole {
        <<enumeration>>
        User
        GameMaster
        Admin
    }`;

                const campaign =
`    class Campaign {
        +int Id
        +string Name
        +GameType GameType
        +bool IsActive
        +int OwnerId
    }
    class GameType {
        <<enumeration>>
        Generic
        DnD5e
    }
    class GameSession {
        +int Id
        +SessionStatus Status
        +int CampaignId
        +DateTime StartedAt
    }
    class SessionStatus {
        <<enumeration>>
        Pending
        Active
        Ended
    }`;

                const character =
`    class Character {
        +int Id
        +string Name
        +GameType GameType
        +string AttributesJson
        +int OwnerId
        +int CampaignId
        +GetAttribute(key) object
    }`;

                const combat =
`    class CombatAction {
        +int Id
        +string DiceExpression
        +int RollResult
        +ActionType Type
        +int SessionId
        +int CharacterId
        +DateTime RolledAt
    }
    class ActionType {
        <<enumeration>>
        Attack
        Spell
        SaveThrow
        Initiative
    }`;

                const relations =
`    User "1" --> "0..*" Campaign : possede
    User "1" --> "0..*" Character : joue
    Campaign "0..*" --> "0..*" User : joueurs
    Campaign "1" --> "0..*" GameSession : heberge
    Campaign "1" --> "0..*" Character : contient
    GameSession "1" --> "0..*" CombatAction : enregistre`;

                const parts = [];
                if (domain === "core" || domain === "all") parts.push(core);
                if (domain === "campaign" || domain === "all") parts.push(campaign);
                if (domain === "character" || domain === "all") parts.push(character);
                if (domain === "combat" || domain === "all") parts.push(combat);
                if (domain === "all") parts.push(relations);

                const diagram = `classDiagram\n${parts.join("\n")}`;

                return `# Diagramme de Classes – domaine ${domain} [C1.5 – C2.2.1]

${mermaidBlock(diagram)}

Note : AttributesJson stocke les attributs spécifiques au système de règles (D&D 5e : FOR/DEX/CON... ; Générique : attributs libres). EF Core Query Filters isolent les données par utilisateur (C2.2.3).

Compétences visées : C1.5 (modélisation UML), C2.2.1 (prototype applicatif)`;
            },
        },
        {
            name: "mermaid_gantt",
            description: "Génère un diagramme de Gantt (C1.4.1, C3.1) – type : certification | phases",
            parameters: {
                type: "object",
                properties: {
                    type: {
                        type: "string",
                        enum: ["certification", "phases"],
                        description: "Type de Gantt : 'certification' | 'phases'",
                    },
                },
            },
            handler: async (args) => {
                const type = args.type || "certification";

                const certificationDiagram =
`gantt
    title Planning Certification RNCP 39583 – 2025/2026
    dateFormat YYYY-MM-DD
    axisFormat %b %Y

    section Developpement
    Phase 1 Architecture et Setup      :done, 2025-10-01, 2025-11-30
    Phase 2 Backend Core               :done, 2025-11-01, 2026-01-31
    Phase 3 Frontend Blazor Server     :done, 2026-02-01, 2026-04-30
    Phase 4 Qualite et Documentation   :done, 2026-05-01, 2026-06-07

    section Blocs de Certification
    BLOC 1 Rendu ecrit 30 pages        :crit, done, 2026-06-15, 1d
    Preparation BLOC 2                 :done, 2026-06-16, 2026-07-22
    BLOC 2 Rendu ecrit 30 pages        :crit, active, 2026-07-23, 1d
    Preparation BLOC 4                 :2026-07-24, 2026-08-19
    BLOC 4 Rendu ecrit 20 pages        :crit, 2026-08-20, 1d
    Preparation BLOC 3                 :2026-08-21, 2026-09-16
    BLOC 3 Oral 45 minutes             :crit, 2026-09-17, 1d
    Jury final                         :crit, 2026-10-01, 2026-10-31`;

                const phasesDiagram =
`gantt
    title Phases de Developpement – Chronique des Mondes
    dateFormat YYYY-MM-DD
    axisFormat %b %Y

    section Phase 1 – Architecture 11 j
    Cadrage et documentation           :done, 2025-10-01, 4d
    Setup Aspire AppHost               :done, 2025-10-06, 2d
    CI/CD GitHub Actions               :done, 2025-10-08, 4d
    Base de donnees EF Core            :done, 2025-10-13, 6d
    Authentification JWT               :done, 2025-10-20, 6d

    section Phase 2 – Backend Core 23 j
    CRUD Utilisateurs et Campagnes     :done, 2025-11-01, 8d
    CRUD Personnages D&D 5e            :done, 2025-11-12, 10d
    Systeme de combat cote serveur     :done, 2025-11-25, 12d
    API Sessions JDR                   :done, 2025-12-10, 8d
    Tests unitaires xUnit 80 pct       :done, 2025-12-20, 8d

    section Phase 3 – Frontend 24 j
    Layout et navigation               :done, 2026-02-01, 4d
    Pages Campagnes et Personnages     :done, 2026-02-06, 12d
    Interface combat SignalR           :done, 2026-02-20, 16d
    Tableau de bord MJ                 :done, 2026-03-10, 8d
    Tests Playwright et RGAA           :done, 2026-03-20, 8d

    section Phase 4 – Qualite 13 j
    Documentation technique            :done, 2026-05-01, 6d
    Cahier de recettes                 :done, 2026-05-08, 4d
    Audit securite OWASP               :done, 2026-05-13, 4d
    Deploiement et monitoring          :done, 2026-05-19, 6d
    Preparation soutenance             :done, 2026-05-26, 6d`;

                const diagrams = { certification: certificationDiagram, phases: phasesDiagram };

                return `# Diagramme de Gantt – ${type} [C1.4.1 – C3.1]

${mermaidBlock(diagrams[type])}

Compétences visées : C1.4.1 (estimation de charge), C3.1 (planification Agile/Kanban)`;
            },
        },
        {
            name: "mermaid_er",
            description: "Génère le schéma entité-relation de la base de données (C1.5, C2.2.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => {
                const diagram =
`erDiagram
    USERS {
        int Id PK
        string Email UK
        string PasswordHash
        string Role
        datetime CreatedAt
    }
    CAMPAIGNS {
        int Id PK
        string Name
        string GameType
        int OwnerId FK
        bit IsActive
        datetime CreatedAt
    }
    CAMPAIGN_PLAYERS {
        int CampaignId PK,FK
        int UserId PK,FK
        datetime JoinedAt
    }
    CHARACTERS {
        int Id PK
        string Name
        string GameType
        nvarchar AttributesJson
        int OwnerId FK
        int CampaignId FK
    }
    GAME_SESSIONS {
        int Id PK
        string Status
        int CampaignId FK
        datetime StartedAt
        datetime EndedAt
    }
    COMBAT_ACTIONS {
        int Id PK
        string DiceExpression
        int RollResult
        string ActionType
        int SessionId FK
        int CharacterId FK
        datetime RolledAt
    }

    USERS ||--o{ CAMPAIGNS : "possede (Owner)"
    USERS }o--o{ CAMPAIGNS : "participe (Player)"
    CAMPAIGNS ||--o{ CHARACTERS : "contient"
    CAMPAIGNS ||--o{ GAME_SESSIONS : "heberge"
    GAME_SESSIONS ||--o{ COMBAT_ACTIONS : "enregistre"
    CHARACTERS ||--o{ COMBAT_ACTIONS : "realise"`;

                return `# Schéma Entité-Relation – Chronique des Mondes [C1.5]

${mermaidBlock(diagram)}

Note : AttributesJson stocke les attributs spécifiques au système de règles. EF Core applique des Query Filters pour isoler les données par utilisateur (C2.2.3 – sécurité A01 OWASP).

Compétences visées : C1.5 (modélisation Merise/entité-relation), C2.2.1 (prototype applicatif)`;
            },
        },
        {
            name: "mermaid_flowchart",
            description: "Génère un logigramme de processus Mermaid (C3.2.2, C2.3.2, C4.2.2) – process : bug_fix | deployment | ci_cd | user_feedback",
            parameters: {
                type: "object",
                properties: {
                    process: {
                        type: "string",
                        enum: ["bug_fix", "deployment", "ci_cd", "user_feedback"],
                        description: "Processus : 'bug_fix' | 'deployment' | 'ci_cd' | 'user_feedback'",
                    },
                },
            },
            handler: async (args) => {
                const process = args.process || "bug_fix";

                const bugFix =
`flowchart TD
    A["Bug signale"] --> B{"Reproductible ?"}
    B -->|"Non"| C["Demander informations"]
    C --> D["Fermer l'issue"]
    B -->|"Oui"| E["Creer issue GitHub"]
    E --> F{"Priorite ?"}
    F -->|"P0 Bloquant"| G["Hotfix – SLA 4 h"]
    F -->|"P1 a P3"| H["Fix planifie en sprint"]
    G --> I["Corriger le code"]
    H --> I
    I --> J["Test xUnit non-regression"]
    J --> K["CI pipeline complet"]
    K --> L{"CI OK ?"}
    L -->|"Non"| I
    L -->|"Oui"| M["PR vers dev"]
    M --> N["Deploiement staging"]
    N --> O{"Valide ?"}
    O -->|"Non"| I
    O -->|"Oui"| P["Deploiement production"]
    P --> Q["CHANGELOG et issue fermee"]`;

                const deployment =
`flowchart LR
    A["Commit sur main"] --> B["GitHub Actions"]
    B --> C["Build .NET 10"]
    C --> D{"Build OK ?"}
    D -->|"Non"| E["Notification echec"]
    D -->|"Oui"| F["Tests xUnit"]
    F --> G{"Tests OK ?"}
    G -->|"Non"| E
    G -->|"Oui"| H["OWASP Dependency Check"]
    H --> I{"0 critique ?"}
    I -->|"Non"| E
    I -->|"Oui"| J["Build Docker Image"]
    J --> K["Deploiement Staging"]
    K --> L["Tests Playwright E2E"]
    L --> M{"E2E OK ?"}
    M -->|"Non"| E
    M -->|"Oui"| N["Tag SemVer et CHANGELOG"]
    N --> O["Deploiement Production"]
    O --> P["Health Check /health"]
    P --> Q["Monitoring Sentry actif"]`;

                const ciCd =
`flowchart TD
    A["Push ou Pull Request"] --> B["GitHub Actions"]
    B --> C{"Branche ?"}
    C -->|"main"| D["Pipeline complet"]
    C -->|"dev"| E["Pipeline staging"]
    C -->|"feature/*"| F["Pipeline PR"]
    D --> G["Restore NuGet"]
    E --> G
    F --> G
    G --> H["Build Release – warnaserror"]
    H --> I["Tests xUnit et Code Coverage"]
    I --> J["OWASP Dependency Scan"]
    J --> K{"main ou dev ?"}
    K -->|"Oui"| L["Build Docker Image"]
    K -->|"Non"| M["Rapport resultats PR"]
    L --> N{"Branch main ?"}
    N -->|"Oui"| O["Deploiement Production"]
    N -->|"Non"| P["Deploiement Staging"]`;

                const userFeedback =
`flowchart TD
    A["Retour utilisateur – GitHub Issue"] --> B["Triage sous 48 h"]
    B --> C{"Type ?"}
    C -->|"Bogue"| D["Fiche anomalie C4.2.1"]
    C -->|"Amelioration"| E["US dans backlog"]
    C -->|"Question"| F["Documentation mise a jour"]
    D --> G["Processus correction C4.2.2"]
    G --> H["Deploiement correctif"]
    E --> I{"Priorite ?"}
    I -->|"Haute"| J["Sprint suivant"]
    I -->|"Normale"| K["Backlog"]
    J --> L["Developpement et tests"]
    L --> H
    H --> M["Notification utilisateur sous 72 h"]
    M --> N["CHANGELOG mis a jour C4.3.2"]
    N --> O["Score NPS mis a jour"]`;

                const diagrams = {
                    bug_fix: bugFix,
                    deployment,
                    ci_cd: ciCd,
                    user_feedback: userFeedback,
                };
                const titles = {
                    bug_fix: "Correction des Bogues",
                    deployment: "Déploiement Progressif",
                    ci_cd: "Flux CI/CD GitHub Actions",
                    user_feedback: "Traitement des Retours Utilisateurs",
                };
                const competences = {
                    bug_fix: "C2.3.2, C4.2.1, C4.2.2",
                    deployment: "C2.2.4, C4.2.2",
                    ci_cd: "C2.1.2, C2.2.4",
                    user_feedback: "C4.3.1, C4.3.3",
                };

                return `# Logigramme – ${titles[process]} [${competences[process]}]

${mermaidBlock(diagrams[process])}

Compétences visées : ${competences[process]}`;
            },
        },
        {
            name: "mermaid_state",
            description: "Génère un diagramme d'états Mermaid (C2.2.1, C2.2.2) – machine : combat | session | auth",
            parameters: {
                type: "object",
                properties: {
                    machine: {
                        type: "string",
                        enum: ["combat", "session", "auth"],
                        description: "Machine d'états : 'combat' | 'session' | 'auth'",
                    },
                },
            },
            handler: async (args) => {
                const machine = args.machine || "combat";

                const combatDiagram =
`stateDiagram-v2
    [*] --> Inactif
    Inactif --> Initialisation : MJ lance le combat
    Initialisation --> EnAttente : Initiatives calculees
    EnAttente --> TourJoueur : Ordre etabli
    TourJoueur --> ActionEnCours : Joueur choisit
    ActionEnCours --> JetDeDe : Jet requis
    ActionEnCours --> TourJoueur : Sans jet de de
    JetDeDe --> CalcServeur : RNG cote serveur
    CalcServeur --> BroadcastSignalR : Resultat calcule
    BroadcastSignalR --> TourJoueur : Tour suivant
    TourJoueur --> Suspendu : Joueur deconnecte
    Suspendu --> TourJoueur : Reconnexion reussie
    Suspendu --> Abandonne : Timeout 5 min
    TourJoueur --> Termine : Condition de fin atteinte
    Abandonne --> [*]
    Termine --> [*]`;

                const sessionDiagram =
`stateDiagram-v2
    [*] --> EnAttente
    EnAttente --> Active : MJ demarre la session
    Active --> EnCombat : MJ lance un combat
    EnCombat --> Active : Combat termine
    Active --> EnPause : MJ met en pause
    EnPause --> Active : MJ reprend
    Active --> Terminee : MJ cloture la session
    EnAttente --> Annulee : MJ annule
    Terminee --> [*]
    Annulee --> [*]`;

                const authDiagram =
`stateDiagram-v2
    [*] --> Anonyme
    Anonyme --> EnAuthentification : Saisie identifiants
    EnAuthentification --> Echec : Identifiants invalides
    EnAuthentification --> Connecte : JWT genere RS256
    Echec --> Anonyme : Nouvelle tentative
    Echec --> Bloque : 5 echecs consecutifs
    Bloque --> Anonyme : Delai ecoule 15 min
    Connecte --> TokenExpire : Expiration 1 h
    TokenExpire --> Connecte : Refresh token valide
    TokenExpire --> Anonyme : Refresh token expire
    Connecte --> Anonyme : Deconnexion
    Connecte --> [*]`;

                const diagrams = {
                    combat: combatDiagram,
                    session: sessionDiagram,
                    auth: authDiagram,
                };
                const titles = {
                    combat: "Machine d'états – Système de Combat SignalR",
                    session: "Machine d'états – Session de Jeu",
                    auth: "Machine d'états – Authentification JWT",
                };

                return `# ${titles[machine]} [C2.2.1 – C2.2.2]

${mermaidBlock(diagrams[machine])}

Compétences visées : C2.2.1 (prototype), C2.2.2 (les états représentent les scénarios de test xUnit à couvrir)`;
            },
        },
    ],
});

await session.log("Skill Mermaid charge. Outils : mermaid_architecture · mermaid_sequence · mermaid_class · mermaid_gantt · mermaid_er · mermaid_flowchart · mermaid_state");
