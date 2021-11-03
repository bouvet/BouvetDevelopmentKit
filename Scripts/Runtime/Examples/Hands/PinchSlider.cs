using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets up the visual components of the Slider, and holds the max and min positions of the track.
/// </summary>
public class PinchSlider : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField]
    public Transform trackTop;

    [SerializeField]
    public Transform trackBottom;

    [SerializeField]
    protected GameObject dotAnchor;

    [Range(2, 20)]
    [SerializeField]
    public int amountOfDots;

    [HideInInspector]
    public List<float> dotHeights = new List<float>();

    public List<MeshRenderer> dots = new List<MeshRenderer>();

    protected virtual void OnValidate()
    {
        if (!dotAnchor)
        {
            return;
        }

        dots.Clear();
        dotHeights.Clear();
        bool skip = true;
        float distance = trackTop.localPosition.y - trackBottom.localPosition.y;
        foreach (MeshRenderer t in dotAnchor.GetComponentsInChildren<MeshRenderer>())
        {
            if (skip)
            {
                skip = false;
            }
            else
            {
                dots.Add(t);
            }
        }

        for (int i = 0; i < dots.Count; i++)
        {
            if (amountOfDots > i)
            {
                dots[i].enabled = true;
                try
                {
                    dots[i].transform.localPosition = new Vector3(0.015f, trackTop.localPosition.y - i * (distance / (amountOfDots - 1)), 0f);
                    dotHeights.Add(dots[i].transform.localPosition.y);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else if (i != dots.Count - 1)
            {
                dots[i].enabled = false;
            }
        }
    }
#pragma warning restore CS0649
}