using HighlightPlus;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static GameInteractionHandlerBase;

[RequireComponent(typeof(Collider))]
public class RotateableObject : InteractableItemBase
{
    #region ------------------- Variables ----------------------------

    public float rotationSpeed = 500f;          // Speed of rotation
    public Vector3 targetRotationEuler;        // Target rotation angles in degrees
    private Quaternion targetRotation;          // Target rotation in quaternion form
    private bool isDragging = false;

    private Vector3 previousMousePosition;
    private Vector3 rotationAxis;               // Rotation axis (either x or y based on selection)

    public UnityEvent OnRotationStart;
    public UnityEvent OnRotationComplete;

    public RotationAxis rotationAxisSelection = RotationAxis.XAxis;

    public Collider objCollider;
    #endregion

    #region ------------------- Initialization -----------------------

    public void Initialize(InteractionData data, HighlightEffect _highlighterEffect, GameInteractionHandler handler, GameObject _Collider)
    {
        interactionData = data;
        interactionHandler = handler;

        rotationSpeed = data.rotationSpeed;
        targetRotationEuler = data.targetRotationEuler;
        rotationAxisSelection = data.rotationAxisSelection;
        objCollider = _Collider.GetComponent<Collider>();
        objCollider.enabled = true;
        if (OnRotationStart == null)
            OnRotationStart = new UnityEvent();
        if (OnRotationComplete == null)
            OnRotationComplete = new UnityEvent();

        // Set the target rotation
        targetRotation = Quaternion.Euler(targetRotationEuler);

        // Set rotation axis based on selection
        rotationAxis = rotationAxisSelection == RotationAxis.YAxis ? Vector3.up : Vector3.right;

        highlighterEffect = _highlighterEffect;

        // Audio
        item_PickorClickSound = handler.item_PickorClickSound;
        item_PlaceorDropSound = handler.item_PlaceorDropSound;
    }

    private TaskCompletionSource<bool> isReachedTargetRotation;
    public async Task ReachedTargetRotation(TaskCompletionSource<bool> taskCompletionSource)
    {
        isReachedTargetRotation = taskCompletionSource;
        await isReachedTargetRotation.Task;
        objCollider.enabled = false;
        Destroy(this);
    }

    #endregion

    #region ------------------- Rotation Logic -----------------------

    public void StartDrag()
    {
        if (!IsAtTargetRotation())
        {
            isDragging = true;
            OnRotationStart.Invoke();
            previousMousePosition = Input.mousePosition;

            highlighterEffect.ProfileLoad(interactionHandler.blue);

            if (targethighlightEffect == null)
                targethighlightEffect = interactionData.item_DropTransform.GetComponent<HighlightEffect>();


            if (targethighlightEffect != null)
            {
                targethighlightEffect.enabled = true;
            }
        }
    }

    public void EndDrag()
    {
        if (isDragging)
        {
            isDragging = false;
            OnRotationComplete.Invoke();

            if (targethighlightEffect != null)
            {
                targethighlightEffect.enabled = false;
            }
        }
    }

    // Rotate based on mouse movement until the target rotation is reached
    public void UpdateRotation()
    {
        if (isDragging && !IsAtTargetRotation())
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - previousMousePosition;

            // Rotate based on mouse drag direction
            if (rotationAxisSelection == RotationAxis.YAxis && Mathf.Abs(mouseDelta.x) > 0.1f) // Horizontal movement for Y-axis
            {
                float rotationDirection = Mathf.Sign(mouseDelta.x); // Right: positive, Left: negative
                transform.Rotate(Vector3.up * rotationSpeed * rotationDirection * Time.deltaTime);
            }
            else if (rotationAxisSelection == RotationAxis.XAxis && Mathf.Abs(mouseDelta.y) > 0.1f) // Vertical movement for X-axis
            {
                float rotationDirection = Mathf.Sign(mouseDelta.y); // Up: positive, Down: negative
                transform.Rotate(Vector3.right * rotationSpeed * rotationDirection * Time.deltaTime);
            }

            // Snap to target rotation if it's close enough
            if (IsCloseToTargetRotation())
            {
                InteractionAudioManager.Instance.PlayAudio(item_PlaceorDropSound);

                SnapToTargetRotation();
                EndDrag();
                _ = interactionHandler.DestroyHighlighter(highlighterEffect, interactionHandler.green);
                if (targethighlightEffect != null)
                {
                    Destroy(targethighlightEffect);
                }

                isReachedTargetRotation.SetResult(true);
            }

            previousMousePosition = currentMousePosition;
        }
    }

    // Check if the current rotation is exactly at the target rotation
    private bool IsAtTargetRotation()
    {
        return transform.rotation == targetRotation;
    }

    // Check if the current rotation is close to the target (within 1 degree)
    private bool IsCloseToTargetRotation()
    {
        return Quaternion.Angle(transform.rotation, targetRotation) < 1f;
    }

    // Snap the rotation to the exact target rotation
    private void SnapToTargetRotation()
    {
        transform.rotation = targetRotation;
    }

    #endregion

    #region ------------------- Interaction Handling -----------------

    void OnMouseDown()
    {
        if (interactionData.interactionType == GameInteractionHandler.InteractionType.ClickAndRotate)
        {
            StartDrag();

            GetAudio();
        }
    }

    void OnMouseDrag()
    {
        if (interactionData.interactionType == GameInteractionHandler.InteractionType.ClickAndRotate)
        {
            UpdateRotation();
        }
    }

    void OnMouseUp()
    {
        if (interactionData.interactionType == GameInteractionHandler.InteractionType.ClickAndRotate)
        {
            EndDrag();
            highlighterEffect.ProfileLoad(interactionHandler.yellow);
        }
    }

    #endregion
}
