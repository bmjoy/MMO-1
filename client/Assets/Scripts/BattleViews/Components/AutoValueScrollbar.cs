using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ RequireComponent(typeof( Scrollbar))]
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
        bar.value = 0;
        yield return null;
        while (Time.time - start < durtion)
        {
            bar.value = (Time.time - start) / durtion;
            yield return null;
        }
        bar.value = 1;
        yield return null;
    }
}
