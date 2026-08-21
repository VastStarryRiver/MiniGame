using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public static class PoolUtils
    {
        public const int DefaultMaxSize = 30; // C#对象池对于每个class的上限，一种class可以有DefaultMaxSize个实例对象，List<int>和List<float>算两种class
        public const int DefaultGameObjectMaxSize = 50; // GameObject对象池对于每个key的上限，一个key可以有DefaultGameObjectMaxSize个GameObject

        private static Dictionary<Type, object> m_typedPools = null;
        private static Dictionary<string, Stack<GameObject>> m_gameObjectPools = null;
        private static Dictionary<string, int> m_gameObjectMaxSizes = null;
        private static Transform m_poolParent = null;

        public class ObjectPool<T> where T : class, new()
        {
            private readonly Stack<T> Stack;
            private readonly Action<T> OnGet;
            private readonly Action<T> OnRelease;
            private readonly int MaxSize;



            public ObjectPool(int maxSize = DefaultMaxSize, Action<T> onGet = null, Action<T> onRelease = null)
            {
                Stack = new Stack<T>();
                OnGet = onGet;
                OnRelease = onRelease;
                MaxSize = maxSize > 0 ? maxSize : DefaultMaxSize;
            }



            /// <summary>
            /// 从池中取出对象，池空时新建
            /// </summary>
            public T Get()
            {
                T item = Stack.Count > 0 ? Stack.Pop() : new T();
                OnGet?.Invoke(item);

                return item;
            }

            /// <summary>
            /// 归还对象，超出上限则丢弃
            /// </summary>
            public void Release(T item)
            {
                if (item == null)
                {
                    return;
                }

                OnRelease?.Invoke(item);

                if (Stack.Count < MaxSize)
                {
                    Stack.Push(item);
                }
            }
        }



        /// <summary>
        /// 从类型池取出对象
        /// </summary>
        public static T Get<T>() where T : class, new()
        {
            return GetPool<T>().Get();
        }

        /// <summary>
        /// 归还对象到类型池
        /// </summary>
        public static void Release<T>(T item) where T : class, new()
        {
            GetPool<T>().Release(item);
        }

        /// <summary>
        /// 清空指定类型的对象池，下次取出时自动重建
        /// </summary>
        public static void ClearPool<T>() where T : class, new()
        {
            if (m_typedPools == null)
            {
                return;
            }

            m_typedPools.Remove(typeof(T));
        }

        /// <summary>
        /// 从 GameObject 池取出实例，池空时按预制体实例化
        /// </summary>
        public static GameObject GetGameObject(string key, GameObject prefab, Transform parent)
        {
            if (string.IsNullOrEmpty(key) || prefab == null)
            {
                GameLog.Error("PoolUtils.GetGameObject 参数无效");

                return null;
            }

            Stack<GameObject> stack = GetGameObjectStack(key);

            while (stack.Count > 0)
            {
                GameObject item = stack.Pop();

                if (item == null)
                {
                    continue;
                }

                if (parent != null)
                {
                    item.transform.SetParent(parent, false);
                }

                item.SetActive(true);

                return item;
            }

            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        /// <summary>
        /// 归还 GameObject，超出单 key 上限则销毁
        /// </summary>
        public static void ReleaseGameObject(string key, GameObject instance)
        {
            if (string.IsNullOrEmpty(key) || instance == null)
            {
                return;
            }

            Stack<GameObject> stack = GetGameObjectStack(key);

            if (stack.Count >= GetGameObjectMaxSize(key))
            {
                GameLog.Info($"PoolUtils 池 [{key}] 超限销毁，可考虑 SetGameObjectPoolMaxSize 调大上限");
                UnityEngine.Object.Destroy(instance);

                return;
            }

            if (m_poolParent != null)
            {
                instance.transform.SetParent(m_poolParent, false);
            }

            instance.SetActive(false);
            stack.Push(instance);
        }

        /// <summary>
        /// 设置指定 key 的 GameObject 池上限，小于等于 0 时回落默认上限
        /// </summary>
        public static void SetGameObjectPoolMaxSize(string key, int maxSize)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (maxSize <= 0)
            {
                if (m_gameObjectMaxSizes != null)
                {
                    m_gameObjectMaxSizes.Remove(key);
                }

                return;
            }

            m_gameObjectMaxSizes ??= new Dictionary<string, int>();
            m_gameObjectMaxSizes[key] = maxSize;
        }

        /// <summary>
        /// 注入对象池根节点，归还时统一挂到该节点下
        /// </summary>
        public static void SetPoolParent(Transform parent)
        {
            m_poolParent = parent;
        }

        /// <summary>
        /// 清空指定 key 的 GameObject 池并销毁实例，保留该 key 的自定义上限
        /// </summary>
        public static void ClearGameObjectPool(string key)
        {
            if (string.IsNullOrEmpty(key) || m_gameObjectPools == null)
            {
                return;
            }

            if (!m_gameObjectPools.TryGetValue(key, out Stack<GameObject> stack))
            {
                return;
            }

            DestroyStackItems(stack);
            m_gameObjectPools.Remove(key);
        }

        /// <summary>
        /// 清空全部 GameObject 池并销毁实例，保留各 key 的自定义上限
        /// </summary>
        public static void ClearAllGameObjectPools()
        {
            if (m_gameObjectPools == null)
            {
                return;
            }

            foreach (KeyValuePair<string, Stack<GameObject>> pair in m_gameObjectPools)
            {
                DestroyStackItems(pair.Value);
            }

            m_gameObjectPools.Clear();
        }

        /// <summary>
        /// 获取或创建指定类型的对象池，集合类型归还时自动清空
        /// </summary>
        private static ObjectPool<T> GetPool<T>() where T : class, new()
        {
            m_typedPools ??= new Dictionary<Type, object>();
            Type type = typeof(T);

            if (m_typedPools.TryGetValue(type, out object poolObj))
            {
                return (ObjectPool<T>)poolObj;
            }

            Action<T> onRelease = null;

            if (typeof(IList).IsAssignableFrom(type))
            {
                onRelease = (item) =>
                {
                    if (item is IList list)
                    {
                        list.Clear();
                    }
                };
            }

            ObjectPool<T> pool = new ObjectPool<T>(DefaultMaxSize, null, onRelease);
            m_typedPools.Add(type, pool);

            return pool;
        }

        /// <summary>
        /// 获取指定 key 的 GameObject 堆栈
        /// </summary>
        private static Stack<GameObject> GetGameObjectStack(string key)
        {
            m_gameObjectPools ??= new Dictionary<string, Stack<GameObject>>();

            if (!m_gameObjectPools.TryGetValue(key, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>();
                m_gameObjectPools.Add(key, stack);
            }

            return stack;
        }

        /// <summary>
        /// 获取指定 key 的 GameObject 池上限，未自定义时回落默认值
        /// </summary>
        private static int GetGameObjectMaxSize(string key)
        {
            if (m_gameObjectMaxSizes != null && m_gameObjectMaxSizes.TryGetValue(key, out int maxSize))
            {
                return maxSize;
            }

            return DefaultGameObjectMaxSize;
        }

        /// <summary>
        /// 销毁堆栈内全部 GameObject 实例
        /// </summary>
        private static void DestroyStackItems(Stack<GameObject> stack)
        {
            while (stack.Count > 0)
            {
                GameObject item = stack.Pop();

                if (item != null)
                {
                    UnityEngine.Object.Destroy(item);
                }
            }
        }
    }
}