# US-006 - Gestion Multi-Rôles (Joueur/MJ)

## 📝 Description

**En tant qu'** utilisateur  
**Je veux** pouvoir avoir plusieurs rôles (Joueur et/ou Maître du Jeu)  
**Afin de** participer aux parties en tant que joueur et créer mes propres campagnes

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Un utilisateur peut avoir un ou plusieurs rôles simultanément
- [ ] Rôles disponibles : Player, GameMaster
- [ ] Attribution rôle "Player" par défaut à l'inscription
- [ ] Possibilité de demander le rôle "GameMaster" (validation manuelle ou auto)
- [ ] Affichage des rôles actifs sur le profil utilisateur
- [ ] Filtrage des fonctionnalités selon les rôles :
  - [ ] Player : Créer personnages, rejoindre parties
  - [ ] GameMaster : Créer campagnes, inviter joueurs, gérer sessions
- [ ] Un utilisateur avec les deux rôles peut alterner entre les vues
- [ ] Badge visuel indiquant le rôle actif (Player/MJ)

### Techniques
- [ ] Table `UserRoles` (relation N-N entre `Users` et `Roles`)
- [ ] Table `Roles` avec rôles prédéfinis : Player, GameMaster
- [ ] JWT contient tous les rôles de l'utilisateur
- [ ] Attribut `[Authorize(Roles = "GameMaster")]` sur endpoints MJ
- [ ] Attribut `[Authorize(Roles = "Player,GameMaster")]` sur endpoints mixtes
- [ ] Endpoint : `POST /api/users/request-gamemaster-role`
- [ ] Endpoint : `GET /api/users/my-roles`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `UserService.AssignRole_WithValidRole_AddsToUser()`
- [ ] `UserService.GetUserRoles_WithMultipleRoles_ReturnsAllRoles()`
- [ ] `JwtService.GenerateToken_WithMultipleRoles_IncludesAllRolesClaims()`
- [ ] `AuthorizationService.HasRole_WithGameMaster_ReturnsTrue()`

### Tests d'Intégration
- [ ] `GameMasterEndpoint_WithPlayerRole_Returns403()`
- [ ] `GameMasterEndpoint_WithGameMasterRole_Returns200()`
- [ ] `RequestGameMasterRole_AddsRoleToUser()`

### Tests E2E
- [ ] Inscription → Rôle Player auto → Création personnage OK
- [ ] Demande rôle MJ → Création campagne OK
- [ ] Utilisateur avec 2 rôles → Accès toutes fonctionnalités

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer table `Roles`
  - [ ] Id (PK, int)
  - [ ] Name (string : "Player", "GameMaster", "Admin")
  - [ ] CreatedAt
- [ ] Créer table `UserRoles` (junction table)
  - [ ] UserId (FK → Users)
  - [ ] RoleId (FK → Roles)
  - [ ] AssignedAt (DateTime)
  - [ ] PK composite (UserId, RoleId)
- [ ] Seed des rôles dans migration :
  ```csharp
  migrationBuilder.InsertData(
      table: "Roles",
      columns: new[] { "Id", "Name" },
      values: new object[,] {
          { 1, "Player" },
          { 2, "GameMaster" },
          { 3, "Admin" }
      }
  );
  ```
- [ ] Modifier `AuthService.RegisterAsync()` :
  - [ ] Assigner rôle "Player" par défaut
- [ ] Créer `UserService.RequestGameMasterRoleAsync()`
  - [ ] Vérifier utilisateur n'a pas déjà le rôle
  - [ ] Assigner rôle "GameMaster"
  - [ ] Logger l'attribution
- [ ] Créer `UserService.GetUserRolesAsync(userId)`
- [ ] Modifier `JwtService.GenerateToken()` :
  - [ ] Ajouter tous les rôles comme claims
  ```csharp
  foreach (var role in user.Roles)
  {
      claims.Add(new Claim(ClaimTypes.Role, role.Name));
  }
  ```
- [ ] Créer endpoints :
  - [ ] `POST /api/users/request-gamemaster-role` [Authorize]
  - [ ] `GET /api/users/my-roles` [Authorize]
- [ ] Appliquer `[Authorize(Roles = "GameMaster")]` sur :
  - [ ] Création campagne
  - [ ] Création PNJ
  - [ ] Lancement session

### Frontend
- [ ] Créer composant `RoleBadge.razor`
  - [ ] Affiche icône Player/MJ
  - [ ] Couleur différente par rôle
- [ ] Ajouter bouton "Devenir Maître du Jeu" sur profil
- [ ] Implémenter `UserService.RequestGameMasterRoleAsync()`
- [ ] Implémenter `UserService.GetMyRolesAsync()`
- [ ] Filtrer navigation selon rôles :
  - [ ] Menu "Mes Personnages" → Player
  - [ ] Menu "Mes Campagnes" → GameMaster
- [ ] Service `RoleService` pour vérifier rôles côté client

### Base de Données
- [ ] Migration : Créer tables `Roles` et `UserRoles`
- [ ] Migration : Seed rôles Player, GameMaster, Admin
- [ ] Script : Assigner rôle Player aux utilisateurs existants

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription
- [US-003](./US-003-gestion-jwt.md) - JWT

### Bloque
- [US-011](../02-Epic-Gestion-Parties/US-011-creation-campagne.md) - Création campagne (nécessite rôle MJ)

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (relations DB, autorisation)
- Effort : 1 jour
- Risques : Migration utilisateurs existants

---

## 📝 Notes Techniques

### Structure JWT avec Rôles
```json
{
  "sub": "userId-guid",
  "email": "user@example.com",
  "role": ["Player", "GameMaster"],
  "exp": 1729522800,
  "iss": "ChroniqueMonde",
  "aud": "ChroniqueMonde-Users"
}
```

### Vérification Rôle Côté Client (Blazor)
```csharp
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private bool isGameMaster = false;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        isGameMaster = user.IsInRole("GameMaster");
    }
}
```

### Autorisation Multi-Rôles
```csharp
// Nécessite un des rôles
[Authorize(Roles = "Player,GameMaster")]

// Nécessite tous les rôles (policy custom)
[Authorize(Policy = "RequirePlayerAndGameMaster")]
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Migration DB appliquée
- [ ] Rôles assignés aux utilisateurs existants
- [ ] JWT contient tous les rôles
- [ ] Autorisation fonctionne sur endpoints
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
