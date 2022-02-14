using Bouvet.DevelopmentKit.Input;
using System.Collections.Generic;
using UnityEngine;

public class DrapeMenu : Slider
{
#pragma warning disable CS0649
    [SerializeField]
    protected GameObject VisibleComponents;

    [SerializeField]
    protected List<InteractableButton> Buttons;

    [SerializeField]
    protected float rightTargetPosition;

    public override void Initialize()
    {
        base.Initialize();
        HideMenu();
    }

    public override void UpdateValue()
    {
        base.UpdateValue();
        VisibleComponents.transform.localScale = Vector3.one - Vector3.right * currentValue;
        VisibleComponents.transform.localPosition = Vector3.right * rightTargetPosition * currentValue;
        if (currentValue == 1)
        {
            VisibleComponents.SetActive(false);
        }
        else if (!VisibleComponents.activeSelf)
        {
            VisibleComponents.SetActive(true);
        }
    }

    public override void BeginInteraction(InputSource inputSource)
    {
        if(inputSource.collidedObject.Equals(gameObject))
        {
            base.BeginInteraction(inputSource);
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].enabled = false;
            }
        }
    }

    public override void EndInteraction(InputSource inputSource)
    {
        base.EndInteraction(inputSource);
        if (currentValue < 0.05f)
        {
            currentValue = 0;
            transform.localPosition = ps.trackBottom.localPosition;
            VisibleComponents.SetActive(true);
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].enabled = true;
            }
        }
        else
        {
            HideMenu();
        }
    }

    protected void HideMenu()
    {
        currentValue = 1;
        transform.localPosition = ps.trackTop.localPosition;
        VisibleComponents.SetActive(false);
    }
#pragma warning restore CS0649
}