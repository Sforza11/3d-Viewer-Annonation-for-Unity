using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnhancedAnnotationSystem : MonoBehaviour
{
    [System.Serializable]
    public class Annotation
    {
        public Vector3 position;
        public string title = "New Annotation";
        [TextArea(3, 5)]
        public string description = "Enter description here";
        public int number;
        public bool isSelected = false;
    }

    [Header("Annotation Settings")]
    public List<Annotation> annotations = new List<Annotation>();

    [Header("Visual Settings")]
    [Range(20f, 100f)]
    public float playModePointSize = 40f;
    [Range(10, 36)]
    public int playModeFontSize = 18;
    public Color playModeSphereColor = Color.blue;
    public Color playModeTextColor = Color.white;
    public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);

    [Header("Description Box Settings")]
    [Range(200, 500)]
    public float descriptionBoxWidth = 300f;
    [Range(100, 300)]
    public float descriptionBoxHeight = 150f;
    [Range(12, 24)]
    public int descriptionFontSize = 16;
    public Color descriptionBackgroundColor = new Color(0, 0, 0, 0.8f);
    public Color descriptionTextColor = Color.white;

    [Header("Editor View Settings")]
    [Range(0.05f, 1f)]
    public float editorSphereRadius = 0.2f;
    [Range(10, 50)]
    public int editorFontSize = 20;

    [Header("Visibility Settings")]
    public bool showAnnotations = true;
    public LayerMask obstacleLayer;

    private Texture2D sphereTexture;
    private Texture2D highlightTexture;
    private Camera mainCamera;

    private void Start()
    {
        CreateTextures();
        mainCamera = Camera.main;
    }

    private void CreateTextures()
    {
        int size = 128;
        sphereTexture = CreateCircleTexture(size, playModeSphereColor, Color.white);
        highlightTexture = CreateCircleTexture(size, highlightColor, Color.clear);
    }

    private Texture2D CreateCircleTexture(int size, Color centerColor, Color borderColor)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size / 2f;
        float borderWidth = size / 20f; // 5% of size for border

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (distance <= radius)
                {
                    if (distance > radius - borderWidth)
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, centerColor);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        return texture;
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !showAnnotations) return;

        for (int i = 0; i < annotations.Count; i++)
        {
            if (IsAnnotationVisible(annotations[i].position))
            {
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(annotations[i].position);
                if (screenPoint.z > 0) // Check if the annotation is in front of the camera
                {
                    Vector2 guiPoint = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
                    Rect sphereRect = new Rect(guiPoint.x - playModePointSize / 2, guiPoint.y - playModePointSize / 2, playModePointSize, playModePointSize);

                    // Draw the colored sphere with border
                    GUI.DrawTexture(sphereRect, sphereTexture);

                    // Draw highlight if mouse is over
                    if (sphereRect.Contains(Event.current.mousePosition))
                    {
                        GUI.DrawTexture(sphereRect, highlightTexture);

                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            annotations[i].isSelected = !annotations[i].isSelected;
                            Event.current.Use();
                        }
                    }

                    // Draw the number
                    GUI.color = playModeTextColor;
                    GUI.Label(sphereRect, (i + 1).ToString(), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = playModeFontSize, fontStyle = FontStyle.Bold });

                    // Draw title and description if selected
                    if (annotations[i].isSelected)
                    {
                        Rect textRect = new Rect(guiPoint.x - descriptionBoxWidth / 2, guiPoint.y - playModePointSize - descriptionBoxHeight, descriptionBoxWidth, descriptionBoxHeight);

                        // Draw background
                        GUI.color = descriptionBackgroundColor;
                        GUI.DrawTexture(textRect, Texture2D.whiteTexture);

                        // Draw text
                        GUI.color = descriptionTextColor;
                        GUIStyle style = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.UpperCenter,
                            wordWrap = true,
                            fontSize = descriptionFontSize,
                            fontStyle = FontStyle.Bold
                        };
                        GUI.Label(textRect, $"{annotations[i].title}\n\n{annotations[i].description}", style);
                    }

                    GUI.color = Color.white; // Reset GUI color
                }
            }
        }
    }

    private bool IsAnnotationVisible(Vector3 annotationPosition)
    {
        Vector3 directionToCamera = mainCamera.transform.position - annotationPosition;
        float distanceToCamera = directionToCamera.magnitude;
        Ray ray = new Ray(annotationPosition, directionToCamera.normalized);

        return !Physics.Raycast(ray, distanceToCamera, obstacleLayer);
    }

    private void OnDrawGizmos()
    {
        if (!showAnnotations) return;

        for (int i = 0; i < annotations.Count; i++)
        {
            // Draw sphere
            Gizmos.color = playModeSphereColor;
            Gizmos.DrawSphere(annotations[i].position, editorSphereRadius);

            // Draw white border
            Handles.color = Color.white;
            Handles.DrawWireDisc(annotations[i].position, Camera.current.transform.forward, editorSphereRadius);

            // Draw number
            Handles.color = playModeTextColor;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = playModeTextColor;
            style.fontSize = editorFontSize;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(annotations[i].position, (i + 1).ToString(), style);

            // Draw title for better visibility in Scene view
            Handles.Label(annotations[i].position + Camera.current.transform.up * editorSphereRadius * 2,
                          annotations[i].title,
                          new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontSize = Mathf.RoundToInt(editorFontSize * 0.75f) });
        }
    }

    public void RenumberAnnotations()
    {
        for (int i = 0; i < annotations.Count; i++)
        {
            annotations[i].number = i + 1;
        }
    }

    public void UpdateTextures()
    {
        CreateTextures();
    }

    public void ToggleAnnotationVisibility()
    {
        showAnnotations = !showAnnotations;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnhancedAnnotationSystem))]
public class EnhancedAnnotationSystemEditor : Editor
{
    private EnhancedAnnotationSystem system;
    private bool isAddingAnnotations = false;

    private void OnEnable()
    {
        system = (EnhancedAnnotationSystem)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            system.UpdateTextures();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button(system.showAnnotations ? "Hide Annotations" : "Show Annotations"))
        {
            system.ToggleAnnotationVisibility();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button(isAddingAnnotations ? "Finish Adding Annotations" : "Start Adding Annotations"))
        {
            isAddingAnnotations = !isAddingAnnotations;
            SceneView.RepaintAll();
        }

        if (isAddingAnnotations)
        {
            EditorGUILayout.HelpBox("Click on the model in the Scene view to add annotations.", MessageType.Info);
        }

        if (GUILayout.Button("Renumber Annotations"))
        {
            system.RenumberAnnotations();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isAddingAnnotations) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Undo.RecordObject(system, "Add Annotation");
                system.annotations.Add(new EnhancedAnnotationSystem.Annotation
                {
                    position = hit.point,
                    title = "New Annotation",
                    description = "Enter description here",
                    number = system.annotations.Count + 1
                });
                e.Use();
            }
        }
    }
}
#endif