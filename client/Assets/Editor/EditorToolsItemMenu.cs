using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public sealed class EditorToolsItemMenu
{

    private static bool ShowSave()
    {
        for (int i = 0; i < EditorSceneManager.loadedSceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isDirty)
            {
                return EditorUtility.DisplayDialog("Notify", "Drop modify？", "Yes", "Cancel");
            }
        }
        return true;
    }
    [MenuItem("GAME/UI/INIT_LANGUAGE_FILE")]
    public static void CreateLanguage()
    {

    }
       

    [MenuItem("GAME/Go_To_EditScene &e")]
    public static void GoToEditorScene()
    {
        if (!ShowSave())
            return;
        
        if (EditorApplication.isPlaying)
        {
            EditorApplication.Beep();
            return;
        }

        var editor ="Assets/Scenes/EditorReleaseMagic.unity";
        EditorSceneManager.OpenScene(editor);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("GAME/Go_To_StarScene &s")]
    public static void GoToStarScene()
    {

        if (!ShowSave())  return;
        if (EditorApplication.isPlaying)
        {
            EditorApplication.Beep();
            return;
        }
        var editor = "Assets/Scenes/Launch.unity";
        EditorSceneManager.OpenScene(editor);
        EditorApplication.isPlaying = true;

    }
    
}


