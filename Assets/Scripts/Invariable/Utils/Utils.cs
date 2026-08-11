using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;



namespace Invariable
{
    public class Utils
    {
        private static Camera[] m_uiCamera = null;
        private static RectTransform m_uiRoot = null;
        private static Dictionary<string, Sprite> m_spriteCache = null;
        private static RectTransform[] m_panelParents = null;
        private static Dictionary<string, Type> m_panelTypeCache = null;
        private static HashSet<string> m_loadingPanels = null;

        public static Camera[] UICamera
        {
            get
            {
                m_uiCamera ??= new Camera[]
                {
                    GameObject.Find(InvariableConst.UICameraPath_0).GetComponent<Camera>(),
                    GameObject.Find(InvariableConst.UICameraPath_1).GetComponent<Camera>(),
                    GameObject.Find(InvariableConst.UICameraPath_2).GetComponent<Camera>(),
                    GameObject.Find(InvariableConst.UICameraPath_3).GetComponent<Camera>(),
                };

                return m_uiCamera;
            }
        }

        public static RectTransform UIRoot
        {
            get
            {
                m_uiRoot ??= GameObject.Find(InvariableConst.UIRootPath).GetComponent<RectTransform>();

                return m_uiRoot;
            }
        }



        #region UI 查找与对象操作

        /// <summary>
        /// 从对象或子路径获取 GameObject
        /// </summary>
        public static GameObject GetGameObject(UnityEngine.Object obj, string childPath = "")
        {
            GameObject gameObject = null;

            if (obj is GameObject)
            {
                gameObject = obj as GameObject;
            }
            else if (obj is Component)
            {
                Component component = obj as Component;
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

        /// <summary>
        /// 按场景路径查找 GameObject
        /// </summary>
        public static GameObject GetGameObject(string childPath)
        {
            GameObject gameObject = GameObject.Find(childPath);

            return gameObject;
        }

        /// <summary>
        /// 从对象或子路径获取 Transform
        /// </summary>
        public static Transform GetTransform(UnityEngine.Object obj, string childPath = "")
        {
            GameObject gameObject = GetGameObject(obj, childPath);

            if (gameObject != null)
            {
                return gameObject.transform;
            }

            return null;
        }

        /// <summary>
        /// 克隆 GameObject 并设置名称与父节点
        /// </summary>
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

        /// <summary>
        /// 设置对象激活状态
        /// </summary>
        public static void SetActive(UnityEngine.Object obj, bool isActive = false)
        {
            GameObject item = GetGameObject(obj);
            item.SetActive(isActive);
        }

        /// <summary>
        /// 设置子节点激活状态
        /// </summary>
        public static void SetActive(UnityEngine.Object obj, string childPath, bool isActive = false)
        {
            GameObject item = GetGameObject(obj);

            if (!string.IsNullOrEmpty(childPath))
            {
                item = item.transform.Find(childPath).gameObject;
            }

            item.SetActive(isActive);
        }

        /// <summary>
        /// 隐藏全部子节点
        /// </summary>
        public static void HideAllChildren(Transform transform)
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform item = transform.GetChild(i);
                    item.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 将字节大小格式化为可读字符串
        /// </summary>
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

        #endregion

        #region 面板开关

        /// <summary>
        /// 关闭指定名称的 UI 面板
        /// </summary>
        public static void CloseUIPrefabPanel(string prefabName)
        {
            UIManager.Instance.CloseUIPanel(prefabName);
        }

        #endregion

        #region 文本与灰态

        /// <summary>
        /// 设置 TextMeshPro 文本内容
        /// </summary>
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

        /// <summary>
        /// 设置图片灰态材质
        /// </summary>
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

        #endregion

        #region Sprite 缓存

        /// <summary>
        /// 异步设置 Image/RawImage 精灵
        /// </summary>
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
                key = $"Atlas_{atlasInfo[0]}";
                imageName = atlasInfo[1];
            }
            else
            {
                key = $"Png_{spritePath}";
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
                    Sprite sprite = GetCachedSprite(key, imageName, atlas);

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

        /// <summary>
        /// 清理图集 Sprite 缓存；传空则清空全部
        /// </summary>
        /// <param name="atlasAddress">图集资源地址</param>
        public static void ClearSpriteCache(string atlasAddress = null)
        {
            if (m_spriteCache == null || m_spriteCache.Count <= 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(atlasAddress))
            {
                m_spriteCache.Clear();

                return;
            }

            string prefix = atlasAddress + "/";
            List<string> removeKeys = null;

            foreach (KeyValuePair<string, Sprite> item in m_spriteCache)
            {
                if (item.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    removeKeys ??= new List<string>();
                    removeKeys.Add(item.Key);
                }
            }

            if (removeKeys == null)
            {
                return;
            }

            for (int i = 0; i < removeKeys.Count; i++)
            {
                m_spriteCache.Remove(removeKeys[i]);
            }
        }

        /// <summary>
        /// 从缓存获取图集 Sprite，未命中则 GetSprite 并缓存
        /// </summary>
        private static Sprite GetCachedSprite(string atlasAddress, string spriteName, SpriteAtlas atlas)
        {
            m_spriteCache ??= new Dictionary<string, Sprite>();
            string cacheKey = atlasAddress + "/" + spriteName;

            if (m_spriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }

            Sprite sprite = atlas.GetSprite(spriteName);
            m_spriteCache[cacheKey] = sprite;

            return sprite;
        }

        #endregion

        #region 按钮监听

        /// <summary>
        /// 为子节点添加单击监听
        /// </summary>
        public static void AddClickListener(UnityEngine.Object obj, string childPath, Action callBack)
        {
            Transform trans = GetTransform(obj, childPath);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddClickListener(callBack);
        }

        /// <summary>
        /// 移除单击监听
        /// </summary>
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

        /// <summary>
        /// 添加按下监听
        /// </summary>
        public static void AddDownListener(UnityEngine.Object obj, Action callBack)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddDownListener(callBack);
        }

        /// <summary>
        /// 移除按下监听
        /// </summary>
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

        /// <summary>
        /// 添加抬起监听
        /// </summary>
        public static void AddUpListener(UnityEngine.Object obj, Action callBack)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddUpListener(callBack);
        }

