using System;
using System.Collections.Generic;
using UGameTools;
using UnityEngine.UI;
using UnityEngine;
using Proto.PServices;
using System.Collections;

public abstract class UUIElement
{
	protected GameObject uiRoot;
	protected abstract void OnDestory ();
	protected abstract void OnCreate ();
    protected RectTransform _rect;

    public RectTransform Rect
    {
        get{ 
            if (_rect)
                return _rect;
            else
            {
                _rect = this.uiRoot.GetComponent<RectTransform>();
                return _rect;
            }
        } 
    }

	public static void Destory(UUIElement el)
    {
		el.OnDestory ();
	}

    protected T FindChild<T>(string name) where T: Component
    {
        return uiRoot.transform.FindChild<T>(name);
    }
}



public class UUIManager:XSingleton<UUIManager>,IEventMasker
{
    protected override void Awake()
    {
        base.Awake();
        eventMask.SetActive(false);
    }

	private readonly Dictionary<string,UUIWindow> _window=new Dictionary<string, UUIWindow> ();
    private readonly Dictionary<int,UUITip> _tips= new Dictionary<int, UUITip> ();

	void Update()
	{

		while (_addTemp.Count > 0) {
			var t = _addTemp.Dequeue ();
			_window.Add (t.GetType ().Name, t);
		}

		foreach (var i in _window) {
			UUIWindow.UpdateUI( i.Value);
			if (i.Value.CanDestory) {
				_delTemp.Enqueue (i.Value);
			}
		}

		while (_delTemp.Count > 0) {
			var t = _delTemp.Dequeue ();
			if (_window.Remove (t.GetType ().Name))
				UUIElement.Destory (t);
		}
            
	}

    public void UpdateUIData()
    {
        foreach (var i in _window)
        {
            UUIWindow.UpdateUIData(i.Value);
        }
    }

    public void UpdateUIData<T>()  where T: UUIWindow, new()
    {
        var ui=  GetUIWindow<T>();
        if (ui != null)
            UUIWindow.UpdateUIData(ui);
    }
    private readonly Queue<UUITip> _tipDelTemp = new Queue<UUITip>();

    void LateUpdate()
    {
        foreach (var i in _tips)
        {
            if (i.Value == null) continue;
            if (i.Value.CanDestory)
            {
                _tipDelTemp.Enqueue(i.Value);
            }
            else
            {
                i.Value.LateUpdate();
            }
        }

        while (_tipDelTemp.Count > 0)
        {
            var tip = _tipDelTemp.Dequeue();
            _tips.Remove(tip.InstanceID);
            UUIElement.Destory(tip);
        }

        if (maskTime > 0 && maskTime < Time.time)
        {
            maskTime = -1;
            eventMask.SetActive(false);
        }
    }

	public T GetUIWindow<T>()where T:UUIWindow, new()
	{
        if (_window.TryGetValue(typeof(T).Name, out UUIWindow obj))
        {
            return obj as T;
        }
        return default;
	}

	private readonly Queue<UUIWindow> _addTemp = new Queue<UUIWindow> ();
	private readonly Queue<UUIWindow> _delTemp = new Queue<UUIWindow> ();

  
    public Coroutine CreateWindowAsync<T>(Action<T> callBack) where T : UUIWindow, new()
    {
        return StartCoroutine(CreateWindow(callBack));
    }

    private IEnumerator CreateWindow<T>(Action<T> callback) where T : UUIWindow, new()
    {
        var ui = GetUIWindow<T>();
        if (ui == null)
        {
            var async = UUIWindow.CreateAsync<T>(this.BaseCanvas.transform, callback);
            yield return async; ui = async.Window;
            _addTemp.Enqueue(ui);
        }
    }

    public int TryToGetTip<T>(int id, bool world, out T tip) where T : UUITip, new()
    {
        if (_tips.TryGetValue(id, out UUITip t))
        {
            tip = t as T;
            return id;
        }

        var tIndex = index++;
        if (index == int.MaxValue) index = 0;
        _tips.Add(tIndex, null);
        StartCoroutine(CreateTipAsync<T>(world, tIndex));
        tip = null;
        return tIndex;
    }

    private int index = 0;

    private IEnumerator CreateTipAsync<T>(bool world,int index) where T : UUITip, new()
    {
        var root = world ? worldTop.transform : this.top.transform;
        var async = UUITip.CreateAsync<T>(index,root, world);
        yield return async;
        var tip = async.Tip;
        if (_tips.ContainsKey(index))
        {
            this._tips[index] = tip;
            UUITip.Update(tip);
        }
        else
        { 
           UUIElement.Destory(tip);
        }
    }

	public void ShowMask(bool show)
    {
        if (show)
        {
            BackImage.ActiveSelfObject(true);
            BackImage.transform.FindChild<AutoValueScrollbar>("LoadingBg").ResetValue(1);
        }
        else
        {
            BackImage.ActiveSelfObject(false);
        }
    }

	public void ShowLoading(float p,string text = "Loading")
	{
		BackImage.transform.FindChild<Scrollbar> ("Scrollbar").value =  p;
        BackImage.transform.FindChild<Text>("LoadingText").text = text;
	}

    private float? duration;

	public Image BackImage;
	public GameObject top;
    public GameObject worldTop;
    public Canvas BaseCanvas;

    private Transform rectTop;

    public Vector2 OffsetInUI(Vector3 position)
    {
        var pos = Camera.main.WorldToScreenPoint(position) ;
        return new Vector2(pos.x, pos.y);
    }

    public void HideAll()
    {
        foreach (var i in _window)
        {
            if (i.Value.IsVisable)
                i.Value.HideWindow();
        }
    }

    public GameObject eventMask;

    /// <summary>
    /// 当前mask
    /// </summary>
    private float maskTime = 0;

    public void MaskEvent()
    {
        maskTime = Time.time+ 2f;
        eventMask.SetActive(true);
    }

    public void UnMaskEvent()
    {
        maskTime = -1;
        eventMask.SetActive(false);
    }

    void IEventMasker.Mask()
    {
        MaskEvent();
    }

    void IEventMasker.UnMask()
    {
        UnMaskEvent();
    }
}