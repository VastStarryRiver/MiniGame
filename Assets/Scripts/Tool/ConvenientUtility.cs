using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;



public static partial class ConvenientUtility
{
    public static Camera MainUICamera
    {
        get
        {
            return GameObject.Find("UI_Root/Canvas_0/UI_Camera").GetComponent<Camera>();
        }
    }

    public static RectTransform MainUIRoot
    {
        get
        {
            return GameObject.Find("UI_Root/Canvas_0").GetComponent<RectTransform>();
        }
    }

    public static Camera MainSceneCamera
    {
        get
        {
            return GameObject.Find("UI_Root/Main Camera").GetComponent<Camera>();
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

    public static Transform GetTransform(UnityEngine.Object obj, string childPath = "")
    {
        var gameObject = GetGameObject(obj, childPath);

        if (gameObject != null)
        {
            return gameObject.transform;
        }

        return null;
    }

    public static void OpenUIPrefabPanel(int index, int layer)
    {
        if (!UIManager.Instance.m_panelList.ContainsKey(index) || UIManager.Instance.m_panelList[index] == null)
        {
            CreateUIGameObject(index, layer);
        }
    }

    public static void CloseUIPrefabPanel(GameObject gameObject)
    {
        if (UIManager.Instance.m_panelList.ContainsValue(gameObject))
        {
            GameObject.Destroy(gameObject);
        }
    }

    public static void CreateUIGameObject(int index, int layer = -1, UnityEngine.Object parent = null)
    {
        GameObject gameObject = AssetsManager.Instance.m_gameObjects[index];
        Transform parentTrans = null;

        if (layer != -1)
        {
            parentTrans = GameObject.Find("UI_Root/Canvas_" + layer + "/Ts_Panel").transform;
        }
        else if (parent != null)
        {
            parentTrans = GetTransform(parent);
        }

        GameObject obj = null;

        if (parentTrans != null)
        {
            obj = GameObject.Instantiate(gameObject, Vector3.zero, Quaternion.identity, parentTrans);
        }
        else
        {
            obj = GameObject.Instantiate(gameObject, Vector3.zero, Quaternion.identity);
        }

        obj.name = gameObject.name;

        UIManager.Instance.m_panelList[index] = obj;
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

    public static void DelayInvoke(Action callBack, float needTime, string key)
    {
        CoroutineManager.Instance.AddDelayInvoke(key, callBack, needTime);
    }

    public static void InvokeRepeating(Action callBack, float needTime, string key)
    {
        CoroutineManager.Instance.AddInvokeRepeating(key, callBack, needTime);
    }

    public static void CancelInvoke(string key)
    {
        CoroutineManager.Instance.CancelInvokeByKey(key);
    }

    public static void TextureToCircle(UnityEngine.Object obj, bool isSetNativeSize = false)
    {
        GameObject gameObject = GetGameObject(obj);

        if (gameObject == null)
        {
            return;
        }

        Image image = gameObject.GetComponent<Image>();

        if (image == null)
        {
            RawImage rawImage = gameObject.GetComponent<RawImage>();

            if (rawImage != null)
            {
                Texture texture = rawImage.texture;

                GameObject.Destroy(rawImage);

                CircleRawImage circleRawImage = gameObject.AddComponent<CircleRawImage>();

                circleRawImage.texture = texture;

                if (isSetNativeSize)
                {
                    circleRawImage.SetNativeSize();
                }
            }
        }
        else
        {
            Sprite sprite = image.sprite;

            GameObject.Destroy(image);

            CircleImage circleImage = gameObject.AddComponent<CircleImage>();

            circleImage.sprite = sprite;

            if (isSetNativeSize)
            {
                circleImage.SetNativeSize();
            }
        }
    }

    public static void TextureToOriginal(UnityEngine.Object obj, bool isSetNativeSize = false)
    {
        GameObject gameObject = GetGameObject(obj);

        if (gameObject == null)
        {
            return;
        }

        CircleImage circleImage = gameObject.GetComponent<CircleImage>();

        if (circleImage == null)
        {
            CircleRawImage circleRawImage = gameObject.GetComponent<CircleRawImage>();

            if (circleRawImage != null)
            {
                Texture texture = circleRawImage.texture;

                GameObject.Destroy(circleRawImage);

                RawImage rawImage = gameObject.AddComponent<RawImage>();

                rawImage.texture = texture;

                if (isSetNativeSize)
                {
                    rawImage.SetNativeSize();
                }
            }
        }
        else
        {
            Sprite sprite = circleImage.sprite;

            GameObject.Destroy(circleImage);

            Image image = gameObject.AddComponent<Image>();

            image.sprite = sprite;

            if (isSetNativeSize)
            {
                image.SetNativeSize();
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
            Material material = null;

            if (isMask)
            {
                material = AssetsManager.Instance.m_materials[2];
            }
            else
            {
                material = AssetsManager.Instance.m_materials[3];
            }

            if (image != null)
            {
                image.material = material;
            }
            else if (rawImage != null)
            {
                rawImage.material = material;
            }
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

    public static void InitDirectory(string path)
    {
        path = path.Replace("\\", "/");

        string extension = Path.GetExtension(path);

        string directoryPath = "";

        if (string.IsNullOrEmpty(extension))
        {
            directoryPath = path;
        }
        else
        {
            directoryPath = Path.GetDirectoryName(path);
        }

        if (!Directory.Exists(directoryPath))
        {
            //确保路径中的所有文件夹都存在
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static Tweener PlayPositionAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startPos = default, Vector3 endPos = default, float duration = 0, Action callBack = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localPosition = startPos;

                tweener = trans.DOLocalMove(endPos, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                trans.localPosition = endPos;
            }
        }

        return tweener;
    }

    public static Tweener PlayAnchoredAnimation(UnityEngine.Object obj, string childPath = "", Vector2 startPos = default, Vector2 endPos = default, float duration = 0, Action callBack = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            RectTransform trans2 = trans.GetComponent<RectTransform>();

            if (duration > 0)
            {
                trans2.anchoredPosition = startPos;

                tweener = trans2.DOAnchorPos(endPos, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                trans2.anchoredPosition = endPos;
            }
        }

        return tweener;
    }

    public static Tweener PlayRotationAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startRot = default, Vector3 endRot = default, float duration = 0, Action callBack = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Incremental, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localRotation = Quaternion.Euler(startRot);

                tweener = trans.DOLocalRotate(endRot, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                trans.localRotation = Quaternion.Euler(endRot);
            }
        }

        return tweener;
    }

    public static Tweener PlayScaleAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startScale = default, Vector3 endScale = default, float duration = 0, Action callBack = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localScale = startScale;

                tweener = trans.DOScale(endScale, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                trans.localScale = endScale;
            }
        }

        return tweener;
    }

    public static Tweener PlayAlphaAnimation(UnityEngine.Object obj, string childPath = "", float startAlpha = 0, float endAlpha = 0, float duration = 0, Action callBack = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        CanvasGroup canvasGroup = trans.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            if (duration > 0)
            {
                canvasGroup.alpha = startAlpha;

                tweener = canvasGroup.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                canvasGroup.alpha = endAlpha;
            }

            return tweener;
        }

        Image image = trans.GetComponent<Image>();

        if (image != null)
        {
            if (duration > 0)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, startAlpha);

                tweener = image.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            }

            return tweener;
        }

