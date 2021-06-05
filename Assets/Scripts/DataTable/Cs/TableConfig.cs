﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2021/6/5 10:13:44]

using System.Collections.Generic;

public class NPCPropertyItem
{
    public int id;
    public int maxHp;
    public int maxMp;
    public int physic;
    public int magic;
    public int attackRange;
    public int viewRange;
    public int moveSpeed;
    public int turnSpeed;
    public List<float> startPoint;
    public float maxRadius;
    public string modelPath;
    public List<int> skillList;
}

public class NPCPropertyTable : TableManager<NPCPropertyItem, NPCPropertyTable>
{
    public override bool Load()
    {
        TableReader tr = new TableReader();
        DataReader dr = new DataReader();
        MyDataTable dt = tr.ReadFile(new MyConfig().WorkDir+"NPCProperty.txt");

        foreach(MyDataRow row in dt.Rows)
        {
            var item = new NPCPropertyItem();
             item.id = dr.GetInt(row["id"].ToString());
             item.maxHp = dr.GetInt(row["maxHp"].ToString());
             item.maxMp = dr.GetInt(row["maxMp"].ToString());
             item.physic = dr.GetInt(row["physic"].ToString());
             item.magic = dr.GetInt(row["magic"].ToString());
             item.attackRange = dr.GetInt(row["attackRange"].ToString());
             item.viewRange = dr.GetInt(row["viewRange"].ToString());
             item.moveSpeed = dr.GetInt(row["moveSpeed"].ToString());
             item.turnSpeed = dr.GetInt(row["turnSpeed"].ToString());
             item.startPoint = dr.GetFloatList(row["startPoint"].ToString());
             item.maxRadius = dr.GetFloat(row["maxRadius"].ToString());
             item.modelPath = (row["modelPath"].ToString());
             item.skillList = dr.GetIntList(row["skillList"].ToString());
              m_Items[item.id] = item;
        }
        return true;
    }
}


public class skillItem
{
    public int id;
    public string config;
    public int priority;
    public string icon;
}

public class skillTable : TableManager<skillItem, skillTable>
{
    public override bool Load()
    {
        TableReader tr = new TableReader();
        DataReader dr = new DataReader();
        MyDataTable dt = tr.ReadFile(new MyConfig().WorkDir+"skill.txt");

        foreach(MyDataRow row in dt.Rows)
        {
            var item = new skillItem();
             item.id = dr.GetInt(row["id"].ToString());
             item.config = (row["config"].ToString());
             item.priority = dr.GetInt(row["priority"].ToString());
             item.icon = (row["icon"].ToString());
              m_Items[item.id] = item;
        }
        return true;
    }
}

public class TableConfig : SingletonTable<TableConfig>
{
    public bool LoadTableConfig()
    {
       if (!NPCPropertyTable.Instance.Load()) return false;
       if (!skillTable.Instance.Load()) return false;
        return true;
    }
}
