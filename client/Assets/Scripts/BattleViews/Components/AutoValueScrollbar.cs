﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof( Scrollbar))]
public class AutoValueScrollbar : MonoBehaviour {


    private Scrollbar bar;
	
    void Awake()
    {
        bar = GetComponent<Scrollbar>();
    }

    public void ResetValue(float durtion)
    {
        StopAllCoroutines();
        StartCoroutine(RunBar(durtion));
    }

    private IEnumerator RunBar(float durtion)
    {
        var start =Time.time;
        bar.size = 0;
        yield return null;
        while (Time.time - start < durtion)
        {
            bar.size = (Time.time - start) / durtion;
            yield return null;
        }
        bar.size = 1;
        yield return null;
    }
}
