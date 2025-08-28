using UnityEngine;
using UnityEngine.TextCore;
using TMPro;



public class Launcher : MonoBehaviour
{
    private void Awake()
    {
        SetEmojiSpriteAsset();
        GameObject objUIRoot = GameObject.Find("UI_Root");
        DontDestroyOnLoad(objUIRoot);
        //MessageNetManager.Instance.Play();
        UIManager.Instance.Play();
    }

    private void Start()
    {
        SdkManager.Instance.InitSDK(Play);
    }

    private void OnDestroy()
    {
        //MessageNetManager.Instance.Stop();
    }



    private void Play()
    {

    }

    private void SetEmojiSpriteAsset()
    {
        TMP_SpriteAsset spriteAsset = Resources.Load<TMP_SpriteAsset>("Sprite Assets/emoji");

        for (int i = 0; i < spriteAsset.spriteGlyphTable.Count; i++)
        {
            TMP_SpriteGlyph glyph = spriteAsset.spriteGlyphTable[i];
            glyph.metrics = new GlyphMetrics(glyph.metrics.width, glyph.metrics.height, 0, glyph.metrics.height * 9 / 10, glyph.metrics.horizontalAdvance);
        }

        spriteAsset.UpdateLookupTables();
    }
}