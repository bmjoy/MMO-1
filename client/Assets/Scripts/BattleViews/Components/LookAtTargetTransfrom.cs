using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTargetTransfrom : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (target)
            this.transform.LookAt(target);
    }

    public Transform target;
}
