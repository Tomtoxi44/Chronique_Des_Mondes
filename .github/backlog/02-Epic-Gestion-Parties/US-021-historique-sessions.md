# US-021 - Historique des Sessions

## 📝 Description

**En tant que** Maître du Jeu ou Joueur  
**Je veux** consulter l'historique des sessions passées de ma campagne  
**Afin de** me remémorer les événements et suivre la progression

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page Détails Campagne → Onglet "Historique" (en plus de "Chapitres", "Joueurs")
- [ ] Liste sessions avec :
  - [ ] Date session
  - [ ] Durée (HH:MM)
  - [ ] Statut (Terminée, En cours, Abandonnée)
  - [ ] Chapitres joués
  - [ ] Nombre participants présents
- [ ] Clic session → Page détails session :
  - [ ] Timeline complète (changements chapitres, combats, événements)
  - [ ] Liste participants avec présence
  - [ ] Logs activité (messages chat sauvegardés)
  - [ ] Statistiques (XP gagné, ennemis vaincus, objets obtenus)
- [ ] Tri par date (descendant par défaut)
- [ ] Filtre par statut (Toutes, Terminées, Abandonnées)
- [ ] Export PDF session (optionnel, future US)

### Techniques
- [ ] Endpoint : `GET /api/campaigns/{campaignId}/sessions`
- [ ] Query params : `?status=completed&page=1&pageSize=20`
- [ ] Response 200 :
```json
{
  "sessions": [
    {
      "id": "guid",
      "startedAt": "2025-01-20T14:00:00Z",
      "endedAt": "2025-01-20T17:30:00Z",
      "duration": 210,
      "status": "Completed",
      "chaptersPlayed": [
        { "id": "guid", "title": "Chapitre 1", "orderNumber": 1 }
      ],
      "participantCount": 4,
      "presentCount": 3
    }
  ],
  "totalCount": 12,
  "page": 1,
  "pageSize": 20
}
```
- [ ] Endpoint : `GET /api/sessions/{id}/timeline`
- [ ] Response 200 : Array de SessionEvent avec détails

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SessionService.GetCampaignSessions_ReturnsOrderedByDate()`
- [ ] `SessionService.GetSessionTimeline_ReturnsAllEvents()`
- [ ] `SessionService.GetSessionStats_CalculatesCorrectly()`

### Tests d'Intégration
- [ ] `SessionEndpoint_GetSessions_ReturnsFilteredResults()`
- [ ] `SessionEndpoint_GetTimeline_IncludesAllEventTypes()`

### Tests E2E
- [ ] MJ consulte historique → Voit liste sessions
- [ ] MJ clique session → Voit timeline complète
- [ ] Joueur consulte historique → Voit seulement sessions auxquelles il a participé

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `SessionService.GetCampaignSessionsAsync(campaignId, status, page, pageSize)` :
  - [ ] Query avec filtres
  - [ ] Include Participants, Chapters
  - [ ] Calculer duration (EndedAt - StartedAt)
  - [ ] Mapper vers DTO
- [ ] Créer `SessionService.GetSessionTimelineAsync(sessionId)` :
  - [ ] Récupérer tous SessionEvents
  - [ ] Ordonner par OccurredAt
  - [ ] Enrichir avec détails (noms joueurs, titres chapitres, etc.)
- [ ] Créer `SessionService.GetSessionStatsAsync(sessionId)` :
```csharp
public class SessionStatsDto
{
    public int TotalRounds { get; set; } // Combats
    public int EnemiesDefeated { get; set; }
    public int XPGained { get; set; }
    public int ItemsFound { get; set; }
    public Dictionary<string, int> DiceRolls { get; set; } // Ex: {"d20": 45, "d6": 23}
}
```
- [ ] Créer endpoints :
  - [ ] `GET /api/campaigns/{campaignId}/sessions` [Authorize]
  - [ ] `GET /api/sessions/{id}/timeline` [Authorize]
  - [ ] `GET /api/sessions/{id}/stats` [Authorize]

### Frontend
- [ ] Créer page `SessionHistory.razor` (/campaigns/{id}/history) :
```razor
<div class="session-history">
    <h2>Historique des Sessions</h2>
    
    <div class="filters">
        <select @bind="StatusFilter">
            <option value="">Toutes</option>
            <option value="Completed">Terminées</option>
            <option value="Active">En cours</option>
        </select>
    </div>
    
    <div class="session-list">
        @foreach (var session in Sessions)
        {
            <SessionHistoryCard Session="@session" OnClick="() => NavigateToDetails(session.Id)" />
        }
    </div>
    
    <Pagination CurrentPage="@CurrentPage" TotalPages="@TotalPages" OnPageChanged="LoadSessions" />
