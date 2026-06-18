// Extension: bloc2-developpement
// BLOC 2 — Concevoir et développer (Certification Expert en Développement Logiciel YNOV)
// Compétences : C2.1.1, C2.1.2, C2.2.1, C2.2.2, C2.2.3, C2.2.4, C2.3.1, C2.3.2, C2.4.1
// Modalité : Dossier écrit + code source

import { joinSession } from "@github/copilot-sdk/extension";

const BLOC2_CONTEXT = `
## 🛠️ BLOC 2 — Concevoir et Développer (Certification YNOV Expert Dev Logiciel)

Tu assistes un étudiant en certification "Expert(e) en Développement Logiciel" YNOV.
Projet : **Chronique des Mondes** — Plateforme JDR multi-systèmes.
Stack : .NET 10 · Aspire · Blazor Server · SignalR · EF Core · SQL Server · xUnit · Playwright

### Compétences évaluées (dossier écrit + code source) :
- **C2.1.1** Mettre en place les environnements de déploiement/test + suivi qualité
- **C2.1.2** Mettre en place l'intégration continue (CI GitHub Actions)
- **C2.2.1** Prototyper l'application (ergonomie, sécurité)
- **C2.2.2** Écrire des tests unitaires (harnais de test xUnit)
- **C2.2.3** Écrire du code sécurisé (OWASP Top 10), accessible (RGAA/OPQUAST)
- **C2.2.4** Déploiement progressif avec versioning (SemVer)
- **C2.3.1** Rédiger un cahier de recettes (scénarios de tests)
- **C2.3.2** Établir un plan de correction des bogues
- **C2.4.1** Rédiger la documentation technique (déploiement, utilisation, MàJ)

### Standards de code projet :
- Indentation 4 espaces, Allman braces (SA1500)
- this. prefix obligatoire (SA1101)
- XML documentation en anglais sur tous les membres publics
- Logging structuré (jamais de données sensibles)
- BCrypt work factor 12 pour les mots de passe
- Validation server-side avec Data Annotations

Indique toujours la compétence C2.X.X visée et produis des livrables prêts pour le dossier écrit.
`;

