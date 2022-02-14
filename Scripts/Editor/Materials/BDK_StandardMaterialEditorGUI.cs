using UnityEngine;
using UnityEditor;

namespace Bouvet.DevelopmentKit
{
    public class BDK_StandardMaterialEditorGUI : ShaderGUI
    {
        private Material matReference;
        private MaterialEditor editor;
        private MaterialProperty[] properties;

        // bools to open/close dropdowns
        private bool basicSettings = true;
        private bool renderingSettings = true;
        private bool fluentSettings = true;
        private bool advancedOptions = false;

        // setup to allow for enum usage
        private enum renderBlendModes
        {
            Zero = 0,
            One = 1,
            DstColor = 2,
            SrcColor = 3,
            OneMinusDstColor = 4,
            SrcAlpha = 5,
            OneMinusSrcColor = 6,
            DstAlpha = 7,
            OneMinusDstAlpha = 8,
            SrcAlphaSaturate = 9,
            OneMinusSrcAlpha = 10
        }
        private renderBlendModes sourceBlend;
        private renderBlendModes dstBlend;

        private UnityEngine.Rendering.BlendOp blendOp;
        private UnityEngine.Rendering.CompareFunction zTestMode;
        private UnityEngine.Rendering.ColorWriteMask colorWriteMask;
        private UnityEngine.Rendering.CullMode cullMode;
        private UnityEngine.Rendering.CompareFunction stencilComparison;
        private UnityEngine.Rendering.StencilOp stencilOperation;
        private UnityEngine.Rendering.RenderQueue renderQueue;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] materialProperties)
        {
            // Render the shader properties using standard GUI
            //base.OnGUI(materialEditor, properties);

            editor = materialEditor;
            properties = materialProperties;
            this.matReference = editor.target as Material;

            EditorGUI.BeginChangeCheck();
            //UnityEngine.Rendering.CullMode

            Basics();
            Rendering();
            Fluent();
            AdvancedOptions();

            EditorGUI.EndChangeCheck();

            // can do stuff on if-changed, but right now everything gets updated on-demand.
        }

        void Basics()
        {

            basicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(basicSettings, "Basic Settings");

            if (basicSettings == true)
            {
                GUISimpleProperty("_Color", "Color");
                GUISimpleProperty("_Metallic", "Metallic");
                GUISimpleProperty("_Smoothness", "Smoothness");
                GUISimpleProperty("_Cutoff", "Alpha Cutoff");
                EditorGUILayout.BeginHorizontal();
                GUISimpleProperty("_EnableEmission", "Enable Emission");
                GUISimpleProperty("_EmissiveColor", "Emissive Color");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        void Rendering()
        {
            renderingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(renderingSettings, "Rendering Settings");

            if (renderingSettings == true)
            {
                GUISimpleProperty("_DirectionalLight", "Directional Light");
                GUISimpleProperty("_SpecularHighlights", "Specular Highlights");
                GUISimpleProperty("_SphericalHarmonics", "Spherical Harmonics");
                GUISimpleProperty("_Reflections", "Reflections");

                GUISimpleProperty("_Refraction", "Refraction");
                if (matReference.GetFloat("_Refraction") > 0)
                {
                    GUIRangeProperty("_RefractiveIndex", "Refractive Index");
                }

                GUISimpleProperty("_RimLight", "Rim Light");
                if (matReference.GetFloat("_RimLight") > 0)
                {
                    GUIColorProperty("_RimColor", "Rim Color");
                    GUIRangeProperty("_RimPower", "Rim Power");
                }

                GUISimpleProperty("_VertexColors", "Vertex Colors");
                GUISimpleProperty("_VertexExtrusion", "Vertex Extrusion");
                if (matReference.GetFloat("_VertexExtrusion") > 0)
                {
                    GUISimpleProperty("_VertexExtrusionValue", "Vertex Ex. Amount");
                    GUISimpleProperty("_VertexExtrusionSmoothNormals", "Vertex Ex. Smooth Normals");
                }

                GUIRangeProperty("_BlendedClippingWidth", "Blended Clipping Width");
                GUISimpleProperty("_ClippingBorder", "Clipping Border");
                if (matReference.GetFloat("_ClippingBorder") > 0)
                {
                    GUIRangeProperty("_ClippingBorderWidth", "Clipping B. Width");
                    GUIColorProperty("_ClippingBorderColor", "Clipping B. Color");
                }

                EditorGUILayout.BeginHorizontal();
                GUISimpleProperty("_NearPlaneFade", "Near Plane Fade");
                GUISimpleProperty("_NearLightFade", "Near Light Fade");
                EditorGUILayout.EndHorizontal();
                if (matReference.GetFloat("_NearPlaneFade") > 0 || matReference.GetFloat("_NearLightFade") > 0)
                {
                    GUIRangeProperty("_FadeBeginDistance", "Fade Begin Distance");
                    GUIRangeProperty("_FadeCompleteDistance", "Fade End Distance");
                    GUIRangeProperty("_FadeMinValue", "Fade Min Value");
                }

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        void Fluent()
        {
            fluentSettings = EditorGUILayout.BeginFoldoutHeaderGroup(fluentSettings, "Fluent Settings");

            if (fluentSettings == true)
            {

                GUISimpleProperty("_HoverLight", "Hover Light");
                if (matReference.GetFloat("_HoverLight") > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUISimpleProperty("_EnableHoverColorOverride", "Hover L. Color Override");
                    GUIColorProperty("_HoverColorOverride", "Hover L. Color");
                    EditorGUILayout.EndHorizontal();
                }

                GUISimpleProperty("_ProximityLight", "Proximity Light");
                if (matReference.GetFloat("_ProximityLight") > 0)
                {
                    GUISimpleProperty("_EnableProximityLightColorOverride", "Prox L. Color Override");
                    if (matReference.GetFloat("_EnableProximityLightColorOverride") > 0)
                    {
                        GUIColorProperty("_ProximityLightCenterColorOverride", "Center Color");
                        GUIColorProperty("_ProximityLightMiddleColorOverride", "Middle Color");
                        GUIColorProperty("_ProximityLightOuterColorOverride", "Outer Color");
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUISimpleProperty("_ProximityLightSubtractive", "Prox L. Subtractive");
                    GUISimpleProperty("_ProximityLightTwoSided", "Prox L. Two-Sided");
                    EditorGUILayout.EndHorizontal();

                }

                GUIRangeProperty("_FluentLightIntensity", "Fluent Light Intensity");
                //EditorGUILayout.TextField("Fluent L. Intensity scales the strength of effects like Hover and Prox light.");
                //Gotta figure out how to to tooltips, later on.

                GUISimpleProperty("_RoundCorners", "Round Corners");
                if (matReference.GetFloat("_RoundCorners") > 0)
                {
                    GUIRangeProperty("_RoundCornerRadius", "Round Corner Radius");
                    GUIRangeProperty("_RoundCornerMargin", "Round Corner Margin");

                    GUISimpleProperty("_IndependentCorners", "Independent Corners");
                    if (matReference.GetFloat("_IndependentCorners") > 0)
                    {
                        GUIVectorProperty("_RoundCornersRadius", "Round Corners Radius");
                    }
                }

                GUISimpleProperty("_BorderLight", "Border Light");
                if (matReference.GetFloat("_BorderLight") > 0)
                {
                    GUISimpleProperty("_BorderLightUsesHoverColor", "Border L. Uses Hover Color");
                    GUISimpleProperty("_BorderLightReplacesAlbedo", "Border L. Replaces Albedo");
                    GUISimpleProperty("_BorderLightOpaque", "Border L. Opaque");

                    GUIRangeProperty("_BorderWidth", "Border L. Width");
                    GUIRangeProperty("_BorderMinValue", "Border L. Min Value");
                    GUIRangeProperty("_EdgeSmoothingValue", "Border L. Edge Smoothing Value");

                    if (matReference.GetFloat("_BorderLightOpaque") > 0)
                    {
                        GUIRangeProperty("_BorderLightOpaqueAlpha", "Border L. Opaque Alpha.");
                    }
                }

                GUISimpleProperty("_InnerGlow", "Inner Glow");
                if (matReference.GetFloat("_InnerGlow") > 0)
                {
                    GUIColorProperty("_InnerGlowColor", "Inner Glow Color");
                    GUIRangeProperty("_InnerGlowPower", "Inner Glow Power");
                }

                GUISimpleProperty("_EnvironmentColoring", "Environment Coloring");
                if (matReference.GetFloat("_EnvironmentColoring") > 0)
                {
                    GUIRangeProperty("_EnvironmentColorThreshold", "Env. Color Threshold");
                    GUIRangeProperty("_EnvironmentColorIntensity", "Env. Color Intensity");
                    GUIColorProperty("_EnvironmentColorX", "Env. Color X (RGB)");
                    GUIColorProperty("_EnvironmentColorY", "Env. Color Y (RGB)");
                    GUIColorProperty("_EnvironmentColorZ", "Env. Color Z (RGB)");
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        void AdvancedOptions()
        {
            advancedOptions = EditorGUILayout.BeginFoldoutHeaderGroup(advancedOptions, "Advanced Options");

            sourceBlend = (renderBlendModes)matReference.GetInt("_SrcBlend");
            dstBlend = (renderBlendModes)matReference.GetInt("_DstBlend");
            blendOp = (UnityEngine.Rendering.BlendOp)matReference.GetInt("_BlendOp");
            zTestMode = (UnityEngine.Rendering.CompareFunction)matReference.GetInt("_ZTest");
            colorWriteMask = (UnityEngine.Rendering.ColorWriteMask)matReference.GetInt("_ColorWriteMask");
            cullMode = (UnityEngine.Rendering.CullMode)matReference.GetInt("_CullMode");
            stencilComparison = (UnityEngine.Rendering.CompareFunction)matReference.GetInt("_StencilComparison");
            stencilOperation = (UnityEngine.Rendering.StencilOp)matReference.GetInt("_StencilOperation");
            //renderQueue = 

            if (advancedOptions == true)
            {
                sourceBlend = (renderBlendModes)EditorGUILayout.EnumPopup("Source Blend", sourceBlend);
                dstBlend = (renderBlendModes)EditorGUILayout.EnumPopup("Destination Blend", dstBlend);
                blendOp = (UnityEngine.Rendering.BlendOp)EditorGUILayout.EnumPopup("Blend Operation", blendOp);
                zTestMode = (UnityEngine.Rendering.CompareFunction)EditorGUILayout.EnumPopup("Depth Test Mode", zTestMode);

                GUIFloatProperty("_ZOffsetFactor", "Depth Offset Factor");
                GUIFloatProperty("_ZOffsetUnits", "Depth Offset Units");

                colorWriteMask = (UnityEngine.Rendering.ColorWriteMask)EditorGUILayout.EnumPopup("Color Write Mask", colorWriteMask);
                cullMode = (UnityEngine.Rendering.CullMode)EditorGUILayout.EnumPopup("Cull Mode", cullMode);
                GUIRangeProperty("_RenderQueueOverride", "Render Queue Override");
                GUISimpleProperty("_IgnoreZScale", "Ignore Z Scale");
                GUISimpleProperty("_Stencil", "Enable Stencil Testing");
                if (matReference.GetFloat("_Stencil") > 0)
                {
                    GUIRangeProperty("_StencilReference", "Stencil Reference");
                    stencilComparison = (UnityEngine.Rendering.CompareFunction)EditorGUILayout.EnumPopup("Stencil Comparison", stencilComparison);
                    stencilOperation = (UnityEngine.Rendering.StencilOp)EditorGUILayout.EnumPopup("Stencil Operation", stencilOperation);
                }

                EditorGUILayout.Space();

                editor.RenderQueueField();
                editor.EnableInstancingField();
                editor.DoubleSidedGIField();

                AdvancedOptionsUpdate();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        void AdvancedOptionsUpdate()
        {
            matReference.SetInt("_SrcBlend", ((int)sourceBlend));
            matReference.SetInt("_DstBlend", ((int)dstBlend));
            matReference.SetInt("_BlendOp", ((int)blendOp));
            matReference.SetInt("_ZTest", ((int)zTestMode));
            matReference.SetInt("_ColorWriteMask", ((int)colorWriteMask));
            matReference.SetInt("_CullMode", ((int)cullMode));
            matReference.SetInt("_StencilComparison", ((int)stencilComparison));
            matReference.SetInt("_StencilOperation", ((int)stencilOperation));
        }

        private void GUISimpleProperty(string propName, string displayName)
        {
            MaterialProperty map = FindProperty(propName, properties);
            editor.ShaderProperty(map, displayName);
        }

        private void GUIFloatProperty(string propName, string displayName)
        {
            MaterialProperty map = FindProperty(propName, properties);
            editor.FloatProperty(map, displayName);
        }

        private void GUIRangeProperty(string propName, string displayName)
        {
            MaterialProperty map = FindProperty(propName, properties);
            editor.RangeProperty(map, displayName);
        }

        private void GUIColorProperty(string propName, string displayName)
        {
            MaterialProperty map = FindProperty(propName, properties);
            editor.ColorProperty(map, displayName);
        }

        private void GUIVectorProperty(string propName, string displayName)
        {
            MaterialProperty map = FindProperty(propName, properties);
            editor.VectorProperty(map, displayName);
        }
    }
}