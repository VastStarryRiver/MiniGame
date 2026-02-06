using UnityEngine;



namespace Invariable
{
    [CreateAssetMenu(fileName = "BinAsset", menuName = "MyAssets/BinAsset", order = 2)]
    public class BinAsset : ScriptableObject
    {
        public byte[] bytes;
    }
}