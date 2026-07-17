# Audit du code & test fonctionnel — Chronique des Mondes

**Date :** 17 juillet 2026
**Portée :** Solution `.NET 10 / Aspire` (backend Minimal API, frontend Blazor Server, SignalR, EF Core, SQL Server)
**Auteur de l'audit :** revue automatisée du code source + tentative de test fonctionnel

---

## 1. Résumé exécutif

Le projet est **globalement de bonne facture** pour un projet de certification : architecture en couches claire, séparation `Abstraction / Common / Data`, orchestration Aspire, sécurité JWT + BCrypt, catalogue d'erreurs structuré, CI complète (build warnaserror, tests xUnit, OWASP Dependency Check, E2E Playwright). C'est nettement au-dessus de la moyenne d'un projet étudiant.

Les points d'attention les plus sérieux ne sont pas des bugs de compilation mais des **risques de sécurité et de fiabilité en production** : clé JWT par défaut committée, absence de rate-limiting sur l'authentification, et surtout un **« filet de sécurité » SQL brut dans `Program.cs` qui peut `DROP TABLE` en production**. Ce sont les trois chantiers prioritaires.

**Note globale indicative : 7,5 / 10** — solide, mais quelques corrections de sécurité/fiabilité à traiter avant une vraie mise en production.

---

## 2. Synthèse de la criticité

Légende : 🔴 Critique · 🟠 Élevé · 🟡 Moyen · 🟢 Faible / cosmétique

| # | Constat | Domaine | Criticité |
|---|---------|---------|-----------|
| 1 | Schéma SQL patché « à la main » (`DROP TABLE`) au démarrage en Production | Fiabilité / Data | 🔴 Critique |
| 2 | Clé secrète JWT par défaut committée dans `appsettings.json` | Sécurité | 🔴 Critique |
| 3 | Aucune limitation de débit (rate-limiting) sur `/api/auth/*` | Sécurité | 🟠 Élevé |
| 4 | Token JWT stocké dans `localStorage` (exposé au XSS) | Sécurité | 🟠 Élevé |
| 5 | Validation des DTO (DataAnnotations) non appliquée automatiquement côté API | Robustesse | 🟠 Élevé |
| 6 | Autorisation par groupe SignalR non vérifiée (IDOR temps réel possible) | Sécurité | 🟠 Élevé |
| 7 | `catch (Exception)` massif qui avale les erreurs (~150 occurrences) | Maintenabilité | 🟡 Moyen |
| 8 | Clé JWT sur `Encoding.ASCII` + longueur non bloquante | Sécurité | 🟡 Moyen |
| 9 | Couverture de tests partielle (services Combat/Session/Npc/Notification non testés) | Qualité | 🟡 Moyen |
| 10 | Enregistrement DI très répétitif dans `Web/Program.cs` | Maintenabilité | 🟡 Moyen |
| 11 | Absence de pas de CSP / en-têtes de sécurité incomplets | Sécurité | 🟡 Moyen |
| 12 | `AuthTokenHandler` = code mort (non enregistré) | Propreté | 🟢 Faible |
| 13 | Register ne renvoie pas de refresh token (incohérence avec Login) | Cohérence | 🟢 Faible |
| 14 | Fichiers temporaires / artefacts (`~$ssier-bloc2.docx`, `bin/obj`) versionnés | Hygiène repo | 🟢 Faible |

---

## 3. Points forts (ce qui est bien fait)

**Architecture.** Découpage propre en couches et en projets : `Cdm.*.Abstraction` (contrats/DTO), `Cdm.Business.Common` (logique), `Cdm.Data.Common` (EF Core), `Cdm.ApiService` (endpoints), `Cdm.Web` (Blazor). L'orchestration Aspire (`AppHost`) branche migrations → API → front avec des dépendances `WaitFor`/`WaitForCompletion` bien ordonnées. La séparation du mode D&D 5e dans `Cdm.Business.DnD5e` montre une extensibilité pensée (multi-systèmes de jeu).

**Sécurité (les bases sont là).** Mots de passe hachés avec **BCrypt work factor 12**. JWT correctement configuré : validation issuer/audience/lifetime, `ClockSkew = 0`, HMAC-SHA256, expiration 1 h, refresh tokens **révoqués et rotés** à chaque usage, générés via `RandomNumberGenerator` (64 octets). `RequireHttpsMetadata` activé hors développement. En-têtes de sécurité OWASP (X-Frame-Options, X-Content-Type-Options, Referrer-Policy) posés. Autorisation par rôle (`RequireRole("GameMaster")`) sur les endpoints sensibles, **et vérification de propriété** (`campaign.CreatedBy == userId`) dans la couche métier — c'est exactement le bon endroit pour le faire.

**Gestion d'erreurs structurée.** `ErrorCatalog` / `CdmErrorCode` associe chaque code à un statut HTTP et une clé de localisation — pattern propre et industrialisable, réutilisé côté API et Blazor.

**CI/CD mature.** `dotnet build /warnaserror`, tests xUnit avec couverture, **OWASP Dependency Check** (`--failOnCVSS 8`), et **tests E2E Playwright** (auth, navigation, thèmes). Peu de projets étudiants vont aussi loin.

