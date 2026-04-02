using System;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.U2D;



namespace Invariable
{
    public class Utils
    {
        private static Camera[] uiCamera;
        public static Camera[] UICamera
        {
            get
            {
                uiCamera ??= new Camera[]
                {
                    GameObject.Find("UI_Root/Canvas_0/UI_Camera").GetComponent<Camera>(),
                    GameObject.Find("UI_Root/Canvas_1/UI_Camera").GetComponent<Camera>(),
                    GameObject.Find("UI_Root/Canvas_2/UI_Camera").GetComponent<Camera>(),
                    GameObject.Find("UI_Root/Canvas_3/UI_Camera").GetComponent<Camera>(),
                };

                return uiCamera;
            }
        }

        public static RectTransform UIRoot
        {
            get
            {
                return GameObject.Find("UI_Root").GetComponent<RectTransform>();
            }
        }

        public static Camera MainSceneCamera
        {
            get
            {
                return GameObject.Find("SceneGameObject/Main Camera").GetComponent<Camera>();
            }
        }

        public static GameObject GetGameObject(UnityEngine.Object obj, string childPath = "")
        {
            GameObject gameObject = null;

            if (obj is GameObject)
            {
                gameObject = obj as GameObject;
            }
            else if (obj is Component)
            {
                var component = obj as Component;
                gameObject = component.gameObject;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                Transform trans = gameObject.transform.Find(childPath);

                if (trans != null)
                {
                    return trans.gameObject;
                }

                return null;
            }

            return gameObject;
        }

        public static GameObject GetGameObject(string childPath)
        {
            GameObject gameObject = GameObject.Find(childPath);
            return gameObject;
        }

        public static Transform GetTransform(UnityEngine.Object obj, string childPath = "")
        {
            var gameObject = GetGameObject(obj, childPath);

            if (gameObject != null)
            {
                return gameObject.transform;
            }

            return null;
        }

        public static GameObject Clone(UnityEngine.Object obj, string name = "cloneName", UnityEngine.Object parent = null)
        {
            GameObject item = GetGameObject(obj);
            GameObject clone;

            if (parent != null)
            {
                Transform parentTrans = GetTransform(parent);
                clone = GameObject.Instantiate<GameObject>(item, Vector3.zero, Quaternion.identity, parentTrans);
            }
            else
            {
                clone = GameObject.Instantiate<GameObject>(item, Vector3.zero, Quaternion.identity);
            }

            clone.name = name;

            return clone;
        }

        public static void SetActive(UnityEngine.Object obj, bool isActive = false)
        {
            GameObject item = GetGameObject(obj);
            item.SetActive(isActive);
        }

        public static void SetActive(UnityEngine.Object obj, string childPath, bool isActive = false)
        {
            GameObject item = GetGameObject(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                item = item.transform.Find(childPath).gameObject;
            }

            item.SetActive(isActive);
        }

