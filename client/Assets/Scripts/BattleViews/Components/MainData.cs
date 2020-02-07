using UnityEngine;
using System.Collections;
using UGameTools;

public class MainData : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        foreach (var i in pos)
        {
            i.ActiveSelfObject(false);
        }
	}

    public Transform[] pos;
}
