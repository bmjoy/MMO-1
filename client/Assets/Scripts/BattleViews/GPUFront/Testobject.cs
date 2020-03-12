using UnityEngine;
using System.Collections;



public class Testobject : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		GPUBillboardBuffer.Instance.Init();
		GPUBillboardBuffer.Instance.SetupBillboard( 1000 );
		GPUBillboardBuffer.Instance.SetDisappear(1);
        GPUBillboardBuffer.Instance.SetScaleParams(0f, 0.5f, 0.5f, 2f, 1f);
	}
	// Update is called once per frame
	float timeSpan = 0.0f;
	void Update ()
	{
		timeSpan += Time.deltaTime;
		if( timeSpan > 0.05f )
		{
			timeSpan = 0.0f;
            //GPUBillboardBuffer.Instance.DisplayNumber( 
            //    Random.Range( 1, 200).ToString(),
            //    new Vector2( 1.0f, 1.0f ),
            //    new Vector3( Random.Range( -10, 10 ),-2,0 ),
            //    //new Color( Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f) ) );
            //    new Color( 1,1,1 ),
            //    true );

            DisplayNumerInputParam param = new DisplayNumerInputParam();
            param.RandomXInitialSpeedMin = 1;
            param.RandomXInitialSpeedMax = 1;

            param.RandomYInitialSpeedMin = 1;
            param.RandomYInitialSpeedMax = 1;

            param.RandomXaccelerationMin = 1;
            param.RandomXaccelerationMax = 1;

            param.RandomYaccelerationMin = 1;
            param.RandomYaccelerationMax = 1;

            param.FadeTime = 1;
            param.NormalTime = 1;

            GPUBillboardBuffer.Instance.DisplayNumberRandom(
             Random.Range(1, 200).ToString(),
             new Vector2(1.0f, 1.0f),
             new Vector3(Random.Range(-10, 10), -2, 0),
             new Color(1, 1, 1),
             true, param);
		}
	}
}
