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
        public Slider colorHueSlider;
        public Slider colorStrengthSlider;
        public Slider colorBrightnessSlider;
        public TextMeshPro hueText;
        public TextMeshPro strengthText;
        public TextMeshPro brightnessText;

        protected void Start()
        {
            InputManager.Instance.OnManipulationUpdated += Instance_OnManipulationUpdated;
            colorHueSlider.UpdateValue();
            colorStrengthSlider.UpdateValue();
            colorBrightnessSlider.UpdateValue();
            SetColor();
        }

        protected void Instance_OnManipulationUpdated(InputSource inputSource)
        {
            if (colorHueSlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind
                || colorBrightnessSlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind
                || colorStrengthSlider.primaryInputSource.inputSourceKind == inputSource.inputSourceKind)
            {
                SetColor();
            }
        }


        protected void SetColor()
        {
            currentColor = Color.HSVToRGB(colorHueSlider.currentValue, colorStrengthSlider.currentValue, colorBrightnessSlider.currentValue);
            if (sampleMaterial)
            {
                sampleMaterial.color = currentColor;
            }
            if (strengthMaterial)
            {
                strengthMaterial.color = Color.HSVToRGB(colorHueSlider.currentValue, 1, 1);
            }
            if (PutYourMaterialHere)
            {
                PutYourMaterialHere.color = currentColor;
            }
            hueText.text = $"Color: {String.Format("{0:0.000}", colorHueSlider.currentValue)}";
            strengthText.text = $"Strength: {String.Format("{0:0.000}", colorStrengthSlider.currentValue)}";
            brightnessText.text = $"Brightness: {String.Format("{0:0.000}", colorBrightnessSlider.currentValue)}";
        }
    }
}