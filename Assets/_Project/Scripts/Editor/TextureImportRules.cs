// AE2 - YerayCAE2
// Reglas automáticas de importación de texturas según la carpeta y el sufijo del archivo.
// Solo se aplican la PRIMERA vez que se importa una textura (importSettingsMissing),
// así cualquier ajuste manual posterior en el Inspector se respeta.
//
// Decisiones (perfil: consola de sobremesa):
//  - Tamaño máximo por categoría: Hero 2048, Modular 1024, Props 512, UI 2048.
//  - Mipmaps activados en 3D: la nave tiene pasillos largos y las texturas se ven a mucha distancia.
//  - Mipmap streaming activado: solo se mantienen en memoria los niveles de mip que la cámara necesita.
//  - Compresión de alta calidad (BC7 en PC/consola).
//  - Mapas de datos (normal, máscara, rugosidad, AO) en espacio lineal (sin sRGB).
//
// Convención de sufijos:
//  _N / _Normal                    -> Normal map
//  _M / _Mask / _R / _Rough / _AO  -> Datos lineales (sin sRGB)
//  _A / _Albedo / _BaseColor / sin sufijo -> Color (sRGB)

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AE2.EditorTools
{
    public class TextureImportRules : AssetPostprocessor
    {
        private const string Root = "Assets/_Project/Art/Textures/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root)) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing) return; // respetar ajustes manuales

            string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();

            // --- Interfaz ---
            if (assetPath.StartsWith(Root + "UI/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;          // la UI se dibuja siempre a tamaño de pantalla
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.alphaIsTransparency = true;
                Debug.Log($"[AE2] Textura UI configurada: {assetPath}");
                return;
            }

            // --- Tamaño máximo por categoría ---
            if (assetPath.StartsWith(Root + "Hero/"))
                importer.maxTextureSize = 2048;
            else if (assetPath.StartsWith(Root + "Modular/"))
                importer.maxTextureSize = 1024;
            else if (assetPath.StartsWith(Root + "Props/"))
                importer.maxTextureSize = 512;
            else
                importer.maxTextureSize = 1024;

            // --- Tipo de mapa según sufijo ---
            if (HasSuffix(fileName, "_n", "_normal"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else if (HasSuffix(fileName, "_m", "_mask", "_r", "_rough", "_roughness", "_ao", "_metallic"))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false; // datos, no color
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;  // color
            }

            // --- Mipmaps y memoria ---
            importer.mipmapEnabled = true;
            importer.mipmapFilter = TextureImporterMipFilter.KaiserFilter; // conserva más detalle a distancia
            importer.streamingMipmaps = true;
            importer.isReadable = false; // sin copia en CPU: ahorra memoria

            // --- Compresión ---
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.anisoLevel = 4; // suelos y estanterías vistos en ángulo rasante

            Debug.Log($"[AE2] Textura configurada ({importer.maxTextureSize}px, {importer.textureType}): {assetPath}");
        }

        private static bool HasSuffix(string fileName, params string[] suffixes)
        {
            foreach (string suffix in suffixes)
            {
                if (fileName.EndsWith(suffix)) return true;
            }
            return false;
        }
    }
}
