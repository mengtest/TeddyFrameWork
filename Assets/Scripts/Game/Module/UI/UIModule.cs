﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    UIModule.cs
 Author:      Zeng Zhiwei
 Time:        2020/12/12 11:33:03
=====================================================
*/
#endregion

/*
框架具体实现的功能和需求
    完整的UI生命周期管理【加载，显示，隐藏，关闭页面，根据标示获得相应界面实例】
    完整的数据更新方案【MVC】
    UI组，
    界面层级管理
    界面导航
    https://zhuanlan.zhihu.com/p/102278660
 */

using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UI框架
/// </summary>
public class UIModule : ModuleBase
{
    /// <summary>
    /// 上次检查缓存的时间
    /// </summary>
    private float m_lastCacheCheckTime = 0f;
    /// <summary>
    /// 导航栈
    /// </summary>
    private static Stack<ViewBase> m_navigationStack;
    /// <summary>
    /// 已打开过的view哈希表
    /// </summary>
    private static Dictionary<ViewID, ViewBase> m_viewMap;
    private static Dictionary<ViewType, Transform> m_viewRoot;
    private static float UIPANEL_CACHE_TIME = ViewDefine.UIPANEL_CACHE_TIME;

    public UIModule()
    {
        // 随缘7个
        m_viewMap = new Dictionary<ViewID, ViewBase>(7);
        m_viewRoot = new Dictionary<ViewType, Transform>
        {
            {ViewType.MAIN,GameObject.Find("Canvas/Main").transform },
            {ViewType.POPUP,GameObject.Find("Canvas/Popup").transform },
            {ViewType.FIXED,GameObject.Find("Canvas/Fixed").transform },
        };
    }

    #region API

    public static void OpenView(ViewID key, UIEventArgs args = null)
    {
        if(BeforeOpen(key) == false)
            return;

        ViewBase view = GetView(key);
        if(view != null)
        {
            view.SetOpenParam(args);

            // 已加载过
            if(view.isLoaded)
            {
                InitView(view);
            }
            else
            {
                view.Load(delegate{ InitView(view); });    
            }
        }
        else
        {
            Debug.LogError("[UIModule]界面实例化失败" + key.ToString());
        }
    }

    public static void CloseView(ViewID key)
    {
        ViewBase view = GetView(key);
        //当前界面隐藏
        view.Close();
        UINavigation.RemoveLastItem(view);
    }

    public static Transform GetParent(ViewType viewType)
    {
        return m_viewRoot[viewType];
    }

    #region Scene

    public static void SceneEnter(){}

    /// <summary>
    /// 切换场景，资源管理相关
    /// </summary>
    public static void SceneExit(){}

    #endregion

    #endregion

    #region Private
    private void Update()
    {
        // 已打开界面的update,有需求才做

        // 自动回收cache的界面，todo这个是不是可以用个协程做
        var curTime = Time.time;
        if(curTime - m_lastCacheCheckTime > 1)
        {
            foreach(var item in m_viewMap)
            {
                var view = item.Value;
                if(view.closeTime - curTime > UIPANEL_CACHE_TIME && view.isOpen)
                {
                    m_viewMap[view.key] = null;
                    view.Dispose();
                }
            }
        }
    }

    // 界面打开前的检测
    private static bool BeforeOpen(ViewID key)
    {
        // 做一些界面打开前的检查操作
        return true;
    }

    private static ViewBase GetView(ViewID key)
    {
        ViewBase view;
        if(m_viewMap.TryGetValue(key,out view))
            return view;

        ViewConfig viewConfig;
        ViewDefine.ViewMapping.TryGetValue(key, out viewConfig);
        Type viewClass = viewConfig.viewClass;

        // 反射拿到实例
        view = Activator.CreateInstance(viewClass) as ViewBase;
        if(view != null)
        {
            view.key = key;
            view.assetPath = viewConfig.path;
            view.panelName = viewConfig.name;
            return view;
        }

        return null;
    }

    //初始化界面管理器状态
    private static void InitView(ViewBase view)
    {
        m_viewMap.Add(view.key, view);
        UINavigation.AddItem(view);
        view.Open();
        //--抛出界面打开完成事件
        //--设置场景摄像机状态

    }

    #endregion
}