</div>
```
- [ ] Créer composant `SessionHistoryCard.razor` :
```razor
<div class="session-card" @onclick="OnClick">
    <div class="session-header">
        <span class="date">@Session.StartedAt.ToString("dd MMM yyyy - HH:mm")</span>
        <span class="duration">⏱️ @FormatDuration(Session.Duration)</span>
        <span class="status @Session.Status.ToString().ToLower()">@Session.Status</span>
    </div>
    
    <div class="session-body">
        <div class="chapters">
            <strong>Chapitres joués :</strong>
            @string.Join(", ", Session.ChaptersPlayed.Select(c => c.Title))
        </div>
        <div class="participants">
            👥 @Session.PresentCount / @Session.ParticipantCount participants
        </div>
    </div>
</div>
```
- [ ] Créer page `SessionDetails.razor` (/sessions/{id}/details) :
  - [ ] Section "Informations Générales"
  - [ ] Section "Timeline" (composant `SessionTimeline.razor`)
  - [ ] Section "Statistiques" (composant `SessionStats.razor`)
  - [ ] Section "Participants" (liste avec présence)
- [ ] Créer composant `SessionTimeline.razor` :
```razor
<div class="timeline">
    @foreach (var evt in Events)
    {
        <div class="timeline-item @evt.Type.ToString().ToLower()">
            <div class="timeline-marker">
                @GetEventIcon(evt.Type)
            </div>
            <div class="timeline-content">
                <span class="time">@evt.OccurredAt.ToString("HH:mm")</span>
                <span class="description">@GetEventDescription(evt)</span>
            </div>
        </div>
    }
</div>

@code {
    private string GetEventIcon(SessionEventType type) => type switch
    {
        SessionEventType.ChapterChanged => "📖",
        SessionEventType.CombatStarted => "⚔️",
        SessionEventType.CombatEnded => "🏆",
        SessionEventType.PlayerJoined => "👋",
        SessionEventType.PlayerLeft => "🚪",
        _ => "•"
    };

    private string GetEventDescription(SessionEventDto evt)
    {
        // Parse evt.Data JSON et formatter description
    }
}
```

### Base de Données
- [ ] Index existants suffisent (SessionEvents indexé par SessionId)

---

## 🔗 Dépendances

### Dépend de
- [US-018](./US-018-lancement-session.md) - Sessions
- [US-019](./US-019-progression-chapitres.md) - SessionEvents

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Faible (CRUD + Affichage)
- Effort : 1 jour
- Risques : Aucun

---

## 📝 Notes Techniques

### Calcul Duration
```csharp
public int GetDuration() // Retourne minutes
{
    if (EndedAt == null) return 0;
    return (int)(EndedAt.Value - StartedAt).TotalMinutes;
}
```

### Format Duration UI
```csharp
private string FormatDuration(int minutes)
{
    var hours = minutes / 60;
    var mins = minutes % 60;
    return $"{hours}h{mins:00}";
}
```

### Timeline Event Description Examples
```
📖 14:05 - Passage au Chapitre 2: "L'attaque des gobelins"
⚔️ 14:12 - Combat commencé (3 gobelins)
🏆 14:28 - Combat terminé (Victoire)
👋 14:30 - Alice a rejoint la session
🚪 14:55 - Bob a quitté la session
```

### Permissions
- **GameMaster** : Voit toutes sessions de ses campagnes
- **Player** : Voit seulement sessions auxquelles il a participé

```csharp
var query = _context.Sessions
    .Where(s => s.CampaignId == campaignId);

if (!User.IsInRole("GameMaster"))
{
    var userId = User.GetUserId();
    query = query.Where(s => s.Participants.Any(p => p.UserId == userId));
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Liste sessions fonctionnelle avec filtres
- [ ] Page détails session complète
- [ ] Timeline affichée correctement
- [ ] Statistiques calculées
- [ ] Permissions respectées (MJ vs Joueur)
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 5