**Internationalisation** (fr/en via `.resx` + `AppStrings`), thématisation centralisée (`ThemeService`), composants partagés réutilisables (`AppEmptyState`, `AppLoadingSpinner`, `DiceRoller`, etc.).

---

## 4. Analyse détaillée & axes d'amélioration

### 4.1 Fiabilité des données — 🔴 le point le plus critique

`Cdm.ApiService/Program.cs` exécute au démarrage, **en Production**, du SQL brut qui, dans certains cas, fait `DROP TABLE [Sessions]` / `[SessionParticipants]` avant de les recréer :

```
IF COL_LENGTH('[dbo].[Sessions]', 'GmUserId') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[SessionParticipants];
    DROP TABLE [dbo].[Sessions];
END
```

C'est un « filet de sécurité » pour réparer un déploiement passé, mais c'est **dangereux** : il court-circuite le système de migrations EF Core, peut détruire des données réelles, et masque la vraie cause (les migrations ne sont pas fiables). Le `catch` avale même l'exception (« allow app to start even if safety net fails »), ce qui peut laisser partir l'app avec un schéma incohérent.

**Recommandation :** supprimer ce bloc, consolider le schéma dans une migration EF Core propre et idempotente, et faire échouer le démarrage (fail-fast) si la migration ne passe pas. Le `Cdm.MigrationsManager` existe déjà pour ça — c'est lui qui doit être la seule source de vérité du schéma.

### 4.2 Sécurité

**Clé JWT par défaut committée (🔴).** `appsettings.json` contient `"SecretKey": "CHANGE-THIS-IN-PRODUCTION-..."`. Le garde-fou `throw if missing` de `Program.cs` **ne se déclenche pas**, puisque la valeur n'est pas nulle — elle est juste non sécurisée. Un déploiement qui oublie de surcharger la variable tournera avec une clé publique et connue de tous (le repo est visible). **Reco :** retirer la valeur du fichier committé, forcer l'injection par variable d'environnement / User Secrets / Key Vault, et faire échouer si la clé est la valeur par défaut ou < 32 caractères (aujourd'hui c'est un simple `LogWarning`).

**Pas de rate-limiting (🟠).** `/api/auth/login` et `/register` n'ont aucune limitation. Brute-force et énumération de comptes possibles. **Reco :** `builder.Services.AddRateLimiter(...)` avec une politique stricte sur le groupe `/api/auth`.

**JWT en `localStorage` (🟠).** Le token est lisible par n'importe quel script → vol de session en cas de XSS. En Blazor Server, un cookie `HttpOnly`/`Secure`/`SameSite` serait plus sûr. À défaut, durcir la CSP pour limiter le risque XSS.