        TextMeshProUGUI text = trans.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            if (duration > 0)
            {
                text.color = new Color(image.color.r, image.color.g, image.color.b, startAlpha);

                tweener = text.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                text.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            }

            return tweener;
        }

        return tweener;
    }

    public static Tweener PlayCurveAnimation(UnityEngine.Object obj, string childPath = "", List<Vector3> posList = null, float duration = 0, Action callBack = null, float delay = 0, PathType pathType = PathType.CatmullRom, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        if (posList == null)
        {
            return tweener;
        }

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            Vector3[] posArray = new Vector3[posList.Count];

            for (int i = 0; i < posList.Count; i++)
            {
                posArray[i] = (Vector3)posList[i + 1];
            }

            if (duration > 0)
            {
                trans.localPosition = posArray[0];

                PathMode pathMode = PathMode.TopDown2D;

                foreach (var item in posArray)
                {
                    if (item.z != 0)
                    {
                        pathMode = PathMode.Full3D;
                        break;
                    }
                }

                tweener = trans.DOLocalPath(posArray, duration, pathType, pathMode, 100);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                tweener.OnComplete(() => { callBack?.Invoke(); });
            }
            else
            {
                trans.localPosition = posArray[posArray.Length - 1];
            }
        }

        return tweener;
    }

    public static void ShowAnimationByTime(UnityEngine.Object obj, string childPath = "", string animName = "", float time = 0)
    {
        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        Transform trans = GetTransform(obj);

        if (trans == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans == null)
        {
            return;
        }

        Animation animation = trans.GetComponent<Animation>();

        if (animation != null && animation.Play(animName))
        {
            AnimationState animationState = animation[animName];
            animationState.time = time;
            animation.Sample();
            animation.Stop();
        }
    }

    public static void PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, Action callBack = null)
    {
        CoroutineManager.Instance.InvokePlayAnimation(obj, childPath, animName, wrapMode, callBack);
    }

    public static void StopAnimation(UnityEngine.Object obj, string childPath = "", string animName = "")
    {
        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        Transform trans = GetTransform(obj);

        if (trans == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans == null)
        {
            return;
        }

        Animation animation = trans.GetComponent<Animation>();

        if (animation != null)
        {
            animation.Stop(animName);
        }
    }
}