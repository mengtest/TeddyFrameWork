﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    BTAction.cs
 Author:      Zeng Zhiwei
 Time:        2021/4/2 11:03:19
=====================================================
*/
#endregion

namespace Aver3
{
    /// <summary>
    /// BTAction是负责游戏逻辑的行为节点，也就是行为树里面的“行为”。
    /// 外部调用Evaluate来判断节点是否可以进入，可以进入的话，调用Update来获取执行结果
    /// 维护生命周期：OnEnter OnExit OnExecute 
    /// </summary>
    public class BTAction : BTNode
    {
        private BTPrecondition m_precondition;
        private BTActionStatus m_status = BTActionStatus.Ready;
        public string Name { get { return GetType().ToString(); } }

        public void SetPrecondition(BTPrecondition precondition)
        {
            m_precondition = precondition;
        }

        public bool Evaluate()
        {
            // 评估这个节点是否可以进入：1.有设置条件；2.条件通过；
            if(m_precondition != null && m_precondition.IsTrue()) //&& OnEvaluate())
                return true;

            return false;
        }

        // 给子类提供个性化检查的接口
        //protected virtual bool OnEvaluate() { return true; }

        /// <summary>
        /// 供外部调用
        /// </summary>
        /// <returns></returns>
        public BTResult Update()
        {
            return OnUpdate();
        }

        /// <summary>
        ///  行为的执行，返回BTResult
        /// </summary>
        protected virtual BTResult OnUpdate()
        {
            BTResult result = BTResult.Finished;
            if(m_status == BTActionStatus.Ready)
            {
                m_status = BTActionStatus.Running;
                OnEnter();
            }

            if(m_status == BTActionStatus.Running)
            {       
                result = OnExecute();
                if(result != BTResult.Running)
                {
                    OnExit();
                    m_status = BTActionStatus.Ready;
                }
            }

            return result;
        }

        /// <summary>
        /// 第一次进入行为
        /// </summary>
        protected virtual void OnEnter() 
        {
            if(BTConst.ENABLE_BTACTION_LOG)
            {   // For debug
                GameLog.Log("OnEnter " + " [" + this.Name + "]");
            }
        }

        /// <summary>
        /// 离开行为
        /// </summary>
        protected virtual void OnExit() 
        {
            if(BTConst.ENABLE_BTACTION_LOG)
            {   // For debug
                GameLog.Log("OnExit " + " [" + this.Name + "]");
            }
        }

        /// <summary>
        /// 行为进行中
        /// </summary>
        /// <returns></returns>
        protected virtual BTResult OnExecute()
        {
            return BTResult.Finished;
        }
    }
}