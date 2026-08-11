using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;



namespace MyTools
{
    [CreateAssetMenu(fileName = "AtlasBuilder", menuName = "MyAssets/AtlasBuilder", order = 1)]
    public class AtlasBuilder : ScriptableObject
    {
        public string m_atlasName;
        public SpriteAlignment m_alignment;
        public Object[] m_directorys;
        private string m_atlasRootPath = Application.dataPath + "/Editor/MyTools/AtlasBuilder/"; // 图集存储路径



        /// <summary>
        /// 构建图集
        /// </summary>
        [ContextMenu(nameof(BuildAtlas))]
        public void BuildAtlas()
        {
            Texture2D[] textures = GetTextures();
            CreateAtlas(textures);
        }



        /// <summary>
        /// 收集目录与引用中的纹理
        /// </summary>
        private Texture2D[] GetTextures()
        {
            IEnumerable<Texture2D> textures = m_directorys.OfType<Texture2D>();
            string[] folderPaths = m_directorys.Select(AssetDatabase.GetAssetPath).Where(AssetDatabase.IsValidFolder).ToArray();

            return AssetDatabase.FindAssets("t:Texture2D", folderPaths).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Texture2D>).Concat(textures).ToArray();
        }

        /// <summary>
        /// 打包纹理并写出图集 PNG
        /// </summary>
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

            string dirPath = m_atlasRootPath + m_atlasName;

            using (FileStream fileStream = new FileStream($"{dirPath}/{m_atlasName}.png", FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                }
            }

            AssetDatabase.Refresh();

            string assetsAtlasPath = $"{dirPath.Replace(Application.dataPath, "Assets")}/{m_atlasName}.png";
            SetAtlasImportSettings(assetsAtlasPath, atlas, textures, rects);
        }

        /// <summary>
        /// 设置图集导入为 Multiple Sprite
        /// </summary>
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

        /// <summary>
        /// 生成单个 Sprite 元数据
        /// </summary>
        private SpriteMetaData GetSpriteMetaData(Rect rect, string name)
        {
            SpriteMetaData spriteMetaData = new SpriteMetaData();

            spriteMetaData.alignment = (int)m_alignment;
            spriteMetaData.name = name;
            spriteMetaData.rect = new Rect(rect.x, rect.y, rect.width, rect.height);

            return spriteMetaData;
        }
    }
}