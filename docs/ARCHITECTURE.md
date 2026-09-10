# Architecture technique de KPassPilot

Ce document décrit l'organisation interne de KPassPilot.

## 1. Vue générale

KPassPilot est un plugin KeePass 2 écrit en C# pour .NET Framework 4.7.2.

```text
Interface utilisateur
        ↓
Configuration et modèles
        ↓
Moteur de génération
        ↓
API KeePass
```

## 2. Flux principal

Lorsqu'un utilisateur crée un équipement :

```text
CreatePCForm
    ↓
PCGenerator.TryParse()
    ↓
Sélection du PCType
    ↓
Résolution des EntryRef
    ↓
PCGenerator.Generate()
    ↓
GeneratedEntry
    ↓
PwEntry / PwGroup KeePass
```

## 3. Point d'entrée du plugin

### `KPassPilotExt.cs`

`KPassPilotExt` hérite de `KeePass.Plugins.Plugin`.

Il gère :

- l'initialisation du plugin ;
- le chargement et l'enregistrement de la configuration ;
- l'initialisation d'une configuration d'exemple au premier lancement ;
- la création du menu KPassPilot ;
- l'ouverture des différentes fenêtres.

Le fichier de configuration local est :

```text
%APPDATA%\KPassPilot\KPassPilot.config.xml
```

## 4. Modèle de données

### `KPassPilotConfig`

Objet racine contenant notamment :

```text
GlobalValues
Models
PCTypes
```

### `PCType`

Représente un profil d'équipement avec son nom, sa description, son icône et ses entrées.

### `EntryTemplate`

Décrit une entrée KeePass configurable :

```text
EntryTemplate
├── Title
├── Username
├── Password
└── Notes
```

Chaque champ est constitué d'une liste de `Block`.

### `EntryRef`

Permet à un profil de référencer une entrée locale ou un modèle partagé.

### `Block`

Un bloc représente une portion d'un champ. Trois types sont disponibles :

```text
Text
Source
Prop
```

Un bloc peut aussi définir une découpe et une transformation de casse.

### `GeneratedEntry`

Objet produit par le moteur avant écriture dans KeePass :

```text
Title
Username
Password
Notes
IconId
```

## 5. Parsing et génération

`PCGenerator.cs` contient la logique fonctionnelle principale :

- analyse de l'identifiant d'un équipement ;
- exposition des sources utilisables par les blocs ;
- découpe des sources ;
- transformation de casse ;
- concaténation des blocs ;
- génération des entrées.

L'identifiant fourni dans les exemples suit la convention :

```text
ZONE.CATEGORY.ENV.ID
```

Exemple :

```text
NORTH.LAPTOP.TEST.4827
```

Le moteur extrait :

```text
Zone        = NORTH
Category    = LAPTOP
Environment = TEST
Id          = 4827
```

## 6. Résolution d'un champ

```text
Liste<Block>
    ↓
BlockValue()
    ↓
Découpe éventuelle
    ↓
Transformation de casse
    ↓
Concaténation
    ↓
FieldValue()
```

Exemple :

```text
[Text "DEMO-"]
[Source ID]
[Text "-"]
[Source Serial, Last 4]
```

peut produire :

```text
DEMO-4827-8271
```

## 7. Interface de création

### `CreatePCForm.cs`

Cette fenêtre gère :

- la saisie de l'identifiant ;
- la saisie du numéro de série ;
- la détection du profil ;
- le choix du groupe KeePass ;
- l'aperçu ;
- la création finale.

KPassPilot utilise les objets KeePass `PwGroup`, `PwEntry` et `ProtectedString` pour l'écriture dans la base.

## 8. Interface de configuration

### `ConfigEditorForm.cs`

Permet de gérer les profils, entrées locales, modèles partagés, valeurs globales, champs, blocs et aperçu en temps réel.

### `BlockEditorForm.cs`

Permet de configurer :

```text
Nature
├── Texte fixe
├── Source dynamique
└── Valeur globale

Découpe
├── Tout
├── Premiers caractères
├── Derniers caractères
└── Tranche

Casse
├── Inchangée
├── Majuscules
└── Minuscules
```

### Autres formulaires

- `TypeEditorForm.cs` : création et modification des profils ;
- `PropertiesForm.cs` : gestion des valeurs globales ;
- `IconPickerForm.cs` : sélection d'une icône KeePass ;
- `Dialogs.cs` : dialogues réutilisables ;
- `Theme.cs` : composants visuels et thème.

## 9. Import et export

`AboutForm.cs` gère l'import et l'export XML de la configuration via `System.Xml.Serialization.XmlSerializer`.

Le fichier exporté contient la configuration KPassPilot, pas la base KeePass.

## 10. Structure du dépôt

```text
KPassPilot/
├── .github/workflows/build.yml
├── docs/
│   ├── USER-GUIDE.md
│   ├── CONFIGURATION.md
│   ├── ARCHITECTURE.md
│   └── SECURITY.md
├── Properties/
├── AboutForm.cs
├── BlockEditorForm.cs
├── ConfigEditorForm.cs
├── CreatePCForm.cs
├── Dialogs.cs
├── IconPickerForm.cs
├── KPassPilot.csproj
├── KPassPilotExt.cs
├── Models.cs
├── PCGenerator.cs
├── PropertiesForm.cs
├── Theme.cs
├── TypeEditorForm.cs
└── build.ps1
```

## 11. Compilation locale

Le projet référence `KeePass.exe` pour accéder à l'API plugin.

```powershell
.\build.ps1
```

Le script localise KeePass et MSBuild, compile le projet puis vérifie la DLL produite.

## 12. Intégration continue

```text
Checkout
   ↓
Installation KeePass
   ↓
Compilation DLL
   ↓
Création PLGX
   ↓
Publication de l'artefact KPassPilot-plugin
```

## 13. Choix de conception

### Configuration séparée du code

Le moteur est générique et les conventions de génération sont stockées dans la configuration plutôt que dispersées dans le code.

### Aperçu avant écriture

La génération est calculée avant la création effective dans KeePass afin que l'utilisateur puisse vérifier le résultat.

### Modèles partagés

Ils permettent de réutiliser une même logique dans plusieurs profils sans duplication.

## Documentation associée

- [Guide utilisateur](USER-GUIDE.md)
- [Configuration et templates](CONFIGURATION.md)
- [Sécurité](SECURITY.md)
