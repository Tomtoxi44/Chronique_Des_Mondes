# US-031 - Réorganisation des Chapitres

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** réorganiser l'ordre des chapitres de ma campagne  
**Afin de** ajuster le scénario sans avoir à recréer les chapitres

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans CampaignDetail, onglet "Chapitres"
- [ ] Chaque chapitre avec poignée de drag (⋮⋮)
- [ ] Drag & drop pour réordonner
- [ ] Feedback visuel pendant drag (ligne bleue insertion)
- [ ] Drop → Mise à jour ordre automatique
- [ ] Numéros chapitres recalculés instantanément (1, 2, 3...)
- [ ] Alternative : Boutons ⬆️⬇️ si drag & drop non supporté
- [ ] Notification "✓ Ordre sauvegardé" après changement
- [ ] Annulation possible (bouton "Annuler" pendant drag)

### Techniques
- [ ] Endpoint : `PATCH /api/campaigns/{campaignId}/chapters/reorder`
- [ ] Body : `{ "orderedChapterIds": ["guid1", "guid2", "guid3"] }`
- [ ] Response 200 : Liste ChapterDto avec nouveaux orderNumber

---

## 🧪 Tests

### Tests Unitaires
- [ ] `ChapterService.ReorderChapters_ValidOrder_UpdatesOrderNumbers()`
- [ ] `ChapterService.ReorderChapters_NonGameMaster_ThrowsUnauthorizedException()`

### Tests d'Intégration
- [ ] `ChapterEndpoint_ReorderChapters_UpdatesDatabase()`

