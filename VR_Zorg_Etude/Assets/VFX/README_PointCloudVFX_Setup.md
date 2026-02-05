# Point Cloud VFX Graph Setup Instructions

Le fichier VFX Graph **ne peut pas être créé automatiquement** - il doit être créé manuellement dans Unity Editor.

## Prérequis

1. **Installer VFX Graph Package**:
   - Ouvrir Unity Editor
   - Menu: `Window` → `Package Manager`
   - Chercher "Visual Effect Graph"
   - Cliquer `Install` (si pas déjà installé)
   - Unity 2021.3.35f1 supporte VFX Graph ✅

## Étape 1: Créer le VFX Graph Asset

1. Dans Unity Project window, naviguer vers `Assets/VFX/`
2. Right-click → `Create` → `Visual Effects` → `Visual Effect Graph`
3. Nommer le fichier: **`PointCloudVFX`**
4. Double-cliquer sur `PointCloudVFX.vfx` pour ouvrir le VFX Graph Editor

## Étape 2: Configurer le System (Settings)

Dans le VFX Graph Editor:

1. Cliquer sur le node **"System"** (en haut)
2. Dans l'Inspector à droite, configurer:
   - **Capacity**: `1048576` (1024×1024 points)
   - **Bounds Mode**: `Manual`
   - **Bounds Size**: `10, 10, 10` (ajuster selon votre scène)

## Étape 3: Configurer Initialize Context

### 3.1 Supprimer Initialize par défaut

1. Cliquer sur le node **"Initialize Particle"** existant
2. Appuyer sur `Delete` pour le supprimer

### 3.2 Créer nouveau Initialize

1. Right-click dans le graph → `Create Node` → `Context` → `Initialize`
2. Connecter **Spawn** → **Initialize** → **Update**

### 3.3 Ajouter Attribute Map

Dans le node **Initialize**:

1. Cliquer sur le bouton `+` en bas du node
2. Sélectionner: `Attribute from Map`

### 3.4 Configurer les Blocks dans Initialize

Ajouter ces blocks (cliquer `+` dans Initialize, puis `Add Block`):

#### Block 1: Set Position
- Click `+` → `Set Position`
- Dans le block Inspector:
  - **Source**: `Attribute from GraphicsBuffer`
  - **GraphicsBuffer**: Créer une exposed property (voir Étape 4)
  - **Attribute**: `position`

#### Block 2: Set Size
- Click `+` → `Set Size`
- **Source**: `Inline`
- **Size**: `0.01` (ou exposed property "PointSize")

#### Block 3: Set Color (optionnel)
- Click `+` → `Set Color`
- **Source**: `Attribute from GraphicsBuffer` (si RGB activé)
- OU **Source**: `Inline` avec couleur blanche

## Étape 4: Créer Exposed Properties (Blackboard)

Dans le panneau **Blackboard** (à gauche):

### Property 1: PositionBuffer
1. Click `+` → `Property` → `GraphicsBuffer`
2. Nommer: **`PositionBuffer`**
3. **Mode**: `Read` (pas `Write`)
4. Glisser cette property vers le block **Set Position** → **GraphicsBuffer** slot

### Property 2: ParticleCount
1. Click `+` → `Property` → `Int`
2. Nommer: **`ParticleCount`**
3. **Default Value**: `1048576`

### Property 3: PointSize
1. Click `+` → `Property` → `Float`
2. Nommer: **`PointSize`**
3. **Default Value**: `0.01`
4. Glisser vers le block **Set Size** → **Size** slot

### Property 4: ColorBuffer (optionnel, pour futur RGB)
1. Click `+` → `Property` → `GraphicsBuffer`
2. Nommer: **`ColorBuffer`**
3. Glisser vers le block **Set Color** (si créé)

## Étape 5: Configurer Update Context

Le context **Update** peut rester minimal (ou vide):

- **Pas de forces** (gravity, drag, etc.)
- **Pas d'animation** (les points sont statiques chaque frame)

Si désiré, ajouter:
- `Age over Lifetime` pour fade-in effect

## Étape 6: Configurer Output Context

Dans le node **Output Particle Quad**:

### 6.1 Output Settings

