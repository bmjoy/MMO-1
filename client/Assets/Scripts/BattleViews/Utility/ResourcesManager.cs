using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExcelConfig;
using org.vxwo.csharp.json;
using EConfig;

public class ResourcesManager : XSingleton<ResourcesManager>, IConfigLoader
{

	List<T> IConfigLoader.Deserialize<T>()
	{
		var name = ExcelToJSONConfigManager.GetFileName<T>();
		var json = LoadText("Json/" + name);
		if (json == null) return null;
		return JsonTool.Deserialize<List<T>>(json);
	}

	public class LoadProcesser
	{
		public CallBackDele CallBack;
		public ResourceRequest Request;
	}

	public delegate void CallBackDele(Object res);


	// Update is called once per frame
	void Update ()
    {
		if (loaders.Count > 0) 
        {
			foreach (var i in loaders)
            {
				if (i.Request.isDone) {
					_dones.Enqueue (i);	
				}
			}

			while (_dones.Count > 0) 
			{
				var d = _dones.Dequeue ();
				d.CallBack (d.Request.asset);
				loaders.Remove (d);
			}
		}
	}

    private readonly Queue<LoadProcesser> _dones = new Queue<LoadProcesser>();
			
	private readonly HashSet<LoadProcesser> loaders = new HashSet<LoadProcesser> ();

	public string LoadText(string path)
	{
		var res = string.Empty;
		var text = LoadResourcesWithExName<TextAsset> (path);
		if (text != null) {
			res = text.text;
		}
		Resources.UnloadAsset (text);
		//Debug.Log (res);
		return res;
	}

	public T LoadResources<T>(string path) where T:Object
	{
		return Resources.Load<T> (path);
	}

	public T LoadResourcesWithExName<T>(string path) where T : Object
	{
		int len = path.LastIndexOf('.');
		if (len < 0)
		{
			Debug.LogError($"{path}");
			return null;
		}
		path = path.Substring(0, path.LastIndexOf('.'));
		return LoadResources<T>(path);
	}

	public T[] LoadAll<T>(string path) where T:Object
	{
		return Resources.LoadAll<T> (path);
	}

	public ResourceRequest LoadAsync<T>(string path) where T:Object
	{
		return Resources.LoadAsync<T> (path);
	}

	public void LoadAsyncCallBack<T>(string path, CallBackDele callBack) where T:Object
	{
		var request = LoadAsync<T> (path);
		var processer = new LoadProcesser{ CallBack = callBack, Request = request };
		loaders.Add (processer);
	}

	public string ReadStreamingFile(string namae)
	{
		var path = Path.Combine(Application.streamingAssetsPath, namae);
		Debug.Log(path);
		return File.ReadAllText(path);
    }

	public Sprite LoadIcon(CharacterMagicData item)
	{
		return LoadResources<Sprite>("Icon/" + item.IconKey);
	}
	public Sprite LoadIcon(ItemData item)
	{
		return LoadResources<Sprite>("Icon/" + item.Icon);
	}
	public Sprite LoadIcon(BattleLevelData item)
	{
		return LoadResources<Sprite>("Icon/" + item.Icon);
	}

	public GameObject LoadModel(ItemData item)
	{
		return LoadResources<GameObject>($"ItemModel/{item.ResModel}");
	}
}
