# US-013 - Liste des Campagnes

## 📝 Description

**En tant qu'** utilisateur (Joueur ou MJ)  
**Je veux** consulter la liste de toutes les campagnes disponibles  
**Afin de** trouver des parties à rejoindre ou gérer mes propres campagnes

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Campagnes" accessible depuis le menu principal
- [ ] Trois onglets de filtrage :
  - [ ] **Mes Campagnes** : Campagnes que je crée (MJ)
  - [ ] **Mes Parties** : Campagnes où je joue (Joueur)
  - [ ] **Campagnes Publiques** : Toutes campagnes publiques disponibles
- [ ] Chaque carte de campagne affiche :
  - [ ] Image de couverture
  - [ ] Nom de la campagne
  - [ ] Système de jeu (D&D 5e, Générique, etc.)
  - [ ] Nom du MJ
  - [ ] Nombre de joueurs (X/Y)
  - [ ] Statut (Active, En pause, Terminée)
  - [ ] Badge "Publique/Privée"
- [ ] Filtres et recherche :
  - [ ] Recherche par nom
  - [ ] Filtre par système de jeu
  - [ ] Filtre par statut
  - [ ] Tri (récentes, alphabétique, nombre joueurs)
- [ ] Pagination (20 campagnes par page)
- [ ] Clic sur carte → Détails de la campagne
- [ ] Bouton "+ Nouvelle Campagne" (visible pour MJ uniquement)

### Techniques
- [ ] Endpoint : `GET /api/campaigns` [Authorize]
- [ ] Query params : `?filter=mine|playing|public&search=xxx&gameType=DnD5e&status=Active&sortBy=recent&page=1&pageSize=20`
- [ ] Response 200 : 
```json
{
  "campaigns": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CampaignService.GetMyCampaigns_WithGameMaster_ReturnsOwnedCampaigns()`
- [ ] `CampaignService.GetMyPlayingCampaigns_WithPlayer_ReturnsJoinedCampaigns()`
- [ ] `CampaignService.GetPublicCampaigns_ReturnsOnlyPublic()`
- [ ] `CampaignService.SearchCampaigns_WithKeyword_ReturnsMatching()`
- [ ] `CampaignService.FilterByGameType_WithDnD_ReturnsOnlyDnD()`

### Tests d'Intégration
- [ ] `CampaignEndpoint_GetCampaigns_WithFilters_ReturnsFilteredList()`
- [ ] `CampaignEndpoint_GetCampaigns_WithPagination_ReturnsCorrectPage()`
- [ ] `CampaignEndpoint_GetCampaigns_Unauthorized_Returns401()`

