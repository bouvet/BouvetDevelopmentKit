using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Materials")]
    public Material BoundingBoxFrameMaterial;
    public Material BoundingBoxMaterial;
    public Material ButtonBackgroundMaterial;
    public Material PanelBackgroundMaterial;

    [Header("Audio clips")]
    public AudioClip StartManipulation;
    public AudioClip EndManipulation;
    public AudioClip StartProximity;
    public AudioClip EndProximity;
    public AudioClip ButtonPressed;

    private void Awake()
    {
        Instance = this;
    }
}
