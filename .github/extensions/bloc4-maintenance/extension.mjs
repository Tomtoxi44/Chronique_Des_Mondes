// Extension: bloc4-maintenance
// BLOC 4 — Maintenir en condition opérationnelle (Certification Expert en Développement Logiciel YNOV)
// Compétences : C4.1.1, C4.1.2, C4.2.1, C4.2.2, C4.3.1, C4.3.2, C4.3.3
// Modalité : Dossier écrit

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC4_CONTEXT = `
## 🔧 BLOC 4 — Maintenir en Condition Opérationnelle (Certification YNOV Expert Dev Logiciel)

Tu assistes un étudiant en certification "Expert(e) en Développement Logiciel" YNOV.
Projet : **Chronique des Mondes** — Plateforme JDR multi-systèmes.
Stack : .NET 10 · Aspire · Blazor Server · GitHub Actions CI/CD

### Compétences évaluées (dossier écrit) :
- **C4.1.1** Gérer les mises à jour des dépendances (NuGet packages)
- **C4.1.2** Mettre en place un système de supervision/alerte (sondes, seuils)
- **C4.2.1** Consigner les anomalies (fiche de bug structurée)
- **C4.2.2** Mettre en place un correctif CI/CD (hotfix pipeline)
- **C4.3.1** Identifier les axes d'amélioration (retours utilisateurs)
- **C4.3.2** Tenir un journal des versions (CHANGELOG)
- **C4.3.3** Collaborer avec le support client

### Livrables attendus par le jury :
1. Politique de gestion des dépendances (Renovate/Dependabot)
2. Plan de supervision (Aspire health checks, Sentry, alertes)
3. Modèle de fiche de bug
4. Pipeline de hotfix CI/CD
5. Processus de collecte des retours utilisateurs
6. CHANGELOG structuré (SemVer)
7. Procédure support client (FAQ, tickets)

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
                additionalContext: `⚡ BLOC 4 actif — Mentionne la compétence C4.X.X visée, produis un livrable structuré pour le dossier écrit, ancre dans le contexte .NET 10/Aspire.`,
            };
        },
    },
    tools: [
        {
            name: "bloc4_dependency_policy",
            description: "Génère la politique de gestion des dépendances NuGet + config Dependabot (C4.1.1)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Politique de Gestion des Dépendances — Chronique des Mondes [C4.1.1]

## Stratégie de mise à jour

### Politique SemVer
| Type | Fréquence | Automatisation | Validation requise |
|------|-----------|---------------|-------------------|
| Patch (x.y.Z) — bugfixes | Hebdomadaire | ✅ Auto Dependabot | CI seul |
| Minor (x.Y.z) — nouvelles fonctionnalités | Mensuel | ✅ PR automatique | CI + review manuelle |
| Major (X.y.z) — breaking changes | Trimestriel | ⚠️ PR manuelle | CI + tests manuels + décision |

### Configuration Dependabot
\`\`\`yaml
# .github/dependabot.yml
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

### Commandes de contrôle
\`\`\`bash
# Lister les packages avec vulnérabilités
dotnet list Cdm/Cdm.slnx package --vulnerable

# Lister les packages obsolètes
dotnet list Cdm/Cdm.slnx package --outdated

# Vérifier la compatibilité avant mise à jour
dotnet outdated Cdm/ --upgrade --pre-release Never
\`\`\`

### Processus de validation après mise à jour
1. \`dotnet build Cdm/Cdm.slnx\` — 0 erreur, 0 warning
2. \`dotnet test Cdm/Cdm.slnx\` — tous les tests verts
3. Vérification manuelle des fonctionnalités impactées
4. Mise à jour du CHANGELOG (C4.3.2)

---
> **Livrable C4.1.1** — À intégrer dans le dossier écrit.`,
        },
        {
            name: "bloc4_monitoring_plan",
            description: "Génère le plan de supervision et d'alerte avec sondes Aspire + Sentry (C4.1.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Plan de Supervision et Alertes — Chronique des Mondes [C4.1.2]

## Architecture de monitoring

\`\`\`
Application .NET 10/Aspire
        │
        ├── Aspire Dashboard (dev) — Métriques, traces, logs
        ├── Health Checks — /health endpoint
        └── Sentry.io (prod) — Erreurs, performance, alertes
\`\`\`

## 1. Health Checks Aspire (.NET 10)

### Configuration
\`\`\`csharp
// Cdm.ServiceDefaults/Extensions.cs
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        // Base health check
        .AddCheck("self", () => HealthCheckResult.Healthy())
        // Database connectivity
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DevDatabase")!,
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql"])
        // SignalR hub availability
        .AddCheck("signalr", () => HealthCheckResult.Healthy(), tags: ["realtime"])
        // Memory usage
        .AddCheck<MemoryHealthCheck>("memory", tags: ["system"]);
    
    builder.Services.MapDefaultEndpoints(); // /health, /alive, /ready
    return builder;
}
\`\`\`

## 2. Seuils d'alerte

| Sonde | Métrique | Seuil WARNING | Seuil CRITICAL | Action |
|-------|----------|---------------|----------------|--------|
| CPU | Utilisation | > 70% | > 90% | Scale up / investigate |
| RAM | Utilisation | > 75% | > 90% | Vérifier leaks mémoire |
| Base de données | Temps réponse | > 500ms | > 2000ms | Optimiser requêtes EF Core |
| API | Latence p95 | > 200ms | > 1000ms | Profile + cache |
| SignalR | Reconnexions/h | > 10 | > 50 | Vérifier réseau + état |
| Erreurs 5xx | Taux | > 1% | > 5% | Investigation immédiate |
| Espace disque | Utilisation | > 75% | > 90% | Purge logs + cleanup |

## 3. Configuration Sentry (production)

\`\`\`csharp
// Program.cs (Cdm.ApiService + Cdm.Web)
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"]; // Via secrets
    options.Environment = builder.Environment.EnvironmentName;
    options.Release = typeof(Program).Assembly.GetName().Version?.ToString();
    options.TracesSampleRate = 0.1; // 10% des transactions tracées
    options.ProfilesSampleRate = 0.1;
    // Ne jamais logger les données sensibles
    options.BeforeSend = @event =>
    {
        // Supprimer les headers Authorization des rapports
        @event.Request?.Headers?.Remove("Authorization");
        return @event;
    };
});
\`\`\`

## 4. Alertes automatiques

### GitHub Actions — Health Check quotidien
\`\`\`yaml
# .github/workflows/health-check.yml
name: Daily Health Check
on:
  schedule:
    - cron: '0 8 * * *'  # Tous les jours à 8h
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

## 5. Dashboard de monitoring

| Vue | Outil | URL |
|-----|-------|-----|
| Dev local | Aspire Dashboard | https://localhost:17223 |
| Prod erreurs | Sentry | https://sentry.io/cdm/ |
| Prod métriques | À définir (Grafana/DataDog) | — |
| CI/CD status | GitHub Actions | https://github.com/Tomtoxi44/Chronique_Des_Mondes/actions |

---
> **Livrable C4.1.2** — Plan de supervision à inclure dans le dossier écrit.`,
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
                return `# 🐛 Fiche d'Anomalie — ${title} [C4.2.1]
## ID : BUG-${Math.floor(Math.random() * 900) + 100}
## Date : ${date}
## Priorité : ${priority}

## 1. Identification
| Champ | Valeur |
|-------|--------|
| Titre | ${title} |
| Priorité | ${priority} — ${priority === "P0" ? "Bloquant (SLA: 4h)" : priority === "P1" ? "Critique (SLA: 24h)" : priority === "P2" ? "Majeur (SLA: 1 sprint)" : "Mineur (Backlog)"} |
| Environnement | Dev / Staging / Production |
| Version | x.y.z |
| Composant | API / Web / SignalR / Base de données |
| Signalé par | |
| Date signalement | ${date} |
| Assigné à | Tommy ANGIBAUD |

## 2. Description
### Comportement observé
[Description claire et précise du problème]

### Comportement attendu
[Ce qui devrait se passer]

## 3. Reproduction
### Prérequis
- [ ] Compte connecté avec rôle : [Cdm-Users / Cdm-GameMasters / Cdm-Admins]
- [ ] [Autre prérequis]

### Étapes de reproduction
1. 
2. 
3. 

### Fréquence de reproduction
- [ ] 100% (reproductible systématiquement)
- [ ] 50% (reproductible parfois)
- [ ] < 10% (rare)

## 4. Analyse technique
### Logs / Stack trace
\`\`\`
[Coller la stack trace ou les logs ici]
\`\`\`

### Hypothèse de cause racine
[Analyse initiale de la cause]

## 5. Correctif
### Branch de correction
\`feature/fix/BUG-XXX-description\`

### Commit
\`fix(BUG-XXX): Description du correctif\`

### Test de non-régression ajouté
- [ ] Test unitaire xUnit : \`[Nom du test]\`
- [ ] Test Playwright : \`[Scénario]\`

## 6. Validation
- [ ] Corrigé en développement : [Date]
- [ ] Validé en staging : [Date]
- [ ] Déployé en production : [Date]
- [ ] Issue GitHub fermée : #[numéro]

---
> **Livrable C4.2.1** — Fiche à archiver dans le dossier de maintenance.`;
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
                return `# Changelog — Chronique des Mondes [C4.3.2]

Toutes les modifications notables de ce projet seront documentées dans ce fichier.

Format basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/)
et respecte [Semantic Versioning](https://semver.org/lang/fr/).

## Conventions SemVer
- **MAJOR** (X.y.z) : Changements incompatibles avec les versions précédentes
- **MINOR** (x.Y.z) : Nouvelles fonctionnalités rétrocompatibles
- **PATCH** (x.y.Z) : Corrections de bugs rétrocompatibles

---

## [Unreleased]
### Ajouté
- [ ] En cours de développement

---

## [${version}] — ${date}

### ✨ Ajouté
- Authentification JWT + BCrypt (work factor 12)
- CRUD Campagnes avec support GameType (Générique / D&D 5e)
- CRUD Personnages avec attributs polymorphiques (JSON)
- Pipeline CI/CD GitHub Actions (build, tests xUnit, OWASP)

### 🔧 Modifié
- Architecture microservices Aspire (AppHost + ServiceDefaults)
- Migrations EF Core séparées (MigrationsContext + MigrationsManager)

### 🐛 Corrigé
- Aucun correctif dans cette version initiale

### 🔒 Sécurité
- Audit OWASP Top 10 initial
- Headers sécurité configurés (CSP, HSTS, X-Frame-Options)

---

## [0.0.1] — ${new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0]}
### Ajouté
- Initialisation du projet .NET 10 + Aspire
- Structure des projets (ApiService, Web, Business, Data, Migrations)
- Configuration de base CI GitHub Actions

---

## Liens
[Unreleased]: https://github.com/Tomtoxi44/Chronique_Des_Mondes/compare/v${version}...HEAD
[${version}]: https://github.com/Tomtoxi44/Chronique_Des_Mondes/releases/tag/v${version}

---
> **Livrable C4.3.2** — CHANGELOG à maintenir à chaque release. Fichier : \`CHANGELOG.md\` à la racine.`;
            },
        },
        {
            name: "bloc4_user_feedback_process",
            description: "Génère le processus de collecte et traitement des retours utilisateurs (C4.3.1 + C4.3.3)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Processus Retours Utilisateurs & Support [C4.3.1 + C4.3.3]

## 1. Canaux de collecte des retours (C4.3.1)

| Canal | Cible | Fréquence | Outil |
|-------|-------|-----------|-------|
| Formulaire in-app | Joueurs + MJ | En continu | GitHub Issues template |
| Sessions beta test | MJ référents | Chaque sprint | Entretien direct |
| Analytics comportementaux | Tous | Continu | Aspire Dashboard |
| NPS (Net Promoter Score) | Tous | Mensuel | Survey simple |

## 2. Template de retour utilisateur (GitHub Issue)

\`\`\`markdown
## 💬 Retour Utilisateur / Feature Request

**Type** : Bug 🐛 | Amélioration ✨ | Question ❓

**Contexte** : [Que faisiez-vous quand ce problème est apparu ?]

**Description** : [Décrivez votre retour]

**Impact** : [Sur 5, à quel point c'est important pour vous ?] ⭐⭐⭐⭐⭐

**Suggestion** : [Avez-vous une idée de solution ?]
\`\`\`

## 3. Processus de traitement (C4.3.3)

\`\`\`
Retour reçu (GitHub Issue)
        │
        ↓
Triage (< 48h) :
  - Labelliser : bug / enhancement / question / wontfix
  - Priorité : P0/P1/P2/P3
  - Affecter au sprint ou backlog
        │
        ↓
Réponse à l'utilisateur (< 72h)
  - Accusé de réception
  - Estimation de traitement
  - Workaround si possible
        │
        ↓
Traitement :
  - Bug → Fiche anomalie C4.2.1 + correctif C4.2.2
  - Amélioration → US dans backlog (.github/backlog/)
  - Question → Documentation mise à jour (C2.4.1)
        │
        ↓
Clôture :
  - Notification utilisateur
  - CHANGELOG mis à jour (C4.3.2)
  - Indicateur satisfaction (NPS)
\`\`\`

## 4. Indicateurs de satisfaction (KPIs support)

| KPI | Cible | Mesure |
|-----|-------|--------|
| Temps de première réponse | < 48h | GitHub Issue response time |
| Taux de résolution | > 90% | Issues closed / opened |
| NPS | > 30 | Survey mensuel |
| Bugs rapportés / sprint | < 5 | GitHub Issues |
| Satisfaction MJ (note /5) | > 4 | Beta test feedback |

## 5. FAQ Support — Questions fréquentes

**Q: Comment rejoindre une campagne ?**
R: Le Maître de Jeu vous envoie un lien d'invitation. Créez un compte sur la plateforme et cliquez sur le lien.

**Q: Mes dés sont-ils vraiment aléatoires ?**
R: Oui, les dés sont générés côté serveur (RNG cryptographique) pour garantir l'équité et prévenir la triche.

**Q: Puis-je utiliser des systèmes autres que D&D 5e ?**
R: Le mode générique est disponible. D&D 5e est supporté nativement. D'autres systèmes sont prévus (roadmap).

---
> **Livrables C4.3.1 + C4.3.3** — Processus à inclure dans le dossier écrit.`,
        },
    ],
});

await session.log("✅ Bloc 4 — Maintenance chargé (C4.1.1→C4.3.3). Outils : bloc4_dependency_policy · bloc4_monitoring_plan · bloc4_bug_report_template · bloc4_changelog · bloc4_user_feedback_process");