        /// <summary>
        /// 移除抬起监听
        /// </summary>
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

        /// <summary>
        /// 添加双击监听
        /// </summary>
        public static void AddDoubleClickListener(UnityEngine.Object obj, Action callBack)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddDoubleClickListener(callBack);
        }

        /// <summary>
        /// 移除双击监听
        /// </summary>
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

        /// <summary>
        /// 添加长按监听
        /// </summary>
        public static void AddLongPressListener(UnityEngine.Object obj, Action callBack)
        {
            Transform trans = GetTransform(obj);

            UIButton btn = trans.GetComponent<UIButton>();

            if (btn == null)
            {
                return;
            }

            btn.AddLongPressListener(callBack);
        }

        /// <summary>
        /// 移除长按监听
        /// </summary>
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

        #endregion

        #region 动画

        /// <summary>
        /// 播放 Animation 动画
        /// </summary>
        public static void PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
        {
            GameManager.Instance.StartCoroutine(PlayAnimation2(obj, childPath, animName, wrapMode, callBack));
        }

        #endregion

        #region 面板加载

        /// <summary>
        /// 打开 UI Prefab 面板（加载中重复调用直接忽略）
        /// </summary>
        public static void OpenUIPrefabPanel(string prefabPath, int layer, Action<GameObject> callBack = null)
        {
            string prefabName = Path.GetFileName(prefabPath);

            if (prefabName.Contains(".prefab"))
            {
                prefabName = prefabName.Replace(".prefab", "");
            }

            if (UIManager.Instance.AllPanel.TryGetValue(prefabName, out UIPanel existingPanel))
            {
                existingPanel.gameObject.SetActive(true);
                callBack?.Invoke(existingPanel.gameObject);

                return;
            }

            m_loadingPanels ??= new HashSet<string>();

            if (m_loadingPanels.Contains(prefabName))
            {
                return;
            }

            m_loadingPanels.Add(prefabName);

            string key = $"Prefabs_{prefabName}";
            Transform parentTrans = GetPanelParent(layer);

            YooAssetManager.Instance.AsyncLoadAsset<GameObject>(key, (asset) =>
            {
                m_loadingPanels.Remove(prefabName);

                if (asset == null)
                {
                    GameLog.Error($"打开面板失败，资源加载为空：{key}");

                    return;
                }

                if (UIManager.Instance.AllPanel.TryGetValue(prefabName, out UIPanel panel))
                {
                    panel.gameObject.SetActive(true);
                    callBack?.Invoke(panel.gameObject);

                    return;
                }

                GameObject gameObject = GameObject.Instantiate(asset, parentTrans);
                gameObject.name = prefabName;
                UIPanel uiPanel = (UIPanel)AddComponent(gameObject, "", prefabName);
                UIManager.Instance.AddUIPanel(prefabName, uiPanel);
                callBack?.Invoke(gameObject);
            });
        }

