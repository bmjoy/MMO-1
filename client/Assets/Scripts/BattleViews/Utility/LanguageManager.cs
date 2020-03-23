using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : XSingleton<LanguageManager>
{

    public Dictionary<string, string> Keys = new Dictionary<string, string>();

    void Awake()
    {
        var xml = ResourcesManager.S.LoadResources<TextAsset>("Language");
        var ls = XmlParser.DeSerialize<LanguageSetting>(xml.text);
        foreach (var i in ls.Keys)
        {
            if (Keys.ContainsKey(i.Value))
            {
                Debug.LogError($"{i.Key} exists!");
                continue;
            }
            Keys.Add(i.Key, i.Value);
        }
    }

    public string this[string key]
    {
        get
        {
            if (Keys.TryGetValue(key, out string v)) return v;
            return key;
        }
    }

    public string Format(string key, params object[] pars)
    {
        return string.Format(this[key], pars);
    }
}
