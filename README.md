# TransportDouala

Application mobile de mobilité urbaine pour **Douala** : consulter les lignes, les arrêts et les tarifs moyens par trajet, rechercher un itinéraire, signaler un tarif constaté et retrouver ses favoris.

Projet réalisé dans le cadre du cours **Atelier de développement mobile (GLO5)**.

**Auteur :** ATANGANA AYISSI NESTOR

## Problème traité

Les usagers du transport urbain à Douala manquent de repères sur les itinéraires, les arrêts et les tarifs usuels, ce qui favorise les abus.

## Fonctionnalités

- Lignes et arrêts
- Tarif moyen par trajet
- Recherche d'itinéraire
- Position de l'utilisateur (simulée pour l'instant)
- Signalement d'un tarif constaté
- Favoris (lignes et itinéraires)
- Connexion et création de compte

## Technologies

- .NET MAUI (cible **Android uniquement**), .NET 10
- Architecture MVVM avec `CommunityToolkit.Mvvm`
- Base locale **SQLite** (`sqlite-net-pcl`) : comptes (mot de passe haché PBKDF2), lignes, arrêts, signalements, favoris, récents
- Géolocalisation Android (`Geolocation`) avec repli sur une position de démonstration

## Données

Lignes, arrêts et tarifs de départ : jeux **GTFS WhereIsMyTransport** pour Douala (Socatur Bus et Douala Yellow Taxis, 2017-2020), convertis en `Resources/Raw/transport_data.json` (278 lignes, 439 noms d'arrêts). Ces données sont **anciennes et indicatives** : les tarifs moyens affichés combinent ce jeu de données et les signalements des usagers. Vérifiez la licence des jeux d'origine avant toute réutilisation.

## Écoconception

Interface sobre, sans image bitmap ni police embarquée, icônes en SVG, carte schématique dessinée en vectoriel (aucune tuile en ligne), composants réutilisables et données locales.

## Lancer le projet

1. Installer Visual Studio avec la charge de travail **.NET MAUI** et le développement Android.
2. Ouvrir la solution `TransportDouala.sln`.
3. Choisir un émulateur Android (ou un téléphone en débogage USB) puis lancer avec **F5**.

Créer un compte au premier lancement (les comptes sont stockés localement sur l'appareil). Sur l'émulateur, régler la position dans Extended controls > Location (par exemple à Douala : 4.0417, 9.7424).

## Structure

```
TransportDouala/
├── Controls/     composants réutilisables (FormField)
├── Drawing/      carte schématique
├── Models/       modèles de données
├── Services/     accès aux données (simulé), favoris, authentification
├── ViewModels/   logique des écrans (MVVM)
├── Views/        les 8 écrans
├── Resources/    styles, couleurs, icônes SVG
└── Platforms/    code spécifique Android
docs/             arborescence, justification, maquettes, captures d'écran
```

## État d'avancement

- [x] Maquettes et arborescence
- [x] Frontend (8 écrans) avec données simulées
- [x] Base de données locale SQLite
- [x] Données réelles GTFS et géolocalisation
- [ ] Tests
