﻿using System;
using System.Collections;
using UnityEngine;


public class UIResourcesAttribute:Attribute
{
	public UIResourcesAttribute(string name)
	{
		Name = name;
	}

	public string Name{ private set; get; }
}

public enum WindowState
{
	NONE,
	ONSHOWING,
	SHOW,
	ONHIDING,
	HIDDEN
}

public abstract class UUIWindow:UUIElement
{
	private MonoBehaviour runner;

	protected UUIWindow ()
	{
        CanDestoryWhenHidden = true;
	}

	public void StartCoroutine(IEnumerator el)
	{
		runner.StartCoroutine(el);
    }


	protected  override void OnDestory()
	{
        UnityEngine.Object.Destroy(uiRoot);
	}

    protected virtual void OnUpdateUIData()
    {
    }

	protected virtual void OnShow()
	{
		
	}

	protected virtual void OnHide()
	{
		
	}

	protected virtual void OnUpdate()
	{
		
	}

	protected virtual void OnBeforeShow()
	{
		
	}

	public void ShowWindow()
	{
		this.state = WindowState.ONSHOWING;
	}

	public void HideWindow()
	{
        this.state = WindowState.ONHIDING;
	}

	private void Update()
    {
        switch (state)
        {
            case WindowState.NONE:
			//state = WindowState.ONSHOWING;
                break;
            case WindowState.ONSHOWING:
                this.uiRoot.SetActive(true);
                OnBeforeShow();
                state = WindowState.SHOW;
                OnShow();
                break;
            case WindowState.SHOW:
                OnUpdate();
                break;
            case WindowState.ONHIDING:
                state = WindowState.HIDDEN;
                OnHide();
                this.uiRoot.SetActive(false);
                break;
            case WindowState.HIDDEN:
                
                break;
        }
    }

	protected bool CanDestoryWhenHidden { set; get; }

	public bool IsVisable{ get { return this.state == WindowState.SHOW;} }

	public bool CanDestory{ get{ return this.state == WindowState.HIDDEN &&CanDestoryWhenHidden; }}

	public static void UpdateUI(UUIWindow w)
	{
		w.Update ();
	}

    public static void UpdateUIData(UUIWindow w)
    {
        if (w.state == WindowState.SHOW)
            w.OnUpdateUIData();
    }

	private WindowState state =  WindowState.NONE;

	private const string UI_PATH ="Windows/{0}";

	public static T Create<T>(Transform uiRoot) where T:UUIWindow, new() 
	{
		var attrs = typeof(T).GetCustomAttributes (typeof(UIResourcesAttribute), false) as UIResourcesAttribute[];
		var window = new T ();
		if (attrs.Length > 0) {
			var name = attrs [0].Name;
			var res = ResourcesManager.Singleton.LoadResources<GameObject> (String.Format (UI_PATH, name));
			var root = GameObject.Instantiate<GameObject> (res);
			window.uiRoot = root;
			window.Rect.SetParent(uiRoot, false);
			window.uiRoot.name = string.Format ("UI_{0}", typeof(T).Name);
			window.runner = root.AddComponent<UIWindowRunner>();
			window.OnCreate ();
		} else {
			throw new Exception ("No found UIResourcesAttribute!");
		}

		return window;
	}
}