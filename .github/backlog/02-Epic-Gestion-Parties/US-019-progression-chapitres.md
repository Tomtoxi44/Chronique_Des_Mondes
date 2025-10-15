# US-019 - Progression entre Chapitres

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** faire progresser ma session d'un chapitre à l'autre  
**Afin de** guider le scénario et révéler le contenu narratif aux joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans SessionLive, affichage chapitre actuel avec :
  - [ ] Titre
  - [ ] Contenu narratif (Markdown → HTML)
  - [ ] Numéro ordre (ex: "Chapitre 2/5")
- [ ] Bouton "Chapitre Suivant" visible UNIQUEMENT pour MJ
- [ ] Clic → Modal confirmation :
  - [ ] "Passer au chapitre {ChapterTitle} ?"
  - [ ] Bouton "Confirmer" / "Annuler"
- [ ] Confirmation → Mise à jour session + notification tous joueurs
- [ ] Joueurs voient contenu nouveau chapitre instantanément (SignalR)
- [ ] Si dernier chapitre → Bouton devient "Terminer la Session"
- [ ] Historique navigation visible (Timeline chapitres passés)
- [ ] Bouton "Chapitre Précédent" (retour arrière possible)
- [ ] Log automatique changements chapitres (SessionEvent)

### Techniques
- [ ] Endpoint : `POST /api/sessions/{sessionId}/change-chapter`
- [ ] Body : `{ "targetChapterId": "guid" }`
- [ ] Response 200 : `{ "currentChapter": { ... }, "previousChapter": { ... } }`
- [ ] SignalR `SessionHub.ChapterChanged` :
```csharp
await Clients.Group($"Session-{sessionId}")
    .SendAsync("ChapterChanged", new
    {
        ChapterId = chapter.Id,
        ChapterTitle = chapter.Title,
        Content = chapter.Content,
        OrderNumber = chapter.OrderNumber
    });
```
- [ ] Validation : Vérifier chapitre appartient à la campagne

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SessionService.ChangeChapter_ValidChapter_UpdatesSession()`
- [ ] `SessionService.ChangeChapter_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `SessionService.ChangeChapter_ChapterNotInCampaign_ThrowsValidationException()`
- [ ] `SessionService.GetChapterTimeline_ReturnsOrderedHistory()`

### Tests d'Intégration
- [ ] `SessionEndpoint_ChangeChapter_UpdatesDatabase()`
- [ ] `SessionEndpoint_ChangeChapter_NotifiesAllPlayers()`
- [ ] `SessionEndpoint_ChangeChapter_CreatesSessionEvent()`

### Tests E2E
- [ ] MJ clique "Chapitre Suivant" → Joueurs reçoivent nouveau contenu instantanément
- [ ] MJ à dernier chapitre → Bouton devient "Terminer Session"

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `SessionEvent` (pour logging) :
```csharp
public class SessionEvent
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Session Session { get; set; }
    public SessionEventType Type { get; set; }
    public string Data { get; set; } // JSON
    public DateTime OccurredAt { get; set; }
    public Guid? TriggeredBy { get; set; } // UserId
}

public enum SessionEventType
{
    ChapterChanged = 0,
    CombatStarted = 1,
    CombatEnded = 2,
    PlayerJoined = 3,
    PlayerLeft = 4,
    SessionPaused = 5,
    SessionResumed = 6
}
```
- [ ] Créer `SessionService.ChangeChapterAsync(sessionId, targetChapterId, userId)` :
  - [ ] Vérifier userId == Session.Campaign.CreatedBy
  - [ ] Vérifier Session.Status == Active
  - [ ] Vérifier targetChapter.CampaignId == Session.CampaignId
  - [ ] Mettre à jour Session.CurrentChapterId
  - [ ] Créer SessionEvent (Type = ChapterChanged)
  - [ ] Notifier SignalR
