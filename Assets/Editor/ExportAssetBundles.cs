using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportAssetBundles : MonoBehaviour {

    [MenuItem("Assets/Build AssetBundle From Selection")]
    static void ExportResourceRGB2()
    {
        // 打开保存面板，获得用户选择的路径  
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "assetbundle");

        if (path.Length != 0)
        {   
            // 选择的要保存的对象  
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            //打包  
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows);
        }
    }

}
