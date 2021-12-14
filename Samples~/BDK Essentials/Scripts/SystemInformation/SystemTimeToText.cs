using System;
using TMPro;
using UnityEngine;

public class SystemTimeToText : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] private bool showSeconds;
    private TextMeshPro clockText;

    private void Start()
    {
        clockText = GetComponent<TextMeshPro>();
        InvokeRepeating(nameof(UpdateTime), 0f, showSeconds ? 1f : 10f);
    }

    private void UpdateTime()
    {
        clockText.text = $"{DateTime.Now.Hour.ToString("00")}:{ DateTime.Now.Minute.ToString("00")}";
        if (showSeconds)
            clockText.text += $":{DateTime.Now.Second.ToString("00")}";
    }
#pragma warning restore CS0649
}
