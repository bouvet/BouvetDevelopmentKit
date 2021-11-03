using TMPro;
using UnityEngine;

public class InformationMessage : MonoBehaviour
{
#pragma warning disable CS0649
    public bool countingDown;

    [SerializeField]
    private TextMeshProUGUI textField;

    [SerializeField]
    private Transform countDownBar;

    public InformationMessage SetText(string text)
    {
        textField.text = text;
        return this;
    }

    // Update is called once per frame
    private void Update()
    {
        if (countingDown)
        {
            countDownBar.localScale = countDownBar.localScale - new Vector3(Time.deltaTime * 7f / (textField.text.Length + 1f), 0f, 0f);
            if (countDownBar.localScale.x < 0.01f)
            {
                GetComponentInParent<SystemMessageController>().CountedDown(gameObject);
            }
        }
    }
#pragma warning restore CS0649
}