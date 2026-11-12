// AE2 - YerayCAE2
// Crea la estructura de carpetas del proyecto desde el menú "AE2 > Crear estructura de carpetas".
// La estructura separa el contenido por tipo (Art) y por función (Prefabs, Zones) para que:
//  - las reglas de importación se apliquen automáticamente según la carpeta (ver TextureImportRules / ModelImportRules),
//  - el contenido residente y el contenido cargable bajo demanda (Fase 4) estén separados desde el principio.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AE2.EditorTools
{
    public static class ProjectFolderSetup
    {
        private const string Root = "Assets/_Project";

        private static readonly string[] Folders =
        {
            // Arte importado
            "Art/Models/Modular",      // paredes, suelos, columnas, estanterías: muy repetidos
            "Art/Models/Props",        // cajas, palés, bidones, mobiliario
            "Art/Models/Vehicles",     // camión del muelle (pieza destacada)
            "Art/Materials/Shared",    // materiales compartidos por muchas piezas (atlas, metal genérico...)
            "Art/Materials/Unique",    // materiales de piezas concretas
            "Art/Materials/Transparent", // cristales de la oficina (aislados para controlarlos)
            "Art/Textures/Hero",       // texturas de piezas destacadas: hasta 2K
            "Art/Textures/Modular",    // texturas de piezas modulares: hasta 1K
            "Art/Textures/Props",      // texturas de props pequeños: hasta 512
            "Art/Textures/UI",         // interfaz: sin mipmaps

            // Contenido montado
            "Prefabs/Modular",
            "Prefabs/Props",
            "Prefabs/Lighting",
            "Prefabs/Player",

            // Zonas: lo residente frente a lo cargable bajo demanda (Fase 4)
            "Zones/Persistent",        // muelle, nave principal, oficina: siempre cargado
            "Zones/BackStorage",       // almacén trasero: se cargará con Addressables

            // Resto
            "Scenes",
            "Scripts/Runtime",
            "Scripts/Editor",
            "Settings",
        };

        [MenuItem("AE2/Crear estructura de carpetas")]
        public static void CreateFolders()
        {
            int created = 0;

            foreach (string relative in Folders)
            {
                string path = $"{Root}/{relative}";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    CreateFolderRecursive(path);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AE2] Estructura de carpetas lista. Carpetas nuevas: {created}.");
        }

        private static void CreateFolderRecursive(string path)
        {
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                CreateFolderRecursive(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
