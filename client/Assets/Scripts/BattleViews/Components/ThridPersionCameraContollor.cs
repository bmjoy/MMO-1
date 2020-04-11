using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Camera))]
public class ThridPersionCameraContollor : UnityEngine.MonoBehaviour
{
    public static ThridPersionCameraContollor Current { private set; get; }
    public Camera CurrenCamera;
    private void Awake()
    {
        Current = this;
        CurrenCamera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {

        rx = Mathf.Lerp(rx, RotationX, Time.deltaTime * 5);
        ry = Mathf.Lerp(ry, RotationY, Time.deltaTime * 5);
        if (lookTarget)  targetPos = lookTarget.position;
        this.transform.position = targetPos - (Quaternion.Euler(rx, ry, 0) * Vector3.forward) * distance;
        this.transform.LookAt(targetPos);
    }


    public float distance = 10;
    private float rx = 0;
    private float ry = 0;
    private Vector3 targetPos;

    public float RotationX { private set; get; } = 45;
    public float RotationY { private set; get; } = 0;

    public Transform lookTarget;

    public void SetLookAt(Transform tr)
    {
        lookTarget = tr;
    }
    public void SetLookAt(Vector3 tr)
    {
        targetPos = tr;
    }

    public ThridPersionCameraContollor RotationByX(float x)
    {
        RotationX += x;
        RotationX = Mathf.Clamp(RotationX ,5, 85);
        return this;
    }

    public ThridPersionCameraContollor RotationByY(float y)
    {
        RotationY -= y;
        return this;
    }

    public Vector3 LookPos { get { return targetPos; } }

    public Quaternion LookRotaion { get { return Quaternion.Euler(0, ry, 0); } }

    internal bool InView(Vector3 position)
    {
        var vp = CurrenCamera.WorldToViewportPoint(position);
        return vp.z > 0;
    }

    public void SetXY(float x, float y)
    {
        this.RotationX = x;
        this.RotationY = y;
    }
}
