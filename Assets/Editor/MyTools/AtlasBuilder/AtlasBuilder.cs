using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;



namespace MyTools
{
    [CreateAssetMenu(fileName = "AtlasBuilder", menuName = "MyAssets/AtlasBuilder", order = 1)]
    public class AtlasBuilder : ScriptableObject
    {
        public string atlasName;
        public SpriteAlignment alignment;
        public Object[] directorys;

        private string m_atlasRootPath = Application.dataPath + "/Editor/AtlasBuilder/";//Í¼¼¯´æ´¢Â·¾¶

        [ContextMenu(nameof(BuildAtlas))]
        public void BuildAtlas()
        {
            Texture2D[] textures = GetTextures();
            CreateAtlas(textures);
        }



        private Texture2D[] GetTextures()
        {
            IEnumerable<Texture2D> textures = directorys.OfType<Texture2D>();
            string[] folderPaths = directorys.Select(AssetDatabase.GetAssetPath).Where(AssetDatabase.IsValidFolder).ToArray();
            return AssetDatabase.FindAssets("t:Texture2D", folderPaths).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Texture2D>).Concat(textures).ToArray();
        }

        private void CreateAtlas(Texture2D[] textures)
        {
            Texture2D atlas = new Texture2D(2048, 2048);
            Rect[] rects = atlas.PackTextures(textures, 2, 2048, false);
            Color[] atlasPixels = atlas.GetPixels();

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                Rect rect = rects[i];
                Color[] texturePixels = texture.GetPixels();

                int x = (int)(rect.x * atlas.width);
                int y = (int)(rect.y * atlas.height);

                for (int h = 0; h < texture.height; h++)
                {
                    for (int w = 0; w < texture.width; w++)
                    {
                        int atlasX = x + w;
                        int atlasY = y + h;

                        int index = atlasX + atlasY * atlas.width;

                        atlasPixels[index] = texturePixels[w + h * texture.width];
                    }
                }
            }

            atlas.SetPixels(atlasPixels);

            atlas.Apply();

            byte[] bytes = atlas.EncodeToPNG();

            string dirPath = m_atlasRootPath + atlasName;

            using (FileStream fileStream = new FileStream(dirPath + "/" + atlasName + ".png", FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                }
            }

            AssetDatabase.Refresh();

            string assetsAtlasPath = dirPath.Replace(Application.dataPath, "Assets") + "/" + atlasName + ".png";
            SetAtlasImportSettings(assetsAtlasPath, atlas, textures, rects);
        }

        private void SetAtlasImportSettings(string assetsAtlasPath, Texture2D atlas, Texture2D[] textures, Rect[] rects)
        {
            TextureImporter atlasImporter = AssetImporter.GetAtPath(assetsAtlasPath) as TextureImporter;

            atlasImporter.textureType = TextureImporterType.Sprite;

            atlasImporter.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();

            for (int i = 0; i < textures.Length; i++)
            {
                Rect rect = rects[i];
                SpriteMetaData spriteMetaData = GetSpriteMetaData(new Rect(rect.x * atlas.width, rect.y * atlas.height, rect.width * atlas.width, rect.height * atlas.height), textures[i].name);
                spriteMetaDatas.Add(spriteMetaData);
            }

            atlasImporter.spritesheet = spriteMetaDatas.ToArray();

            EditorUtility.SetDirty(atlasImporter);

            atlasImporter.SaveAndReimport();

            AssetDatabase.Refresh();
        }

        private SpriteMetaData GetSpriteMetaData(Rect rect, string name)
        {
            SpriteMetaData spriteMetaData = new SpriteMetaData();

            spriteMetaData.alignment = (int)alignment;
            spriteMetaData.name = name;
            spriteMetaData.rect = new Rect(rect.x, rect.y, rect.width, rect.height);

            return spriteMetaData;
        }
    }
}