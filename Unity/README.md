# Configuration
- Unity Editor version : **2021.3.11f1**
- Unity template : **3D**

# Fonctionalités présentes
- Génération d'objets 3D au format Vertex-Face (quads)
  - Le Mesh Generator 
  - La Box
  - Le Chips
  - Le Polygone Régulier
  - Le Pacman
- Développement des classes de code
  - WingedEdge
    - Conversion VertexFace <-> WingedEdge
    - DrawGizmos
    - ConvertToCSVFormat
  - HalfEdge
    - Conversion VertexFace <-> HalfEdge
    - DrawGizmos
    - ConvertToCSVFormat
- Algorithme de Subdivision de Catmull-Clark avec HalfEdge

# Utilisation

## 1. Les objets 3D
Chaque objet 3D est présent dans la scène et activable pour l'utiliser.

## 2. Draw
Dans la fenetre inspector de Unity pour chaque objet 3D on peut choisir quels données afficher entre : None - Everything - Vertices - Edges - Faces - Handles - Centroid.

## 3. Mode
Dans la fenetre inspector de Unity pour chaque objet 3D on peut passer en mode : VertexFace - HalfEdge - WingedEdge. Le DrawGizmos sera alors modifié automatiquement.

## 4. Highlight & HighlightIndex
Dans la fenetre inspector de Unity pour chaque objet 3D on peut sélectionner une donnée à mettre en valeur entre : Vertex - Edge - Face et l'indice de celui-ci. 

Exemple j'ai envie de mettre en valeur l'edge d'indice 10 s'il existe, je séléctionne Highlight : Edge et HighlightIndex : 10, utile pour débugger.

## 5. Subdivide
Dans la fenetre inspector de Unity pour chaque objet 3D il faudra cliquer sur la coche "Subdivide" pour utiliser la méthode de subdivision dur l'objet.

## 6. Convert To CSV Format
Dans la fenetre inspector de Unity pour chaque objet 3D il faudra cliquer sur la coche "Convert To CSV Format" pour automatiquement copier dans le press-papier la structure de données correspondante en format CSV et l'afficher dans la console Unity.

# Répartition des tâches

## TADRES Nicolas - Pro des objets 3D

Je me suis chargé principalement du développement des objets 3D avec Raj Porus et j'ai aussi fait le développement de WingedEdge avec Antonin.

## HIRUTHAYARAJ Raj Porus - Pro de excel

J'ai participé au développement des objets 3D et j'ai également joué un rôle dans les conversions en CSV pour WingedEdge et HalfEdge.

## JUQUEL Antonin - Pro de Unity3D

J'ai développé la structure de données HalfEdge et avec l'aide de Nicolas et des ses cours WingedEdge c'est à dire pour les deux : le constructeur à partir d'un VertexFace, conversion vers VertexFace et DrawGizmos, de plus j'ai implémenté l'algorithme de Subdivision de Catmull-Clark avec HalfEdge.