using System;
using UnityEditor;
using UnityEngine;
using static GameInteractionHandler;
using static UnityEngine.Rendering.DebugUI.MessageBox;

[CustomEditor(typeof(GameInteractionHandler))]
public class GameInteractionHandlerEditor : Editor
{
    SerializedProperty interactions;
    SerializedProperty executeAllInteraction;
    SerializedProperty uiSetupProperty;
    private GUIStyle buttonStyle;
    private void OnEnable()
    {
        interactions = serializedObject.FindProperty("interactions");
        executeAllInteraction = serializedObject.FindProperty("executeAllInteraction");
    }

    public override void OnInspectorGUI()
    {
        // Custom header style
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(10, 10, 10, 10),
            normal = { background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f)) } // Dark background with transparency
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,                          // Set font size
            fontStyle = FontStyle.Bold,              // Set font style to bold
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(10, 10, 10, 10),// Center the text
            stretchWidth = true,                    // Allow button to stretch width
            stretchHeight = true                    // Allow button to stretch height
        };
        serializedObject.Update();
        DrawMainCamera(headerStyle);
        EditorGUILayout.Space();
        DrawHighlightProfiles(headerStyle);
        EditorGUILayout.Space();
        DrawAudioClips(headerStyle);
        EditorGUILayout.Space();
        DrawTestingOptions(headerStyle);
        EditorGUILayout.Space();
        DrawCommonUnityEvents(headerStyle);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Game Interaction Handler", EditorStyles.boldLabel);
        // Iterate through each interaction
        for (int i = 0; i < interactions.arraySize; i++)
        {
            SerializedProperty interaction = interactions.GetArrayElementAtIndex(i);
            DrawInteractionHeader(headerStyle, i, interaction);

            if (interaction.isExpanded)
            {
                DrawInteractionProperties(interaction, headerStyle, boxStyle);

                DrawColoredBox(() =>
                {
                    if (GUILayout.Button("Remove Interaction", buttonStyle))
                    {
                        if (EditorUtility.DisplayDialog("Remove Interaction", "Are you sure you want to remove this interaction?", "Yes", "No"))
                        {
                            interactions.DeleteArrayElementAtIndex(i);
                        }
                    }
                });

                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------", EditorStyles.boldLabel);
        DrawAddNewInteraction(headerStyle);
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawAddNewInteraction(GUIStyle headerStyle)
    {
        headerStyle.normal.textColor = Color.white; // Change to your desired color

        EditorGUILayout.LabelField("Add New Interaction", headerStyle);
        DrawColoredBox(() =>
        {
            if (GUILayout.Button("Add New Interaction", buttonStyle))
            {
                interactions.arraySize++;
            }
        });
    }
    private void DrawMainCamera(GUIStyle headerStyle)
    {
        headerStyle.normal.textColor = Color.white; // Change to your desired color

        EditorGUILayout.LabelField("Main Camera", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MainCam"), new GUIContent("Main Camera"));

        });
    }
    private void DrawHighlightProfiles(GUIStyle headerStyle)
    {
        EditorGUILayout.LabelField("Highlight Profiles", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yellow"), new GUIContent("Yellow Profile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blue"), new GUIContent("Blue Profile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("green"), new GUIContent("Green Profile"));
        });
    }

    private void DrawAudioClips(GUIStyle headerStyle)
    {
        EditorGUILayout.LabelField("Audio Clips", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("item_PickorClickSound"), new GUIContent("Pick Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("item_PlaceorDropSound"), new GUIContent("Drop Sound"));
        });
    }

    private void DrawTestingOptions(GUIStyle headerStyle)
    {
        EditorGUILayout.LabelField("Testing Options", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(executeAllInteraction, new GUIContent("Execute All Interactions"));
        });
    }

    private void DrawCommonUnityEvents(GUIStyle headerStyle)
    {
        EditorGUILayout.LabelField("Common Unity Events", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EventBeforeInteractionStart"), new GUIContent("Event Before Interaction Start"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EventAfterInteractionComplete"), new GUIContent("Event After Interaction Complete"));
        });
    }

    private void DrawInteractionHeader(GUIStyle headerStyle, int index, SerializedProperty interaction)
    {
        headerStyle.normal.textColor = Color.red; // Change to your desired color
        GUILayout.Label($"\n{index + 1} ------------------------------------------------------------------------------ \n", headerStyle);
        interaction.isExpanded = EditorGUILayout.Foldout(interaction.isExpanded, $"INTERACTION {index + 1}");
    }

    private void DrawInteractionProperties(SerializedProperty interaction, GUIStyle headerStyle, GUIStyle boxStyle)
    {
        headerStyle.normal.textColor = Color.white; // Change to your desired color
        EditorGUILayout.LabelField("Common Interaction Events", headerStyle);
        DrawColoredBox(() =>
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("OnInteractionStartEvent"), new GUIContent("On Interaction StartEvent"));
            EditorGUILayout.LabelField("\n");
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("OnInteractionEndEvent"), new GUIContent("On Interaction EndEvent"));

        });

        EditorGUILayout.LabelField("Action List", headerStyle);
        DrawColoredBox(() =>
        {
            uiSetupProperty = interaction.FindPropertyRelative("UISetup");
            EditorGUILayout.PropertyField(uiSetupProperty, new GUIContent("UISetup"));
        });

        if (uiSetupProperty != null)
        {
            SerializedProperty hasInteractableItem = uiSetupProperty.FindPropertyRelative("hasInteractableItem");
            if (hasInteractableItem != null && hasInteractableItem.boolValue)
            {
                headerStyle.normal.textColor = Color.white; // Change to your desired color
                EditorGUILayout.LabelField("Interaction Type", headerStyle);
                DrawColoredBox(() =>
                {
                    DrawInteractionType(interaction);

                });
            }

            SerializedProperty hasCameraAction = uiSetupProperty.FindPropertyRelative("hasCameraAction");
            if (hasCameraAction != null && hasCameraAction.boolValue)
            {
                headerStyle.normal.textColor = Color.white; // Change to your desired color

                EditorGUILayout.LabelField("Camera Action", headerStyle);
                DrawColoredBox(() =>
                {
                    DrawCameraAction(interaction, headerStyle);

                });
            }

            SerializedProperty hasAnimationProperty = uiSetupProperty.FindPropertyRelative("hasAnimationProperty");
            if (hasAnimationProperty != null && hasAnimationProperty.boolValue)
            {
                headerStyle.normal.textColor = Color.white; // Change to your desired color

                EditorGUILayout.LabelField("Animation Property", headerStyle);
                DrawColoredBox(() =>
                {
                    DrawAnimationProperties(interaction, headerStyle);

                });
            }

            if (hasInteractableItem != null && hasInteractableItem.boolValue ||
                hasCameraAction != null && hasCameraAction.boolValue || hasAnimationProperty != null && hasAnimationProperty.boolValue)
            {
                headerStyle.normal.textColor = Color.white; // Change to your desired color

                EditorGUILayout.LabelField("Execution Order", headerStyle);
                DrawColoredBox(() =>
                {
                    DrawExecutionOrder(interaction, headerStyle);

                });
            }
        }

    }

    private void DrawInteractionType(SerializedProperty interaction)
    {
        // Conditional properties based on interaction type
        var clickAction = interaction.FindPropertyRelative("clickAction");
        var dragAction = interaction.FindPropertyRelative("dragAction");
        var reachTransform = interaction.FindPropertyRelative("click_ReachTransform");
        var clickEvent = interaction.FindPropertyRelative("clickEvent");
        var xAxisLimits = interaction.FindPropertyRelative("xAxisLimits");
        var yAxisLimits = interaction.FindPropertyRelative("yAxisLimits");
        var zAxisLimits = interaction.FindPropertyRelative("zAxisLimits");

        var itemDropTransform = interaction.FindPropertyRelative("item_DropTransform");
        var interactableItemDelay = interaction.FindPropertyRelative("interactableItemDelay");
        var OnItemInteractionComplete = interaction.FindPropertyRelative("OnItemInteractionComplete");
        var rotationSpeed = interaction.FindPropertyRelative("rotationSpeed");
        var rotationAxisSelection = interaction.FindPropertyRelative("rotationAxisSelection");
        var targetRotationEuler = interaction.FindPropertyRelative("targetRotationEuler");

        var interactionType = interaction.FindPropertyRelative("interactionType");
        var needInteractionUnityEvent = interaction.FindPropertyRelative("needInteractionUnityEvent");

        EditorGUILayout.PropertyField(interactionType, new GUIContent("Interaction Type"));


        //-----------------

        // Only show highlighter fields if the interactable item is assigned


        //--------------------------------
        if ((GameInteractionHandler.InteractionType)interactionType.enumValueIndex == GameInteractionHandler.InteractionType.ClickItem)
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("InteractableItem"), new GUIContent("Interactable Item"));

            var interactableItem = interaction.FindPropertyRelative("InteractableItem");
            if (interactableItem.objectReferenceValue != null)
            {
                /*EditorGUILayout.PropertyField(interaction.FindPropertyRelative("proximityThreshold"), new GUIContent("Proximity Threshold"));

                EditorGUILayout.PropertyField(interaction.FindPropertyRelative("preDragHighlighter"), new GUIContent("Pre-Drag Highlighter"));
                EditorGUILayout.PropertyField(interaction.FindPropertyRelative("inDragHighlighter"), new GUIContent("In-Drag Highlighter"));*/
                EditorGUILayout.PropertyField(clickAction, new GUIContent("Click Action"));
                if ((GameInteractionHandler.ClickAction)clickAction.enumValueIndex == GameInteractionHandler.ClickAction.MoveObject)
                {
                    EditorGUILayout.PropertyField(reachTransform, new GUIContent("Reach Transform"));
                }
                else if ((GameInteractionHandler.ClickAction)clickAction.enumValueIndex == GameInteractionHandler.ClickAction.ClickEvent)
                {
                    EditorGUILayout.PropertyField(clickEvent, new GUIContent("Click Event"));
                }
            }


        }
        else if ((GameInteractionHandler.InteractionType)interactionType.enumValueIndex == GameInteractionHandler.InteractionType.ClickAndDrag)
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("InteractableItem"), new GUIContent("Interactable Item"));
            var interactableItem = interaction.FindPropertyRelative("InteractableItem");
            if (interactableItem.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(dragAction, new GUIContent("Drag Action"));
                if ((GameInteractionHandler.DragAction)dragAction.enumValueIndex == GameInteractionHandler.DragAction.PlaceInSlot)
                {
                    EditorGUILayout.PropertyField(itemDropTransform, new GUIContent("Item Drop Transform"));
                    EditorGUILayout.PropertyField(xAxisLimits, new GUIContent("xAxisLimits"));
                    EditorGUILayout.PropertyField(yAxisLimits, new GUIContent("yAxisLimits"));
                    EditorGUILayout.PropertyField(zAxisLimits, new GUIContent("zAxisLimits"));

                    EditorGUILayout.PropertyField(interaction.FindPropertyRelative("proximityThreshold"), new GUIContent("Proximity Threshold"));
                }
            }
        }
        else if ((GameInteractionHandler.InteractionType)interactionType.enumValueIndex == GameInteractionHandler.InteractionType.ClickAndRotate)
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("InteractableItem"), new GUIContent("Interactable Item"));

            var interactableItem = interaction.FindPropertyRelative("InteractableItem");
            if (interactableItem.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(rotationAxisSelection, new GUIContent("Rotation Axis Selection"));
                EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("Rotation Speed"));
                EditorGUILayout.PropertyField(targetRotationEuler, new GUIContent("Target Rotation"));
                EditorGUILayout.PropertyField(itemDropTransform, new GUIContent("Target Highlighter"));
            }
        }

        EditorGUILayout.PropertyField(needInteractionUnityEvent, new GUIContent("Need Interaction UnityEvent"));

        if ((GameInteractionHandler.NeedUnityEvent)needInteractionUnityEvent.enumValueIndex == GameInteractionHandler.NeedUnityEvent.Yes)
        {
            EditorGUILayout.PropertyField(OnItemInteractionComplete, new GUIContent("On Item Interaction Complete"));
        }

        EditorGUILayout.PropertyField(interactableItemDelay, new GUIContent("Interactable Item Delay"));
    }

    private void DrawCameraAction(SerializedProperty interaction, GUIStyle headerStyle)
    {

        //  headerStyle.normal.textColor = Color.cyan; // Change to your desired color
        // EditorGUILayout.LabelField("Camera Action-------------------------------------------------------", headerStyle);

        var cameraAction = interaction.FindPropertyRelative("cameraAction");
        var cameraInteractionData = interaction.FindPropertyRelative("cameraInteractionData");
        var cameraActionDelay = interaction.FindPropertyRelative("cameraActionDelay");
        var OnCameraActionComplete = interaction.FindPropertyRelative("OnCameraActionComplete");
        var needCameraActionUnityEvent = interaction.FindPropertyRelative("needCameraActionUnityEvent");

        EditorGUILayout.PropertyField(cameraAction, new GUIContent("Camera Action"));
        if ((GameInteractionHandler.CameraAction)cameraAction.enumValueIndex == GameInteractionHandler.CameraAction.MoveCamera)
        {
            if (cameraInteractionData != null)
            {
                EditorGUILayout.PropertyField(cameraInteractionData.FindPropertyRelative("cameraStartTransform"), new GUIContent("Camera Start Transform"));
                EditorGUILayout.PropertyField(cameraInteractionData.FindPropertyRelative("cameraEndTransform"), new GUIContent("Camera End Transform"));
                EditorGUILayout.PropertyField(cameraInteractionData.FindPropertyRelative("cameraMoveDuration"), new GUIContent("Camera Move Duration"));
                EditorGUILayout.PropertyField(cameraInteractionData.FindPropertyRelative("cameraReturnDuration"), new GUIContent("Camera Return Duration"));
                EditorGUILayout.PropertyField(cameraInteractionData.FindPropertyRelative("lockCamera"), new GUIContent("Lock Camera"));

            }
        }

        EditorGUILayout.PropertyField(needCameraActionUnityEvent, new GUIContent("Need CameraAction UnityEvent"));

        if ((GameInteractionHandler.NeedUnityEvent)needCameraActionUnityEvent.enumValueIndex == GameInteractionHandler.NeedUnityEvent.Yes)
        {
            EditorGUILayout.PropertyField(OnCameraActionComplete, new GUIContent("On Camera Action Complete"));
        }

        EditorGUILayout.PropertyField(cameraActionDelay, new GUIContent("Camera Action Delay"));

    }

    private void DrawAnimationProperties(SerializedProperty interaction, GUIStyle headerStyle)
    {

        //    headerStyle.normal.textColor = Color.cyan; // Change to your desired color
        //  EditorGUILayout.LabelField("Animation Properties-------------------------------------------------------", headerStyle);

        var animationType = interaction.FindPropertyRelative("animationType");
        var playableDirector = interaction.FindPropertyRelative("playableDirector");
        var needAnimationUnityEvent = interaction.FindPropertyRelative("needAnimationUnityEvent");

        EditorGUILayout.PropertyField(animationType, new GUIContent("Animation Type"));
        if ((GameInteractionHandler.AnimationType)animationType.enumValueIndex == GameInteractionHandler.AnimationType.Timeline)
        {
            EditorGUILayout.PropertyField(playableDirector, new GUIContent("Playable Director"));
        }
        else if ((GameInteractionHandler.AnimationType)animationType.enumValueIndex == GameInteractionHandler.AnimationType.Animation)
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("animation"), new GUIContent("Animation Component"));
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("animationName"), new GUIContent("Animation Name"));
        }

        EditorGUILayout.PropertyField(needAnimationUnityEvent, new GUIContent("Need Animation UnityEvent"));

        if ((GameInteractionHandler.NeedUnityEvent)needAnimationUnityEvent.enumValueIndex == GameInteractionHandler.NeedUnityEvent.Yes)
        {
            EditorGUILayout.PropertyField(interaction.FindPropertyRelative("OnTimelineAnimationComplete"), new GUIContent("On Timeline Animation Complete"));
        }

        EditorGUILayout.PropertyField(interaction.FindPropertyRelative("animationDelay"), new GUIContent("Animation Delay"));
    }

    private void DrawExecutionOrder(SerializedProperty interaction, GUIStyle headerStyle)
    {
        //  headerStyle.normal.textColor = Color.cyan; // Change to your desired color
        //  EditorGUILayout.LabelField("Execution Order-------------------------------------------------------", headerStyle);
        EditorGUILayout.PropertyField(interaction.FindPropertyRelative("executionOrder"), new GUIContent("Execution Order"), true);

    }
    // Utility method for drawing a box with a border
    private void DrawColoredBox(Action drawContent, GUIStyle style = null, float borderSize = 1f)
    {
        Color borderColor = Color.grey;
        Rect rect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(new Rect(rect.x - borderSize, rect.y - borderSize, rect.width + 2 * borderSize, rect.height + 2 * borderSize), borderColor);

        EditorGUILayout.BeginVertical(style ?? new GUIStyle(GUI.skin.box));
        drawContent.Invoke();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    // Helper method to create a texture for background color
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pixels);
        result.Apply();

        return result;
    }
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
}
