using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class OpenSaveFile : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Birfia/Open Save File")]
    private static void Apply()
    {
        Debug.Log("Open Save File");
        string path = Path.Combine(Application.persistentDataPath, "data");
        path = Path.Combine(path, "PlayersInformation.txt");
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Debug.LogWarning("Save Directory Not Exits Yet");
            return;
        }
        System.Diagnostics.Process.Start(path);
    }     
#endif

}
