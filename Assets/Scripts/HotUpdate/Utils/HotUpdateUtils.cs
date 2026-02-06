using Invariable;
using System;
using System.IO;
using UnityEngine;



namespace HotUpdate
{
    public class HotUpdateUtils
    {
        public static void OpenUIPrefabPanel(string prefabPath, int layer)
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
                component = gameObject.GetComponent("HotUpdate." + componentName);
            }

            if (component == null)
            {
                Type type = Type.GetType(componentName);

                if (type == null)
                {
                    type = Type.GetType("HotUpdate." + componentName);
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
    }
}