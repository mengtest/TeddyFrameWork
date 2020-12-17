﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    ViewDefine.cs
 Author:      Zeng Zhiwei
 Time:        2020/12/12 15:33:43
=====================================================
*/
#endregion

using System;
using System.Collections.Generic;

public enum ViewType
{
    /// <summary>
    /// 主界面
    /// </summary>
    MAIN = 1,
    /// <summary>
    /// 弹出窗体
    /// </summary>
    POPUP,
    /// <summary>
    /// 固定弹窗
    /// </summary>
    FIXED,
}

/// <summary>
/// 模糊类型
/// </summary>
public enum LucencyType
{
    /// <summary>
    /// 透明
    /// </summary>
    Lucency,
    /// <summary>
    /// 半透明
    /// </summary>
    Translucence,
}

/// <summary>
/// 穿透类型
/// </summary>
public enum PentrateType
{
    /// <summary>
    /// 能穿透
    /// </summary>
    Pentrate,
    /// <summary>
    /// 不能穿透
    /// </summary>
    ImPenetrable,
}

public enum ViewID
{
    Test = 1,
    Test2,
}

public struct ViewConfig
{
    public string name;
    public string path;
    public Type viewClass;

    public ViewConfig(string name, string path, Type viewClass)
    {
        this.name = name;
        this.path = path;
        this.viewClass = viewClass;
    }
}

public static class ViewDefine
{
    public static float UIPANEL_CACHE_TIME = 5f;
    public static Dictionary<ViewID, ViewConfig> ViewMapping = new Dictionary<ViewID, ViewConfig> 
    {
        { ViewID.Test,new ViewConfig("TestView","test/TestPanel",typeof(TestView))},
        { ViewID.Test2,new ViewConfig("TestView2","test/TestPanel2",typeof(TestView2))},
    };
}