### Tests E2E
- [ ] Navigation "Mes Campagnes" → Affiche campagnes créées
- [ ] Navigation "Mes Parties" → Affiche campagnes jouées
- [ ] Recherche "Dragon" → Résultats filtrés
- [ ] Pagination → Changement de page → Nouvelles campagnes

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CampaignListResponse` DTO :
```csharp
public class CampaignListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public GameType GameType { get; set; }
    public string CoverImageUrl { get; set; }
    public string CreatedByName { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public CampaignStatus Status { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
}
```
- [ ] Créer enum `CampaignStatus` : Active, Paused, Completed
- [ ] Implémenter `CampaignService.GetCampaignsAsync(filter, search, gameType, status, sortBy, page, pageSize, userId)` :
  - [ ] Query de base avec Includes (CreatedBy, Players)
  - [ ] Filtre "mine" : `Where(c => c.CreatedBy == userId)`
  - [ ] Filtre "playing" : `Where(c => c.Players.Any(p => p.UserId == userId))`
  - [ ] Filtre "public" : `Where(c => c.Visibility == Visibility.Public)`
  - [ ] Recherche : `Where(c => c.Name.Contains(search))`
  - [ ] Filtre gameType : `Where(c => c.GameType == gameType)`
  - [ ] Tri récent : `OrderByDescending(c => c.CreatedAt)`
  - [ ] Pagination : `Skip((page-1)*pageSize).Take(pageSize)`
  - [ ] Projection vers DTO
- [ ] Créer endpoint `GET /api/campaigns` [Authorize]
- [ ] Ajouter colonne `Status` à table `Campaigns` si absent

### Frontend
- [ ] Créer page `Campaigns.razor` (/campaigns)
- [ ] Créer composant `CampaignCard.razor` :
```razor
<div class="campaign-card" @onclick="OnClick">
    <img src="@Campaign.CoverImageUrl" alt="@Campaign.Name" />
    <div class="campaign-info">
        <h3>@Campaign.Name</h3>
        <p class="game-type">@Campaign.GameType</p>
        <p class="gm">MJ: @Campaign.CreatedByName</p>
        <div class="players">
            <span>@Campaign.CurrentPlayers/@Campaign.MaxPlayers joueurs</span>
        </div>
        <div class="badges">
            <span class="badge-@(Campaign.IsPublic ? "public" : "private")">
                @(Campaign.IsPublic ? "Publique" : "Privée")
            </span>
            <span class="badge-status">@Campaign.Status</span>
        </div>
    </div>
</div>
```
- [ ] Créer composant `CampaignFilters.razor` :
  - [ ] Barre de recherche
  - [ ] Dropdown système de jeu
  - [ ] Dropdown statut
  - [ ] Radio boutons tri
- [ ] Créer composant `Pagination.razor` réutilisable
- [ ] Implémenter `CampaignService.GetCampaignsAsync(filter, search, ...)`
- [ ] Gestion état avec onglets actifs
- [ ] Bouton "Nouvelle Campagne" si rôle GameMaster
- [ ] Loading spinner pendant chargement
- [ ] Message "Aucune campagne trouvée" si liste vide

### Base de Données
- [ ] Migration : Ajouter colonne `Status` (enum : 0=Active, 1=Paused, 2=Completed)
- [ ] Migration : Valeur par défaut Status = Active
- [ ] Index sur `CreatedBy` pour performance
- [ ] Index sur `Visibility` pour filtre public
- [ ] Index composite sur `(GameType, Status)` pour filtres combinés

---

## 🔗 Dépendances

### Dépend de
- [US-011](./US-011-creation-campagne.md) - Création campagne

### Bloque
- [US-012](./US-012-modification-campagne.md) - Modification (accès détails)

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (filtres, pagination)
- Effort : 1 jour
- Risques : Performance avec beaucoup de campagnes

---

## 📝 Notes Techniques

### Query Optimisée
```csharp
var query = _db.Campaigns
    .Include(c => c.CreatedBy)
    .Include(c => c.Players)
    .AsQueryable();

// Filtres
if (filter == "mine")
    query = query.Where(c => c.CreatedBy == userId);
else if (filter == "playing")
    query = query.Where(c => c.Players.Any(p => p.UserId == userId));
else if (filter == "public")
    query = query.Where(c => c.Visibility == Visibility.Public);

// Total count avant pagination
var totalCount = await query.CountAsync();

// Pagination
var campaigns = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(c => new CampaignListResponse
    {
        Id = c.Id,
        Name = c.Name,
        GameType = c.GameType,
        CreatedByName = c.CreatedBy.Username,
        CurrentPlayers = c.Players.Count,
        MaxPlayers = c.MaxPlayers,
        // ...
    })
    .ToListAsync();
```

### Enum CampaignStatus
```csharp
public enum CampaignStatus
{
    Active = 0,      // Campagne en cours
    Paused = 1,      // En pause (MJ peut reprendre)
    Completed = 2    // Terminée définitivement
}
```

### Performance
- Index sur colonnes fréquemment filtrées
- Pagination obligatoire (max 100 par page)
- Cache côté client (5 minutes) pour éviter requêtes répétées
- Projection DTO pour éviter over-fetching

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Filtres fonctionnels
- [ ] Pagination opérationnelle
- [ ] Performance acceptable (< 500ms)
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
