using System.Collections;
using System.Collections.Generic;
using System.IO;
using org.vxwo.csharp.json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using XNet.Libs.Utility;


public class StartType : MonoBehaviour
{
   
    private IEnumerator Start()
    {
       yield return  Addressables.InitializeAsync();
        Debuger.Loger = new UnityLoger();

        yield return SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);

        yield return new WaitForEndOfFrame();

#if UNITY_SERVER
        scene = "Server";
        Application.targetFrameRate = 30;
#else
#if !UNITY_EDITOR
        scene = "Application";
#endif
#endif

        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        Destroy(this);
    }

    [Header("Type:Server/Application")]
    public string scene;
}
