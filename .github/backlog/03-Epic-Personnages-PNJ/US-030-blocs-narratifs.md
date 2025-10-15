# US-030 - Blocs de Texte Narratif dans Chapitres

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer des blocs de texte narratif dans mes chapitres  
**Afin de** structurer le contenu et le déroulement de l'histoire

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans ChapterEditor, section "Contenu Narratif"
- [ ] Liste blocs de texte affichés dans l'ordre
- [ ] Bouton "➕ Ajouter un Bloc"
- [ ] Clic → Formulaire création bloc :
  - [ ] Type de bloc (dropdown) :
    - [ ] 📖 Narration (texte descriptif)
    - [ ] 💬 Dialogue (paroles PNJ)
    - [ ] ⚠️ Information (indices, notes)
    - [ ] ⚔️ Événement (combat, action)
  - [ ] Titre du bloc (optionnel)
  - [ ] Contenu (rich text editor : gras, italique, listes)
  - [ ] Ordre (auto-calculé, modifiable)
- [ ] Bouton "Sauvegarder" → Bloc ajouté à la liste
- [ ] Chaque bloc affiché avec :
  - [ ] Icône type
  - [ ] Titre
  - [ ] Extrait contenu (50 premiers caractères)
  - [ ] Actions (✏️ Modifier, 🗑️ Supprimer, ⬆️⬇️ Réordonner)
- [ ] Clic "Modifier" → Édition inline
- [ ] Drag & drop pour réordonner (optionnel, sinon boutons ⬆️⬇️)
- [ ] Aperçu formaté (Markdown → HTML)

### Techniques
- [ ] Endpoint : `POST /api/chapters/{chapterId}/blocks`
- [ ] Body :
```json
{
  "type": "Narration",
  "title": "L'arrivée au village",
  "content": "**Vous arrivez** au village après 3 jours de marche. Une fumée noire s'élève à l'horizon...",
  "orderIndex": 1
}
```
- [ ] Response 201 : NarrativeBlockDto créé
- [ ] Endpoint : `GET /api/chapters/{chapterId}/blocks`
- [ ] Response 200 : Liste blocs ordonnés

---

## 🧪 Tests

