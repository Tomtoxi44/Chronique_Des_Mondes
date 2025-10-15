# US-014 - Création et Gestion des Chapitres

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** structurer ma campagne en chapitres avec contenu narratif  
**Afin de** organiser l'histoire et guider la progression des joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page de détails campagne avec section "Chapitres"
- [ ] Bouton "+ Nouveau Chapitre" (visible pour MJ uniquement)
- [ ] Formulaire création chapitre :
  - [ ] Titre du chapitre (obligatoire, 3-200 caractères)
  - [ ] Numéro d'ordre (auto-incrémenté ou manuel)
  - [ ] Contenu narratif (éditeur riche Markdown/HTML)
  - [ ] Statut (Brouillon, Prêt, Joué, Terminé)
- [ ] Liste des chapitres affichée dans l'ordre
- [ ] Chaque chapitre affiche : Numéro, Titre, Statut, NB PNJ associés
- [ ] Actions sur chapitre :
  - [ ] Modifier titre et contenu
  - [ ] Supprimer (avec confirmation)
  - [ ] Réorganiser ordre (drag-and-drop)
  - [ ] Associer des PNJ
- [ ] Aperçu du contenu en lecture seule
- [ ] Sauvegarde automatique pendant édition (toutes les 30s)

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/chapters`
- [ ] Body : `{ "title": "Chapitre 1", "orderNumber": 1, "content": "...", "status": "Ready" }`
- [ ] Response 201 : Chapitre créé
- [ ] Endpoint : `PUT /api/chapters/{id}`
- [ ] Endpoint : `DELETE /api/chapters/{id}`
- [ ] Endpoint : `PUT /api/chapters/{id}/reorder` (body: `{ "newOrderNumber": 3 }`)

---

## 🧪 Tests

### Tests Unitaires
- [ ] `ChapterService.CreateChapter_WithValidData_CreatesChapter()`
- [ ] `ChapterService.CreateChapter_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `ChapterService.UpdateChapter_UpdatesContent()`
- [ ] `ChapterService.DeleteChapter_RemovesChapter()`
- [ ] `ChapterService.ReorderChapters_UpdatesOrderNumbers()`

### Tests d'Intégration
- [ ] `ChapterEndpoint_CreateChapter_SavesInDatabase()`
- [ ] `ChapterEndpoint_Reorder_UpdatesAllAffectedChapters()`
- [ ] `ChapterEndpoint_Delete_RemovesFromCampaign()`

### Tests E2E
- [ ] Création chapitre → Affichage dans liste → Modification → Suppression
- [ ] Réorganisation drag-and-drop → Ordre mis à jour
- [ ] Sauvegarde automatique pendant édition

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Chapter` :
```csharp
public class Chapter
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public string Title { get; set; }
    public int OrderNumber { get; set; }
    public string Content { get; set; } // Markdown/HTML
    public ChapterStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Relations
    public ICollection<ChapterNPC> NPCs { get; set; }
}

public enum ChapterStatus
{
    Draft = 0,
    Ready = 1,
    Playing = 2,
    Completed = 3
}
```
- [ ] Créer `ChapterService` avec méthodes CRUD complètes
- [ ] Implémenter `ReorderChaptersAsync(chapterId, newOrderNumber)` :
  - [ ] Récupérer tous chapitres de la campagne
  - [ ] Mettre à jour ordres en cascade
  - [ ] Sauvegarder batch
- [ ] Créer endpoints :
  - [ ] `POST /api/campaigns/{campaignId}/chapters` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/campaigns/{campaignId}/chapters`
  - [ ] `PUT /api/chapters/{id}` [Authorize(Roles = "GameMaster")]
  - [ ] `DELETE /api/chapters/{id}` [Authorize(Roles = "GameMaster")]
  - [ ] `PUT /api/chapters/{id}/reorder` [Authorize(Roles = "GameMaster")]
- [ ] Validation : MJ doit être créateur de la campagne

### Frontend
- [ ] Créer page `ChapterManager.razor` (section de CampaignDetail)
- [ ] Créer composant `ChapterEditor.razor` :
  - [ ] Éditeur Markdown (package : Markdig ou Monaco Editor)
  - [ ] Preview en temps réel
  - [ ] Boutons formatage (gras, italique, liste, etc.)
- [ ] Créer composant `ChapterList.razor` :
  - [ ] Liste ordonnée drag-and-drop (Blazor.DragDrop ou Sortable.js)
  - [ ] Cards avec titre, numéro, statut
  - [ ] Actions : Modifier, Supprimer
- [ ] Implémenter sauvegarde automatique :
```csharp
private System.Timers.Timer _autoSaveTimer;

protected override void OnInitialized()
{
    _autoSaveTimer = new System.Timers.Timer(30000); // 30s
    _autoSaveTimer.Elapsed += async (s, e) => await AutoSaveAsync();
    _autoSaveTimer.Start();
}

private async Task AutoSaveAsync()
{
    if (_isDirty)
    {
        await _chapterService.UpdateChapterAsync(_chapter);
        _isDirty = false;
    }
}
```
- [ ] Modal confirmation suppression
- [ ] Toast notification sauvegarde réussie

### Base de Données
- [ ] Migration : Créer table `Chapters`
- [ ] Index sur `(CampaignId, OrderNumber)` pour tri
- [ ] Relation 1-N : `Campaign` → `Chapters`
- [ ] Relation N-N : `Chapters` ↔ `NPCs` (via `ChapterNPCs`)

---

## 🔗 Dépendances

### Dépend de
- [US-011](./US-011-creation-campagne.md) - Création campagne

### Bloque
- [US-019](./US-019-progression-chapitres.md) - Progression chapitres
- [US-027](../03-Epic-Personnages-PNJ/US-027-creation-pnj.md) - Association PNJ

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne-Haute (éditeur, réorganisation)
- Effort : 1-2 jours
- Risques : Éditeur riche, drag-and-drop

---

## 📝 Notes Techniques

### Éditeur Markdown
```csharp
// Package : Markdig
using Markdig;

var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
var html = Markdown.ToHtml(markdownContent, pipeline);
```

### Réorganisation Ordre
```csharp
public async Task ReorderChapterAsync(Guid chapterId, int newOrderNumber)
{
    var chapter = await _db.Chapters.FindAsync(chapterId);
    var oldOrderNumber = chapter.OrderNumber;
    
    if (newOrderNumber > oldOrderNumber)
    {
        // Déplacement vers le bas : décaler vers le haut
        var chaptersToUpdate = await _db.Chapters
            .Where(c => c.CampaignId == chapter.CampaignId &&
                        c.OrderNumber > oldOrderNumber &&
                        c.OrderNumber <= newOrderNumber)
            .ToListAsync();
        
        foreach (var ch in chaptersToUpdate)
        {
            ch.OrderNumber--;
        }
    }
    else
    {
        // Déplacement vers le haut : décaler vers le bas
        var chaptersToUpdate = await _db.Chapters
            .Where(c => c.CampaignId == chapter.CampaignId &&
                        c.OrderNumber >= newOrderNumber &&
                        c.OrderNumber < oldOrderNumber)
            .ToListAsync();
        
        foreach (var ch in chaptersToUpdate)
        {
            ch.OrderNumber++;
        }
    }
    
    chapter.OrderNumber = newOrderNumber;
    await _db.SaveChangesAsync();
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Éditeur Markdown fonctionnel
- [ ] Drag-and-drop opérationnel
- [ ] Sauvegarde automatique active
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