**Validation DTO non appliquée (🟠).** `RegisterRequest` porte une belle politique de mot de passe en DataAnnotations (regex, longueur, confirmation), mais **les Minimal APIs ne valident pas automatiquement** ces attributs. Sauf validation explicite (filtre d'endpoint ou lib type `MinimalApis.Extensions`/FluentValidation), la politique n'est pas garantie côté serveur. **Reco :** ajouter un `AddEndpointFilter` de validation ou FluentValidation sur les endpoints qui reçoivent un body.

**Autorisation SignalR par groupe (🟠).** `CombatHub`/`SessionHub` sont `[Authorize]` (bien), mais `JoinCombat(combatId)` ajoute l'utilisateur au groupe `combat_{id}` **sans vérifier qu'il participe à ce combat/campagne**. Un utilisateur authentifié peut donc s'abonner aux événements temps réel d'une partie qui n'est pas la sienne (IDOR). **Reco :** vérifier l'appartenance en base avant `AddToGroupAsync`.

**Détails 🟡 :** clé signée via `Encoding.ASCII` (préférer UTF8 et surtout une clé ≥ 256 bits) ; en-têtes de sécurité sans `Content-Security-Policy` ni `Permissions-Policy` ; `AllowedHosts: "*"`.

### 4.3 Backend / Data

Le pattern « endpoint extrait `userId` des claims → passe au service → le service vérifie la propriété » est **cohérent et correct** dans `CampaignService`. À généraliser et à tester sur **tous** les services (Character, Session, Npc, Combat).

`catch (Exception)` est omniprésent (~150 occurrences) : il log puis renvoie `null`/`Failure`. Conséquences : erreurs réelles masquées, difficulté de diagnostic, et incohérence de contrat (certains services renvoient `null`, d'autres un `ServiceResult`). **Reco :** ne capturer que les exceptions attendues, laisser remonter le reste vers un middleware d'exception global (déjà présent : `UseExceptionHandler`), et uniformiser sur `ServiceResult<T>`.

28 migrations dont plusieurs « AddMissing… » / « Ensure… » : signe que le schéma a dérivé plusieurs fois. Une fois le point 4.1 traité, envisager de **squasher** les migrations en une baseline propre.

### 4.4 Frontend Blazor

`Web/Program.cs` répète ~13 fois le même bloc `AddScoped<XApiClient> + AddHttpClient("XApiClient")` (≈ 300 lignes). **Reco :** une méthode d'extension générique `AddApiClient<TClient>(name)` réduirait ça à quelques lignes et supprimerait les risques de copier-coller (ex. `ProfileApiClient` enregistré manuellement, incohérences de nommage de clients).

`AuthTokenHandler` (DelegatingHandler propre) **n'est jamais enregistré** : code mort. L'injection du Bearer se fait à la place via `BaseApiClient.AddAuthorizationHeaderAsync()` qui écrit dans `HttpClient.DefaultRequestHeaders` — acceptable en scoped, mais le `DelegatingHandler` serait plus robuste (et thread-safe). **Reco :** brancher `AuthTokenHandler` sur les `HttpClient` via `.AddHttpMessageHandler<AuthTokenHandler>()` et supprimer la logique dupliquée.

`CustomAuthStateProvider` est bien construit (refresh proactif avec buffer de 30 s, parsing JWT sûr). Rappel : les claims côté client sont **cosmétiques** — la vraie autorisation reste côté API, ce qui est le cas ici. 👍

### 4.5 Tests & CI

68 tests xUnit sur 9 services + tests E2E Playwright (auth, navigation, thèmes). Bonne base. **Manques :** services `Combat`, `Session`, `Npc`, `Notification`, `Character`, `DnD*` non couverts ; aucun test d'intégration des endpoints ni des Hubs SignalR. Les tests utilisent `UseInMemoryDatabase`, qui ne reproduit ni les contraintes SQL Server, ni le SQL brut du point 4.1 — donc le risque le plus critique **n'est pas couvert par les tests**. **Reco :** ajouter des tests d'intégration avec `WebApplicationFactory` + base SQL (Testcontainers), et couvrir la vérification de propriété/autorisation de chaque service.

### 4.6 Hygiène du dépôt (🟢)

Présence de fichiers à ne pas versionner : `~$ssier-bloc2.docx` (fichier de verrou Word), et des `bin/`/`obj/` apparaissent dans les recherches (à confirmer dans `.gitignore`). Nettoyage recommandé.

---

## 5. Test fonctionnel — statut

### Ce qui était prévu
Builder l'application en local puis parcourir les scénarios (inscription, connexion, création de monde/campagne, session, combat, lancer de dés) avec un navigateur piloté.

### Blocage rencontré
**Le test fonctionnel n'a pas pu être exécuté dans mon environnement d'exécution.** L'application requiert une stack que le bac à sable Linux ne fournit pas et que je ne peux pas installer (accès réseau restreint) :

- **.NET 10 SDK** — absent, et le script d'installation officiel est bloqué (HTTP 403) ;
- **SQL Server** (LocalDB en dev, Azure SQL en prod) — absent ;
- **Runtime de conteneurs** pour l'orchestration Aspire (`AppHost`) — absent.

Sans instance en cours d'exécution, aucun parcours navigateur n'est possible depuis ici.

### Deux façons de débloquer le test fonctionnel

1. **Tu me fournis l'URL de production** (celle que tu proposais). Je lance alors un vrai test de parcours piloté au navigateur et je te livre un second rapport : ce qui marche, ce qui casse, captures à l'appui.
2. **Tu lances l'app en local** sur ta machine (`dotnet run` sur `Cdm.AppHost`, avec ta clé JWT et SQL Server LocalDB) et tu me donnes l'URL exposée — même déroulé.

Bonne nouvelle : le dépôt contient déjà des **tests E2E Playwright** (`Cdm/.playwright/tests/` : `auth.spec.ts`, `navigation.spec.ts`, `themes.spec.ts`). En attendant, tu peux lancer `dotnet test Cdm/.playwright` en local pour un premier filet fonctionnel automatisé.

---

## 6. Plan d'action priorisé

**Sprint sécurité/fiabilité (à faire avant prod) :**
1. 🔴 Supprimer le SQL brut de `Program.cs` → migration EF Core propre + fail-fast.
2. 🔴 Sortir la clé JWT du fichier committé → variable d'env/secret + rejet de la valeur par défaut.
3. 🟠 Ajouter le rate-limiting sur `/api/auth/*`.
4. 🟠 Appliquer la validation des DTO côté API (filtre/FluentValidation).
5. 🟠 Vérifier l'appartenance avant `AddToGroupAsync` dans les Hubs SignalR.

**Sprint qualité :**
6. 🟠 Migrer le JWT vers un cookie `HttpOnly` (ou durcir la CSP).
7. 🟡 Uniformiser la gestion d'erreurs (`ServiceResult<T>`, moins de `catch(Exception)`).
8. 🟡 Factoriser l'enregistrement des `ApiClient` (`Web/Program.cs`).
9. 🟡 Étendre la couverture de tests (services non couverts + intégration endpoints/Hubs).

**Nettoyage :**
10. 🟢 Supprimer le code mort (`AuthTokenHandler` non branché) et les fichiers temporaires versionnés.

---

*Rapport limité à l'analyse statique du code source. Le volet test fonctionnel dynamique reste à réaliser une fois une instance accessible (voir §5).*
