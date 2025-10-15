# US-028 - Association PNJ aux Chapitres

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** associer des PNJ à mes chapitres  
**Afin de** définir quels personnages apparaissent dans chaque partie du scénario

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans ChapterEditor, section "PNJ présents"
- [ ] Liste PNJ actuellement associés au chapitre
- [ ] Bouton "➕ Ajouter un PNJ"
- [ ] Modal sélection avec :
  - [ ] Liste mes PNJ (créés par MJ)
  - [ ] Recherche par nom
  - [ ] Filtre par type (Allié, Ennemi, Neutre)
  - [ ] Checkbox multi-sélection
  - [ ] Bouton "Ajouter"
- [ ] Ajout → PNJ apparaît dans liste chapitre
- [ ] Bouton "✕" pour retirer PNJ du chapitre (sans le supprimer de la base)
- [ ] PNJ affiché avec avatar + nom + type
- [ ] Ordre des PNJ modifiable (drag & drop optionnel)

### Techniques
- [ ] Endpoint : `POST /api/chapters/{chapterId}/npcs`
- [ ] Body : `{ "npcIds": ["guid1", "guid2"] }`
- [ ] Response 200 : Liste NPCs ajoutés
- [ ] Endpoint : `DELETE /api/chapters/{chapterId}/npcs/{npcId}`
- [ ] Response 204 : No Content

---

## 🧪 Tests

### Tests Unitaires
- [ ] `ChapterService.AddNPCsToChapter_ValidIds_CreatesAssociations()`
- [ ] `ChapterService.AddNPCsToChapter_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `ChapterService.RemoveNPCFromChapter_RemovesAssociation()`

### Tests d'Intégration
- [ ] `ChapterEndpoint_AddNPCs_CreatesChapterNPCs()`
- [ ] `ChapterEndpoint_RemoveNPC_DeletesAssociation()`

### Tests E2E
- [ ] MJ ouvre ChapterEditor → Ajoute PNJ → PNJ visible
- [ ] MJ retire PNJ → PNJ disparaît du chapitre

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `ChapterNPC` (relation N-N) :
```csharp
public class ChapterNPC
{
    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; }
    
    public Guid NPCId { get; set; }
    public NPC NPC { get; set; }
    
    public int OrderIndex { get; set; } // Pour ordre affichage
    public DateTime AddedAt { get; set; }
}
```
- [ ] Mettre à jour entités :
```csharp
public class Chapter
{
    // ... Propriétés existantes
    public ICollection<ChapterNPC> ChapterNPCs { get; set; }
}

