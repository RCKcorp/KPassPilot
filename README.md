# KPassPilot

KPassPilot est un plugin KeePass 2 qui automatise la création d'entrées à partir de profils d'équipements et de modèles configurables.

## Présentation

Au lieu de créer manuellement plusieurs entrées KeePass pour un équipement, KPassPilot collecte les informations nécessaires, applique un profil, génère un aperçu puis crée automatiquement l'arborescence et les entrées correspondantes dans la base KeePass ouverte.

```text
Informations équipement
        ↓
Profil
        ↓
Templates + blocs
        ↓
Aperçu
        ↓
Création dans KeePass
```

## Fonctionnalités

- création guidée d'un équipement depuis KeePass ;
- détection ou sélection d'un profil ;
- profils d'équipements configurables ;
- entrées propres à un profil et modèles partagés ;
- composition visuelle des champs Titre, Identifiant, Mot de passe et Notes ;
- blocs de texte fixe, source dynamique ou valeur globale ;
- extraction des premiers ou derniers caractères et découpe par tranche ;
- transformation en majuscules ou minuscules ;
- aperçu avant création ;
- prévention des doublons de groupes ;
- écriture directe dans la base KeePass ouverte ;
- import et export de la configuration XML ;
- compilation automatique de la DLL et du PLGX avec GitHub Actions.

## Exemple de fonctionnement

Un template peut être composé à partir de plusieurs blocs :

```text
[Texte "DEMO-"] + [ID] + [Texte "-"] + [4 derniers caractères du Serial]
```

Avec :

```text
ID     = 4827
Serial = DEMO-SN-938271
```

le moteur produit :

```text
DEMO-4827-8271
```

Les valeurs ci-dessus sont uniquement des données d'exemple. La logique des profils et des templates est entièrement configurable.

## Résultat dans KeePass

Selon le profil choisi, KPassPilot peut créer une arborescence de ce type :

```text
Infrastructure
└── NORTH.LAPTOP.TEST.4827
    ├── Maintenance
    ├── Application
    └── Recovery
```

## Architecture rapide

```text
KeePass
   │
   ▼
KPassPilotExt
   │
   ├── CreatePCForm
   ├── ConfigEditorForm
   ├── BlockEditorForm
   │
   ▼
PCGenerator
   │
   ▼
GeneratedEntry
   │
   ▼
PwEntry / PwGroup KeePass
```

## Documentation

| Guide | Contenu |
|---|---|
| **[Guide utilisateur](docs/USER-GUIDE.md)** | Installation, premier lancement, création d'un équipement, aperçu et import/export |
| **[Configuration](docs/CONFIGURATION.md)** | Profils, modèles partagés, champs, blocs, valeurs globales et transformations |
| **[Architecture](docs/ARCHITECTURE.md)** | Classes C#, flux interne, moteur de génération, intégration KeePass et CI |
| **[Sécurité](docs/SECURITY.md)** | Modèle de sécurité, stockage local, gestion des secrets, import/export et chaîne de build |

## Configuration locale

KPassPilot stocke sa configuration dans :

```text
%APPDATA%\KPassPilot\KPassPilot.config.xml
```

La configuration contient les profils et règles de génération utilisés par le plugin. Elle doit être protégée selon la sensibilité des conventions qu'elle contient.

## Compilation locale

Prérequis :

- Windows ;
- .NET Framework 4.7.2 ;
- KeePass 2.x ;
- MSBuild ou Visual Studio.

```powershell
.\build.ps1
```

Il est également possible d'indiquer explicitement le chemin de KeePass :

```powershell
.\build.ps1 -KeePassPath "C:\Path\To\KeePass.exe"
```

## Intégration continue

Le workflow **Build KPassPilot** exécute :

```text
Checkout
   ↓
Installation KeePass
   ↓
Compilation KPassPilot.dll
   ↓
Création KPassPilot.plgx
   ↓
Publication de l'artefact KPassPilot-plugin
```

Les sorties de build sont disponibles depuis GitHub Actions.

## Structure du projet

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
├── KPassPilotExt.cs
├── Models.cs
├── PCGenerator.cs
├── Theme.cs
├── build.ps1
└── KPassPilot.csproj
```
