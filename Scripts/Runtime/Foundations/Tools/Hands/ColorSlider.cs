using Bouvet.DevelopmentKit.Input;
using System;
using Bouvet.DevelopmentKit.Input.Hands;
using TMPro;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools.Hands
{
    public class ColorSlider : MonoBehaviour
    {
        public Material PutYourMaterialHere;
        public Color currentColor;
        public Material sampleMaterial;
        public Material strengthMaterial;
        public Slider xSlider;
        public Slider ySlider;
        public Slider zSlider;
        public TextMeshPro xOffset;
        public TextMeshPro yOffest;
        public TextMeshPro zOffset;

        public HandMenuExample handMenu;

        protected void Start()
        {
            InputManager.Instance.OnManipulationUpdated += Instance_OnManipulationUpdated;
            xSlider.UpdateValue();
            ySlider.UpdateValue();
            zSlider.UpdateValue();
            SetColor();
        }

        protected void Instance_OnManipulationUpdated(InputSource inputSource)
        {
            if (xSlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind
                || zSlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind
                || ySlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind)
            {
                SetColor();
            }
        }


        protected void SetColor()
        {
            handMenu.offset = new Vector3(xSlider.currentValue, ySlider.currentValue, zSlider.currentValue);
            xOffset.text = $"x: {xSlider.currentValue}";
            yOffest.text = $"y: {ySlider.currentValue}";
            zOffset.text = $"z: {zSlider.currentValue}";
        }
    }
}