1. Cliquer sur le node **Output**
2. Dans Inspector:
   - **Blend Mode**: `Opaque` (ou `Additive` pour effet lumineux)
   - **Orient**: `Camera` (billboard, toujours face à la caméra)

### 6.2 Blocks Output

Blocks par défaut sont OK:
- `Set Color` (si vous voulez une couleur unie)
- `Set Size` (devrait hériter de Initialize)

## Étape 7: Configurer Spawn Context

Dans le node **Spawn**:

1. Cliquer sur **Spawn** context
2. Configurer:
   - **Rate**: `Constant` → `0` (on ne spawn qu'une fois)
   - Ou mieux: Supprimer le "Constant Rate" block

3. Ajouter **Single Burst**:
   - Click `+` dans Spawn → `Single Burst`
   - **Count**: Glisser la property **`ParticleCount`** ici
   - **Delay**: `0`

## Étape 8: Configuration Finale

### 8.1 Vérifier les Connections

Le flow doit être:
```
Spawn → Initialize → Update → Output
```

### 8.2 Compile

- Le VFX Graph compile automatiquement
- Vérifier qu'il n'y a pas d'erreurs (panneau en bas)

### 8.3 Sauvegarder

- `Ctrl+S` ou `File` → `Save`

## Étape 9: Test dans Scene

1. Créer un GameObject vide: `Right-click Hierarchy` → `Create Empty`
2. Nommer: **`DepthPointCloud`**
3. Ajouter components:
   - `Add Component` → `Visual Effect` → `Visual Effect`
   - `Add Component` → `RosSubDepthImageToPointCloud`

4. Configurer **Visual Effect** component:
   - **Asset Template**: Glisser `PointCloudVFX.vfx`

5. Configurer **RosSubDepthImageToPointCloud** component:
   - **Compute Shader**: Glisser `PointCloudProcessor.compute`
   - **Depth Topic Name**: `/camera/depth/image_raw`
   - **Camera Info Topic Name**: `/camera/depth/camera_info`
   - **Use Mock Intrinsics**: ✅ (pour tester sans ROS)
   - **Mock Width**: `1024`
   - **Mock Height**: `1024`
   - **Mock Focal Length**: `600`

6. Test avec données mock:
   - Right-click sur le script dans Inspector
   - Sélectionner **`Generate Mock Depth Data`**
   - Vous devriez voir une sphère de points dans la Scene view

## Configuration Avancée (Optionnel)

### Améliorer le Rendu

1. **LOD Distance Culling**:
   - Ajouter block dans Update: `Collision` → `Depth Buffer Collision`

2. **Size by Distance**:
   - Dans Output, ajouter: `Set Size` avec formule basée sur distance à caméra

3. **Soft Particles**:
   - Dans Output settings: Enable `Soft Particles`

### Performance

Si FPS < 30:

1. **Réduire Capacity** dans System settings (ex: 262144 pour 512×512)
2. **Ajuster Downsampling Factor** dans le script C# (2 ou 4)
3. **Réduire Point Size** (0.005 au lieu de 0.01)

## Troubleshooting

### Erreur: "VFX Graph package not found"
- Installer le package (voir Prérequis)

### Erreur: "GraphicsBuffer is null"
- Vérifier que le script C# a bien créé le buffer
- Vérifier les logs Unity Console

### Points ne s'affichent pas
- Vérifier que GameObject est actif
- Vérifier que isOpen = true dans le script
- Vérifier qu'il y a des données (Generate Mock Data)
- Vérifier la caméra scene view (peut-être trop loin)

### Performance faible
- Réduire Capacity
- Activer Downsampling (2× ou 4×)
- Vérifier GPU usage dans Profiler

## Résultat Attendu

Après configuration:
- ✅ VFX Graph compile sans erreurs
- ✅ Peut recevoir GraphicsBuffer depuis C# script
- ✅ Affiche 1M points à 30+ FPS
- ✅ Points suivent les données depth
- ✅ Toggle ON/OFF fonctionne

## Ressources

- [VFX Graph Documentation](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@12.0/manual/index.html)
- [GraphicsBuffer with VFX](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@12.0/manual/GraphicsBufferReference.html)
- [Point Cloud Renderer Example](https://github.com/pablothedolphin/Point-Cloud-Renderer)
