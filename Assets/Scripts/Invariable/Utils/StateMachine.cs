using System;
using System.Collections.Generic;



namespace Invariable
{
    public class StateMachine
    {
        private Dictionary<string, object> m_blackboard1 = new Dictionary<string, object>();
        private Dictionary<string, Action> m_blackboard2 = new Dictionary<string, Action>();
        private Dictionary<string, IStateNode> m_nodes = new Dictionary<string, IStateNode>();
        private IStateNode m_curNode;
        private IStateNode m_preNode;

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public object Owner { private set; get; }

        /// <summary>
        /// 当前运行的节点名称
        /// </summary>
        public string CurrentNode
        {
            get { return m_curNode != null ? m_curNode.GetType().FullName : string.Empty; }
        }

        /// <summary>
        /// 之前运行的节点名称
        /// </summary>
        public string PreviousNode
        {
            get { return m_preNode != null ? m_preNode.GetType().FullName : string.Empty; }
        }


        private StateMachine() { }
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
                m_curNode.OnUpdate();
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        public void Play<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            Play(nodeName);
        }
        public void Play(Type entryNode)
        {
            var nodeName = entryNode.FullName;
            Play(nodeName);
        }
        public void Play(string entryNode)
        {
            m_curNode = TryGetNode(entryNode);
            m_preNode = m_curNode;

            if (m_curNode == null)
                throw new Exception($"Not found entry node: {entryNode}");

            m_curNode.OnEnter();
        }

        /// <summary>
        /// 加入一个节点
        /// </summary>
        public void AddNode<TNode>() where TNode : IStateNode
        {
            var nodeType = typeof(TNode);
            var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
            AddNode(stateNode);
        }
        public void AddNode(IStateNode stateNode)
        {
            if (stateNode == null)
                throw new ArgumentNullException();

            var nodeType = stateNode.GetType();
            var nodeName = nodeType.FullName;

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
            var nodeType = typeof(TNode);
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }
        public void ChangeState(Type nodeType)
        {
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }
        public void ChangeState(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException();

            IStateNode node = TryGetNode(nodeName);
            if (node == null)
            {
                return;
            }

            m_preNode = m_curNode;
            m_curNode.OnExit();
            m_curNode = node;
            m_curNode.OnEnter();
        }

        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardValue(string key, object value)
        {
            m_blackboard1[key] = value;
        }
        public void SetBlackboardValue(string key, Action value)
        {
            m_blackboard2[key] = value;
        }

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        public object GetBlackboardValue(string key)
        {
            if (m_blackboard1.ContainsKey(key))
            {
                return m_blackboard1[key];
            }

            return null;
        }

        /// <summary>
        /// 获取所有黑板数据
        /// </summary>
        public Dictionary<string, Action> GetAllBlackboardValue()
        {
            return m_blackboard2;
        }

        /// <summary>
        /// 尝试获取节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        private IStateNode TryGetNode(string nodeName)
        {
            m_nodes.TryGetValue(nodeName, out IStateNode result);
            return result;
        }
    }
}