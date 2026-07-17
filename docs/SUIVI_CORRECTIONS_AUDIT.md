# Suivi des corrections — Audit des 14 points

**Branche :** `fix/audit-14-points`
**Date :** 17 juillet 2026

Ce document trace, point par point, ce qui a été corrigé, comment, et ce qui reste à finaliser côté machine locale (build/tests que je ne peux pas exécuter dans mon environnement).

## Tableau de suivi

| # | Point | Criticité | Statut | Où |
|---|-------|-----------|--------|-----|
| 1 | SQL brut `DROP TABLE` au démarrage | 🔴 | ✅ Corrigé | `Cdm.ApiService/Program.cs` |
| 2 | Clé JWT par défaut committée | 🔴 | ✅ Corrigé | `appsettings.json` + `Program.cs` |
| 3 | Pas de rate-limiting auth | 🟠 | ✅ Corrigé | `Program.cs` + `AuthEndpoints.cs` |
| 4 | JWT en localStorage (XSS) | 🟠 | ⚠️ Mitigé (CSP) | `Cdm.Web/Program.cs` |
| 5 | Validation DTO non appliquée | 🟠 | ✅ Corrigé | `Filters/ValidationFilter.cs` + `AuthEndpoints.cs` |
| 6 | Autorisation groupe SignalR | 🟠 | ✅ Corrigé | `Hubs/CombatHub.cs`, `Hubs/SessionHub.cs` |
| 7 | `catch(Exception)` massif | 🟡 | 📝 Guideline + à dérouler | ce document (§7) |
| 8 | Clé JWT ASCII + longueur non bloquante | 🟡 | ✅ Corrigé | `JwtService.cs` + `Program.cs` |
| 9 | Couverture de tests partielle | 🟡 | 📝 Plan fourni | ce document (§9) |
| 10 | Enregistrement DI répétitif | 🟡 | ✅ Corrigé | `Cdm.Web/Program.cs` |
| 11 | En-têtes de sécurité incomplets (CSP) | 🟡 | ✅ Corrigé | API + Web `Program.cs` |
| 12 | `AuthTokenHandler` = code mort | 🟢 | ⏳ `git rm` à faire | voir §Manuel |
| 13 | Register ne renvoie pas de refresh token | 🟢 | ✅ Corrigé | `RegisterResponse.cs` + `AuthService.cs` |
| 14 | Fichier temporaire versionné | 🟢 | ⏳ `git rm` à faire | voir §Manuel |

Légende : ✅ corrigé dans le code · ⚠️ mitigé (amélioration + suite recommandée) · 📝 documenté avec plan · ⏳ action manuelle requise.

## Détail des corrections de code

**#1 —** Suppression complète du bloc « safety net » SQL brut exécuté en Production (qui pouvait `DROP TABLE Sessions`/`SessionParticipants`). La source de vérité du schéma redevient le `Cdm.MigrationsManager` (migrations EF Core).

**#2 / #8 —** La clé secrète en clair a été retirée de `appsettings.json` (valeur vidée). `Program.cs` et `JwtService.cs` échouent désormais au démarrage (fail-fast) si la clé est absente, reste le placeholder `CHANGE-THIS`, ou fait moins de 32 caractères. La signature utilise `Encoding.UTF8` au lieu de `ASCII`. La clé doit être fournie par variable d'environnement / User Secrets / Key Vault (`Jwt__SecretKey`). La clé de dev reste dans `appsettings.Development.json` (non sensible).

**#3 —** `AddRateLimiter` : fenêtre fixe stricte (10 req/min) sur le groupe `/api/auth` via `.RequireRateLimiting("auth")`, plus un limiteur global par IP (100 req/min). `app.UseRateLimiter()` ajouté au pipeline.

