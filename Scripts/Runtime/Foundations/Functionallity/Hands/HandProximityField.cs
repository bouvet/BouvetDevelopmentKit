using Bouvet.DevelopmentKit;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HandProximityField : MonoBehaviour
{
#pragma warning disable CS0649
    public HandInteractionMode ignoreHand;
    public bool targetFieldWithCursor = true;

    private void Start()
    {
        gameObject.layer = 2;
    }
#pragma warning restore CS0649
}