public class NPC
{
    // ... Propriétés existantes
    public ICollection<ChapterNPC> ChapterNPCs { get; set; }
}
```
- [ ] Créer `ChapterService.AddNPCsToChapterAsync(chapterId, npcIds, userId)` :
```csharp
public async Task<List<NPCDto>> AddNPCsToChapterAsync(Guid chapterId, List<Guid> npcIds, Guid userId)
{
    var chapter = await _context.Chapters
        .Include(c => c.Campaign)
        .FirstOrDefaultAsync(c => c.Id == chapterId);
    
    if (chapter == null)
        throw new NotFoundException("Chapitre non trouvé");
    
    if (chapter.Campaign.CreatedBy != userId)
        throw new UnauthorizedException("Vous n'êtes pas le créateur de cette campagne");
    
    // Vérifier que NPCs appartiennent au MJ
    var npcs = await _context.NPCs
        .Where(n => npcIds.Contains(n.Id) && n.CreatedBy == userId)
        .ToListAsync();
    
    if (npcs.Count != npcIds.Count)
        throw new ValidationException("Certains PNJ sont invalides");
    
    // Récupérer orderIndex max actuel
    var maxOrder = await _context.ChapterNPCs
        .Where(cn => cn.ChapterId == chapterId)
        .MaxAsync(cn => (int?)cn.OrderIndex) ?? -1;
    
    // Créer associations
    var associations = npcs.Select((npc, index) => new ChapterNPC
    {
        ChapterId = chapterId,
        NPCId = npc.Id,
        OrderIndex = maxOrder + index + 1,
        AddedAt = DateTime.UtcNow
    }).ToList();
    
    _context.ChapterNPCs.AddRange(associations);
    await _context.SaveChangesAsync();
    
    return npcs.Select(n => n.ToDto()).ToList();
}
```
- [ ] Créer `ChapterService.RemoveNPCFromChapterAsync(chapterId, npcId, userId)` :
```csharp
public async Task RemoveNPCFromChapterAsync(Guid chapterId, Guid npcId, Guid userId)
{
    var chapter = await _context.Chapters
        .Include(c => c.Campaign)
        .FirstOrDefaultAsync(c => c.Id == chapterId);
    
    if (chapter?.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    var association = await _context.ChapterNPCs
        .FirstOrDefaultAsync(cn => cn.ChapterId == chapterId && cn.NPCId == npcId);
    
    if (association != null)
    {
        _context.ChapterNPCs.Remove(association);
        await _context.SaveChangesAsync();
    }
}
```
- [ ] Créer endpoints :
  - [ ] `POST /api/chapters/{chapterId}/npcs` [Authorize(Roles = "GameMaster")]
  - [ ] `DELETE /api/chapters/{chapterId}/npcs/{npcId}` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/chapters/{chapterId}/npcs` [Authorize]

### Frontend
- [ ] Créer section dans `ChapterEditor.razor` :
```razor
<div class="chapter-npcs-section">
    <div class="section-header">
        <h3>PNJ présents dans ce chapitre</h3>
        <button @onclick="ShowAddNPCModal" class="btn-secondary">
            ➕ Ajouter un PNJ
        </button>
    </div>
    
    <div class="npcs-list">
        @if (ChapterNPCs.Any())
        {
            @foreach (var npc in ChapterNPCs.OrderBy(n => n.OrderIndex))
            {
                <div class="npc-item">
                    <img src="@(npc.AvatarUrl ?? "/images/default-npc.png")" 
                         alt="@npc.Name" 
                         class="npc-avatar-small" />
                    <span class="npc-name">@npc.Name</span>
                    <span class="npc-type @npc.NPCType.ToString().ToLower()">
                        @GetTypeLabel(npc.NPCType)
                    </span>
                    <button @onclick="() => RemoveNPC(npc.Id)" 
                            class="btn-icon btn-danger">
                        ✕
                    </button>
                </div>
            }
        }
        else
        {
            <p class="text-muted">Aucun PNJ associé</p>
        }
    </div>
</div>

@code {
    private List<NPCDto> ChapterNPCs { get; set; } = new();

    private async Task LoadChapterNPCs()
    {
        ChapterNPCs = await Http.GetFromJsonAsync<List<NPCDto>>(
            $"/api/chapters/{ChapterId}/npcs") ?? new();
    }

    private async Task RemoveNPC(Guid npcId)
    {
        await Http.DeleteAsync($"/api/chapters/{ChapterId}/npcs/{npcId}");
        ChapterNPCs.RemoveAll(n => n.Id == npcId);
        Toast.Success("PNJ retiré du chapitre");
    }
}
```
- [ ] Créer composant `AddNPCModal.razor` :
```razor
<Modal IsVisible="@IsVisible" OnClose="OnClose" Size="large">
    <div class="modal-header">
        <h3>Ajouter des PNJ au chapitre</h3>
    </div>
    
    <div class="modal-body">
        <div class="filters">
            <input @bind="SearchTerm" 
                   @bind:event="oninput" 
                   placeholder="Rechercher..." 
                   class="form-control" />
            
            <select @bind="TypeFilter" class="form-control">
                <option value="">Tous types</option>
                <option value="Ally">Alliés</option>
                <option value="Enemy">Ennemis</option>
                <option value="Neutral">Neutres</option>
            </select>
        </div>
        
        <div class="npcs-selection-list">
            @foreach (var npc in GetFilteredNPCs())
            {
                <label class="npc-selection-item">
                    <input type="checkbox" 
                           checked="@SelectedNPCIds.Contains(npc.Id)"
                           @onchange="e => ToggleNPC(npc.Id, e.Value)" />
                    <img src="@(npc.AvatarUrl ?? "/images/default-npc.png")" 
                         alt="@npc.Name" 
                         class="npc-avatar-small" />
                    <span>@npc.Name</span>
                    <span class="npc-type @npc.NPCType.ToString().ToLower()">
                        @GetTypeLabel(npc.NPCType)
                    </span>
                </label>
            }
        </div>
    </div>
    
    <div class="modal-footer">
        <button @onclick="AddSelectedNPCs" 
                class="btn-primary" 
                disabled="@(!SelectedNPCIds.Any())">
            Ajouter (@SelectedNPCIds.Count)
        </button>
        <button @onclick="OnClose" class="btn-secondary">Annuler</button>
    </div>
</Modal>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Guid ChapterId { get; set; }
    [Parameter] public EventCallback OnNPCsAdded { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private List<NPCDto> AvailableNPCs { get; set; } = new();
    private HashSet<Guid> SelectedNPCIds { get; set; } = new();
    private string SearchTerm { get; set; } = "";
    private string TypeFilter { get; set; } = "";

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            AvailableNPCs = await Http.GetFromJsonAsync<List<NPCDto>>("/api/npcs") ?? new();
        }
    }

    private IEnumerable<NPCDto> GetFilteredNPCs()
    {
        return AvailableNPCs
            .Where(n => string.IsNullOrEmpty(SearchTerm) || n.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(n => string.IsNullOrEmpty(TypeFilter) || n.NPCType.ToString() == TypeFilter);
    }

    private void ToggleNPC(Guid npcId, object? isChecked)
    {
        if (isChecked is bool b && b)
            SelectedNPCIds.Add(npcId);
        else
            SelectedNPCIds.Remove(npcId);
    }

    private async Task AddSelectedNPCs()
    {
        var response = await Http.PostAsJsonAsync($"/api/chapters/{ChapterId}/npcs", 
            new { npcIds = SelectedNPCIds.ToList() });
        
        if (response.IsSuccessStatusCode)
        {
            await OnNPCsAdded.InvokeAsync();
            SelectedNPCIds.Clear();
            await OnClose.InvokeAsync();
        }
    }
}
```

### Base de Données
- [ ] Migration : Créer table `ChapterNPCs`
- [ ] PK composite : `(ChapterId, NPCId)`
- [ ] Index sur `ChapterId` pour requêtes liste PNJ par chapitre

---

## 🔗 Dépendances

### Dépend de
- [US-027](./US-027-creation-pnj.md) - PNJ créés
- [US-014](../02-Epic-Gestion-Parties/US-014-creation-chapitres.md) - Chapitres existants

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Faible-Moyenne (Relation N-N)
- Effort : 1 jour
- Risques : Aucun

---

## 📝 Notes Techniques

### OrderIndex pour Drag & Drop Future
```csharp
// API pour réordonner
[HttpPatch("/api/chapters/{chapterId}/npcs/reorder")]
public async Task<IActionResult> ReorderNPCs(Guid chapterId, [FromBody] List<Guid> orderedNPCIds)
{
    // Mettre à jour OrderIndex selon ordre liste
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Modal sélection PNJ fonctionnelle
- [ ] Ajout/retrait PNJ opérationnels
- [ ] Liste PNJ affichée dans ChapterEditor
- [ ] Filtres et recherche fonctionnels
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 7