### Tests Unitaires
- [ ] `ChapterService.AddNarrativeBlock_ValidData_CreatesBlock()`
- [ ] `ChapterService.AddNarrativeBlock_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `ChapterService.ReorderBlocks_UpdatesOrderIndexes()`

### Tests d'Intégration
- [ ] `ChapterEndpoint_AddBlock_SavesInDatabase()`
- [ ] `ChapterEndpoint_GetBlocks_ReturnsOrdered()`

### Tests E2E
- [ ] MJ ajoute bloc narration → Apparaît dans liste
- [ ] MJ réordonne blocs → Ordre mis à jour

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `NarrativeBlock` :
```csharp
public class NarrativeBlock
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; }
    
    public NarrativeBlockType Type { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } // Markdown
    public int OrderIndex { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum NarrativeBlockType
{
    Narration = 0,
    Dialogue = 1,
    Information = 2,
    Event = 3
}
```
- [ ] Créer `ChapterService.AddNarrativeBlockAsync(chapterId, dto, userId)` :
```csharp
public async Task<NarrativeBlockDto> AddNarrativeBlockAsync(
    Guid chapterId, 
    NarrativeBlockCreateDto dto, 
    Guid userId)
{
    var chapter = await _context.Chapters
        .Include(c => c.Campaign)
        .FirstOrDefaultAsync(c => c.Id == chapterId);
    
    if (chapter?.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    // Auto-calculer orderIndex si non fourni
    if (dto.OrderIndex == 0)
    {
        var maxOrder = await _context.NarrativeBlocks
            .Where(nb => nb.ChapterId == chapterId)
            .MaxAsync(nb => (int?)nb.OrderIndex) ?? 0;
        
        dto.OrderIndex = maxOrder + 1;
    }
    
    var block = new NarrativeBlock
    {
        ChapterId = chapterId,
        Type = dto.Type,
        Title = dto.Title,
        Content = dto.Content,
        OrderIndex = dto.OrderIndex,
        CreatedAt = DateTime.UtcNow
    };
    
    _context.NarrativeBlocks.Add(block);
    await _context.SaveChangesAsync();
    
    return block.ToDto();
}
```
- [ ] Créer `ChapterService.ReorderBlocksAsync(chapterId, orderedIds, userId)` :
```csharp
public async Task ReorderBlocksAsync(Guid chapterId, List<Guid> orderedIds, Guid userId)
{
    var chapter = await _context.Chapters
        .Include(c => c.Campaign)
        .Include(c => c.NarrativeBlocks)
        .FirstOrDefaultAsync(c => c.Id == chapterId);
    
    if (chapter?.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    for (int i = 0; i < orderedIds.Count; i++)
    {
        var block = chapter.NarrativeBlocks.FirstOrDefault(b => b.Id == orderedIds[i]);
        if (block != null)
        {
            block.OrderIndex = i + 1;
        }
    }
    
    await _context.SaveChangesAsync();
}
```
- [ ] Créer endpoints :
  - [ ] `POST /api/chapters/{chapterId}/blocks` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/chapters/{chapterId}/blocks` [Authorize]
  - [ ] `PUT /api/blocks/{id}` [Authorize(Roles = "GameMaster")]
  - [ ] `DELETE /api/blocks/{id}` [Authorize(Roles = "GameMaster")]
  - [ ] `PATCH /api/chapters/{chapterId}/blocks/reorder` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer composant `NarrativeBlocksEditor.razor` :
```razor
<div class="narrative-blocks-editor">
    <div class="section-header">
        <h3>Contenu Narratif</h3>
        <button @onclick="ShowAddBlockModal" class="btn-secondary">
            ➕ Ajouter un Bloc
        </button>
    </div>
    
    @if (Blocks.Any())
    {
        <div class="blocks-list">
            @foreach (var (block, index) in Blocks.Select((b, i) => (b, i)))
            {
                <div class="narrative-block @block.Type.ToString().ToLower()">
                    <div class="block-header">
                        <span class="block-icon">@GetBlockIcon(block.Type)</span>
                        <h4>@(block.Title ?? GetDefaultTitle(block.Type))</h4>
                        <div class="block-actions">
                            <button @onclick="() => MoveBlockUp(index)" 
                                    disabled="@(index == 0)"
                                    class="btn-icon">
                                ⬆️
                            </button>
                            <button @onclick="() => MoveBlockDown(index)" 
                                    disabled="@(index == Blocks.Count - 1)"
                                    class="btn-icon">
                                ⬇️
                            </button>
                            <button @onclick="() => EditBlock(block)" class="btn-icon">✏️</button>
                            <button @onclick="() => DeleteBlock(block.Id)" class="btn-icon btn-danger">🗑️</button>
                        </div>
                    </div>
                    
                    <div class="block-content">
                        @((MarkupString)Markdig.Markdown.ToHtml(block.Content))
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <p class="empty-state">Aucun contenu narratif. Ajoutez un bloc pour commencer.</p>
    }
</div>

<NarrativeBlockModal IsVisible="@ShowModal"
                     Block="@EditingBlock"
                     ChapterId="@ChapterId"
                     OnSaved="OnBlockSaved"
                     OnClose="() => ShowModal = false" />

@code {
    [Parameter] public Guid ChapterId { get; set; }
    
    private List<NarrativeBlockDto> Blocks { get; set; } = new();
    private bool ShowModal { get; set; }
    private NarrativeBlockDto? EditingBlock { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadBlocks();
    }

    private async Task LoadBlocks()
    {
        Blocks = await Http.GetFromJsonAsync<List<NarrativeBlockDto>>(
            $"/api/chapters/{ChapterId}/blocks") ?? new();
    }

    private void ShowAddBlockModal()
    {
        EditingBlock = null;
        ShowModal = true;
    }

    private void EditBlock(NarrativeBlockDto block)
    {
        EditingBlock = block;
        ShowModal = true;
    }

    private async Task OnBlockSaved()
    {
        await LoadBlocks();
        ShowModal = false;
    }

    private async Task MoveBlockUp(int index)
    {
        if (index > 0)
        {
            (Blocks[index], Blocks[index - 1]) = (Blocks[index - 1], Blocks[index]);
            await SaveOrder();
        }
    }

    private async Task MoveBlockDown(int index)
    {
        if (index < Blocks.Count - 1)
        {
            (Blocks[index], Blocks[index + 1]) = (Blocks[index + 1], Blocks[index]);
            await SaveOrder();
        }
    }

    private async Task SaveOrder()
    {
        var orderedIds = Blocks.Select(b => b.Id).ToList();
        await Http.PatchAsJsonAsync($"/api/chapters/{ChapterId}/blocks/reorder", 
            new { orderedIds });
    }

    private async Task DeleteBlock(Guid blockId)
    {
        await Http.DeleteAsync($"/api/blocks/{blockId}");
        Blocks.RemoveAll(b => b.Id == blockId);
        Toast.Success("Bloc supprimé");
    }

    private string GetBlockIcon(NarrativeBlockType type) => type switch
    {
        NarrativeBlockType.Narration => "📖",
        NarrativeBlockType.Dialogue => "💬",
        NarrativeBlockType.Information => "⚠️",
        NarrativeBlockType.Event => "⚔️",
        _ => "📄"
    };

    private string GetDefaultTitle(NarrativeBlockType type) => type switch
    {
        NarrativeBlockType.Narration => "Narration",
        NarrativeBlockType.Dialogue => "Dialogue",
        NarrativeBlockType.Information => "Information",
        NarrativeBlockType.Event => "Événement",
        _ => "Bloc"
    };
}
```
- [ ] Créer composant `NarrativeBlockModal.razor` :
```razor
<Modal IsVisible="@IsVisible" OnClose="OnClose" Size="large">
    <div class="modal-header">
        <h3>@(Block == null ? "Ajouter" : "Modifier") un Bloc Narratif</h3>
    </div>
    
    <EditForm Model="@Model" OnValidSubmit="SaveBlock">
        <DataAnnotationsValidator />
        <ValidationSummary />
        
        <div class="modal-body">
            <div class="form-group">
                <label>Type de bloc</label>
                <InputSelect @bind-Value="Model.Type" class="form-control">
                    <option value="@NarrativeBlockType.Narration">📖 Narration</option>
                    <option value="@NarrativeBlockType.Dialogue">💬 Dialogue</option>
                    <option value="@NarrativeBlockType.Information">⚠️ Information</option>
                    <option value="@NarrativeBlockType.Event">⚔️ Événement</option>
                </InputSelect>
            </div>
            
            <div class="form-group">
                <label>Titre (optionnel)</label>
                <InputText @bind-Value="Model.Title" 
                          class="form-control" 
                          placeholder="Ex: L'arrivée au village" />
            </div>
            
            <div class="form-group">
                <label>Contenu *</label>
                <InputTextArea @bind-Value="Model.Content" 
                              class="form-control" 
                              rows="10"
                              placeholder="Utilisez **gras**, *italique*, et listes...">
                </InputTextArea>
                <small class="form-text text-muted">
                    Support Markdown : **gras**, *italique*, listes, etc.
                </small>
            </div>
            
            <div class="markdown-preview">
                <h4>Aperçu</h4>
                <div class="preview-content">
                    @((MarkupString)Markdig.Markdown.ToHtml(Model.Content ?? ""))
                </div>
            </div>
        </div>
        
        <div class="modal-footer">
            <button type="submit" class="btn-primary">Sauvegarder</button>
            <button @onclick="OnClose" type="button" class="btn-secondary">Annuler</button>
        </div>
    </EditForm>
</Modal>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Guid ChapterId { get; set; }
    [Parameter] public NarrativeBlockDto? Block { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private NarrativeBlockCreateDto Model { get; set; } = new();

    protected override void OnParametersSet()
    {
        if (Block != null)
        {
            Model = new NarrativeBlockCreateDto
            {
                Type = Block.Type,
                Title = Block.Title,
                Content = Block.Content,
                OrderIndex = Block.OrderIndex
            };
        }
        else
        {
            Model = new NarrativeBlockCreateDto();
        }
    }

    private async Task SaveBlock()
    {
        if (Block == null) // Création
        {
            await Http.PostAsJsonAsync($"/api/chapters/{ChapterId}/blocks", Model);
            Toast.Success("Bloc créé");
        }
        else // Modification
        {
            await Http.PutAsJsonAsync($"/api/blocks/{Block.Id}", Model);
            Toast.Success("Bloc mis à jour");
        }
        
        await OnSaved.InvokeAsync();
    }
}
```

### Base de Données
- [ ] Migration : Créer table `NarrativeBlocks`
- [ ] Index sur `(ChapterId, OrderIndex)` pour tri

---

## 🔗 Dépendances

### Dépend de
- [US-029](./US-029-creation-chapitres.md) - Chapitres existants

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (Rich text, Markdown)
- Effort : 1-2 jours
- Risques : Markdown parser

---

## 📝 Notes Techniques

### Markdig NuGet Package
```xml
<PackageReference Include="Markdig" Version="0.34.0" />
```

### Markdown Examples
```markdown
**Vous arrivez** au village après *3 jours* de marche.

- Le chef du village vous accueille
- Une fumée noire s'élève à l'horizon
- Les villageois semblent paniqués
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Modal création/édition bloc fonctionnelle
- [ ] Markdown → HTML rendering
- [ ] Réordonnancement blocs (⬆️⬇️)
- [ ] Types blocs avec icônes
- [ ] Aperçu Markdown live
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 7
