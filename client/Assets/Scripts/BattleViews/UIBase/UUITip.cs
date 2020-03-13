﻿using System;
using UnityEngine;


public class UITipResourcesAttribute:Attribute
{
	public UITipResourcesAttribute(string name)
	{
		this.Name = name;
	}

	public string Name{set;get;}
}

public abstract class UUITip:UUIElement
{
	public UUITip ()
	{
        InstanceID = _index++;
        if (_index == int.MaxValue)
            _index = 0;
	}

    private static int _index = 0;

	private bool LastUpdate = false;

    public int InstanceID { get; } = 0;

    protected override void OnDestory ()
	{
		GameObject.Destroy(this.uiRoot, 0.1f);
		this._rect = null;
	}

    public bool IsWorld { private set; get; } = false;

	public void LateUpdate()
	{
		LastUpdate = false;
	}

    public bool CanDestory{ get{ return !LastUpdate;}}
    public void LookAt(Camera c)
    {
        uiRoot.transform.LookAt(c.transform);
    }

    public static T Create<T>(Transform parent, bool world) where T: UUITip,new()
    {
        var attrs = typeof(T).GetCustomAttributes(typeof(UITipResourcesAttribute), false) as UITipResourcesAttribute[];
        if(attrs.Length ==0) return default(T);
        var resources = attrs[0].Name;
        var res = ResourcesManager.Singleton.LoadResources<GameObject>("Tips/" + resources);
        var root = GameObject.Instantiate(res) as GameObject;
        var tip = new T();
        tip.IsWorld = world;
        root.name = string.Format("_TIP_{0}_{1}", tip.InstanceID , typeof(T).Name);
        tip.uiRoot = root;
        tip.Rect.SetParent(parent, false);
        tip.OnCreate();
        return tip;
    }

	public static void Update(UUITip tip,Vector2 pos)
	{
        tip.Rect.position = new Vector3(pos.x, pos.y, 0);
        Update(tip);
	}

    public static void Update(UUITip tip, Vector3 pos)
    {
        tip.uiRoot.transform.position = pos;
        Update(tip);
    }

    public static void Update(UUITip tip)
    {
        tip.LastUpdate = true;
        tip.OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        
    }



}