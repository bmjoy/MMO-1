﻿using UnityEngine;
using System.Collections;
using System.Linq;
using ExcelConfig;
using UnityEngine.SceneManagement;
using UGameTools;
using EConfig;
#if UNITY_EDITOR

public class EditorStarter : MonoBehaviour {

	// Use this for initialization
    IEnumerator Start () 
    {
        UApplication.IsEditorMode = true;
        yield return null;
        SceneManager.LoadScene("Welcome", LoadSceneMode.Additive);
        yield return null;
        yield return null;
        tcamera = GameObject.FindObjectOfType<ThridPersionCameraContollor> ();
		UApplication.S.ChangeGate<EditorGate>();
        isStarted = true;
    }
	

    private void Awake()
    {
        isStarted = false;
		UApplication.IsEditorMode = true;
        //yield return null;
    }

    private bool isStarted = false;

	private ThridPersionCameraContollor tcamera;

	// Update is called once per frame
	void Update()
	{
		if (!isStarted) return;

		tcamera.forward.y = -1.08f + slider;
		tcamera.Distance = 22 - distance;
		tcamera.rotationY = ry;

		var midd = tcamera.lookAt;
		var gate = UApplication.G<EditorGate>();
		if (gate != null && isChanged)
		{
			var position = midd.position;
			var left = position + (Vector3.left * distanceCharacter / 2);
			var right = position + (Vector3.right * distanceCharacter / 2);
			gate.releaser.View.SetPosition(left.ToPVer3());
			gate.target.View.SetPosition(right.ToPVer3());
			isChanged = false;
		}
	}

	private bool isChanged = false;

	private void ReleaceReleaser(bool stay)
	{
		var data = ExcelToJSONConfigManager
			.Current.GetConfigByID<CharacterData>(int.Parse(index));
		if (data == null) return;
		var g = UApplication.G<EditorGate>();
		if (g == null) return;
        g.ReplaceRelease(data, stay, aiEnable);
	}

	private void ReplaceTarget(bool stay)
	{
		var data= ExcelToJSONConfigManager
            .Current.GetConfigByID<CharacterData>(int.Parse(index));
		if (data == null)
			return;
		var g = UApplication.G<EditorGate>();
		if (g == null) return;
		g.ReplaceTarget (data,stay,aiEnable);
	}


	private void ReleaseSkill(string key)
	{
		var action = new Proto.Action_ClickSkillIndex { MagicKey = key };
		var g = UApplication.G<EditorGate>();
		if (g == null) return;
		g.DoAction(action);
	}

    private string index ="1";

	private float slider = 1f;
	private float distance = -5f;
	private float ry =0;
	private float distanceCharacter = 10;

	public  GameObject target;
	public bool stay = false;
	public bool aiEnable = false;

	void OnGUI()
	{
		slider = GUI.VerticalSlider (new Rect (10, 10, 30, 200), slider, 0, 1);
		distance = GUI.HorizontalSlider (new Rect (50, 10, 200, 30), distance, -10, 20 );
		ry = GUI.HorizontalSlider (new Rect (50, 35, 200, 30), ry, 0, 180 );
		float last = distanceCharacter;
		distanceCharacter= GUI.HorizontalSlider (new Rect (50, 70, 200, 30), distanceCharacter, -10, 20 );
		if (last != distanceCharacter)
			isChanged = true;

        Time.timeScale = GUI.HorizontalSlider (new Rect (50, 110, 200, 30), Time.timeScale, 0.1f, 1 );
		
		int h = 30;
		int w = 430;
		var rect =new Rect(5,Screen.height-h-20,w+10,h+20);
		GUI.Box(rect,"编辑");
		GUI.BeginGroup(new Rect(10,Screen.height-h,w,h));
		GUILayout.BeginVertical(GUILayout.Width(w));

		GUILayout.BeginHorizontal();

		index =GUILayout.TextField(index,GUILayout.Width(30));

		if(GUILayout.Button("释放者"))
		{
			ReleaceReleaser(stay);
		}
		if(GUILayout.Button("目标"))
		{
			ReplaceTarget(stay);
		}


        if(GUILayout.Button("魔法"))
        {
            ReleaseSkill(index);
        }
		stay= GUILayout.Toggle(stay,"保留");
		aiEnable = GUILayout.Toggle(aiEnable,"AI");

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();



		GUI.EndGroup();

	}

}
#endif
