# Configuration de KPassPilot

Ce document décrit le moteur de configuration de KPassPilot : profils, entrées, modèles partagés, valeurs globales et blocs.

## 1. Vue générale

KPassPilot fonctionne avec quatre niveaux :

```text
Profil d'équipement
    ↓
Entrées
    ↓
Champs KeePass
    ↓
Blocs
```

Exemple :

```text
Profil LAPTOP
├── Maintenance
│   ├── Titre
│   ├── Identifiant
│   ├── Mot de passe
│   └── Notes
└── Recovery
    ├── Titre
    ├── Identifiant
    ├── Mot de passe
    └── Notes
```

## 2. Profils d'équipements

Un profil décrit une catégorie d'équipement et les entrées qui doivent être créées pour cette catégorie.

Un profil possède :

- un nom ;
- une description ;
- une icône KeePass ;
- une liste d'entrées ;
- éventuellement des références vers des modèles partagés.

Les exemples fournis utilisent des profils génériques comme `LAPTOP`, `KIOSK` et `SERVER`.

## 3. Détection d'un profil

L'identifiant d'exemple suit la structure :

```text
ZONE.CATEGORY.ENV.ID
```

Exemple :

```text
NORTH.LAPTOP.TEST.4827
```

La partie `CATEGORY` peut être utilisée pour sélectionner automatiquement un profil correspondant, ici `LAPTOP`.

L'utilisateur reste libre de choisir un autre profil avant la génération.

## 4. Entrées locales et modèles partagés

### Entrée locale

Une entrée locale appartient uniquement à un profil.

```text
LAPTOP
└── Support local
```

Modifier cette entrée n'affecte aucun autre profil.

### Modèle partagé

Un modèle partagé peut être référencé par plusieurs profils.

```text
Modèle partagé : Inventory
        ↑           ↑
     LAPTOP       SERVER
```

Une modification du modèle est alors appliquée à tous les profils qui l'utilisent.

## 5. Champs d'une entrée

Chaque entrée contient quatre champs configurables :

```text
Titre
Identifiant
Mot de passe
Notes
```

Chaque champ est composé d'une liste ordonnée de blocs.

## 6. Types de blocs

### Texte fixe

Ajoute exactement le texte configuré.

```text
DEMO-
```

### Source dynamique

Récupère une donnée issue de l'équipement saisi, par exemple :

- nom complet ;
- zone ;
- catégorie ;
- environnement ;
- identifiant ;
- numéro de série.

### Valeur globale

Ajoute une valeur définie dans la configuration et réutilisable dans plusieurs templates.

## 7. Ordre des blocs

Les blocs sont évalués de gauche à droite.

```text
[Texte "DEMO-"] [ID] [Texte "-"] [Serial]
```

Avec :

```text
ID = 4827
Serial = SN938271
```

le résultat devient :

```text
DEMO-4827-SN938271
```

L'ordre peut être modifié depuis l'éditeur.

## 8. Découpe d'une source

Une source dynamique peut être utilisée entièrement ou partiellement.

### Tout

```text
SN938271 → SN938271
```

### Premiers caractères

```text
3 premiers caractères
SN938271 → SN9
```

### Derniers caractères

```text
4 derniers caractères
SN938271 → 8271
```

### Tranche

```text
à partir du caractère 3, longueur 4
ABCDEFGH → CDEF
```

Les bornes sont adaptées à la longueur disponible afin d'éviter les erreurs de découpe.

## 9. Transformation de casse

Un bloc peut conserver sa casse ou transformer son résultat :

```text
Inchangée
MAJUSCULES
minuscules
```

Exemple :

```text
North → NORTH
North → north
```

## 10. Exemple complet

Équipement :

```text
NORTH.LAPTOP.TEST.4827
Serial = DEMO-SN-938271
```

### Titre

```text
"Maintenance - " + Nom complet
```

Résultat :

```text
Maintenance - NORTH.LAPTOP.TEST.4827
```

### Identifiant

```text
Zone en minuscules + "\\operator"
```

Résultat :

```text
north\operator
```

### Mot de passe

```text
"DEMO-" + ID + "-" + 4 derniers caractères du Serial
```

Résultat :

```text
DEMO-4827-8271
```

Cet exemple illustre uniquement le fonctionnement du moteur de composition ; il ne constitue pas une recommandation de politique de mot de passe.

## 11. Valeurs globales

La fenêtre **Gérer les propriétés** permet d'ajouter, modifier ou supprimer des valeurs réutilisables dans les templates.

Exemples :

```text
DEMO-
-EXAMPLE
LAB
```

## 12. Aperçu en temps réel

L'éditeur de configuration recalcule les champs sur un équipement d'exemple à chaque modification.

L'aperçu permet de vérifier :

- le titre ;
- l'identifiant ;
- le mot de passe ;
- les notes.

## 13. Ajouter un profil

1. Ouvrir **Configuration**.
2. Cliquer sur **Ajouter** dans la zone des profils.
3. Donner un nom au profil.
4. Ajouter une description et une icône.
5. Ajouter des entrées locales ou rattacher des modèles partagés.
6. Construire les champs avec des blocs.
7. Vérifier l'aperçu.

## 14. Créer un modèle partagé

1. Ouvrir **Configuration**.
2. Dans **Modèles partagés**, cliquer sur **Nouveau**.
3. Donner un nom au modèle.
4. Configurer ses quatre champs.
5. Sélectionner un profil.
6. Cliquer sur **Depuis un modèle**.
7. Choisir le modèle à rattacher.

## 15. Import et export XML

La configuration complète peut être exportée en XML depuis la fenêtre **À propos** puis réimportée sur une autre installation.

Un export peut contenir des règles et conventions sensibles. Il doit être protégé comme tout autre fichier de configuration.

## 16. Bonnes pratiques

- privilégier des modèles simples et lisibles ;
- utiliser les modèles partagés pour éviter les duplications ;
- vérifier l'aperçu avant la création ;
- protéger les exports XML ;
- sauvegarder la configuration avant une modification importante.

## Documentation associée

- [Guide utilisateur](USER-GUIDE.md)
- [Architecture technique](ARCHITECTURE.md)
- [Sécurité](SECURITY.md)
