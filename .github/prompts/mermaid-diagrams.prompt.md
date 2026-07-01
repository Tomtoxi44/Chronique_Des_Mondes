# Skill Mermaid – Diagrammes techniques

Skill transversal disponible dans tous les agents de certification (Bloc 1, 2, 3, 4).
Les diagrammes sont rendus visuellement dans l'interface Copilot CLI.

---

## Outils disponibles

| Outil | Paramètre | Description | Compétences |
|---|---|---|---|
| `mermaid_architecture` | – | Diagramme C4 de l'architecture complète du projet | C1.5, C2.2.1 |
| `mermaid_sequence` | `flow` | Diagramme de séquence : `auth` / `combat` / `campaign_creation` | C2.2.1, C2.2.2 |
| `mermaid_class` | `domain` | Diagramme de classes : `core` / `campaign` / `character` / `combat` / `all` | C1.5, C2.2.1 |
| `mermaid_gantt` | `type` | Planning Gantt : `certification` (2025/26) / `phases` (développement) | C1.4.1, C3.1 |
| `mermaid_er` | – | Schéma entité-relation de la base de données SQL Server | C1.5 |
| `mermaid_flowchart` | `process` | Logigramme : `bug_fix` / `deployment` / `ci_cd` / `user_feedback` | C3.2.2, C4.2.2 |
| `mermaid_state` | `machine` | Machine d'états : `combat` / `session` / `auth` | C2.2.1, C2.2.2 |

---

## Utilisation

Mentionner l'un de ces termes dans votre message active le skill automatiquement :
diagramme, architecture, séquence, gantt, schéma, logigramme, classes, états, ER, C4, UML.

---

## Correspondance certification

### Bloc 1 – Cadrage et analyse (rendu écrit 15 juin 2026 – passé)
- `mermaid_architecture` – justifier les choix d'architecture (C1.5)
- `mermaid_gantt` type `phases` – présenter l'estimation de charge (C1.4.1)
- `mermaid_er` – modélisation Merise du schéma de données (C1.5)
- `mermaid_class` domain `all` – modèle de domaine complet (C1.5)

### Bloc 2 – Développement (rendu écrit 23 juillet 2026 – priorité immédiate)
- `mermaid_sequence` flow `auth` – justifier l'implémentation JWT (C2.2.1)
- `mermaid_sequence` flow `combat` – documenter le flux SignalR (C2.2.2)
- `mermaid_flowchart` process `ci_cd` – pipeline CI/CD GitHub Actions (C2.1.2, C2.2.4)
- `mermaid_state` machine `auth` – scénarios de tests unitaires (C2.2.2)

### Bloc 3 – Pilotage (oral 17 septembre 2026)
- `mermaid_gantt` type `certification` – vue d'ensemble du planning (C3.1)
- `mermaid_flowchart` process `deployment` – processus de déploiement (C3.2.2)

### Bloc 4 – Maintenance (rendu écrit 20 août 2026)
- `mermaid_flowchart` process `bug_fix` – processus de correction (C4.2.1, C4.2.2)
- `mermaid_flowchart` process `user_feedback` – collecte des retours (C4.3.1, C4.3.3)
- `mermaid_state` machine `combat` – identifier les états à surveiller (C4.1.2)

---

## Exemples de commandes

```
Génère le diagramme d'architecture C4 du projet
Montre le diagramme de séquence du combat
Affiche le Gantt de certification 2025/2026
Génère le schéma ER de la base de données
Montre le logigramme de correction de bug
Diagramme d'états de l'authentification JWT
```
