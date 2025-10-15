# US-002 - Connexion Utilisateur

## 📝 Description

**En tant qu'** utilisateur enregistré  
**Je veux** me connecter avec mon email et mot de passe  
**Afin de** accéder à mon compte et mes parties de jeu de rôle

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [x] Le formulaire de connexion contient : email, mot de passe
- [x] L'utilisateur peut cocher "Se souvenir de moi" (optionnel)
- [x] Les identifiants sont vérifiés contre la base de données
- [x] Le mot de passe est vérifié avec BCrypt
- [x] Un JWT valide est généré et retourné
- [x] Le token contient : userId, email, roles, expiration
- [x] Le token est stocké côté client (sessionStorage ou localStorage)
- [x] L'utilisateur est redirigé vers le dashboard
- [x] Message d'erreur clair si identifiants incorrects
- [x] Délai de sécurité après échec (rate limiting)

### Techniques
- [x] Endpoint : `POST /api/auth/login`
- [x] Body : `{ "email": "user@example.com", "password": "SecurePass123!" }`
- [x] Response 200 : `{ "token": "jwt.token.here", "expiresAt": "2025-10-22T12:00:00Z", "user": { "id": "guid", "email": "user@example.com" } }`
- [x] Response 401 : `{ "error": "Identifiants incorrects" }`
- [x] Response 429 : `{ "error": "Trop de tentatives, réessayez dans X minutes" }`

---

## 🧪 Tests

### Tests Unitaires
- [x] `AuthService.Login_WithValidCredentials_ReturnsJWT()`
- [x] `AuthService.Login_WithInvalidEmail_ThrowsException()`
- [x] `AuthService.Login_WithInvalidPassword_ThrowsException()`
- [x] `JwtService.GenerateToken_WithUserData_ReturnsValidToken()`
- [x] `JwtService.ValidateToken_WithValidToken_ReturnsTrue()`

### Tests d'Intégration
- [x] `LoginEndpoint_WithValidCredentials_ReturnsJWT()`
- [x] `LoginEndpoint_WithInvalidPassword_Returns401()`
- [x] `LoginEndpoint_WithNonExistentUser_Returns401()`

### Tests E2E
- [x] Connexion réussie → Redirection vers dashboard
- [x] Connexion échouée → Message d'erreur affiché
- [x] Token valide → Accès aux pages protégées

---

## 🔧 Tâches Techniques

### Backend
- [x] Créer `LoginRequest` DTO
- [x] Créer `LoginResponse` DTO avec JWT
- [x] Implémenter `AuthService.LoginAsync()`
  - [x] Recherche utilisateur par email
  - [x] Vérification mot de passe BCrypt
  - [x] Génération JWT
  - [x] Logging tentatives de connexion
- [x] Créer endpoint `POST /api/auth/login`
- [x] Configurer JWT (SecretKey, Issuer, Audience, ExpiryDays)
- [x] Implémenter rate limiting (5 tentatives / 15 min)

### Frontend
- [x] Créer composant `LoginForm.razor`
- [x] Créer page `Login.razor` (/login)
- [x] Implémenter `AuthenticationService.LoginAsync()`
- [x] Stocker token dans localStorage
- [x] Implémenter `AuthStateProvider` (Blazor)
- [x] Redirection après connexion

### Base de Données
- [x] Ajouter index sur `Users.Email` pour performance
- [x] Créer table `LoginAttempts` pour rate limiting

---

## 🔗 Dépendances

### Dépend de
- [US-001](./US-001-inscription-utilisateur.md) - Inscription utilisateur

### Bloque
- [US-003](./US-003-gestion-jwt.md) - Gestion JWT
- [US-004](./US-004-profil-utilisateur.md) - Profil utilisateur
- Toutes les fonctionnalités nécessitant authentification

---

## 📊 Estimation

**Story Points** : 3

---

## 📝 Notes Techniques

### Configuration JWT
```json
{
  "Authentication": {
    "JWT": {
      "SecretKey": "votre-clé-secrète-64-caractères-minimum",
      "Issuer": "ChroniqueMonde",
      "Audience": "ChroniqueMonde-Users",
      "ExpiryDays": 7
    }
  }
}
```

### Génération du Token
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, "Player")
};

var token = new JwtSecurityToken(
    issuer: _config["JWT:Issuer"],
    audience: _config["JWT:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddDays(7),
    signingCredentials: credentials
);
```

---

## ✅ Definition of Done

- [x] Code implémenté et testé
- [x] Tests unitaires passent
- [x] Tests d'intégration passent
- [x] Tests E2E passent
- [x] Rate limiting fonctionnel
- [x] Documentation API mise à jour
- [x] Mergé dans main

---

**Statut** : ✅ Terminé  
**Assigné à** : Tommy ANGIBAUD  
**Date de complétion** : 15 octobre 2025
