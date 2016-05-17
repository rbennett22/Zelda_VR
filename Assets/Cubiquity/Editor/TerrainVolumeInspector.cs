// Uncomment the following line to enable eight materials on the
// terrain. Be aware that this may have problems on Unity 5.
// You will also need to uncomment the same define in the shaders.
//#define EIGHT_MATERIALS

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cubiquity
{
    //public enum TerrainTools { None, Sculpt, Smooth, Paint, Settings };

    [CustomEditor(typeof(TerrainVolume))]
    public class TerrainVolumeInspector : Editor
    {
        TerrainVolume terrainVolume;

        private const int NoOfBrushes = 5;

        public static Tool lastTool = Tool.None;

        // Making these static lets the values persist when switching away from the volume
        // and then back to it. Actually I though serialization would be the solution here,
        // but serializing properties of an inspector doesn't seem to work.
        private static float brushOuterRadius = 5.0f;

        private static float brushOpacity = 1.0f;

        private static bool mSculptPressed = true;
        private static bool mSmoothPressed = false;
        private static bool mPaintPressed = false;
        private static bool mSettingsPressed = false;

        private static bool sculptPressed
        {
            get { return mSculptPressed; }
            set { if (mSculptPressed != value) { mSculptPressed = value; OnTerrainToolChanged(); } }
        }

        private static bool smoothPressed
        {
            get { return mSmoothPressed; }
            set { if (mSmoothPressed != value) { mSmoothPressed = value; OnTerrainToolChanged(); } }
        }

        private static bool paintPressed
        {
            get { return mPaintPressed; }
            set { if (mPaintPressed != value) { mPaintPressed = value; OnTerrainToolChanged(); } }
        }

        private static bool settingsPressed
        {
            get { return mSettingsPressed; }
            set { if (mSettingsPressed != value) { mSettingsPressed = value; OnTerrainToolChanged(); } }
        }

        private static int selectedBrush = 0;
        private static int selectedTexture = 0;

        Texture[] brushTextures;

        GUIContent warningLabelContent;

        public void OnEnable()
        {
            terrainVolume = target as TerrainVolume;

            brushTextures = new Texture[NoOfBrushes];
            brushTextures[0] = Resources.Load("Icons/SoftBrush") as Texture;
            brushTextures[1] = Resources.Load("Icons/MediumSoftBrush") as Texture;
            brushTextures[2] = Resources.Load("Icons/MediumBrush") as Texture;
            brushTextures[3] = Resources.Load("Icons/MediumHardBrush") as Texture;
            brushTextures[4] = Resources.Load("Icons/HardBrush") as Texture;
        }

        public override void OnInspectorGUI()
        {
            // Check whether the selected Unity transform tool has changed.
            if (TerrainVolumeInspector.lastTool != Tools.current)
            {
                OnTransformToolChanged();
                TerrainVolumeInspector.lastTool = Tools.current;
            }

            // This group of toggle buttons mimics Unity's built-in terrain editor. Note that there is no way to unselect
            // a button by clicking on them (this could be implemented but felt strange), but you can get no button selected
            // by activating one of the Unity transform tools.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(sculptPressed, "Sculpt", EditorStyles.miniButtonLeft, GUILayout.Height(24)))
            {
                sculptPressed = true;
                smoothPressed = false;
                paintPressed = false;
                settingsPressed = false;
            }
            if (GUILayout.Toggle(smoothPressed, "Smooth", EditorStyles.miniButtonMid, GUILayout.Height(24)))
            {
                sculptPressed = false;
                smoothPressed = true;
                paintPressed = false;
                settingsPressed = false;
            }
            if (GUILayout.Toggle(paintPressed, "Paint", EditorStyles.miniButtonMid, GUILayout.Height(24)))
            {
                sculptPressed = false;
                smoothPressed = false;
                paintPressed = true;
                settingsPressed = false;
            }
            if (GUILayout.Toggle(settingsPressed, "Settings", EditorStyles.miniButtonRight, GUILayout.Height(24)))
            {
                sculptPressed = false;
                smoothPressed = false;
                paintPressed = false;
                settingsPressed = true;
            }
            EditorGUILayout.EndHorizontal();

            if ((sculptPressed || smoothPressed || paintPressed) && (terrainVolume.data != null) &&
                (terrainVolume.data.writePermissions == VolumeData.WritePermissions.ReadOnly))
            {
                EditorGUILayout.HelpBox("The attached volume data (" + terrainVolume.data.name + ") is set to read only! Changes you make here cannot be saved.", MessageType.Error);
            }

            if (sculptPressed)
            {
                DrawSculptControls();
            }

            if (smoothPressed)
            {
                DrawSmoothControls();
            }

            if (paintPressed)
            {
                DrawPaintControls();
            }

            if (settingsPressed)
            {
                DrawSettingsControls();
            }

            if (!Licensing.isCommercial)
            {
                // Warn about unlicensed version.
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("This version of Cubiquity is for non-commercial and evaluation " +
                    "use only. Please see LICENSE.txt for further details.", MessageType.Warning);
            }
        }

        private void DrawSculptControls()
        {
            DrawInstructions("Click on the terrain to pull the surface out. Hold down shift while clicking to push in instead.");

            DrawBrushSelector();

            DrawBrushSettings(10.0f, 1.0f);
        }

        private void DrawSmoothControls()
        {
            DrawInstructions("Click on the terrain to smooth the surface or to soften the boundary between textures.");

            DrawBrushSelector();

            DrawBrushSettings(10.0f, 1.0f);
        }

        private void DrawPaintControls()
        {
            DrawInstructions("Select a brush and texture below, then click the terrain to paint the texture on it.");

            DrawBrushSelector();

            DrawTextureSelector();

            DrawBrushSettings(10.0f, 1.0f);
        }

        private void DrawSettingsControls()
        {
            DrawInstructions("Create new volume data through 'Main Menu -> Assets -> Create -> Terrain Volume Data' and then assign it below.");
            terrainVolume.data = EditorGUILayout.ObjectField("Volume Data: ", terrainVolume.data, typeof(TerrainVolumeData), true) as TerrainVolumeData;
        }

        private void DrawInstructions(string message)
        {
            EditorGUILayout.LabelField("Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(message, MessageType.None);
            EditorGUILayout.Space();
        }

        private void DrawBrushSelector()
        {
            EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
            selectedBrush = DrawTextureSelectionGrid(selectedBrush, brushTextures, 50);
            EditorGUILayout.Space();
        }

        private void DrawTextureSelector()
        {
#if EIGHT_MATERIALS
            const int noOfMaterials = 8;
#else
            const int noOfMaterials = 4;
#endif

            EditorGUILayout.LabelField("Textures", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("The avalible textures are defined by the currently active " +
                "material, which you can set on the Terrain Volume Renderer component", MessageType.None);

            Texture2D[] diffuseMaps = new Texture2D[noOfMaterials];

            // If we have a renderer and a material available then we can attempt
            // to set the texture on the buttons according to what is in the material
            if ((terrainVolume.GetComponent<TerrainVolumeRenderer>()) &&
                (terrainVolume.GetComponent<TerrainVolumeRenderer>().material))
            {
                for (int i = 0; i < noOfMaterials; i++)
                {
                    diffuseMaps[i] = terrainVolume.GetComponent<TerrainVolumeRenderer>().material.GetTexture("_Tex" + i) as Texture2D;
                }
            }
            selectedTexture = DrawTextureSelectionGrid(selectedTexture, diffuseMaps, 80);

            EditorGUILayout.Space();
        }

        private void DrawBrushSettings(float maxBrushRadius, float maxOpacity)
        {
            EditorGUILayout.LabelField("Brush settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Radius:", GUILayout.Width(50));
            brushOuterRadius = GUILayout.HorizontalSlider(brushOuterRadius, 0.0f, maxBrushRadius);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Opacity:", GUILayout.Width(50));
            brushOpacity = GUILayout.HorizontalSlider(brushOpacity, 0.0f, maxOpacity);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private int DrawTextureSelectionGrid(int selected, Texture[] images, int thumbnailSize)
        {
            // Don't think the selection grid handles wrapping automatically, so we compute it ourselves.
            int imageThumbnailSize = thumbnailSize;
            int scrollbarWidth = 20; // Just a guess
            int inspectorWidth = Screen.width - scrollbarWidth;
            int widthInThumbnails = inspectorWidth / imageThumbnailSize;
            int noOfThumbbails = images.Length;
            int noOfRows = noOfThumbbails / widthInThumbnails;
            if (noOfThumbbails % widthInThumbnails != 0)
            {
                noOfRows++;
            }

            GUILayoutOption[] layoutOptions = new GUILayoutOption[2];
            layoutOptions[0] = GUILayout.Height(imageThumbnailSize * noOfRows);
            layoutOptions[1] = GUILayout.MaxWidth(inspectorWidth);

            //Now draw the texture selection grid
            return GUILayout.SelectionGrid(selected, images, widthInThumbnails, layoutOptions);
        }

        public void OnSceneGUI()
        {
            // We use these 'Handles' functions to allow us to embed OnGUI() code into OnSceneGUI().
            // See http://answers.unity3d.com/questions/26669/using-editorguilayout-controls-in-onscenegui.html
            Handles.BeginGUI();
            terrainVolume.OnGUI();
            Handles.EndGUI();

            // If we don't have a renderer then there's no terrain being
            // displayed, and so not much we can do in this function.
            TerrainVolumeRenderer terrainVolumeRenderer = terrainVolume.GetComponent<TerrainVolumeRenderer>();
            if (terrainVolumeRenderer == null)
            {
                return;
            }

            // By default we disable the brush marker, and only turn it on if we later find a hit.
            Material material = terrainVolumeRenderer.material;
            List<string> keywords = new List<string> { "BRUSH_MARKER_OFF" };

            if (sculptPressed || smoothPressed || paintPressed)
            {
                Event e = Event.current;

                Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));

                // Perform the raycasting.
                PickSurfaceResult pickResult;
                bool hit = Picking.PickSurface(terrainVolume, ray.origin, ray.direction, 1000.0f, out pickResult);

                if (hit)
                {
                    //Debug.Log("Hit");
                    // Selected brush is in the range 0 to NoOfBrushes - 1. Convert this to a 0 to 1 range.
                    float brushInnerScaleFactor = (float)selectedBrush / ((float)(NoOfBrushes - 1));
                    // Use this value to compute the inner radius as a proportion of the outer radius.
                    float brushInnerRadius = brushOuterRadius * brushInnerScaleFactor;

                    if (material != null)
                    {
                        keywords = new List<string> { "BRUSH_MARKER_ON" };
                        material.SetVector("BrushCenter", pickResult.volumeSpacePos);
                        material.SetVector("BrushSettings", new Vector4(brushInnerRadius, brushOuterRadius, brushOpacity, 0.0f));
                        material.SetVector("BrushColor", new Vector4(0.0f, 0.5f, 1.0f, 1.0f));
                    }

                    if (((e.type == EventType.MouseDown) || (e.type == EventType.MouseDrag)) && (e.button == 0))
                    {
                        if (sculptPressed)
                        {
                            float multiplier = 1.0f;
                            if (e.modifiers == EventModifiers.Shift)
                            {
                                multiplier = -1.0f;
                            }
                            TerrainVolumeEditor.SculptTerrainVolume(terrainVolume, pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, brushInnerRadius, brushOuterRadius, brushOpacity * multiplier);
                        }
                        else if (smoothPressed)
                        {
                            TerrainVolumeEditor.BlurTerrainVolume(terrainVolume, pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, brushInnerRadius, brushOuterRadius, brushOpacity);
                        }
                        else if (paintPressed)
                        {
                            TerrainVolumeEditor.PaintTerrainVolume(terrainVolume, pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, brushInnerRadius, brushOuterRadius, brushOpacity, (uint)selectedTexture);
                        }
                    }


                    // Forcing the update seems to be requireded in Unity 5, whereas in Unity 4 it seems to happen automatically
                    // later as a result of assigning the shader keywords. Seems best to manually call it just in case.
                    terrainVolume.ForceUpdate();
                }

                if (e.type == EventType.Layout)
                {
                    // See: http://answers.unity3d.com/questions/303248/how-to-paint-objects-in-the-editor.html
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
                }

                // We need to repaint so that the brush marker follows the mouse even when a mouse button is not pressed.
                HandleUtility.Repaint();
            }

            if (material != null)
            {
                material.shaderKeywords = keywords.ToArray();
            }
        }

        private static void OnTerrainToolChanged()
        {
            // Whenever the user selects a terrain editing tool we need to make sure that Unity's transform widgets
            // are disabled. Otherwise the user can end up moving the terrain around while they are editing it.
            // Note that we include the 'settings' toll in the test below... technically we could have this one active
            // and still allow terrain transforms but it feels more consistent if we disable them in this case too.
            if (sculptPressed || smoothPressed || paintPressed || settingsPressed)
            {
                Tools.current = Tool.None;
            }
        }

        private static void OnTransformToolChanged()
        {
            // Deselect our editor tools if the user has selected a transform tool
            if (Tools.current != Tool.None)
            {
                mSculptPressed = false;
                mSmoothPressed = false;
                mPaintPressed = false;
                mSettingsPressed = false;
            }
        }
    }
}