### Tests E2E
- [ ] MJ drag chapitre 3 vers position 1 → Ordre devient 3, 1, 2
- [ ] MJ clique ⬆️ sur chapitre 2 → Ordre devient 2, 1, 3

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `ChapterService.ReorderChaptersAsync(campaignId, orderedIds, userId)` :
```csharp
public async Task<List<ChapterDto>> ReorderChaptersAsync(
    Guid campaignId, 
    List<Guid> orderedIds, 
    Guid userId)
{
    var campaign = await _context.Campaigns
        .Include(c => c.Chapters)
        .FirstOrDefaultAsync(c => c.Id == campaignId);
    
    if (campaign == null)
        throw new NotFoundException("Campagne non trouvée");
    
    if (campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    // Vérifier que tous IDs sont valides
    var chapterIds = campaign.Chapters.Select(c => c.Id).ToHashSet();
    if (!orderedIds.All(id => chapterIds.Contains(id)))
        throw new ValidationException("IDs chapitres invalides");
    
    // Mettre à jour orderNumber
    for (int i = 0; i < orderedIds.Count; i++)
    {
        var chapter = campaign.Chapters.First(c => c.Id == orderedIds[i]);
        chapter.OrderNumber = i + 1;
        chapter.UpdatedAt = DateTime.UtcNow;
    }
    
    await _context.SaveChangesAsync();
    
    return campaign.Chapters
        .OrderBy(c => c.OrderNumber)
        .Select(c => c.ToDto())
        .ToList();
}
```
- [ ] Créer endpoint `PATCH /api/campaigns/{campaignId}/chapters/reorder` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Mettre à jour composant `ChaptersList.razor` :
```razor
@using Microsoft.JSInterop
@inject IJSRuntime JS

<div class="chapters-list">
    <div class="list-header">
        <h3>Chapitres</h3>
        <button @onclick="CreateChapter" class="btn-primary">
            ➕ Créer un Chapitre
        </button>
    </div>
    
    @if (Chapters.Any())
    {
        <div class="chapters-sortable" @ref="chaptersContainer">
            @foreach (var (chapter, index) in Chapters.Select((c, i) => (c, i)))
            {
                <div class="chapter-item" 
                     data-chapter-id="@chapter.Id"
                     draggable="true"
                     @ondragstart="() => OnDragStart(chapter, index)"
                     @ondragover:preventDefault
                     @ondragover="() => OnDragOver(index)"
                     @ondrop="() => OnDrop(index)">
                    
                    <span class="drag-handle">⋮⋮</span>
                    <span class="chapter-number">@chapter.OrderNumber</span>
                    
                    <div class="chapter-info">
                        <h4>@chapter.Title</h4>
                        <p class="text-muted">@chapter.ShortDescription</p>
                    </div>
                    
                    <span class="status-badge @chapter.Status.ToString().ToLower()">
                        @GetStatusLabel(chapter.Status)
                    </span>
                    
                    <div class="chapter-actions">
                        <button @onclick="() => MoveChapterUp(index)" 
                                disabled="@(index == 0)"
                                class="btn-icon"
                                title="Monter">
                            ⬆️
                        </button>
                        <button @onclick="() => MoveChapterDown(index)" 
                                disabled="@(index == Chapters.Count - 1)"
                                class="btn-icon"
                                title="Descendre">
                            ⬇️
                        </button>
                        <button @onclick="() => Nav.NavigateTo($'/chapters/{chapter.Id}/edit')" 
                                class="btn-icon"
                                title="Modifier">
                            ✏️
                        </button>
                        <button @onclick="() => DeleteChapter(chapter.Id)" 
                                class="btn-icon btn-danger"
                                title="Supprimer">
                            🗑️
                        </button>
                    </div>
                </div>
            }
        </div>
        
        @if (HasUnsavedChanges)
        {
            <div class="reorder-actions">
                <button @onclick="SaveOrder" class="btn-primary">
                    Sauvegarder l'ordre
                </button>
                <button @onclick="CancelReorder" class="btn-secondary">
                    Annuler
                </button>
            </div>
        }
    }
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }
    
    private List<ChapterDto> Chapters { get; set; } = new();
    private List<ChapterDto> OriginalOrder { get; set; } = new();
    private ChapterDto? DraggedChapter { get; set; }
    private int DraggedIndex { get; set; }
    private bool HasUnsavedChanges { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadChapters();
    }

    private async Task LoadChapters()
    {
        Chapters = await Http.GetFromJsonAsync<List<ChapterDto>>(
            $"/api/campaigns/{CampaignId}/chapters") ?? new();
        
        OriginalOrder = Chapters.Select(c => c).ToList();
        HasUnsavedChanges = false;
    }

    private void OnDragStart(ChapterDto chapter, int index)
    {
        DraggedChapter = chapter;
        DraggedIndex = index;
    }

    private void OnDragOver(int index)
    {
        // Visual feedback (CSS class)
    }

    private void OnDrop(int targetIndex)
    {
        if (DraggedChapter == null || DraggedIndex == targetIndex)
            return;
        
        Chapters.RemoveAt(DraggedIndex);
        Chapters.Insert(targetIndex, DraggedChapter);
        
        // Recalculer orderNumber
        for (int i = 0; i < Chapters.Count; i++)
        {
            Chapters[i].OrderNumber = i + 1;
        }
        
        HasUnsavedChanges = true;
        DraggedChapter = null;
        StateHasChanged();
    }

    private void MoveChapterUp(int index)
    {
        if (index > 0)
        {
            (Chapters[index], Chapters[index - 1]) = (Chapters[index - 1], Chapters[index]);
            
            // Recalculer orderNumber
            Chapters[index].OrderNumber = index + 1;
            Chapters[index - 1].OrderNumber = index;
            
            HasUnsavedChanges = true;
        }
    }

    private void MoveChapterDown(int index)
    {
        if (index < Chapters.Count - 1)
        {
            (Chapters[index], Chapters[index + 1]) = (Chapters[index + 1], Chapters[index]);
            
            // Recalculer orderNumber
            Chapters[index].OrderNumber = index + 1;
            Chapters[index + 1].OrderNumber = index + 2;
            
            HasUnsavedChanges = true;
        }
    }

    private async Task SaveOrder()
    {
        var orderedIds = Chapters.Select(c => c.Id).ToList();
        
        var response = await Http.PatchAsJsonAsync(
            $"/api/campaigns/{CampaignId}/chapters/reorder",
            new { orderedChapterIds = orderedIds });
        
        if (response.IsSuccessStatusCode)
        {
            Toast.Success("✓ Ordre sauvegardé");
            OriginalOrder = Chapters.Select(c => c).ToList();
            HasUnsavedChanges = false;
        }
        else
        {
            Toast.Error("Erreur lors de la sauvegarde");
        }
    }

    private void CancelReorder()
    {
        Chapters = OriginalOrder.Select(c => c).ToList();
        HasUnsavedChanges = false;
    }

    private async Task DeleteChapter(Guid chapterId)
    {
        // Modal confirmation + DELETE
    }
}
```
- [ ] CSS pour drag & drop :
```css
.chapter-item {
    transition: transform 0.2s ease;
}

.chapter-item[draggable="true"] {
    cursor: move;
}

.chapter-item.dragging {
    opacity: 0.5;
}

.chapter-item.drag-over {
    border-top: 3px solid #2196f3;
}

.drag-handle {
    cursor: grab;
    color: #999;
    font-size: 20px;
    padding: 0 8px;
}

.drag-handle:active {
    cursor: grabbing;
}

.reorder-actions {
    display: flex;
    gap: 1rem;
    margin-top: 1rem;
    padding: 1rem;
    background: #f5f5f5;
    border-radius: 8px;
}
```

### Base de Données
- [ ] Aucune modification nécessaire

---

## 🔗 Dépendances

### Dépend de
- [US-029](./US-029-creation-chapitres.md) - Chapitres existants

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible-Moyenne (Drag & drop)
- Effort : 0.5-1 jour
- Risques : UX drag & drop

---

## 📝 Notes Techniques

### HTML5 Drag & Drop API
```javascript
// Si besoin JS Interop pour fallback
window.initializeSortable = (element) => {
    // Utiliser Sortable.js library si nécessaire
}
```

### Alternative : Sortable.js Library
```html
<script src="https://cdn.jsdelivr.net/npm/sortablejs@latest/Sortable.min.js"></script>
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Drag & drop fonctionnel
- [ ] Boutons ⬆️⬇️ fonctionnels
- [ ] Feedback visuel pendant drag
- [ ] Sauvegarde ordre en base
- [ ] Annulation possible
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 8
