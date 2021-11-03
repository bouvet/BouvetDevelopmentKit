using System;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEditor;
using UnityEngine;

public class UnpackPrefab : MonoBehaviour
{
#pragma warning disable CS0168
#if UNITY_EDITOR
    private void OnValidate()
    {
        try
        {
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(this);
            };
        }
        catch (Exception e)
        {
            
        }
    }
#endif
#pragma warning restore CS0168
}
