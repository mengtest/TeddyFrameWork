﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    ActionIdle.cs
 Author:      Zeng Zhiwei
 Time:        2021/4/2 16:51:34
=====================================================
*/
#endregion

namespace Aver3
{
    public class ActionIdle : BTAction{ }
    public class ActionPatrol : BTAction
    {
        protected override void OnEnter()
        {
            base.OnEnter();
        }

        protected override BTResult OnExecute()
        {
            base.OnExecute();
            return BTResult.Running;
        }

        protected override void OnExit()
        {
            base.OnExit();
        }
    }
}