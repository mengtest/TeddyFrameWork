﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    DummyEntity.cs
 Author:      Zeng Zhiwei
 Time:        2020/5/29 13:13:40
=====================================================
*/
#endregion

public class DummyUnit : Unit
{
    public DummyUnit(float pointX,float pointZ)
    {
        Set3DPosition(pointX,0, pointZ);
    }
}