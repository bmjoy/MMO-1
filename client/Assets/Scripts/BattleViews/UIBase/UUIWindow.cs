using System;
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
    public class OpenUIAsync<T> : CustomYieldInstruction where T : UUIWindow, new()
    {
        private const string UI_PATH = "Windows/{0}.prefab";

        public OpenUIAsync(Transform uiRoot,Action<T> callBack)
        {
            var attrs = typeof(T).GetCustomAttributes(typeof(UIResourcesAttribute), false) as UIResourcesAttribute[];
            if (attrs.Length > 0)
            {
                var name = attrs[0].Name;
                ResourcesManager.S.LoadResourcesWithExName<GameObject>(string.Format(UI_PATH, name), (res) =>
                {
                    var root = GameObject.Instantiate<GameObject>(res);
                    var window = new T();
                    window.uiRoot = root;
                    window.Rect.SetParent(uiRoot, false);
                    window.uiRoot.name = string.Format("UI_{0}", typeof(T).Name);
                    window.runner = root.AddComponent<UIWindowRunner>();
                    window.OnCreate();
                    this.Window = window;
                   // window.uiRoot.SetActive(false);
                    IsDone = true;
                    callBack?.Invoke(this.Window);
                });
            }
            else
            {
                throw new Exception("No found UIResourcesAttribute!");
            }
        }

        public T Window { private set; get; }

        public bool IsDone { private set; get; } = false;

        public override bool keepWaiting
        {
            get
            {
                return !IsDone;
            }
        }
    }

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

	
	public static OpenUIAsync<T> CreateAsync<T>(Transform uiRoot, Action<T> callBack) where T:UUIWindow, new() 
	{
        return new OpenUIAsync<T>(uiRoot, callBack);
	}
}