        #endregion

        #region 组件与类型解析

        /// <summary>
        /// 按组件名添加组件
        /// </summary>
        public static Component AddComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = GetGameObject(obj);

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

            Type type = ResolveComponentType(componentName);

            if (type != null)
            {
                Component existing = gameObject.GetComponent(type);

                if (existing != null)
                {
                    return existing;
                }

                return gameObject.AddComponent(type);
            }

            Component component = gameObject.GetComponent(componentName);

            if (component == null)
            {
                component = gameObject.GetComponent($"Invariable.{componentName}");
            }

            if (component == null)
            {
                component = gameObject.GetComponent($"HotUpdate.{componentName}");
            }

            return component;
        }

        /// <summary>
        /// 获取指定 Canvas 层的面板父节点（按层缓存）
        /// </summary>
        private static Transform GetPanelParent(int layer)
        {
            m_panelParents ??= new RectTransform[4];

            if (layer < 0 || layer >= m_panelParents.Length)
            {
                layer = 0;
            }

            if (m_panelParents[layer] == null)
            {
                string path = InvariableConst.UIPanelPath_0;

                if (layer == 1)
                {
                    path = InvariableConst.UIPanelPath_1;
                }
                else if (layer == 2)
                {
                    path = InvariableConst.UIPanelPath_2;
                }
                else if (layer == 3)
                {
                    path = InvariableConst.UIPanelPath_3;
                }

                GameObject parentObject = GameObject.Find(path);
                m_panelParents[layer] = parentObject.GetComponent<RectTransform>();
            }

            return m_panelParents[layer];
        }

        /// <summary>
        /// 解析组件 Type（带缓存，覆盖 Invariable/HotUpdate/内置 UI）
        /// </summary>
        private static Type ResolveComponentType(string componentName)
        {
            m_panelTypeCache ??= new Dictionary<string, Type>();

            if (m_panelTypeCache.TryGetValue(componentName, out Type cachedType) && cachedType != null)
            {
                return cachedType;
            }

            Type type = Type.GetType(componentName);

            if (type == null)
            {
                type = Type.GetType($"Invariable.{componentName}");
            }

            if (type == null)
            {
                type = Type.GetType($"HotUpdate.{componentName}");
            }

            if (type == null)
            {
                type = FindTypeTool.GetComponentType(componentName);
            }

            if (type == null)
            {
                System.Reflection.Assembly hotUpdateAssembly = YooAssetManager.Instance.HotUpdateAssembly;

                if (hotUpdateAssembly != null)
                {
                    type = hotUpdateAssembly.GetType($"HotUpdate.{componentName}") ?? hotUpdateAssembly.GetType(componentName);
                }
            }

            if (type != null)
            {
                m_panelTypeCache[componentName] = type;
            }

            return type;
        }

        /// <summary>
        /// 按组件名获取组件
        /// </summary>
        public static Component GetComponent(UnityEngine.Object obj, string childPath, string componentName)
        {
            GameObject gameObject = GetGameObject(obj);
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

        #endregion

        #region 管理器与杂项

        /// <summary>
        /// 创建并常驻 Manager 实例
        /// </summary>
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

        /// <summary>
        /// 将颜色字符串解析为 Color
        /// </summary>
        public static Color GetColorByString(string colorStr)
        {
            if (ColorUtility.TryParseHtmlString(colorStr, out Color color))
            {
                return color;
            }

            return Color.white;
        }



        /// <summary>
        /// 播放动画并在完成后回调
        /// </summary>
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
                yield return new WaitWhile(() =>
                {
                    return animation.isPlaying;
                });
                callBack?.Invoke();
            }
        }
        #endregion
    }
}