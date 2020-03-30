using System;
using UnityEngine;
using System.Collections;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;

public class UParticlePlayer:MonoBehaviour, IParticlePlayer
{
    public string Path;

    private bool IsDestory = false;

    private IEnumerator Start()
    {
        yield return ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(Path, (obj) =>
        {
             if (obj == null) return;
             Instantiate(obj, this.transform);
        });
    }

    #region IParticlePlayer implementation
    public void DestoryParticle()
    {
        IsDestory = true;
        Destroy(this.gameObject);
    }

    public void AutoDestory(float time)
    {
        IsDestory = true;
        Destroy(gameObject, time); 
    }
        

    public bool CanDestory
    {
        get
        {
            return !IsDestory;
        }
    }

    
    #endregion

}
