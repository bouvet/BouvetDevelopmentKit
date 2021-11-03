using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemMessageController : MonoBehaviour
{
#pragma warning disable CS0649
    public static SystemMessageController Instance;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Transform gridAnchor;

    public List<InformationMessage> messages = new List<InformationMessage>();
    private Queue queue = new Queue();

    private void Awake()
    {
        Instance = this;
    }

    public void AddMessage(string message)
    {
        messages.Add(Instantiate(prefab, gridAnchor).GetComponent<InformationMessage>().SetText(message));
        if (messages.Count == 1)
        {
            messages[0].countingDown = true;
        }
    }

    internal void CountedDown(GameObject listing)
    {
        messages.RemoveAt(0);
        Destroy(listing);
        if (messages.Count != 0)
        {
            messages[0].countingDown = true;
        }
    }
#pragma warning restore CS0649
}