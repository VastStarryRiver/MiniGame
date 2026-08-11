using System;
using System.Collections.Generic;



namespace Invariable
{
    public class StateMachine
    {
        private Dictionary<string, object> m_blackboardDic = new Dictionary<string, object>();
        private Dictionary<string, IStateNode> m_nodes = new Dictionary<string, IStateNode>();
        private IStateNode m_curNode = null;
        private IStateNode m_preNode = null;

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public object Owner
        {
            private set;
            get;
        }

        /// <summary>
        /// 当前运行的节点名称
        /// </summary>
        public string CurrentNode
        {
            get
            {
                return m_curNode != null ? m_curNode.GetType().FullName : "";
            }
        }

        /// <summary>
        /// 之前运行的节点名称
        /// </summary>
        public string PreviousNode
        {
            get
            {
                return m_preNode != null ? m_preNode.GetType().FullName : "";
            }
        }



        private StateMachine()
        {
        }

        public StateMachine(object owner)
        {
            Owner = owner;
        }



        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            if (m_curNode != null)
            {
                m_curNode.OnUpdate();
            }
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public void Play<TNode>() where TNode : IStateNode
        {
            Type nodeType = typeof(TNode);
            string nodeName = nodeType.FullName;
            Play(nodeName);
        }

        /// <summary>
        /// 按类型启动状态机
        /// </summary>
        public void Play(Type entryNode)
        {
            string nodeName = entryNode.FullName;
            Play(nodeName);
        }

        /// <summary>
        /// 按节点名启动状态机
        /// </summary>
        public void Play(string entryNode)
        {
            m_curNode = TryGetNode(entryNode);
            m_preNode = m_curNode;

            if (m_curNode == null)
            {
                GameLog.Error($"Not found entry node: {entryNode}");

                return;
            }

            m_curNode.OnEnter();
        }

        /// <summary>
        /// 加入一个节点
        /// </summary>
        public void AddNode<TNode>() where TNode : IStateNode
        {
            Type nodeType = typeof(TNode);
            IStateNode stateNode = Activator.CreateInstance(nodeType) as IStateNode;
            AddNode(stateNode);
        }

        /// <summary>
        /// 加入一个状态节点实例
        /// </summary>
        public void AddNode(IStateNode stateNode)
        {
            if (stateNode == null)
            {
                GameLog.Error("AddNode stateNode is null");

                return;
            }

            Type nodeType = stateNode.GetType();
            string nodeName = nodeType.FullName;

            if (!m_nodes.ContainsKey(nodeName))
            {
                stateNode.OnCreate(this);
                m_nodes.Add(nodeName, stateNode);
            }
        }

        /// <summary>
        /// 切换状态节点
        /// </summary>
        public void ChangeState<TNode>() where TNode : IStateNode
        {
            Type nodeType = typeof(TNode);
            string nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }

        /// <summary>
        /// 按类型切换状态节点
        /// </summary>
        public void ChangeState(Type nodeType)
        {
            string nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }

        /// <summary>
        /// 按节点名切换状态节点
        /// </summary>
        public void ChangeState(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                GameLog.Error("ChangeState nodeName is null or empty");

                return;
            }

            IStateNode node = TryGetNode(nodeName);

            if (node == null)
            {
                GameLog.Error($"ChangeState node not found: {nodeName}");

                return;
            }

            m_preNode = m_curNode;

            if (m_curNode != null)
            {
                m_curNode.OnExit();
            }

            m_curNode = node;
            m_curNode.OnEnter();
        }

        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key">黑板键</param>
        /// <param name="value">黑板值</param>
        public void SetBlackboardValue(string key, object value)
        {
            m_blackboardDic[key] = value;
        }

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        /// <param name="key">黑板键</param>
        /// <returns>黑板值</returns>
        public object GetBlackboardValue(string key)
        {
            if (m_blackboardDic.ContainsKey(key))
            {
                return m_blackboardDic[key];
            }

            return null;
        }

        /// <summary>
        /// 尝试获取节点
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <returns>状态节点</returns>
        private IStateNode TryGetNode(string nodeName)
        {
            m_nodes.TryGetValue(nodeName, out IStateNode result);

            return result;
        }
    }
}