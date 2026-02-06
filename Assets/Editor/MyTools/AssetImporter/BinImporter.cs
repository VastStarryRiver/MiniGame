using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using Invariable;



namespace MyTools
{
    [ScriptedImporter(1, ".bin")]
    public class BinImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] bytes = File.ReadAllBytes(ctx.assetPath);
            BinAsset asset = ScriptableObject.CreateInstance<BinAsset>();
            asset.bytes = bytes;
            ctx.AddObjectToAsset("main obj", asset);
            ctx.SetMainObject(asset);
        }
    }
}