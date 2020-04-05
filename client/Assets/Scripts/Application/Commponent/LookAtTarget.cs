using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (target)
            this.transform.LookAt(target);
    }

    public Transform target;
}
