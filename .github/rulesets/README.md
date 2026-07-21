# Protection des branches `main` et `dev`

Objectif :
- **Seule `dev` peut être mergée sur `main`** (aucune autre branche).
- **`main` et `dev` ne peuvent pas être supprimées** (ni force-push).

GitHub ne sait pas restreindre nativement la *branche source* d'une PR. On combine donc :
1. un **workflow** (`.github/workflows/protect-main.yml`) qui fait échouer toute PR vers `main` ne provenant pas de `dev` ;
2. deux **rulesets** (`main.json`, `dev.json`) qui exigent la PR + le check, et interdisent suppression/force-push.

## Appliquer via l'interface GitHub (recommandé)

Pour **chaque** fichier (`main.json` puis `dev.json`) :
1. Repo → **Settings** → **Rules** → **Rulesets** → **New ruleset** → **Import a ruleset**.
2. Sélectionner le fichier JSON → **Create**.

> ⚠️ Le check requis est nommé **« Seule dev peut viser main »**. Il n'apparaît dans la liste des status checks qu'**après la première exécution** du workflow (donc après avoir ouvert une première PR vers `main`). Si l'import ne le retrouve pas, ouvre d'abord une PR de test `dev → main`, puis ajoute le check requis dans le ruleset `Protéger main`.

## Appliquer via `gh` CLI (alternative)

```bash
gh auth login   # une seule fois

gh api -X POST repos/Tomtoxi44/Chronique_Des_Mondes/rulesets \
  --input .github/rulesets/main.json

gh api -X POST repos/Tomtoxi44/Chronique_Des_Mondes/rulesets \
  --input .github/rulesets/dev.json
```

## Vérifier

- Une PR `feature/x → main` doit être **bloquée** (check « Seule dev peut viser main » en échec).
- Une PR `dev → main` passe le check.
- Tenter de supprimer `main` ou `dev` doit être **refusé**.

## Notes

- `bypass_actors` est **vide** : les règles s'appliquent aussi aux administrateurs (blocage strict demandé). Pour t'autoriser un contournement d'urgence, ajoute ton rôle dans *Bypass list* du ruleset.
- `required_approving_review_count: 0` : pas de review obligatoire (tu merges tes propres PR), seulement PR + check.
