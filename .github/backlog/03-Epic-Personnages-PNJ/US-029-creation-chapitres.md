# US-029 - Création de Chapitres avec Contenu Narratif

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer des chapitres avec du contenu narratif structuré  
**Afin de** organiser mon scénario et guider le déroulement des sessions

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans CampaignDetail, onglet "Chapitres" avec bouton "➕ Créer un Chapitre"
- [ ] Formulaire création avec champs :
  - [ ] Titre (requis, max 100 caractères)
  - [ ] Numéro d'ordre (auto-calculé, modifiable)
  - [ ] Description courte (textarea, optionnel)
  - [ ] Objectifs du chapitre (liste puces, optionnel)
- [ ] Bouton "Créer" → Sauvegarde → Redirection vers ChapterEditor
- [ ] Éditeur chapitre avec :
  - [ ] Aperçu titre + description
  - [ ] Section "Contenu narratif" (géré par US-030)
  - [ ] Section "PNJ présents" (géré par US-028)
  - [ ] Bouton "Sauvegarder"
- [ ] Liste chapitres dans CampaignDetail :
  - [ ] Ordre numéroté (1, 2, 3...)
  - [ ] Titre
  - [ ] Statut (Brouillon, Prêt, Joué)
  - [ ] Actions (Modifier, Supprimer)

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/chapters`
- [ ] Body :
```json
{
  "title": "L'attaque des gobelins",
  "orderNumber": 2,
  "shortDescription": "Les héros défendent le village",
  "objectives": [
    "Repousser l'attaque",
    "Sauver le chef du village",
    "Découvrir qui a orchestré l'attaque"
  ]
}
```
- [ ] Response 201 : ChapterDto créé
- [ ] Endpoint : `GET /api/campaigns/{campaignId}/chapters`
- [ ] Response 200 : Liste ChapterDto ordonnée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `ChapterService.CreateChapter_ValidData_CreatesChapter()`
- [ ] `ChapterService.CreateChapter_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `ChapterService.CreateChapter_AutoCalculatesOrderNumber()`

### Tests d'Intégration
- [ ] `ChapterEndpoint_CreateChapter_SavesInDatabase()`
- [ ] `ChapterEndpoint_GetChapters_ReturnsOrderedList()`

### Tests E2E
- [ ] MJ crée chapitre → Apparaît dans liste
- [ ] MJ crée plusieurs chapitres → Ordre respecté

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer/Mettre à jour entité `Chapter` :
```csharp
public class Chapter
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    
    public string Title { get; set; }
    public int OrderNumber { get; set; }
    public string? ShortDescription { get; set; }
    public string? Objectives { get; set; } // JSON array
    public ChapterStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Relations
    public ICollection<NarrativeBlock> NarrativeBlocks { get; set; }
    public ICollection<ChapterNPC> ChapterNPCs { get; set; }
}

public enum ChapterStatus
{
    Draft = 0,
    Ready = 1,
    Played = 2
}
```
- [ ] Créer `ChapterService.CreateChapterAsync(campaignId, createDto, userId)` :
```csharp
public async Task<ChapterDto> CreateChapterAsync(Guid campaignId, ChapterCreateDto dto, Guid userId)
{
    var campaign = await _context.Campaigns.FindAsync(campaignId);
    
    if (campaign == null)
        throw new NotFoundException("Campagne non trouvée");
    
    if (campaign.CreatedBy != userId)
        throw new UnauthorizedException("Vous n'êtes pas le créateur de cette campagne");
    
    // Auto-calculer orderNumber si non fourni
    if (dto.OrderNumber == 0)
    {
        var maxOrder = await _context.Chapters
            .Where(c => c.CampaignId == campaignId)
            .MaxAsync(c => (int?)c.OrderNumber) ?? 0;
        
        dto.OrderNumber = maxOrder + 1;
    }
    
    var chapter = new Chapter
    {
        CampaignId = campaignId,
        Title = dto.Title,
        OrderNumber = dto.OrderNumber,
        ShortDescription = dto.ShortDescription,
        Objectives = dto.Objectives != null 
            ? JsonSerializer.Serialize(dto.Objectives) 
            : null,
        Status = ChapterStatus.Draft,
        CreatedAt = DateTime.UtcNow
    };
    
    _context.Chapters.Add(chapter);
    await _context.SaveChangesAsync();
    
    return chapter.ToDto();
}
```
- [ ] Créer `ChapterService.GetCampaignChaptersAsync(campaignId)` :
```csharp
public async Task<List<ChapterDto>> GetCampaignChaptersAsync(Guid campaignId)
{
    return await _context.Chapters
        .Where(c => c.CampaignId == campaignId)
        .OrderBy(c => c.OrderNumber)
        .Select(c => c.ToDto())
        .ToListAsync();
}
```
- [ ] Créer endpoints :
  - [ ] `POST /api/campaigns/{campaignId}/chapters` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/campaigns/{campaignId}/chapters` [Authorize]
  - [ ] `GET /api/chapters/{id}` [Authorize]
  - [ ] `PUT /api/chapters/{id}` [Authorize(Roles = "GameMaster")]
  - [ ] `DELETE /api/chapters/{id}` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer page `ChapterCreate.razor` (/campaigns/{id}/chapters/create) :
```razor
@page "/campaigns/{campaignId:guid}/chapters/create"
@attribute [Authorize(Roles = "GameMaster")]
@inject HttpClient Http
@inject NavigationManager Nav
@inject IToastService Toast