**#4 —** Mitigation XSS via `Content-Security-Policy` côté Web : `connect-src` restreint au même origine (+ websockets), ce qui bloque l'exfiltration du token même en cas d'injection de script. Suite recommandée : migrer le JWT vers un cookie `HttpOnly`/`Secure`/`SameSite` et retirer `'unsafe-inline'` de `script-src` (nonce/hash). À valider lors du test fonctionnel (une CSP trop stricte peut casser Blazor/FluentUI).

**#5 —** Nouveau `ValidationFilter<T>` (endpoint filter) qui applique les DataAnnotations (les Minimal APIs ne le font pas automatiquement). Branché sur `/register` et `/login`. À étendre aux autres endpoints recevant un body.

**#6 —** `CombatHub.JoinCombat` et `SessionHub.JoinSession` vérifient désormais que l'utilisateur appartient à la campagne concernée (GM via `Campaign.CreatedBy`, ou joueur via `SessionParticipant → WorldCharacter → Character.UserId`) avant `AddToGroupAsync`. Accès refusé = `HubException`.

**#10 —** Les 15 configurations identiques de `HttpClient` dans `Cdm.Web/Program.cs` sont factorisées dans un délégué `Action<HttpClient> ConfigureApiClient`.

**#11 —** Ajout de `Content-Security-Policy` et `Permissions-Policy` aux en-têtes de sécurité, côté API et côté Web.

**#13 —** `RegisterResponse` expose `RefreshToken` + `RefreshTokenExpiry`, peuplés dans `AuthService.RegisterAsync` (parité avec le login).

## §7 — Guideline gestion d'erreurs (à dérouler progressivement)

Le pattern actuel `catch (Exception ex) { log; return null/Failure; }` (~150 occurrences) masque les erreurs et rend le diagnostic difficile. Refactor à mener méthodiquement (idéalement couvert par des tests avant/après) :

1. Ne capturer que les exceptions **attendues** (ex. `DbUpdateException`, `TimeoutException`).
2. Laisser remonter les exceptions inattendues vers le middleware global `UseExceptionHandler` (déjà en place) → réponse `ProblemDetails` cohérente.
3. Uniformiser le contrat de retour sur `ServiceResult<T>` (supprimer les retours `null` ambigus).
4. Ne jamais avaler une exception sans au minimum la journaliser **et** signaler l'échec à l'appelant.

Refactor transverse volontairement laissé hors de cette passe car il modifie le comportement runtime de nombreux services et doit être validé par la compilation + les tests (impossibles dans l'environnement d'audit).

## §9 — Plan d'extension des tests

Services non couverts à ce jour : `CombatService`, `SessionService`, `NpcService`, `NotificationService`, `CharacterService`, `DndCharacterService`, `DndReferenceService`, `DndNpcService`.

Priorités :
1. Tests unitaires des services ci-dessus, en miroir du pattern existant (`InMemoryDatabase` + `Moq`), en couvrant notamment la **vérification de propriété/autorisation**.
2. Tests d'intégration des endpoints avec `WebApplicationFactory` + base SQL réelle (Testcontainers) — indispensable pour couvrir ce que `InMemoryDatabase` ne reproduit pas (contraintes, transactions).
3. Tests des Hubs SignalR (dont la nouvelle logique d'autorisation #6).

## Actions manuelles requises (git)

Mon environnement d'exécution ne peut pas supprimer de fichiers du dépôt (le montage bloque `unlink`). À exécuter en local :

```bash
# Point 12 — supprimer le code mort
git rm Cdm/Cdm.Web/Services/AuthTokenHandler.cs

# Point 14 — supprimer le fichier temporaire Word versionné
git rm "~\$ssier-bloc2.docx"
```

## Validation avant merge

```bash
dotnet build Cdm/Cdm.slnx --configuration Release /warnaserror
dotnet test  Cdm/Cdm.slnx --configuration Release
```

Ces deux commandes n'ont pas pu être exécutées côté audit (pas de SDK .NET 10 disponible). Elles constituent le filet de sécurité final : la CI `warnaserror` confirmera qu'aucun `using`/référence n'a été cassé par les modifications.
