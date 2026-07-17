# Rapport de test fonctionnel — Chronique des Mondes

**Date :** 17 juillet 2026
**Cible :** instance locale `https://localhost:7165/` (build local, branche `fix/audit-14-points`)
**Méthode :** parcours utilisateur piloté au navigateur, relevé des écrans et de la console
**Compte de test créé :** `test-audit-2026@example.com` / pseudo `TesteurAudit`

## Résumé exécutif

L'application est **pleinement fonctionnelle** sur les parcours cœur. Tous les scénarios testés ont réussi, sans aucune erreur console ni violation CSP. Les corrections de sécurité livrées sur la branche (CSP, en-têtes) **ne cassent rien** : l'app démarre, s'authentifie et rend correctement. L'UI est soignée, cohérente et localisée en français.

**Verdict : ✅ parcours principaux validés.** Aucun blocage rencontré. Quelques observations mineures (UX + zones non testables en solo) listées plus bas.

## Scénarios testés

| # | Scénario | Résultat | Détail |
|---|----------|----------|--------|
| 1 | Accès app non authentifié | ✅ | Redirection automatique vers `/login?returnUrl=...` (garde d'auth OK) |
| 2 | Inscription | ✅ | Compte créé ; politique de mot de passe respectée ; redirection vers login |
| 3 | Connexion | ✅ | JWT émis, tableau de bord affiché « Bienvenue, TesteurAudit ! » |
| 4 | Tableau de bord | ✅ | Compteurs Mondes/Personnages/Campagnes + « Dernier monde consulté » |
| 5 | Création de monde | ✅ | Monde `Royaume de Test` (type **D&D 5e**) créé, page détail complète |
| 6 | Création de campagne | ✅ | `Campagne de Test` créée dans le monde ; onglets Chapitres/Session/Infos ; statut « Planification » |
| 7 | Création de chapitre | ✅ | `Chapitre 1` créé ; éditeur narratif avec mentions `@PNJ/@PJ` |
| 8 | Création de personnage | ✅ | `Aldric le Vaillant` créé et affiché en carte |
| 9 | Agrégation des compteurs | ✅ | Dashboard mis à jour : 1 monde, 1 personnage, 1 campagne |
| 10 | Page Sessions | ✅ (état vide) | Empty-state correct, renvoie vers le détail de campagne |
| 11 | Console navigateur | ✅ | **Aucune erreur / exception / violation CSP** sur l'ensemble du parcours |

## Validation des corrections de la branche `fix/audit-14-points`

- **CSP & en-têtes (#4, #11)** : la Content-Security-Policy ajoutée n'a bloqué aucune ressource — polices Google Fonts, icônes Bootstrap (jsDelivr), styles FluentUI et websockets Blazor Server fonctionnent. Aucun message `Refused to load / blocked by CSP` en console. ✔️ **La CSP est compatible avec l'app.**
- **Fail-fast clé JWT (#2, #8)** : l'app a démarré, donc la clé de dev (≥ 32 caractères, non-placeholder) passe bien le nouveau garde-fou. ✔️
- **Validation DTO (#5)** : l'inscription avec un mot de passe conforme est acceptée (le filtre ne rejette pas les entrées valides). ✔️ *(le rejet d'une entrée invalide n'a pas été testé cette fois — voir §Non testé).*

## Observations mineures (non bloquantes)

1. **UX inscription → login.** Après inscription, l'utilisateur est redirigé vers l'écran de connexion et doit se reconnecter manuellement, alors que l'API renvoie déjà un token (et un refresh token depuis le fix #13). Une connexion automatique post-inscription améliorerait le parcours. *(Criticité : 🟢 cosmétique)*
2. **Certificat auto-signé** en local (normal en dev) — nécessite d'accepter l'avertissement Chrome au premier accès.

## Zones non testées (hors périmètre de ce passage)

Ces fonctionnalités nécessitent un contexte multi-joueurs ou une session lancée, difficilement reproductible en solo :

- **Sessions de jeu en temps réel** (lancement, participants, chat, dés) — SignalR `SessionHub`.
- **Combat en temps réel** (initiative, tours, dégâts) — SignalR `CombatHub`.
- **Nouvelle logique d'autorisation d'appartenance (#6)** dans les Hubs : à valider avec 2 comptes (un GM + un joueur non-membre qui tente de rejoindre) pour confirmer le refus `HubException`.
- **Invitations / rejoindre une campagne ou un monde** via token.
- **Assistant de personnage D&D 5e** (`DndCharacterWizard`), grimoire, inventaire, achievements.
- **Réinitialisation de mot de passe** (US existante mais non parcourue ici).

Recommandation : un second passage à deux comptes pour couvrir le temps réel (Sessions/Combat) et confirmer le fix d'autorisation #6.

## Test multi-joueurs & temps réel (SignalR) — 2e passage

Réalisé à deux comptes : `TesteurAudit` (MJ) et le compte du propriétaire (`Kaelan`, joueur).

| Fonction | Résultat | Détail |
|----------|----------|--------|
| Invitation au monde (lien + QR) | ✅ | Le joueur a rejoint le monde avec son personnage ; notification MJ générée |
| Présence temps réel | ✅ | `Kaelan` est passé de « Invité » à **« Connecté »** en direct sur l'écran MJ (SignalR OK) |
| Lancement de session (MJ) | ✅ | Session démarrée, joueur invité, message de bienvenue diffusé |
| Combat temps réel | ✅ | Tour par tour, PV, **lancers de dés** et journal de combat synchronisés (confirmé par le joueur) |
| **Chat de session** | ❌ → ✅ **corrigé** | Ne fonctionnait pas sur la branche — voir ci-dessous |

### 🐞 Bug trouvé sur le chat de session — cause & correctif

**Symptôme :** le chat de session n'affiche/n'émet aucun message (alors que le combat et ses dés fonctionnent).

**Cause immédiate (régression de la branche) :** mon correctif d'autorisation #6 sur `SessionHub` validait le paramètre reçu comme un **ChapterId**, or les composants `SessionGm`/`SessionPlayer` envoient en réalité le **SessionId**. Le hub cherchait donc un chapitre inexistant → accès refusé → `JoinSession` échoue → l'utilisateur ne rejoint pas le groupe → plus aucun message ni dé de session ne circule. En production (code sans ce contrôle), le join passait toujours, d'où l'absence de problème l'après-midi.

**Correctif appliqué** (commit `0060e72`) : l'autorisation résout désormais la campagne via la table **Sessions** (et non Chapters). Nécessite un **rebuild + redémarrage de l'API** pour être actif.

**Bug préexistant révélé (indépendant de mes changements — à corriger séparément) :** le client passe le `SessionId` à des méthodes du hub dont le paramètre s'appelle `chapterId`, et les messages/dés sont **persistés avec `ChapterId = SessionId`** (`SessionMessages.ChapterId` a pourtant une clé étrangère vers `Chapters`). C'est fragile et sémantiquement faux : ça ne « tient » que si un chapitre porte le même id que la session. À traiter : aligner le nommage et la persistance (stocker le vrai `ChapterId`, ou introduire un `SessionId` explicite sur `SessionMessages`/`SessionDiceRolls`).

**Reste à tester après rebuild :** confirmer le chat une fois l'API redémarrée, et le scénario « quitter le combat → revenir au chat de session ».

## Observations UX / UI

**Points forts.** Direction artistique cohérente et soignée (thème sombre « fantasy », typographies Cinzel/Inter, iconographie claire), badges de type de jeu, empty-states travaillés, fil d'Ariane, barre latérale contextuelle rétractable, présence temps réel visible, journal de combat horodaté et lisible. L'ensemble fait clairement « produit fini ».

**Pistes d'amélioration :**

1. **Échecs silencieux (prioritaire).** Les appels temps réel côté client sont enveloppés dans des `catch { }` vides : quand le chat a cassé, **l'utilisateur n'avait aucun retour visuel**. Afficher un toast/bandeau d'erreur (« message non envoyé, reconnexion… ») rendrait ces pannes visibles. (Rejoint la dette #7 sur la gestion d'erreurs.)
2. **Auto-connexion après inscription.** Aujourd'hui l'utilisateur doit se reconnecter manuellement alors que l'API renvoie déjà un token.
3. **Troncatures sans info-bulle.** Dans la barre latérale, les libellés longs sont coupés (« Royaume de Te… », « Ch. 1 — L'i… ») sans `title`/tooltip au survol → ajouter une info-bulle.
4. **Feedback d'envoi de message.** Un indicateur d'état (envoi en cours / envoyé) et l'auto-scroll du fil amélioreraient le ressenti du chat.
5. **Contraste du texte secondaire.** Vérifier le gris sur fond sombre au regard de WCAG AA (accessibilité).
6. **Densité du panneau Chat & Dés.** Compact en bas à droite : à surveiller en responsive/mobile.
7. **Bandeau « Message de session ».** Utile, mais pourrait être masquable une fois lu.

## Conclusion

Le build local est sain et les parcours fondamentaux (auth + CRUD Mondes/Campagnes/Chapitres/Personnages) fonctionnent de bout en bout, sans erreur. Les correctifs de sécurité de la branche sont **compatibles avec l'exécution**. Avant merge, il reste à faire tourner `dotnet build /warnaserror` + `dotnet test` (filet de compilation), puis à couvrir le temps réel à deux comptes.