        public static void HideAllChildren(Transform transform)
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var item = transform.GetChild(i);
                    item.gameObject.SetActive(false);
                }
            }
        }

        public static string FormatFileByteSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "G", "T" };
            int unitIndex = 0;
            double size = bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.##} {units[unitIndex]}";
        }

        public static void CloseUIPrefabPanel(string prefabName)
        {
            UIManager.Instance.CloseUIPanel(prefabName);
        }

        public static void SetText(UnityEngine.Object obj, string childPath = "", string des = "")
        {
            Transform trans = GetTransform(obj, childPath);

            if (trans != null)
            {
                TextMeshProUGUI text = trans.GetComponent<TextMeshProUGUI>();

                if (text != null)
                {
                    text.text = des;
                }
            }
        }

        public static void SetGray(UnityEngine.Object obj, string childPath = "", bool isGray = true, bool isMask = false)
        {
            Transform trans = GetTransform(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans == null)
            {
                return;
            }

            Image image = trans.GetComponent<Image>();
            RawImage rawImage = trans.GetComponent<RawImage>();

            if (image == null && rawImage == null)
            {
                return;
            }

            if (isGray)
            {
                string key;

                if (isMask)
                {
                    key = "Materials_UIMaskGrayscaleMaterial";
                }
                else
                {
                    key = "Materials_GrayscaleMaterial";
                }

                YooAssetManager.Instance.AsyncLoadAsset<Material>(key, (material) =>
                {
                    if (image != null)
                    {
                        image.material = material;
                    }
                    else if (rawImage != null)
                    {
                        rawImage.material = material;
                    }
                });
            }
            else if (image != null)
            {
                image.material = null;
            }
            else if (rawImage != null)
            {
                rawImage.material = null;
            }
        }

        public static void SetImage(UnityEngine.Object obj, string childPath = "", string spritePath = "", bool isSetNativeSize = false)
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                return;
            }

            Transform trans = GetTransform(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans == null)
            {
                return;
            }

            Image image = trans.GetComponent<Image>();
            RawImage rawImage = trans.GetComponent<RawImage>();

            string[] atlasInfo = spritePath.Split('/');
            string key = "";
            string imageName = "";

            if (atlasInfo.Length == 2)
            {
                key = "Atlas_" + atlasInfo[0];
                imageName = atlasInfo[1];
            }
            else
            {
                key = "Png_" + spritePath;
            }

            if (string.IsNullOrEmpty(imageName))
            {
                YooAssetManager.Instance.AsyncLoadAsset<Sprite>(key, (sprite) =>
                {
                    if (image != null)
                    {
                        image.sprite = sprite;

                        if (isSetNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    }
                    else if (rawImage != null)
                    {
                        rawImage.texture = sprite.texture;

                        if (isSetNativeSize)
                        {
                            rawImage.SetNativeSize();
                        }
                    }
                });
            }
            else
            {
                YooAssetManager.Instance.AsyncLoadAsset<SpriteAtlas>(key, (atlas) =>
                {
                    Sprite sprite = atlas.GetSprite(imageName);

                    if (image != null)
                    {
                        image.sprite = sprite;

                        if (isSetNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    }
                    else if (rawImage != null)
                    {
                        rawImage.texture = sprite.texture;

                        if (isSetNativeSize)
                        {
                            rawImage.SetNativeSize();
                        }
                    }
                });
            }
        }

        public static void AddClickListener(UnityEngine.Object obj, string childPath, Action luaFunc)
        {
            Transform trans = GetTransform(obj, childPath);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddClickListener(luaFunc);
        }

        public static void ReleaseClickListener(UnityEngine.Object obj)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.ReleaseClickListener();
        }

        public static void AddDownListener(UnityEngine.Object obj, Action luaFunc)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddDownListener(luaFunc);
        }

        public static void ReleaseDownListener(UnityEngine.Object obj)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.ReleaseDownListener();
        }

        public static void AddUpListener(UnityEngine.Object obj, Action luaFunc)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddUpListener(luaFunc);
        }

        public static void ReleaseUpListener(UnityEngine.Object obj)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.ReleaseUpListener();
        }

        public static void AddDoubleClickListener(UnityEngine.Object obj, Action luaFunc)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddDoubleClickListener(luaFunc);
        }

        public static void ReleaseDoubleClickListener(UnityEngine.Object obj)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.ReleaseDoubleClickListener();
        }

        public static void AddLongPressListener(UnityEngine.Object obj, Action luaFunc)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddLongPressListener(luaFunc);
        }

        public static void ReleaseLongPressListener(UnityEngine.Object obj)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.ReleaseLongPressListener();
        }

        public static void PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            GameManager.Instance.StartCoroutine(PlayAnimation2(obj, childPath, animName, wrapMode, callBack));
        }

        private static IEnumerator PlayAnimation2(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            if (string.IsNullOrEmpty(animName))
            {
                yield break;
            }

            Transform trans = GetTransform(obj);

            if (trans == null)
            {
                yield break;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans == null)
            {
                yield break;
            }

            Animation animation = trans.GetComponent<Animation>();

            if (animation == null)
            {
                yield break;
            }

            animation.wrapMode = wrapMode;

            animation.Play(animName);

            if (wrapMode == WrapMode.Once)
            {
                yield return new WaitWhile(() => animation.isPlaying);
                callBack?.Invoke();
            }
        }

        public static void OpenUIPrefabPanel(string prefabPath, int layer, Action<GameObject> callBack = null)
        {
            string prefabName = Path.GetFileName(prefabPath);

            if (prefabName.Contains(".prefab"))
            {
                prefabName = prefabName.Replace(".prefab", "");
            }

            string key = "Prefabs_" + prefabName;

            GameObject gameObject = null;
            Transform parentTrans = GameObject.Find("UI_Root/Canvas_" + layer + "/Ts_Panel").transform;

            YooAssetManager.Instance.AsyncLoadAsset<GameObject>(key, (asset) => {
                gameObject = GameObject.Instantiate(asset, parentTrans);
                gameObject.name = prefabName;
                UIPanel uiPanel = (UIPanel)AddComponent(gameObject, "", prefabName);
                UIManager.Instance.AddUIPanel(prefabName, uiPanel);
                callBack?.Invoke(gameObject);
            });
        }

        public static Component AddComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = Utils.GetGameObject(obj);

            if (gameObject == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                Transform trans = gameObject.transform.Find(childPath);

                if (trans == null)
                {
                    return null;
                }

                gameObject = trans.gameObject;
            }

            Component component = gameObject.GetComponent(componentName);

            if (component == null)
            {
                component = gameObject.GetComponent("Invariable." + componentName);
            }

            if (component == null)
            {
                Type type = Type.GetType(componentName);

                if (type == null)
                {
                    type = Type.GetType("Invariable." + componentName);
                }

                if (type == null)
                {
                    type = FindTypeTool.GetComponentType(componentName);
                }

                if (type != null)
                {
                    component = gameObject.AddComponent(type);
                }
            }

            return component;
        }

        public static Component GetComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = Utils.GetGameObject(obj);
            Transform trans = null;

            if (gameObject != null)
            {
                trans = gameObject.transform;
            }
            else
            {
                return null;
            }

            if (!string.IsNullOrEmpty(childPath))
            {
                trans = trans.Find(childPath);
            }

            if (trans != null)
            {
                return trans.GetComponent(componentName);
            }

            return null;
        }

        public static void CreateManagerInstance(string managerName, string[] components = null)
        {
            GameObject obj = GameObject.Find(managerName);

            if (obj != null)
            {
                return;
            }

            obj = new GameObject(managerName);

            if (components != null && components.Length > 0)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    AddComponent(obj, "", components[i]);
                }
            }

            AddComponent(obj, "", managerName);

            UnityEngine.Object.DontDestroyOnLoad(obj);
        }
    }
}