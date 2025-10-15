# US-039 - Fin de Combat et Résumé

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** terminer un combat et générer un résumé  
**Afin de** clôturer l'affrontement et consulter les statistiques

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Bouton "Terminer le Combat" (MJ uniquement)
- [ ] Modal confirmation avec options :
  - [ ] Victoire
  - [ ] Défaite
  - [ ] Fuite
  - [ ] Autre (texte libre)
- [ ] Génération résumé automatique :
  - [ ] Durée combat
  - [ ] Nombre de rounds
  - [ ] Dégâts totaux infligés
  - [ ] Participants vaincus
  - [ ] Actions effectuées
- [ ] Résumé affiché + ajouté à SessionHistory
- [ ] Retour à SessionLive
- [ ] XP attribués (futur, Phase 2)

### Techniques
- [ ] Endpoint : `POST /api/combats/{combatId}/end`
- [ ] Body : `{ "outcome": "Victory", "notes": "..." }`
- [ ] Response 200 : CombatSummaryDto

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.EndCombat_GeneratesSummary()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CombatService.EndCombatAsync(combatId, dto, userId)` :
```csharp
public async Task<CombatSummaryDto> EndCombatAsync(Guid combatId, EndCombatDto dto, Guid userId)
{
    var combat = await _context.Combats
        .Include(c => c.Session.Campaign)
        .Include(c => c.Participants)
        .Include(c => c.Rounds)
        .FirstOrDefaultAsync(c => c.Id == combatId);
    
    if (combat?.Session.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    combat.Status = CombatStatus.Completed;
    combat.EndedAt = DateTime.UtcNow;
    
    var summary = new CombatSummary
    {
        Outcome = dto.Outcome,
        Duration = (combat.EndedAt.Value - combat.StartedAt).TotalMinutes,
        TotalRounds = combat.CurrentRound,
        ParticipantsCount = combat.Participants.Count,
        DefeatedCount = combat.Participants.Count(p => p.IsDefeated),
        Notes = dto.Notes
    };
    
    // Log SessionEvent
    _context.SessionEvents.Add(new SessionEvent
    {
        SessionId = combat.SessionId,
        Type = SessionEventType.CombatEnded,
        Data = JsonSerializer.Serialize(summary),
        OccurredAt = DateTime.UtcNow,
        TriggeredBy = userId
    });
    
    await _context.SaveChangesAsync();
    
    return summary.ToDto();
}
```

### Frontend
- [ ] Modal `EndCombatModal.razor`
- [ ] Page `CombatSummary.razor`

---

## 📊 Estimation

**Story Points** : 3

---

## ✅ Definition of Done

- [ ] Fin de combat fonctionnelle
- [ ] Résumé généré
- [ ] Ajouté à historique session
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 10
