using Bouvet.DevelopmentKit.Input;
using UnityEngine;

public class GenerateGrabbable : MonoBehaviour
{
#if UNITY_EDITOR
    public void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Initialize();
        }
    }
#endif
    protected Bounds bounds;
    protected MeshFilter[] targetMesh;

    public TwoHandGrabbable Initialize(GrabbableGenerationMode generationMode = GrabbableGenerationMode.OnSelf, bool meshColliderConvex = false, bool autoSetup = true)
    {
        TwoHandGrabbable thg;
        switch (generationMode)
        {
            case GrabbableGenerationMode.OnSelf:
                targetMesh = new MeshFilter[] { GetComponent<MeshFilter>() };
                break;
            case GrabbableGenerationMode.FirstChild:
                targetMesh = new MeshFilter[] { GetComponentInChildren<MeshFilter>() };
                break;
            case GrabbableGenerationMode.LargestChild:
                targetMesh = FindLargestMeshFilter();
                break;
            case GrabbableGenerationMode.AllChildren:
                targetMesh = FindAllMeshFiltersInChildren();
                break;
        }

        thg = gameObject.GetComponent<TwoHandGrabbable>();
        if (!thg)
        {
            thg = gameObject.AddComponent<TwoHandGrabbable>();
        }

        foreach (var mesh in targetMesh)
        {
            bounds = mesh.sharedMesh.bounds;

            if (generationMode == GrabbableGenerationMode.OnSelf)
            {
                if (autoSetup)
                {
                    AddCollider(mesh, meshColliderConvex);
                    thg.SetupGrabbable(true, true, true, 0.01f, float.MaxValue);
                }
                return thg;
            }
            AddCollider(mesh, meshColliderConvex);
            mesh.gameObject.AddComponent<ReferToGrabbable>().referableGrabbable = thg;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(this);
            };
        }
        else
        {
            Destroy(this);
        }
#else
        Destroy(this);
#endif
        if (autoSetup)
        {
            thg.SetupGrabbable(true, true, true, 0.01f, float.MaxValue);
        }
        return thg;
    }

    private void AddCollider(MeshFilter mesh, bool meshColliderConvex)
    {
        if (!meshColliderConvex)
        {
            if (!mesh.gameObject.GetComponent<BoxCollider>())
            {
                mesh.gameObject.AddComponent<BoxCollider>();
            }
        }
        else
        {
            if (!mesh.gameObject.GetComponent<MeshCollider>())
            {
                var collider = mesh.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
            }
        }
    }

    private MeshFilter[] FindLargestMeshFilter()
    {
        // Find the subobject holding a MeshFilter and calculate its diagonal size
        var children = FindAllMeshFiltersInChildren();

        float biggestDiagonal = 0f;
        int indexOfBiggestDiagonal = -1;
        float tempDiagonal;
        for (int i = 0; i < children.Length; i++)
        {
            tempDiagonal = Mathf.Abs(Vector3.Scale(children[i].sharedMesh.bounds.size, children[i].transform.lossyScale).magnitude);
            if (biggestDiagonal < tempDiagonal)
            {
                biggestDiagonal = tempDiagonal;
                indexOfBiggestDiagonal = i;
            }
        }
        if (indexOfBiggestDiagonal != -1)
        {
            return new MeshFilter[] { children[indexOfBiggestDiagonal] };
        }
        return null;
    }

    private MeshFilter[] FindAllMeshFiltersInChildren()
    {
        return gameObject.GetComponentsInChildren<MeshFilter>();
    }

    public enum GrabbableGenerationMode
    {
        OnSelf = 0,
        FirstChild = 1,
        LargestChild = 2,
        AllChildren = 3
    }
}