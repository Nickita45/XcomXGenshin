#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class ExcludeLogsFolder : UnityEditor.Build.IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        string logsFolderPath = Application.dataPath + "/StreamingAssets/logs";

        if (Directory.Exists(logsFolderPath))
        {
            Debug.Log("Excluding logs folder from build: " + logsFolderPath);
            Directory.Delete(logsFolderPath, true);
            AssetDatabase.Refresh();
        }
    }
}
#endif