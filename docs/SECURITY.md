# Sécurité — KPassPilot

Ce document décrit le modèle de sécurité de KPassPilot, les données manipulées par le plugin et les limites à prendre en compte lors de son utilisation.

## 1. Fonctionnement local

KPassPilot fonctionne localement avec KeePass 2.

- aucune API distante n'est nécessaire pour générer les entrées ;
- aucune base KeePass n'est envoyée vers un serveur par le plugin ;
- aucune télémétrie applicative n'est requise ;
- les profils et templates sont lus depuis la configuration locale de l'utilisateur.

## 2. Configuration locale

KPassPilot stocke sa configuration dans :

```text
%APPDATA%\KPassPilot\KPassPilot.config.xml
```

Cette configuration peut contenir :

- noms de profils ;
- modèles partagés ;
- valeurs globales ;
- règles de composition ;
- conventions de nommage ;
- paramètres de génération.

La configuration doit être protégée selon la sensibilité des conventions qu'elle contient.

## 3. Données KeePass

KPassPilot écrit directement dans la base KeePass actuellement ouverte au travers de l'API KeePass.

Lorsqu'une entrée est créée, le plugin utilise notamment :

```text
PwGroup
PwEntry
ProtectedString
```

La protection mémoire configurée dans KeePass pour les champs standards est respectée lors de l'écriture.

KPassPilot ne remplace pas les mécanismes de chiffrement, de verrouillage ou de protection mémoire fournis par KeePass.

## 4. Gestion des mots de passe

Les mots de passe générés sont construits en mémoire à partir des templates configurés puis transmis à KeePass pour création de l'entrée.

Le plugin n'a pas besoin d'un service distant pour effectuer cette génération.

Une règle de génération peut elle-même être sensible : connaître tous ses fragments et toutes ses transformations peut révéler la manière dont une organisation construit certaines valeurs. La configuration doit donc être traitée comme une donnée de sécurité lorsqu'elle contient ce type de logique.

## 5. Aperçu avant écriture

KPassPilot calcule les valeurs avant de modifier la base KeePass et présente un aperçu à l'utilisateur.

Cette étape permet de vérifier :

- le profil sélectionné ;
- les entrées qui seront créées ;
- les champs générés ;
- l'emplacement cible dans KeePass.

La création n'est effectuée qu'après validation de l'utilisateur.

## 6. Prévention des doublons

Avant de créer un équipement, KPassPilot vérifie si un groupe portant le même nom existe déjà sous le groupe parent sélectionné.

Cette vérification réduit le risque de création accidentelle de structures en double.

## 7. Import et export XML

KPassPilot peut importer et exporter sa configuration au format XML.

Un fichier exporté peut contenir l'ensemble des profils, valeurs globales et règles de composition. Il doit donc être protégé au même niveau que la configuration locale correspondante.

L'import remplace la configuration chargée par celle contenue dans le fichier sélectionné. Il est recommandé de vérifier la provenance et le contenu du fichier avant import.

## 8. Frontière de sécurité

KPassPilot s'exécute dans le contexte de KeePass et dépend de la sécurité du poste utilisateur, de KeePass et de la base ouverte.

Le plugin ne fournit pas :

- de stockage chiffré indépendant de KeePass ;
- de contrôle d'accès supplémentaire autour de la base ;
- de coffre distant ;
- de synchronisation réseau ;
- de mécanisme de rotation centralisée des secrets.

## 9. Chaîne de build

GitHub Actions reconstruit les sorties à partir du code source :

```text
Code source
    ↓
Compilation
    ↓
KPassPilot.dll
    ↓
Packaging KeePass
    ↓
KPassPilot.plgx
```

Le workflow permet de vérifier qu'une version du plugin peut être reconstruite de manière reproductible à partir du dépôt.

## 10. Bonnes pratiques d'utilisation

- protéger le profil Windows et la base KeePass ;
- contrôler les droits sur les fichiers de configuration exportés ;
- vérifier l'aperçu avant création ;
- conserver KeePass et ses composants à jour ;
- éviter de stocker directement dans les templates des secrets statiques qui n'ont pas besoin d'y être ;
- sauvegarder la base KeePass avant une modification importante de configuration.

## 11. Limites

KPassPilot automatise la création d'entrées mais ne valide pas qu'une politique de mot de passe ou qu'un modèle métier est adapté au contexte de l'organisation.

La qualité et la sécurité des valeurs générées dépendent des templates configurés par l'utilisateur.

## Documents liés

- [Guide utilisateur](USER-GUIDE.md)
- [Configuration et templates](CONFIGURATION.md)
- [Architecture technique](ARCHITECTURE.md)
