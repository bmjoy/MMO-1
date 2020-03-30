using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ExcelConfig;
using org.vxwo.csharp.json;
using EConfig;
using UnityEngine.AddressableAssets;
using System.Collections;


public class ResourcesManager : XSingleton<ResourcesManager>, IConfigLoader
{
	private IEnumerator Start()
	{
		yield return null;
		//yield return Addressables.InitializeAsync();
	}

	List<T> IConfigLoader.Deserialize<T>()
	{
		var name = ExcelToJSONConfigManager.GetFileName<T>();
		var json = LoadText("Json/" + name);
		if (json == null) return null;
		return JsonTool.Deserialize<List<T>>(json);
	}

	public delegate void CallBackDele<T>(T res);

	public string LoadText(string path)
	{
		return ReadStreamingFile(path);
	}

	public Coroutine LoadResourcesWithExName<T>(string path, CallBackDele<T> call) where T : Object
	{
		var res = $"Assets/AssetRes/{path}";
		return StartCoroutine(LoadPath(res, call));
	}

	private IEnumerator LoadPath<T>(string path, CallBackDele<T> callback) where T : Object
	{
		var asset = Addressables.LoadAssetAsync<T>(path);
		yield return asset;
		Debug.Log($"{path}->{asset.Result}");
		callback?.Invoke(asset.Result);
	}

	private string ReadStreamingFile(string namae)
	{
		var path = Path.Combine(Application.streamingAssetsPath, namae);
		Debug.Log(path);
		return File.ReadAllText(path);
	}

	public void LoadIcon(CharacterMagicData item, CallBackDele<Sprite> callBack)
	{
		LoadSpriteAsset($"Icon/{item.IconKey}.png", callBack);
	}
	public void LoadIcon(ItemData item, CallBackDele<Sprite> callBack)
	{
		LoadSpriteAsset($"Icon/{item.Icon}.png",callBack);
	}
	public void LoadIcon(BattleLevelData item, CallBackDele<Sprite> callBack)
	{
		LoadSpriteAsset($"Icon/{item.Icon}.png", callBack);
       
	}

	private void LoadSpriteAsset(string path, CallBackDele<Sprite> callBack)
	{
        void tCall(Texture2D tex)
        {
            var s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            callBack?.Invoke(s);
        }
        LoadResourcesWithExName($"{path}", (CallBackDele<Texture2D>)tCall);
    }

	public void LoadModel(ItemData item, CallBackDele<GameObject> call)
	{
		LoadResourcesWithExName($"ItemModel/{item.ResModel}.prefab", call);
	}

}