<div class="chapter-create">
    <h1>Créer un Chapitre</h1>
    
    <EditForm Model="@Model" OnValidSubmit="CreateChapter">
        <DataAnnotationsValidator />
        <ValidationSummary />
        
        <div class="form-group">
            <label>Titre du chapitre *</label>
            <InputText @bind-Value="Model.Title" 
                       class="form-control" 
                       placeholder="Ex: L'attaque des gobelins" />
        </div>
        
        <div class="form-group">
            <label>Numéro d'ordre</label>
            <InputNumber @bind-Value="Model.OrderNumber" 
                        class="form-control" 
                        placeholder="0 = auto" />
            <small class="form-text text-muted">
                Laisser à 0 pour auto-calcul
            </small>
        </div>
        
        <div class="form-group">
            <label>Description courte</label>
            <InputTextArea @bind-Value="Model.ShortDescription" 
                          class="form-control" 
                          rows="3"
                          placeholder="Résumé du chapitre..." />
        </div>
        
        <div class="form-group">
            <label>Objectifs</label>
            @foreach (var (objective, index) in Objectives.Select((o, i) => (o, i)))
            {
                <div class="objective-row">
                    <input @bind="Objectives[index]" 
                           class="form-control" 
                           placeholder="Objectif..." />
                    <button @onclick="() => RemoveObjective(index)" 
                            class="btn-icon btn-danger">
                        ✕
                    </button>
                </div>
            }
            <button @onclick="AddObjective" 
                    type="button" 
                    class="btn-secondary">
                ➕ Ajouter un objectif
            </button>
        </div>
        
        <div class="form-actions">
            <button type="submit" class="btn-primary">Créer</button>
            <button @onclick="Cancel" type="button" class="btn-secondary">Annuler</button>
        </div>
    </EditForm>
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }
    
    private ChapterCreateDto Model { get; set; } = new();
    private List<string> Objectives { get; set; } = new() { "" };

    private void AddObjective()
    {
        Objectives.Add("");
    }

    private void RemoveObjective(int index)
    {
        Objectives.RemoveAt(index);
    }

    private async Task CreateChapter()
    {
        Model.Objectives = Objectives.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
        
        var response = await Http.PostAsJsonAsync($"/api/campaigns/{CampaignId}/chapters", Model);
        
        if (response.IsSuccessStatusCode)
        {
            var chapter = await response.Content.ReadFromJsonAsync<ChapterDto>();
            Toast.Success($"✓ Chapitre {Model.Title} créé");
            Nav.NavigateTo($"/chapters/{chapter.Id}/edit");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Toast.Error(error);
        }
    }

    private void Cancel() => Nav.NavigateTo($"/campaigns/{CampaignId}");
}
```
- [ ] Créer page `ChapterEditor.razor` (/chapters/{id}/edit) :
```razor
@page "/chapters/{id:guid}/edit"
@attribute [Authorize(Roles = "GameMaster")]
@inject HttpClient Http
@inject IToastService Toast