const session = await joinSession({
    hooks: {
        onSessionStart: async () => ({ additionalContext: BLOC2_CONTEXT }),
        onUserPromptSubmitted: async (input) => {
            const p = input.prompt.toLowerCase();
            const relevant = ["test","ci","cd","owasp","sécurité","securite","accessibilité","rgaa",
                "déploiement","deploiement","versioning","documentation","bug","bogue","recette",
                "prototype","xunit","playwright","pipeline","github actions","c2.","bloc 2",
                "harnais","semver","opquast"].some(k => p.includes(k));
            if (!relevant) return;
            return {
                additionalContext: `⚡ BLOC 2 actif — Mentionne la compétence C2.X.X visée, applique les standards .NET 10/StyleCop, produis du code ou un livrable prêt pour le dossier écrit.`,
            };
        },
    },
    tools: [
        {
            name: "bloc2_ci_pipeline",
            description: "Génère un pipeline CI/CD GitHub Actions complet pour le projet .NET 10 + Aspire (C2.1.2 + C2.2.4)",
            parameters: {
                type: "object",
                properties: {
                    include_playwright: { type: "boolean", description: "Inclure les tests Playwright (défaut : true)" },
                },
            },
            handler: async (args) => {
                const playwright = args.include_playwright !== false;
                const playwrightJob = playwright ? `
  playwright-tests:
    name: E2E Tests (Playwright)
    runs-on: ubuntu-latest
    needs: build-and-test
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${"${{ env.DOTNET_VERSION }}"}
      - name: Install Playwright browsers
        run: |
          dotnet tool install --global Microsoft.Playwright.CLI || true
          playwright install --with-deps chromium
      - name: Start Aspire AppHost
        run: dotnet run --project Cdm/Cdm.AppHost &
        env:
          ASPIRE_ALLOW_UNSECURED_TRANSPORT: true
      - name: Wait for application
        run: sleep 15
      - name: Run Playwright tests
        run: dotnet test Cdm/.playwright --configuration Release
      - name: Upload Playwright artifacts
        uses: actions/upload-artifact@v4
        if: failure()
        with:
          name: playwright-report
          path: playwright-report/` : "";
                const dotnetVersion = "${{ env.DOTNET_VERSION }}";
                const solutionPath = "${{ env.SOLUTION_PATH }}";
                const githubSha = "${{ github.sha }}";
                const githubRef = "${{ github.ref }}";
                return `# Pipeline CI/CD GitHub Actions [C2.1.2 + C2.2.4]
## Fichier : \`.github/workflows/ci.yml\`

\`\`\`yaml
name: CI/CD — Chronique des Mondes

on:
  push:
    branches: [ main, dev ]
  pull_request:
    branches: [ main, dev ]

env:
  DOTNET_VERSION: '10.0.x'
  SOLUTION_PATH: 'Cdm/Cdm.slnx'

jobs:
  build-and-test:
    name: Build, Test & Analyze
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup .NET ${dotnetVersion}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${dotnetVersion}

      - name: Restore dependencies
        run: dotnet restore ${solutionPath}

      - name: Build (no warnings)
        run: dotnet build ${solutionPath} --no-restore --configuration Release /warnaserror

      - name: Run unit tests (xUnit)
        run: |
          dotnet test ${solutionPath} \\
            --no-build --configuration Release \\
            --collect:"XPlat Code Coverage" \\
            --results-directory ./coverage \\
            --logger "trx;LogFileName=test-results.trx"

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: ./coverage/**

      - name: Check code coverage (>= 70%)
        uses: codecov/codecov-action@v4
        with:
          threshold: 70
${playwrightJob}
  security-scan:
    name: Security Scan (OWASP)
    runs-on: ubuntu-latest
    needs: build-and-test
    steps:
      - uses: actions/checkout@v4
      - name: OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'Chronique-des-Mondes'
          path: 'Cdm/'
          format: 'HTML'
      - name: Upload OWASP report
        uses: actions/upload-artifact@v4
        with:
          name: owasp-report
          path: reports/

  deploy-staging:
    name: Deploy to Staging
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: ${githubRef} == 'refs/heads/dev'
    environment: staging
    steps:
      - uses: actions/checkout@v4
      - name: Build Docker image
        run: docker build -t cdm-api:${githubSha} ./Cdm/Cdm.ApiService
      - name: Deploy to staging
        run: echo "Deploy to staging — à implémenter selon votre infra"
\`\`\`

---
> **Livrable C2.1.2 + C2.2.4** — Pipeline CI/CD complet.`;
            },
        },
        {
            name: "bloc2_owasp_checklist",
            description: "Génère la checklist de sécurité OWASP Top 10 appliquée au projet .NET 10 (C2.2.3)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Checklist Sécurité OWASP Top 10 — Chronique des Mondes [C2.2.3]

## A01 — Contrôle d'accès défaillant
- [ ] Authorization Policies ASP.NET Core pour chaque endpoint (\`IsCampaignOwner\`, \`IsCampaignPlayer\`)
- [ ] Query Filters EF Core pour isolation des données par utilisateur
- [ ] Validation server-side : un joueur ne peut pas modifier un personnage qui ne lui appartient pas
- [ ] **Code** : \`[Authorize(Policy = "IsCampaignOwner")]\` sur tous les endpoints sensibles

## A02 — Défaillances cryptographiques
- [ ] BCrypt work factor 12 pour tous les mots de passe (\`BCrypt.Net.BCrypt.HashPassword(pwd, 12)\`)
- [ ] HTTPS forcé (HSTS) en production
- [ ] Pas de données sensibles dans les logs (userId, campagneId OK — jamais password/token)
- [ ] JWT signé avec clé asymétrique RS256 (pas HS256 en prod)

## A03 — Injection
- [ ] EF Core paramétré par défaut — vérifier TOUTES les requêtes raw SQL
- [ ] Validation des entrées avec Data Annotations + FluentValidation
- [ ] Sanitisation des données JSON polymorphiques (\`AttributesJson\`)
- [ ] **Code** : Jamais de \`context.Database.ExecuteSqlRaw($"... {userInput}")\`

## A04 — Conception non sécurisée
- [ ] Dés lancés côté serveur uniquement (anti-triche)
- [ ] Rate limiting sur les endpoints d'authentification
- [ ] Validation GameType côté serveur (pas seulement header)

## A05 — Configuration de sécurité incorrecte
- [ ] Suppression des pages d'erreur détaillées en prod (\`app.UseExceptionHandler\`)
- [ ] Headers sécurité : CSP, X-Frame-Options, X-Content-Type-Options
- [ ] Aspire Dashboard inaccessible en production
- [ ] \`.env\` / secrets via User Secrets ou Azure Key Vault — jamais dans le code

## A06 — Composants vulnérables et obsolètes
- [ ] OWASP Dependency Check dans CI (voir pipeline C2.1.2)
- [ ] \`dotnet list package --vulnerable\` à chaque sprint
- [ ] Politique de mise à jour des dépendances documentée (C4.1.1)

## A07 — Défaillances d'authentification
- [ ] Expiration JWT configurable (1h access / 7j refresh)
- [ ] Révocation des tokens (blacklist Redis ou rotation)
- [ ] Limitation des tentatives de connexion (brute force protection)

## A08 — Défaillances d'intégrité
- [ ] Validation des données reçues via SignalR (mêmes règles que l'API REST)
- [ ] Vérification intégrité des mises à jour progressives (C2.2.4)

## A09 — Défaillances de journalisation
- [ ] Logging structuré Serilog avec contexte métier (UserId, CampaignId, Action)
- [ ] Pas d'interpolation de string dans les logs
- [ ] Alertes sur erreurs 500 et tentatives d'accès non autorisées (C4.1.2)

## A10 — Falsification de requête côté serveur (SSRF)
- [ ] Validation des URLs dans les intégrations externes
- [ ] Whitelist des domaines autorisés pour les ressources externes

## Accessibilité RGAA / OPQUAST
- [ ] Contrastes couleurs >= 4.5:1 (WCAG AA)
- [ ] Navigation clavier complète (Tab, Enter, Escape)
- [ ] Attributs ARIA sur les composants Blazor dynamiques
- [ ] Tests Playwright accessibilité avec axe-core

---
> **Livrable C2.2.3** — Audit de sécurité + accessibilité à intégrer dans le dossier écrit.`,
        },
        {
            name: "bloc2_test_recipe",
            description: "Génère un cahier de recettes avec scénarios de tests fonctionnels (C2.3.1)",
            parameters: {
                type: "object",
                properties: {
                    feature: {
                        type: "string",
                        description: "Fonctionnalité à tester : 'auth' | 'campaign' | 'character' | 'combat'",
                        enum: ["auth", "campaign", "character", "combat"],
                    },
                },
            },
            handler: async (args) => {
                const feature = args.feature || "auth";
                const recipes = {
                    auth: `## Authentification (JWT + BCrypt)
| ID | Scénario | Préconditions | Étapes | Résultat attendu | Statut |
|----|----------|---------------|--------|-----------------|--------|
| CA-001 | Connexion utilisateur valide | Compte existant | 1. POST /api/auth/login avec identifiants valides | JWT retourné, 200 OK | ⬜ |
| CA-002 | Connexion mot de passe incorrect | Compte existant | 1. POST /api/auth/login avec mauvais mdp | 401 Unauthorized, message générique | ⬜ |
| CA-003 | Token expiré | Token > 1h | 1. Appel API avec token expiré | 401 Unauthorized + refresh proposé | ⬜ |
| CA-004 | Accès ressource non autorisée | Connecté en tant que joueur | 1. Tenter modification campagne d'un autre MJ | 403 Forbidden | ⬜ |
| CA-005 | Brute force protection | Aucune | 1. 5 tentatives erronées | Blocage temporaire + log sécurité | ⬜ |`,
                    campaign: `## Gestion des Campagnes
| ID | Scénario | Préconditions | Étapes | Résultat attendu | Statut |
|----|----------|---------------|--------|-----------------|--------|
| CC-001 | Créer une campagne D&D 5e | Connecté en tant que MJ | 1. POST /api/campaigns avec GameType=DnD | Campagne créée, 201 Created | ⬜ |
| CC-002 | Lister mes campagnes | MJ avec campagnes | 1. GET /api/campaigns | Uniquement les campagnes du MJ | ⬜ |
| CC-003 | Inviter un joueur | Campagne active, joueur existant | 1. POST /api/campaigns/{id}/invite | Invitation envoyée, joueur notifié SignalR | ⬜ |
| CC-004 | Compatibilité GameType | Personnage D&D dans campagne Skyrim | 1. Tenter d'ajouter personnage incompatible | 400 Bad Request + message explicite | ⬜ |`,
                    character: `## Gestion des Personnages
| ID | Scénario | Préconditions | Étapes | Résultat attendu | Statut |
|----|----------|---------------|--------|-----------------|--------|
| CH-001 | Créer personnage D&D 5e | Connecté en tant que joueur | 1. POST /api/characters avec attributs D&D JSON | Personnage créé avec stats calculées | ⬜ |
| CH-002 | Validation attributs D&D | Personnage en cours | 1. Attribut hors limites (STR > 30) | 400 Bad Request + erreur de validation | ⬜ |
| CH-003 | Attribution sort à personnage | Perso D&D, sort D&D | 1. POST /api/characters/{id}/spells | Sort ajouté, niveau validé | ⬜ |
| CH-004 | Accès perso d'un autre joueur | Connecté en tant que joueur B | 1. GET /api/characters/{id_joueurA} | 403 Forbidden (Query Filter) | ⬜ |`,
                    combat: `## Système de Combat (SignalR)
| ID | Scénario | Préconditions | Étapes | Résultat attendu | Statut |
|----|----------|---------------|--------|-----------------|--------|
| CO-001 | Rejoindre un combat | Session active | 1. WS : JoinCombat(sessionId, combatId) | Ajouté au groupe SignalR, état reçu | ⬜ |
| CO-002 | Lancer un dé (côté serveur) | Tour du joueur | 1. WS : RollDice("1d20+5") | Résultat calculé côté serveur, broadcast groupe | ⬜ |
| CO-003 | Anti-triche dés | Tentative manipulation | 1. Envoyer résultat de dé depuis client | Ignoré, seul le serveur calcule | ⬜ |
| CO-004 | Déconnexion en combat | Joueur connecté | 1. Couper la connexion | État combat préservé, reconnexion possible | ⬜ |
| CO-005 | Notification tour suivant | Combat actif | 1. MJ passe au tour suivant | Tous les joueurs notifiés via PlayerTurnNotification | ⬜ |`,
                };
                return `# Cahier de Recettes — Chronique des Mondes [C2.3.1]
## Module : ${feature}

${recipes[feature]}

## Critères de validation
- ✅ Tests manuels validés par le MJ/joueur référent
- ✅ Tests automatisés Playwright couvrant les scénarios UI
- ✅ Tests unitaires xUnit couvrant la logique métier
- ✅ Aucune régression sur les scénarios existants

---
> **Livrable C2.3.1** — À compléter et faire valider lors des sprints de recette.`;
            },
        },
        {
            name: "bloc2_unit_test_template",
            description: "Génère un template de tests unitaires xUnit pour un service .NET 10 (C2.2.2)",
            parameters: {
                type: "object",
                properties: {
                    service_name: { type: "string", description: "Nom du service à tester (ex: CharacterService, CampaignService)" },
                },
                required: ["service_name"],
            },
            handler: async (args) => {
                const svc = args.service_name || "CharacterService";
                return `# Template Tests Unitaires xUnit — ${svc} [C2.2.2]
## Fichier : \`Cdm.Business.Common.Tests/${svc}Tests.cs\`

\`\`\`csharp
// ${svc}Tests.cs — Unit tests for ${svc}
// Certification YNOV C2.2.2 — Test harness

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Cdm.Business.Common.Services;
using Cdm.Business.Abstraction;
using Cdm.Data.Common.Models;
using Cdm.Common;

namespace Cdm.Business.Common.Tests;

/// <summary>
/// Unit tests for <see cref="${svc}"/>.
/// </summary>
public sealed class ${svc}Tests : IDisposable
{
    private readonly Mock<I${svc.replace("Service", "Repository")}> this.repositoryMock;
    private readonly Mock<ILogger<${svc}>> this.loggerMock;
    private readonly ${svc} this.sut; // System Under Test

    /// <summary>Initializes a new instance of the <see cref="${svc}Tests"/> class.</summary>
    public ${svc}Tests()
    {
        this.repositoryMock = new Mock<I${svc.replace("Service", "Repository")}>();
        this.loggerMock = new Mock<ILogger<${svc}>>();
        this.sut = new ${svc}(
            this.repositoryMock.Object,
            this.loggerMock.Object);
    }

    // ─── Happy Path Tests ────────────────────────────────────────────────────

    /// <summary>Should return entity when ID exists.</summary>
    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var entityId = 1;
        var expected = new /* TODO: Entity type */ { Id = entityId };
        this.repositoryMock
            .Setup(r => r.GetByIdAsync(entityId))
            .ReturnsAsync(expected);

        // Act
        var result = await this.sut.GetByIdAsync(entityId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entityId);
    }

    // ─── Edge Case Tests ──────────────────────────────────────────────────────

    /// <summary>Should throw when ID is invalid.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_WhenIdIsInvalid_ThrowsArgumentException(int invalidId)
    {
        // Act & Assert
        await this.sut.Invoking(s => s.GetByIdAsync(invalidId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ID*");
    }

    /// <summary>Should throw when entity not found.</summary>
    [Fact]
    public async Task GetByIdAsync_WhenEntityNotFound_ThrowsNotFoundException()
    {
        // Arrange
        this.repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((object)null);

        // Act & Assert
        await this.sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ─── Security Tests (C2.2.3) ─────────────────────────────────────────────

    /// <summary>Should deny access when user is not authorized.</summary>
    [Fact]
    public async Task GetByIdAsync_WhenUserNotAuthorized_ThrowsUnauthorizedException()
    {
        // Arrange — simulate a user trying to access another user's entity
        var entity = new /* TODO: Entity type */ { Id = 1, UserId = 99 }; // Different user
        this.repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        var currentUserId = 1; // Not the owner

        // Act & Assert
        await this.sut.Invoking(s => s.GetByIdAsync(1, currentUserId))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Cleanup if needed
    }
}
\`\`\`

## Commande d'exécution
\`\`\`bash
dotnet test Cdm/Cdm.slnx --filter "FullyQualifiedName~${svc}Tests" --logger "console;verbosity=detailed"
\`\`\`

---
> **Livrable C2.2.2** — Harnais de tests unitaires xUnit pour le dossier écrit.`;
            },
        },
        {
            name: "bloc2_bug_fix_plan",
            description: "Génère un plan de correction des bogues structuré (C2.3.2)",
            parameters: { type: "object", properties: {} },
            handler: async () => `# Plan de Correction des Bogues — Chronique des Mondes [C2.3.2]

## Processus de gestion des bogues

### 1. Signalement (Fiche de bug)
\`\`\`markdown
## 🐛 Bug Report — [ID]: [Titre court]
**Priorité** : P0 (Bloquant) | P1 (Critique) | P2 (Majeur) | P3 (Mineur)
**Environnement** : Dev | Staging | Production
**Version** : x.y.z

### Description
[Description claire du comportement observé]

### Étapes de reproduction
1. ...
2. ...

### Comportement attendu vs Observé
- **Attendu** : ...
- **Observé** : ...

### Logs / Stack trace
\`\`\`

### 2. Triage et priorisation
| Priorité | Critère | SLA |
|----------|---------|-----|
| P0 — Bloquant | Application inaccessible, perte de données | Correction en 4h |
| P1 — Critique | Fonctionnalité principale cassée (combat, auth) | Correction en 24h |
| P2 — Majeur | Fonctionnalité secondaire dégradée | Correction en 1 sprint |
| P3 — Mineur | UI/UX, cosmétique | Backlog |

### 3. Processus de correction
\`\`\`
Bug signalé
    ↓
Reproductible ? → Non → Demander plus d'infos → Fermer si pas reproductible
    ↓ Oui
Créer issue GitHub + label "bug" + priorité
    ↓
Branche : fix/BUG-XXX-description
    ↓
Corriger + test unitaire xUnit pour non-régression
    ↓
CI pipeline passe (build + tests + OWASP)
    ↓
PR vers dev → Review → Merge
    ↓
Déploiement staging → Validation recette
    ↓
Release note mise à jour (C4.3.2)
    ↓
Clôture issue GitHub
\`\`\`

### 4. Template de commit pour correctif
\`\`\`
fix(BUG-XXX): Description courte du correctif

- Cause racine : ...
- Solution appliquée : ...
- Tests ajoutés : [nom du test xUnit]
- Closes #XXX

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
\`\`\`

---
> **Livrable C2.3.2** — Plan de correction à inclure dans le dossier écrit.`,
        },
    ],
});

await session.log("✅ Bloc 2 — Développement chargé (C2.1.1→C2.4.1). Outils : bloc2_ci_pipeline · bloc2_owasp_checklist · bloc2_test_recipe · bloc2_unit_test_template · bloc2_bug_fix_plan");
