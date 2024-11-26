using HighlightPlus;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static RotateableObject;
using UnityEngine.Events;
using UnityEngine.Playables;

public class GameInteractionHandlerBase : MonoBehaviour
{

    #region ---------------- Enums
    public enum InteractionType
    {
        None,
        ClickItem,
        ClickAndDrag,
        ClickAndRotate
    }

    public enum ClickAction
    {
        None,
        MoveObject,
        ClickEvent
    }

    public enum CameraAction
    {
        None,
        MoveCamera
    }

    public enum DragAction
    {
        None,
        PlaceInSlot
    }

    // Enum to specify rotation axis
    public enum RotationAxis
    {
        XAxis,
        YAxis
    }

    public enum AnimationType
    {
        None,
        Timeline,
        Animation
    }

    public enum ExecutionStep
    {
        CameraAction,
        AnimationAction,
        ItemInteraction
    }

    public enum NeedUnityEvent
    {
        No,
        Yes
    }

    #endregion

    #region -------------------- Classes ----------------------------

    [System.Serializable]
    public class CameraInteractionData
    {
        public Transform cameraStartTransform;
        public Transform cameraEndTransform;
        public float cameraMoveDuration = 1f;
        public float cameraReturnDuration = 1f;
        public bool lockCamera = false;

        public UnityEvent OnCameraActionComplete;
    }

    [System.Serializable]
    public class InteractionData
    {
        public UnityEvent OnInteractionStartEvent;

        public UISetup UISetup;


        public GameObject InteractableItem;
        public float proximityThreshold = 2f;
        public InteractionType interactionType;
        public ClickAction clickAction;
        public NeedUnityEvent needInteractionUnityEvent = NeedUnityEvent.No;
        public UnityEvent clickEvent;

        public DragAction dragAction;
        public Vector2 xAxisLimits = new Vector2(-10f, 10f); // Min and Max values for X-axis
        public Vector2 yAxisLimits = new Vector2(-10f, 10f); // Min and Max values for Y-axis
        public Vector2 zAxisLimits = new Vector2(-10f, 10f); // Min and Max values for Z-axis

        public float rotationSpeed = 500f;
        public RotationAxis rotationAxisSelection;
        public Vector3 targetRotationEuler;

        public AudioClip item_PickorClickSound;
        public AudioClip item_PlaceorDropSound;

        public UnityEvent OnItemInteractionComplete;
        public int interactableItemDelay;

        public CameraAction cameraAction;
        public CameraInteractionData cameraInteractionData;
        public Transform click_ReachTransform;
        public Transform item_DropTransform;
        public NeedUnityEvent needCameraActionUnityEvent = NeedUnityEvent.No;
        public UnityEvent OnCameraActionComplete;
        public int cameraActionDelay;

        public AnimationType animationType;
        public PlayableDirector playableDirector;
        public Animation animation;
        public string animationName;
        public NeedUnityEvent needAnimationUnityEvent = NeedUnityEvent.No;
        public UnityEvent OnTimelineAnimationComplete;
        public int animationDelay;

        // New list to control execution order
        public List<ExecutionStep> executionOrder = new List<ExecutionStep>();

        public UnityEvent OnInteractionEndEvent;
    }

    [System.Serializable]
    public class UISetup
    {
        public bool hasInteractableItem;
        public bool hasCameraAction;
        public bool hasAnimationProperty;
    }

    #endregion

    [Header("Highlight Profiles")]
    [Space(10)]
    public HighlightProfile yellow;
    public HighlightProfile blue;
    public HighlightProfile green;

    [Header("Common Data's")]
    [Space(10)]
    public AudioClip item_PickorClickSound;
    public AudioClip item_PlaceorDropSound;

    [Header("Testing")]
    [Space(10)]
    public bool executeAllInteraction = false;
    [Space(25)]
    public UnityEvent EventBeforeInteractionStart;
    public UnityEvent EventAfterInteractionComplete;

    public List<InteractionData> interactions = new List<InteractionData>();


    public Vector3 initialCameraPosition;
    public Quaternion initialCameraRotation;
    public bool isAnimatingCamera = false; // Flag to check if camera animation is in progress

    public CancellationTokenSource cameraAnimationCancellationTokenSource; // Cancellation token source for camera animation
    public CancellationTokenSource itemAnimationCancellationTokenSource; // Cancellation token source for item animation
}
