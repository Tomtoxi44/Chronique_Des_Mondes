// Extension: bloc4-maintenance
// BLOC 4 — Maintenir en condition opérationnelle (Certification Expert en Développement Logiciel YNOV)
// Compétences : C4.1.1, C4.1.2, C4.2.1, C4.2.2, C4.3.1, C4.3.2, C4.3.3
// Modalité : Dossier écrit

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC4_CONTEXT = `
BLOC 4 – Maintenir en condition opérationnelle (Certification YNOV Expert en Développement Logiciel)
Titre : Expert en développement logiciel – RNCP 39583 – Niveau 7

Tu assistes un étudiant en certification « Expert(e) en Développement Logiciel » YNOV.
Projet : Chronique des Mondes – Plateforme de gestion de campagnes de jeu de rôle multi-systèmes.
Stack : .NET 10, Aspire, Blazor Server, GitHub Actions CI/CD

Modalité et deadline BLOC 4 :
- Epreuve : rendu écrit – dossier 20 pages maximum (hors annexes et pages de garde) – Individuel
- Date : 20 août 2026
- Dépôt : sur DigiformaCertif – https://ynov.mycertif.app/selection-connexion
- Attention : sans dépôt dans le délai imparti, le bloc est automatiquement invalidé

Règles de validation :
- Bloc validé si au moins 50 % des compétences sont « acquises »
- Aucune compétence éliminatoire ne doit être « non acquise »
- La certification nécessite la validation de tous les blocs

Structure du dossier écrit (20 pages maximum) :
1. Page de garde + sommaire (2 p)
2. Présentation du projet + contexte technique (2 p)
3. Politique de gestion des dépendances NuGet – C4.1.1 (3 p)
4. Plan de supervision et alertes (Aspire + Sentry) – C4.1.2 (3 p)
5. Modèle de fiche de bug + exemples réels – C4.2.1 (2 p)
6. Pipeline de hotfix CI/CD – C4.2.2 (2 p)
7. Processus de collecte des retours utilisateurs – C4.3.1 (2 p)
8. CHANGELOG structuré SemVer – C4.3.2 (2 p)
9. Procédure support client + FAQ – C4.3.3 (2 p)
Annexes (hors comptage) : configurations Dependabot, captures dashboard, extraits CHANGELOG

Compétences évaluées (dossier écrit) :
- C4.1.1 : Gérer les mises à jour des dépendances (NuGet packages)
- C4.1.2 : Mettre en place un système de supervision/alerte (sondes, seuils)
- C4.2.1 : Consigner les anomalies (fiche de bug structurée)
- C4.2.2 : Mettre en place un correctif CI/CD (hotfix pipeline)
- C4.3.1 : Identifier les axes d'amélioration (retours utilisateurs)
- C4.3.2 : Tenir un journal des versions (CHANGELOG)
- C4.3.3 : Collaborer avec le support client

Règles de génération des livrables :
- Aucun émoji dans les réponses
- Guillemets français : « » pour les citations et termes techniques
- Tiret demi-cadratin : – (U+2013) pour les listes et séparations, jamais le tiret cadratin —
- Contenu concis et factuel, adapté à un document professionnel

Indique toujours la compétence C4.X.X visée et produis des livrables prêts pour le dossier écrit.
`;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => ({ additionalContext: BLOC4_CONTEXT }),
        onUserPromptSubmitted: async (input) => {
            const p = input.prompt.toLowerCase();
            const relevant = ["dépendance","dependance","nuget","mise à jour","monitoring","supervision",
                "alerte","sonde","health check","bug","anomalie","hotfix","changelog","version",
                "support","feedback","retour utilisateur","c4.","bloc 4","sentry","log",
                "correctif","maintenance"].some(k => p.includes(k));
            if (!relevant) return;
            return {
                additionalContext: `BLOC 4 actif – Mentionne la compétence C4.X.X visée, produis un livrable structuré pour le dossier écrit, ancre dans le contexte .NET 10/Aspire.`,
            };
        },
    },
    tools: [
        {
            name: "bloc4_dependency_policy",
            description: "Génère la politique de gestion des dépendances NuGet + config Dependabot (C4.1.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Politique de Gestion des Dépendances – Chronique des Mondes [C4.1.1]

Stratégie de mise à jour :

| Type                             | Fréquence     | Automatisation       | Validation requise          |
|----------------------------------|---------------|----------------------|-----------------------------|
| Patch (x.y.Z) – correctifs       | Hebdomadaire  | Auto Dependabot      | CI seul                     |
| Minor (x.Y.z) – nouvelles fonct. | Mensuel       | PR automatique       | CI + revue manuelle         |
| Major (X.y.z) – breaking changes | Trimestriel   | PR manuelle          | CI + tests manuels + décision |

Configuration Dependabot (.github/dependabot.yml) :

\`\`\`yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/Cdm"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
      timezone: "Europe/Paris"
    groups:
      microsoft-extensions:
        patterns: ["Microsoft.Extensions.*", "Microsoft.AspNetCore.*"]
      aspire:
        patterns: ["Aspire.*", "Microsoft.Extensions.ServiceDiscovery"]
      ef-core:
        patterns: ["Microsoft.EntityFrameworkCore*"]
    labels:
      - "dependencies"
      - "automated"
    commit-message:
      prefix: "chore(deps)"
    open-pull-requests-limit: 10

  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
\`\`\`

Commandes de contrôle :

\`\`\`bash
# Lister les packages avec vulnérabilités
dotnet list Cdm/Cdm.slnx package --vulnerable

# Lister les packages obsolètes
dotnet list Cdm/Cdm.slnx package --outdated

# Vérifier la compatibilité avant mise à jour
dotnet outdated Cdm/ --upgrade --pre-release Never
\`\`\`

Processus de validation après mise à jour :
1. dotnet build Cdm/Cdm.slnx – 0 erreur, 0 warning
2. dotnet test Cdm/Cdm.slnx – tous les tests verts
3. Vérification manuelle des fonctionnalités impactées
4. Mise à jour du CHANGELOG (C4.3.2)

Compétence visée : C4.1.1 – A intégrer dans le dossier écrit.`,
        },
        {
            name: "bloc4_monitoring_plan",
            description: "Génère le plan de supervision et d'alerte avec sondes Aspire + Sentry (C4.1.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Plan de Supervision et Alertes – Chronique des Mondes [C4.1.2]

Architecture de monitoring :

Application .NET 10/Aspire
–> Aspire Dashboard (dev) : métriques, traces, logs
–> Health Checks : endpoint /health
–> Sentry.io (prod) : erreurs, performance, alertes

1. Health Checks Aspire (.NET 10) :

\`\`\`csharp
// Cdm.ServiceDefaults/Extensions.cs
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy())
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DevDatabase")!,
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql"])
        .AddCheck("signalr", () => HealthCheckResult.Healthy(), tags: ["realtime"])
        .AddCheck<MemoryHealthCheck>("memory", tags: ["system"]);

    builder.Services.MapDefaultEndpoints(); // /health, /alive, /ready
    return builder;
}
\`\`\`

2. Seuils d'alerte :

| Sonde          | Métrique           | Seuil WARNING | Seuil CRITIQUE | Action                    |
|----------------|--------------------|---------------|----------------|---------------------------|
| CPU            | Utilisation        | > 70 %        | > 90 %         | Scale up / investigation  |
| RAM            | Utilisation        | > 75 %        | > 90 %         | Vérifier fuites mémoire   |
| Base de données | Temps de réponse  | > 500 ms      | > 2 000 ms     | Optimiser requêtes EF Core |
| API            | Latence p95        | > 200 ms      | > 1 000 ms     | Profiling + cache          |
| SignalR        | Reconnexions/h     | > 10          | > 50           | Vérifier réseau + état    |
| Erreurs 5xx    | Taux               | > 1 %         | > 5 %          | Investigation immédiate   |
| Espace disque  | Utilisation        | > 75 %        | > 90 %         | Purge logs + nettoyage    |

3. Configuration Sentry (production) :

\`\`\`csharp
// Program.cs (Cdm.ApiService + Cdm.Web)
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"]; // via secrets
    options.Environment = builder.Environment.EnvironmentName;
    options.Release = typeof(Program).Assembly.GetName().Version?.ToString();
    options.TracesSampleRate = 0.1; // 10 % des transactions tracées
    options.ProfilesSampleRate = 0.1;
    options.BeforeSend = @event =>
    {
        @event.Request?.Headers?.Remove("Authorization");
        return @event;
    };
});
\`\`\`

4. Health check quotidien (GitHub Actions) :

\`\`\`yaml
# .github/workflows/health-check.yml
name: Daily Health Check
on:
  schedule:
    - cron: '0 8 * * *'
jobs:
  health-check:
    runs-on: ubuntu-latest
    steps:
      - name: Check production health
        run: |
          response=$(curl -s -o /dev/null -w "%{http_code}" https://your-prod-url/health)
          if [ $response -ne 200 ]; then
            echo "::error::Health check failed with status $response"
            exit 1
          fi
\`\`\`

5. Tableau de bord de monitoring :

| Vue             | Outil              | URL                                                              |
|-----------------|--------------------|------------------------------------------------------------------|
| Dev local       | Aspire Dashboard   | https://localhost:17223                                          |
| Prod – erreurs  | Sentry             | https://sentry.io/cdm/                                           |
| Prod – métriques | A définir (Grafana) | –                                                               |
| CI/CD           | GitHub Actions     | https://github.com/Tomtoxi44/Chronique_Des_Mondes/actions        |

Compétence visée : C4.1.2 – Plan de supervision à inclure dans le dossier écrit.`,
        },
        {
            name: "bloc4_bug_report_template",
            description: "Génère un template de fiche de bug / consignation d'anomalie (C4.2.1)",
            parameters: {
                type: "object",
                properties: {
                    bug_title: { type: "string", description: "Titre court du bug (optionnel)" },
                    priority: { type: "string", enum: ["P0", "P1", "P2", "P3"], description: "Priorité du bug" },
                },
            },
            handler: async (args) => {
                const title = args.bug_title || "[Titre du bug]";
                const priority = args.priority || "P2";
                const date = new Date().toLocaleDateString("fr-FR");
                return `# Fiche d'Anomalie – ${title} [C4.2.1]
ID : BUG-${Math.floor(Math.random() * 900) + 100}
Date : ${date}
Priorité : ${priority}

1. Identification :

| Champ             | Valeur                                                                                              |
|-------------------|-----------------------------------------------------------------------------------------------------|
| Titre             | ${title}                                                                                            |
| Priorité          | ${priority} – ${priority === "P0" ? "Bloquant (SLA : 4 h)" : priority === "P1" ? "Critique (SLA : 24 h)" : priority === "P2" ? "Majeur (SLA : 1 sprint)" : "Mineur (Backlog)"} |
| Environnement     | Dev / Staging / Production                                                                          |
| Version           | x.y.z                                                                                               |
| Composant         | API / Web / SignalR / Base de données                                                               |
| Signalé par       |                                                                                                     |
| Date signalement  | ${date}                                                                                             |
| Assigné à         | Tommy ANGIBAUD                                                                                      |

2. Description :

Comportement observé : [description claire et précise du problème]
Comportement attendu : [ce qui devrait se passer]

3. Reproduction :

Prérequis :
- [ ] Compte connecté avec rôle : Cdm-Users / Cdm-GameMasters / Cdm-Admins
- [ ] [Autre prérequis]

Etapes de reproduction :
1.
2.
3.

Fréquence de reproduction :
- [ ] 100 % (reproductible systématiquement)
- [ ] 50 % (reproductible parfois)
- [ ] < 10 % (rare)

4. Analyse technique :

Logs / Stack trace :
[Coller la stack trace ou les logs ici]

Hypothèse de cause racine : [analyse initiale]

5. Correctif :

Branche de correction : fix/BUG-XXX-description
Commit : fix(BUG-XXX): Description du correctif

Test de non-régression ajouté :
- [ ] Test unitaire xUnit : [Nom du test]
- [ ] Test Playwright : [Scénario]

6. Validation :
- [ ] Corrigé en développement : [Date]
- [ ] Validé en staging : [Date]
- [ ] Déployé en production : [Date]
- [ ] Issue GitHub fermée : #[numéro]

Compétence visée : C4.2.1 – Fiche à archiver dans le dossier de maintenance.`;
            },
        },
        {
            name: "bloc4_changelog",
            description: "Génère un CHANGELOG structuré au format SemVer (C4.3.2)",
            parameters: {
                type: "object",
                properties: {
                    version: { type: "string", description: "Version actuelle (ex: 1.0.0)" },
                },
            },
            handler: async (args) => {
                const version = args.version || "0.1.0";
                const date = new Date().toISOString().split("T")[0];
                return `# Changelog – Chronique des Mondes [C4.3.2]

Toutes les modifications notables de ce projet sont documentées dans ce fichier.
Format : Keep a Changelog (https://keepachangelog.com/fr/1.0.0/)
Versionnage : Semantic Versioning (https://semver.org/lang/fr/)

Conventions SemVer :
- MAJOR (X.y.z) : changements incompatibles avec les versions précédentes
- MINOR (x.Y.z) : nouvelles fonctionnalités rétrocompatibles
- PATCH (x.y.Z) : corrections de bogues rétrocompatibles

[A venir]
Ajouté :
- [ ] En cours de développement

[${version}] – ${date}

Ajouté :
- Authentification JWT + BCrypt (work factor 12)
- CRUD Campagnes avec support GameType (Générique / D&D 5e)
- CRUD Personnages avec attributs polymorphiques (JSON)
- Pipeline CI/CD GitHub Actions (build, tests xUnit, OWASP)

Modifié :
- Architecture microservices Aspire (AppHost + ServiceDefaults)
- Migrations EF Core séparées (MigrationsContext + MigrationsManager)

Corrigé :
- Aucun correctif dans cette version initiale

Sécurité :
- Audit OWASP Top 10 initial
- En-têtes de sécurité configurés (CSP, HSTS, X-Frame-Options)

[0.0.1] – ${new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0]}

Ajouté :
- Initialisation du projet .NET 10 + Aspire
- Structure des projets (ApiService, Web, Business, Data, Migrations)
- Configuration de base CI GitHub Actions

Liens :
[A venir] : https://github.com/Tomtoxi44/Chronique_Des_Mondes/compare/v${version}...HEAD
[${version}] : https://github.com/Tomtoxi44/Chronique_Des_Mondes/releases/tag/v${version}

Compétence visée : C4.3.2 – CHANGELOG à maintenir à chaque release. Fichier : CHANGELOG.md à la racine.`;
            },
        },
        {
            name: "bloc4_user_feedback_process",
            description: "Génère le processus de collecte et traitement des retours utilisateurs (C4.3.1 + C4.3.3)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Processus Retours Utilisateurs & Support [C4.3.1 + C4.3.3]

1. Canaux de collecte des retours (C4.3.1) :

| Canal                       | Cible         | Fréquence     | Outil                   |
|-----------------------------|---------------|---------------|-------------------------|
| Formulaire in-app           | Joueurs + MJ  | En continu    | GitHub Issues template  |
| Sessions de beta test       | MJ référents  | Chaque sprint | Entretien direct        |
| Analytics comportementaux   | Tous          | En continu    | Aspire Dashboard        |
| NPS (Net Promoter Score)    | Tous          | Mensuel       | Questionnaire simple    |

2. Template de retour utilisateur (GitHub Issue) :

Type : Bogue | Amélioration | Question
Contexte : [Que faisiez-vous quand ce problème est apparu ?]
Description : [Décrivez votre retour]
Impact : [Sur 5, à quel point est-ce important pour vous ?]
Suggestion : [Avez-vous une idée de solution ?]

3. Processus de traitement (C4.3.3) :

Retour reçu (GitHub Issue)
–> Triage (< 48 h) :
    – Labelliser : bug / enhancement / question / wontfix
    – Priorité : P0/P1/P2/P3
    – Affecter au sprint ou au backlog
–> Réponse à l'utilisateur (< 72 h) :
    – Accusé de réception
    – Estimation de traitement
    – Contournement si possible
–> Traitement :
    – Bogue : fiche anomalie C4.2.1 + correctif C4.2.2
    – Amélioration : US dans backlog (.github/backlog/)
    – Question : documentation mise à jour (C2.4.1)
–> Clôture :
    – Notification de l'utilisateur
    – CHANGELOG mis à jour (C4.3.2)
    – Indicateur de satisfaction (NPS)

4. Indicateurs de satisfaction (KPIs support) :

| KPI                          | Cible  | Mesure                         |
|------------------------------|--------|--------------------------------|
| Temps de première réponse    | < 48 h | GitHub Issue response time     |
| Taux de résolution           | > 90 % | Issues closed / opened         |
| NPS                          | > 30   | Questionnaire mensuel          |
| Bogues rapportés / sprint    | < 5    | GitHub Issues                  |
| Satisfaction MJ (note /5)    | > 4    | Retours beta test              |

5. FAQ – Questions fréquentes :

Q : Comment rejoindre une campagne ?
R : Le Maître de Jeu vous envoie un lien d'invitation. Créez un compte sur la plateforme et cliquez sur le lien.

Q : Les dés sont-ils vraiment aléatoires ?
R : Oui. Les dés sont générés côté serveur (RNG cryptographique) pour garantir l'équité et prévenir la triche.

Q : Puis-je utiliser des systèmes autres que D&D 5e ?
R : Le mode générique est disponible. D&D 5e est supporté nativement. D'autres systèmes sont prévus (roadmap).

Compétences visées : C4.3.1 + C4.3.3 – Processus à inclure dans le dossier écrit.`,
        },
        {
            name: "bloc4_project_summary",
            description: "Génère la présentation du projet pour le dossier écrit Bloc 4 (section 2 du dossier)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Présentation du Projet [Dossier Bloc 4 – Section 2]

Chronique des Mondes

Chronique des Mondes est une plateforme web de gestion de campagnes de jeu de rôle (JDR)
multi-systèmes. Elle permet aux maîtres de jeu de créer et d'administrer des campagnes, et
aux joueurs de gérer leurs personnages, de participer à des sessions et de jouer des combats
en temps réel depuis un navigateur, sans installation logicielle.

La plateforme supporte plusieurs systèmes de règles, notamment D&D 5e (via le SRD 5.1 OGL)
et un système générique paramétrable. Les dés sont lancés côté serveur pour garantir
l'impartialité des résultats.

Contexte technique :

| Couche            | Technologie                                |
|-------------------|--------------------------------------------|
| Orchestration     | .NET 10 + Aspire AppHost                   |
| Backend API       | ASP.NET Core 10 – API REST + SignalR       |
| Frontend          | Blazor Server                              |
| Temps réel        | SignalR (combat, notifications)            |
| Persistance       | EF Core 10 + SQL Server (dev/prod)         |
| Authentification  | JWT (RS256) + BCrypt (work factor 12)      |
| Tests             | xUnit (unitaires) + Playwright (E2E)       |
| CI/CD             | GitHub Actions + déploiement progressif    |
| Monitoring        | Aspire Dashboard + Sentry + Health Checks  |

Profils utilisateurs :
- Maître de Jeu (MJ) : création et gestion de campagnes, sessions, PNJ, scénarios
- Joueur : création et gestion de personnages, participation aux sessions, combat
- Administrateur : gestion de la plateforme, modération, configuration

Ce projet constitue la réalisation pratique de la certification RNCP 39583 – Expert en
Développement Logiciel, Niveau 7 – YNOV, promotion 2025/2026.`,
        },
    ],
});

await session.log("Bloc 4 – Maintenance charge (C4.1.1 a C4.3.3). Outils : bloc4_project_summary · bloc4_dependency_policy · bloc4_monitoring_plan · bloc4_bug_report_template · bloc4_changelog · bloc4_user_feedback_process");