- [ ] Implémenter `SessionHub.ChangeChapter(sessionId, chapterId)` :
```csharp
[Authorize(Roles = "GameMaster")]
public async Task ChangeChapter(Guid sessionId, Guid chapterId)
{
    var userId = Context.User.GetUserId();
    var chapter = await _sessionService.ChangeChapterAsync(sessionId, chapterId, userId);

    await Clients.Group($"Session-{sessionId}")
        .SendAsync("ChapterChanged", new
        {
            ChapterId = chapter.Id,
            ChapterTitle = chapter.Title,
            Content = chapter.Content,
            OrderNumber = chapter.OrderNumber,
            TotalChapters = chapter.Campaign.Chapters.Count
        });
}
```
- [ ] Créer `SessionService.GetChapterTimelineAsync(sessionId)` :
  - [ ] Récupérer SessionEvents où Type == ChapterChanged
  - [ ] Mapper vers DTO avec détails chapitres

### Frontend
- [ ] Créer composant `ChapterNavigation.razor` :
```razor
@if (IsGameMaster)
{
    <div class="chapter-controls">
        <button @onclick="PreviousChapter" disabled="@IsFirstChapter">
            ← Chapitre Précédent
        </button>
        
        <button @onclick="NextChapter" disabled="@IsLastChapter">
            @(IsLastChapter ? "Terminer Session" : "Chapitre Suivant →")
        </button>
    </div>
}
```
- [ ] Créer composant `ChapterTimeline.razor` :
  - [ ] Liste verticale chapitres passés
  - [ ] Icône ✓ pour chapitres complétés
  - [ ] Timestamp passage chapitre
- [ ] Mettre à jour `SessionLive.razor` :
  - [ ] Abonnement SignalR `ChapterChanged`
  - [ ] Mise à jour CurrentChapter au changement
  - [ ] Animation transition (fade-in contenu)
- [ ] Modal confirmation changement chapitre :
```razor
<ConfirmDialog Title="Passer au chapitre suivant ?"
               Message="Vous allez passer au chapitre '@NextChapter.Title'. Cette action sera notifiée à tous les joueurs."
               OnConfirm="ConfirmChangeChapter" />
```

### Base de Données
- [ ] Migration : Créer table `SessionEvents`
- [ ] Index sur `(SessionId, OccurredAt)` pour timeline

---

## 🔗 Dépendances

### Dépend de
- [US-018](./US-018-lancement-session.md) - Session active
- [US-014](./US-014-creation-chapitres.md) - Chapitres existants

### Bloque
- [US-020](./US-020-auto-sauvegarde.md) - Auto-save (inclut CurrentChapterId)

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (SignalR, validation ordre)
- Effort : 1-2 jours
- Risques : Synchronisation temps réel

---

## 📝 Notes Techniques

### SignalR ChapterChanged Flow
1. MJ clique "Chapitre Suivant"
2. Frontend appelle API `POST /api/sessions/{id}/change-chapter`
3. Backend met à jour Session.CurrentChapterId
4. Backend appelle `SessionHub.ChapterChanged`
5. Tous clients connectés reçoivent notification
6. Frontend Blazor met à jour UI

### Timeline UI
```
Session Timeline
├─ ✓ Chapitre 1: Introduction (14:30 - 14:45)
├─ ✓ Chapitre 2: L'attaque (14:45 - 15:10)
├─ ⏵ Chapitre 3: La forêt (15:10 - En cours)
└─ ⏱ Chapitre 4: Le boss final (À venir)
```

### Validation Ordre
```csharp
var chapters = await _context.Chapters
    .Where(c => c.CampaignId == session.CampaignId)
    .OrderBy(c => c.OrderNumber)
    .ToListAsync();

var currentIndex = chapters.FindIndex(c => c.Id == session.CurrentChapterId);
var targetIndex = chapters.FindIndex(c => c.Id == targetChapterId);

// Autoriser saut chapitres (ex: passer chapitre 2 → 4 si MJ le décide)
if (targetIndex < 0)
    throw new ValidationException("Chapitre invalide");
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] SignalR notifications opérationnelles
- [ ] Timeline chapitres fonctionnelle
- [ ] Navigation avant/arrière fonctionne
- [ ] Logs SessionEvent créés
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 4
