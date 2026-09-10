# Guide utilisateur KPassPilot

Ce document présente KPassPilot de bout en bout : installation, premier lancement, création d'un équipement et résultat dans KeePass.

## 1. À quoi sert KPassPilot ?

KPassPilot est un plugin KeePass 2 qui automatise la création d'un ensemble d'entrées à partir d'un équipement.

Le principe est simple :

```text
Informations sur l'équipement
        ↓
Détection / choix d'un profil
        ↓
Application des modèles et blocs
        ↓
Aperçu
        ↓
Création dans la base KeePass ouverte
```

## 2. Installation

### Depuis GitHub Actions

1. Ouvrir l'onglet **Actions** du dépôt.
2. Ouvrir le workflow **Build KPassPilot**.
3. Télécharger l'artefact **KPassPilot-plugin** du dernier build réussi.
4. Extraire `KPassPilot.dll` ou utiliser `KPassPilot.plgx` selon le mode d'installation souhaité.
5. Dans KeePass, ouvrir **Outils > Plugins > Ouvrir le dossier**.
6. Installer le plugin puis redémarrer KeePass.

Le plugin apparaît ensuite dans le menu principal sous **KPassPilot**.

## 3. Premier lancement

Au démarrage, KPassPilot cherche sa configuration locale dans :

```text
%APPDATA%\KPassPilot\KPassPilot.config.xml
```

Si aucun fichier n'existe, une configuration d'exemple est créée automatiquement afin de permettre une prise en main immédiate.

## 4. Menu KPassPilot

Le menu expose trois fonctions principales.

### Créer un équipement

Ouvre l'assistant de génération et d'écriture dans KeePass.

### Configuration

Ouvre l'éditeur des profils, modèles partagés, entrées et blocs.

### À propos

Affiche les informations du plugin et permet d'importer ou d'exporter la configuration XML.

## 5. Créer un équipement

Ouvrir **KPassPilot > Créer un nouvel équipement**.

### Étape 1 — Saisir l'identifiant

L'exemple fourni utilise le format :

```text
ZONE.CATEGORY.ENV.ID
```

Exemple :

```text
NORTH.LAPTOP.TEST.4827
```

Le moteur sépare cet identifiant en quatre valeurs :

| Segment | Exemple |
|---|---|
| Zone | `NORTH` |
| Category | `LAPTOP` |
| Environment | `TEST` |
| ID | `4827` |

### Étape 2 — Saisir le numéro de série

Le numéro de série peut être utilisé par les modèles pour composer certains champs.

```text
DEMO-SN-938271
```

### Étape 3 — Détection du profil

KPassPilot tente de faire correspondre la partie `CATEGORY` avec un profil configuré.

Avec `NORTH.LAPTOP.TEST.4827`, le profil `LAPTOP` peut être sélectionné automatiquement.

L'utilisateur peut modifier le profil avant la génération.

### Étape 4 — Choisir le dossier KeePass

KPassPilot affiche l'arborescence de la base KeePass ouverte et permet de choisir le groupe parent dans lequel l'équipement sera créé.

### Étape 5 — Vérifier l'aperçu

Avant toute écriture, l'aperçu présente notamment :

- le profil sélectionné ;
- les entrées à générer ;
- le titre de chaque entrée ;
- l'identifiant ;
- le mot de passe calculé ;
- l'arborescence cible.

### Étape 6 — Créer dans KeePass

Le bouton **Créer dans KeePass** crée un groupe portant le nom complet de l'équipement puis y ajoute les entrées générées.

```text
Infrastructure
└── NORTH.LAPTOP.TEST.4827
    ├── Maintenance
    ├── Application
    └── Recovery
```

Le contenu exact dépend du profil configuré.

## 6. Génération d'une entrée

Chaque entrée contient quatre champs configurables :

```text
Titre
Identifiant
Mot de passe
Notes
```

Chaque champ est composé d'une suite de blocs.

Exemple :

```text
"DEMO-" + ID + "-" + 4 derniers caractères du numéro de série
```

Avec :

```text
ID = 4827
Serial = DEMO-SN-938271
```

le moteur peut produire :

```text
DEMO-4827-8271
```

Cet exemple illustre le moteur de composition ; il ne constitue pas une recommandation de politique de mot de passe.

## 7. Prévention des doublons

Avant la création, KPassPilot vérifie si un groupe portant déjà le même nom existe dans le groupe parent sélectionné.

Si c'est le cas, la création est interrompue afin d'éviter un doublon accidentel.

## 8. Modifier les profils

Ouvrir **KPassPilot > Configuration**.

Il est possible de :

- créer ou renommer un profil ;
- définir sa description et son icône ;
- ajouter des entrées propres au profil ;
- rattacher des modèles partagés ;
- modifier les champs et les blocs ;
- gérer les valeurs globales ;
- vérifier le résultat dans l'aperçu.

Le fonctionnement détaillé est documenté dans [CONFIGURATION.md](CONFIGURATION.md).

## 9. Importer et exporter une configuration

Depuis **À propos**, KPassPilot peut sérialiser sa configuration dans un fichier XML.

### Export

Crée une copie de la configuration courante.

### Import

Charge la configuration contenue dans le fichier sélectionné et remplace celle actuellement utilisée.

Un export peut contenir des conventions sensibles. Il doit être protégé comme tout autre fichier de configuration.

## 10. Où sont stockées les données ?

La configuration KPassPilot est locale au profil Windows :

```text
%APPDATA%\KPassPilot\KPassPilot.config.xml
```

Les entrées générées sont écrites directement dans la base KeePass ouverte via l'API KeePass.

Aucun serveur distant n'est nécessaire au fonctionnement du plugin.

## 11. Parcours utilisateur résumé

```text
Installer le plugin
      ↓
Ouvrir une base KeePass
      ↓
Configurer les profils si nécessaire
      ↓
Créer un équipement
      ↓
Saisir identifiant + numéro de série
      ↓
Choisir le groupe cible
      ↓
Contrôler l'aperçu
      ↓
Créer
      ↓
Enregistrer la base KeePass
```

## Pour aller plus loin

- [Configuration et moteur de templates](CONFIGURATION.md)
- [Architecture technique](ARCHITECTURE.md)
- [Sécurité](SECURITY.md)
