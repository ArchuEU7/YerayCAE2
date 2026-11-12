// AE2 - YerayCAE2
// Reglas automáticas de importación de modelos (FBX/OBJ) dentro de Assets/_Project/Art/Models.
// Solo se aplican la PRIMERA vez que se importa un modelo (importSettingsMissing).
//
// Decisiones:
//  - No se importan cámaras, luces ni materiales del FBX: los materiales se crean a mano en
//    Art/Materials/Shared para que muchas piezas compartan el mismo material (menos variantes de
//    shader y mejor agrupación con el SRP Batcher).
//  - Read/Write desactivado: la malla no necesita copia en CPU (ahorra memoria).
//  - UV2 de lightmap generadas en Modular y Vehicles: son piezas estáticas que recibirán luz horneada.
//  - Sin colisionador de malla automático: las colisiones se añaden a mano en los prefabs con
//    primitivas simples (Box/Capsule), mucho más baratas que MeshCollider.
//  - Sin animación ni blend shapes: la escena es estática.
//  - Compresión de malla baja en Props (piezas pequeñas, la pérdida de precisión no se nota).
//
// LOD: si un FBX contiene mallas con sufijo _LOD0, _LOD1, _LOD2..., Unity crea el LOD Group
// automáticamente al instanciar el modelo.

using UnityEditor;
using UnityEngine;

namespace AE2.EditorTools
{
    public class ModelImportRules : AssetPostprocessor
    {
        private const string Root = "Assets/_Project/Art/Models/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root)) return;

            ModelImporter importer = (ModelImporter)assetImporter;
            if (!importer.importSettingsMissing) return; // respetar ajustes manuales

            bool isModular = assetPath.StartsWith(Root + "Modular/");
            bool isProp = assetPath.StartsWith(Root + "Props/");
            bool isVehicle = assetPath.StartsWith(Root + "Vehicles/");

            // --- Contenido que no necesitamos del FBX ---
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.importBlendShapes = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;

            // --- Memoria ---
            importer.isReadable = false;

            // --- Geometría ---
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.meshCompression = isProp ? ModelImporterMeshCompression.Low : ModelImporterMeshCompression.Off;

            // --- Iluminación horneada ---
            importer.generateSecondaryUV = isModular || isVehicle;

            // --- Colisiones ---
            importer.addCollider = false;

            Debug.Log($"[AE2] Modelo configurado (modular: {isModular}, prop: {isProp}, vehículo: {isVehicle}): {assetPath}");
        }
    }
}
