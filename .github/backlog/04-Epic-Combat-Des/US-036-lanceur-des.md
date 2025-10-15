# US-036 - Lanceur de Dés Intégré

## 📝 Description

**En tant que** Joueur ou Maître du Jeu  
**Je veux** lancer des dés virtuels  
**Afin de** résoudre actions, attaques et jets de compétence

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Composant DiceRoller accessible partout dans CombatView
- [ ] Sélection type de dé : d4, d6, d8, d10, d12, d20, d100
- [ ] Nombre de dés (1-10)
- [ ] Modificateur (+/- valeur)
- [ ] Notation affichée : "2d6+3"
- [ ] Bouton "Lancer" → Animation → Résultat
- [ ] Affichage détail : `[4, 5] + 3 = 12`
- [ ] Historique derniers jets (5 max)
- [ ] Jets visibles par tous en temps réel (SignalR)
- [ ] MJ peut voir jets privés joueurs (optionnel)

### Techniques
- [ ] Endpoint : `POST /api/dice/roll`
- [ ] Body : `{ "diceType": "d20", "count": 1, "modifier": 5 }`
- [ ] Response : `{ "rolls": [15], "modifier": 5, "total": 20 }`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `DiceService.Roll_ValidInput_ReturnsResults()`
- [ ] `DiceService.Roll_MultipleD20_ReturnsCorrectCount()`

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `DiceService.RollAsync(dto)` :
```csharp
public DiceRollResultDto Roll(DiceRollDto dto)
{
    var rolls = new List<int>();
    
    for (int i = 0; i < dto.Count; i++)
    {
        rolls.Add(Random.Shared.Next(1, dto.DiceType + 1));
    }
    
    var total = rolls.Sum() + dto.Modifier;
    
    return new DiceRollResultDto
    {
        Rolls = rolls,
        Modifier = dto.Modifier,
        Total = total,
        Notation = $"{dto.Count}d{dto.DiceType}{(dto.Modifier >= 0 ? "+" : "")}{dto.Modifier}"
    };
}
```

### Frontend
- [ ] Créer composant `DiceRoller.razor` :
```razor
<div class="dice-roller">
    <h3>🎲 Lanceur de Dés</h3>
    
    <div class="dice-selection">
        <label>Type de dé</label>
        <select @bind="SelectedDiceType">
            <option value="4">d4</option>
            <option value="6">d6</option>
            <option value="8">d8</option>
            <option value="10">d10</option>
            <option value="12">d12</option>
            <option value="20">d20</option>
            <option value="100">d100</option>
        </select>
        
        <label>Nombre</label>
        <input type="number" @bind="Count" min="1" max="10" />
        
        <label>Modificateur</label>
        <input type="number" @bind="Modifier" />
    </div>
    
    <div class="notation">@GetNotation()</div>
    
    <button @onclick="RollDice" class="btn-primary">🎲 Lancer</button>
    
    @if (LastResult != null)
    {
        <div class="result">
            <div class="rolls">[@string.Join(", ", LastResult.Rolls)]</div>
            <div class="total">Total : @LastResult.Total</div>
        </div>
    }
    
    <div class="history">
        <h4>Historique</h4>
        @foreach (var roll in RollHistory)
        {
            <div>@roll.Notation = @roll.Total</div>
        }
    </div>
</div>

@code {
    private int SelectedDiceType { get; set; } = 20;
    private int Count { get; set; } = 1;
    private int Modifier { get; set; } = 0;
    private DiceRollResultDto? LastResult { get; set; }
    private List<DiceRollResultDto> RollHistory { get; set; } = new();

    private async Task RollDice()
    {
        var dto = new DiceRollDto
        {
            DiceType = SelectedDiceType,
            Count = Count,
            Modifier = Modifier
        };
        
        var result = await Http.PostAsJsonAsync("/api/dice/roll", dto);
        LastResult = await result.Content.ReadFromJsonAsync<DiceRollResultDto>();
        
        RollHistory.Insert(0, LastResult);
        if (RollHistory.Count > 5) RollHistory.RemoveAt(5);
        
        // Animation (optionnel)
        await AnimateDice();
    }

    private string GetNotation()
    {
        var mod = Modifier >= 0 ? $"+{Modifier}" : Modifier.ToString();
        return $"{Count}d{SelectedDiceType}{mod}";
    }

    private async Task AnimateDice()
    {
        // CSS animation
    }
}
```

---

## 📊 Estimation

**Story Points** : 5

---

## ✅ Definition of Done

- [ ] Lanceur de dés fonctionnel
- [ ] Tous types supportés
- [ ] Animation (optionnel)
- [ ] Historique jets
- [ ] SignalR notifications
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 10
