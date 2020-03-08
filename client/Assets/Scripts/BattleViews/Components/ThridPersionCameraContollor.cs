using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Camera))]
public class ThridPersionCameraContollor : UnityEngine.MonoBehaviour
{
    public static ThridPersionCameraContollor Current { private set; get; }
    private Camera CurrenCamera;
    private void Awake()
    {
        Current = this;
        CurrenCamera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {

        rx = Mathf.Lerp(rx, rotationX, Time.deltaTime * 5);
        ry = Mathf.Lerp(ry, rotationY, Time.deltaTime * 5);

       // var targetPos = Vector3.zero;
        if (lookTarget)
            targetPos = lookTarget.position;
        this.transform.position = targetPos - (Quaternion.Euler(rx, ry, 0) * Vector3.forward) * distance;
        this.transform.LookAt(targetPos);
    }

    public float distance = 10;
    private float rx = 0;
    private float ry = 0;
    private Vector3 targetPos;

    public float rotationX = 45;
    public float rotationY = 0;

    public Transform lookTarget;

    public void SetLookAt(Transform tr)
    {
        lookTarget = tr;
    }
    public void SetLookAt(Vector3 tr)
    {
        targetPos = tr;
    }

    public ThridPersionCameraContollor RotationX(float x)
    {
        rotationX += x;
        rotationX = Mathf.Clamp(rotationX ,5, 85);
        return this;
    }

    public ThridPersionCameraContollor RotationY(float y)
    {
        rotationY -= y;
        //rotationY %= 360;
        return this;
    }

    public Vector3 LookPos { get { return targetPos; } }

    public Quaternion LookRotaion { get { return Quaternion.Euler(0, ry, 0); } }

    internal bool InView(Vector3 position)
    {
        var vp = CurrenCamera.WorldToViewportPoint(position);
        return vp.z > 0;
    }
}
