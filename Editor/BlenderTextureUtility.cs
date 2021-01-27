using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace ProjectGameDev.Unity.Utility
{
    public enum TextureType
    {
        Smoothness = 2,
        Roughness = 1
    }
    public class BlenderTextureUtility : EditorWindow
    {
        private Object _texture;

        private bool _triggerProcess;

        private GUIStyle _filePathLabelStyle;
        private GUIStyle _wrapperStyle;

        private TextureType _textureType = TextureType.Roughness;

        private string _saveAs = string.Empty;

        private Color _metallicColor;

        private Vector2 _scrollPos;

        private void CreateGUI()
        {
            _filePathLabelStyle = new GUIStyle() { richText = true, wordWrap = true };
            _filePathLabelStyle.normal.textColor = Color.white;

            _wrapperStyle = new GUIStyle();

            _wrapperStyle = new GUIStyle();
            _wrapperStyle.margin = new RectOffset(5, 5, 5, 5);

        }

        [MenuItem("Window/PGD Tools/Blender Texture Utility")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BlenderTextureUtility), false, "Blender Texture Utility");
        }
        void OnGUI()
        {

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            GUILayout.BeginVertical(_wrapperStyle, GUILayout.MaxWidth(500));
            _metallicColor = EditorGUILayout.ColorField("Metallic Base Color: ", _metallicColor);
            GUILayout.Space(5);
            _texture = EditorGUILayout.ObjectField("Texture:", _texture, typeof(Texture2D), true);
            GUILayout.Space(5);
            _textureType = (TextureType)EditorGUILayout.EnumPopup(new GUIContent("Texture Type", "Determines whether to use inverted or original texture to calculate smoothness"), _textureType);
            
            GUILayout.Space(30);
            
            bool clickedSaveAs = GUILayout.Button("Select Destination Path...");

            if (clickedSaveAs)
            {
                _saveAs = EditorUtility.SaveFilePanel("Save Image As", Application.dataPath, string.Empty, "png");
            }

            string path = string.IsNullOrWhiteSpace(_saveAs) ? "(none selected)" : _saveAs;

            GUILayout.Space(5);

            GUILayout.Label(new GUIContent($"<b>File Name: </b> {path}"), _filePathLabelStyle);

            GUILayout.FlexibleSpace();

            _triggerProcess = GUILayout.Button($"Convert Blender {_textureType} to Metallic");
            if (_triggerProcess && _texture != null)
            {
                Texture2D tex = (Texture2D)_texture;
                ConvertRGBToAlpha(tex, _saveAs);
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

        }


        void ConvertRGBToAlpha(Texture2D texture, string saveAs)
        {
            int width, height;

            width = texture.width;
            height = texture.height;

            int x = 0, y = 0;

            Texture2D target = new Texture2D(width, height);

            while(x < width)
            {
                y = 0;
                while(y < height)
                {

                    Color c = texture.GetPixel(x, y);

                    if (_textureType == TextureType.Roughness)
                    {
                        //Invert color
                        c = Color.white - c;
                    }

                    float alpha = (c.r + c.g + c.b) / 3f;

                    Color newColor = new Color(_metallicColor.r, _metallicColor.g, _metallicColor.b, alpha);
                    target.SetPixel(x, y, newColor);

                    y++;
                }

                x++;
            }

            target.name = saveAs;
            byte[] bytes = target.EncodeToPNG();

            File.WriteAllBytes($"{saveAs}", bytes);

            _triggerProcess = false;

            AssetDatabase.Refresh();

        }
    }
}