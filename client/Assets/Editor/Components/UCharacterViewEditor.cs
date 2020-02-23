using UnityEngine;
using System.Collections;
using UnityEditor;
using GameLogic.Game.Elements;

[CustomEditor(typeof(UCharacterView))]
public class UCharacterViewEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginVertical();
		if (GUILayout.Button("Open AI Tree"))
		{
			var target = this.target as UCharacterView;
			var window = EditorWindow.GetWindow<AITreeEditor>();
			if (window == null) return;
			if (!(target.GElement is BattleCharacter character)) return;
			var root = character.AiRoot;
			if (root == null)
			{
				EditorUtility.DisplayDialog("Failuer", "Current character no ai tree", "OK");
			}
			else
			{
				window.AttachRoot(root);
				AIRunner.Current?.Attach(character);
			}
		}
		EditorGUILayout.EndVertical();
		base.OnInspectorGUI();
	}
}
