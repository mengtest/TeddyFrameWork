﻿//THIS FILE IS GENERATED BY tabtool, DO NOT EDIT IT!
//GENERATE TIME [2020/3/8 15:14:51]
#pragma once
#include "tabletool/readtablefield.h"
#include "tabletool/myconfig.h"


struct tbsIdCount : public ITableObject<tbsIdCount>
{
	int id;
	int count;

	virtual bool FromString(string s)
	{
		DataReader dr;
		vector<string> vs = dr.GetStringList(s,',');
		if (vs.size() != 2)
		{
			ErrorLog("tbsIdCount字段配置错误");
			return false;
		}
		id = stoi(vs[0]);
		count = stoi(vs[1]);
		return true;
	}
};


struct tbsKeyValue : public ITableObject<tbsKeyValue>
{
	int key;
	int value;

	virtual bool FromString(string s)
	{
		DataReader dr;
		vector<string> vs = dr.GetStringList(s,',');
		if (vs.size() != 2)
		{
			ErrorLog("tbsKeyValue字段配置错误");
			return false;
		}
		key = stoi(vs[0]);
		value = stoi(vs[1]);
		return true;
	}
};


struct tbsTest : public ITableObject<tbsTest>
{
	int a;
	string b;
	float c;

	virtual bool FromString(string s)
	{
		DataReader dr;
		vector<string> vs = dr.GetStringList(s,',');
		if (vs.size() != 3)
		{
			ErrorLog("tbsTest字段配置错误");
			return false;
		}
		a = stoi(vs[0]);
		b = (vs[1]);
		c = stof(vs[2]);
		return true;
	}
};