<div class="chapter-editor">
    @if (Chapter != null)
    {
        <div class="editor-header">
            <h1>@Chapter.Title</h1>
            <span class="status-badge @Chapter.Status.ToString().ToLower()">
                @GetStatusLabel(Chapter.Status)
            </span>
        </div>
        
        <div class="editor-content">
            <!-- Section Info -->
            <div class="section">
                <h2>Informations</h2>
                <p><strong>Ordre :</strong> Chapitre @Chapter.OrderNumber</p>
                <p><strong>Description :</strong> @Chapter.ShortDescription</p>
                
                @if (Chapter.Objectives?.Any() == true)
                {
                    <div class="objectives">
                        <strong>Objectifs :</strong>
                        <ul>
                            @foreach (var obj in Chapter.Objectives)
                            {
                                <li>@obj</li>
                            }
                        </ul>
                    </div>
                }
            </div>
            
            <!-- Section Contenu Narratif (US-030) -->
            <div class="section">
                <h2>Contenu Narratif</h2>
                <NarrativeBlocksEditor ChapterId="@Id" />
            </div>
            
            <!-- Section PNJ (US-028) -->
            <div class="section">
                <h2>PNJ présents</h2>
                <ChapterNPCsManager ChapterId="@Id" />
            </div>
        </div>
        
        <div class="editor-actions">
            <button @onclick="Save" class="btn-primary">Sauvegarder</button>
            <button @onclick="() => Nav.NavigateTo($'/campaigns/{Chapter.CampaignId}')" 
                    class="btn-secondary">
                Retour
            </button>
        </div>
    }
</div>

@code {
    [Parameter] public Guid Id { get; set; }
    
    private ChapterDto? Chapter { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Chapter = await Http.GetFromJsonAsync<ChapterDto>($"/api/chapters/{Id}");
    }

    private async Task Save()
    {
        // Logique sauvegarde (si modifications)
        Toast.Success("Chapitre sauvegardé");
    }

    private string GetStatusLabel(ChapterStatus status) => status switch
    {
        ChapterStatus.Draft => "Brouillon",
        ChapterStatus.Ready => "Prêt",
        ChapterStatus.Played => "Joué",
        _ => ""
    };
}
```
- [ ] Créer composant `ChaptersList.razor` (pour CampaignDetail) :
```razor
<div class="chapters-list">
    <div class="list-header">
        <h3>Chapitres</h3>
        <button @onclick="() => Nav.NavigateTo($'/campaigns/{CampaignId}/chapters/create')" 
                class="btn-primary">
            ➕ Créer un Chapitre
        </button>
    </div>
    
    @if (Chapters.Any())
    {
        <div class="chapters">
            @foreach (var chapter in Chapters)
            {
                <div class="chapter-item">
                    <span class="chapter-number">@chapter.OrderNumber</span>
                    <div class="chapter-info">
                        <h4>@chapter.Title</h4>
                        <p class="text-muted">@chapter.ShortDescription</p>
                    </div>
                    <span class="status-badge @chapter.Status.ToString().ToLower()">
                        @GetStatusLabel(chapter.Status)
                    </span>
                    <div class="chapter-actions">
                        <button @onclick="() => Nav.NavigateTo($'/chapters/{chapter.Id}/edit')" 
                                class="btn-icon">
                            ✏️
                        </button>
                        <button @onclick="() => DeleteChapter(chapter.Id)" 
                                class="btn-icon btn-danger">
                            🗑️
                        </button>
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <p class="empty-state">Aucun chapitre. Créez-en un pour structurer votre campagne !</p>
    }
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }
    
    private List<ChapterDto> Chapters { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadChapters();
    }

    private async Task LoadChapters()
    {
        Chapters = await Http.GetFromJsonAsync<List<ChapterDto>>(
            $"/api/campaigns/{CampaignId}/chapters") ?? new();
    }

    private async Task DeleteChapter(Guid chapterId)
    {
        // Modal confirmation + DELETE
    }
}
```

### Base de Données
- [ ] Migration : Table `Chapters` déjà créée (US-014)
- [ ] Ajouter colonnes manquantes si besoin

---

## 🔗 Dépendances

### Dépend de
- [US-011](../02-Epic-Gestion-Parties/US-011-creation-campagne.md) - Campagnes

### Bloque
- [US-030](./US-030-blocs-narratifs.md) - Blocs narratifs

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (CRUD + ordre)
- Effort : 1-2 jours
- Risques : Gestion ordre

---

## 📝 Notes Techniques

### Auto-OrderNumber Logic
Si MJ crée chapitres 1, 2, 3 puis veut insérer entre 1 et 2 :
- Option 1 : Décaler tous suivants (1, **1.5**, 2, 3)
- Option 2 : Utiliser doubles (1.0, 1.5, 2.0)
- **Retenu** : Entiers simples, réorganisation via US-031

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Formulaire création chapitre fonctionnel
- [ ] Liste chapitres ordonnée
- [ ] ChapterEditor basique fonctionnel
- [ ] Auto-calcul orderNumber
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 7
