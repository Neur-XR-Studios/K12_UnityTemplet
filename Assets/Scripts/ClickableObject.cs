using HighlightPlus;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickableObject : InteractableItemBase
{
    public Transform targetTransform; // The transform to move to
    public float moveDuration = 1f; // Duration of the movement

    private bool isMoving = false;
    private TaskCompletionSource<bool> isClickActionComplete = new TaskCompletionSource<bool>();
    private TaskCompletionSource<bool> isItemClicked = new TaskCompletionSource<bool>();

    private Vector3 initialitemPosition;
    private Quaternion initialitemRotation;
    private bool isAnimatingitem = false; // Flag to check if camera animation is in progress
    private HighlightEffect highlightEffect;

    private CancellationTokenSource itemAnimationCancellationTokenSource; // Cancellation token source for camera animation
    public Collider Objcollider;

    private void Start()
    {
   /*     // Ensure the object has a collider
        Objcollider = GetComponent<Collider>();
        if (Objcollider == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
*/
        // Cache the camera’s initial position and rotation at the start
        initialitemPosition = transform.position;
        initialitemRotation = transform.rotation;

    }

    private void OnMouseDown()
    {
        if (interactionData != null && interactionData.interactionType == GameInteractionHandler.InteractionType.ClickItem)
        {
            if(interactionData.clickAction == GameInteractionHandler.ClickAction.ClickEvent)
            {
                Objcollider.enabled = false;
                Destroy(this);
                isItemClicked.SetResult(true);
                GetAudio();
                _ = interactionHandler.DestroyHighlighter(highlightEffect, interactionHandler.green);

                return;
            }

            GetAudio();
            HandleClickAction();

            if (interactionHandler != null)
            {
                _ = interactionHandler.HandleInteractionComplete(interactionData);
            }
        }

        _ = interactionHandler.DestroyHighlighter(highlightEffect, interactionHandler.green);
    }

    private void HandleClickAction()
    {
        if (interactionData.clickAction == GameInteractionHandler.ClickAction.MoveObject && !isMoving)
        {
            // Initialize TaskCompletionSource for this action
            // Start moving the object to the target transform's position, rotation, and scale

            isAnimatingitem = true;
            itemAnimationCancellationTokenSource = new CancellationTokenSource();



            try
            {
                _ = MoveToTargetAsync(targetTransform, itemAnimationCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, do nothing special
            }
            finally
            {
                isAnimatingitem = false;
            }
        }
    }

    private void GetAudio()
    {
        if (interactionData.item_PickorClickSound != null) item_PickorClickSound = interactionData.item_PickorClickSound;
        if (interactionData.item_PlaceorDropSound != null) item_PlaceorDropSound = interactionData.item_PlaceorDropSound;

        InteractionAudioManager.Instance.PlayAudio(item_PickorClickSound);
    }

    public async Task WaitForClickActionComplete()
    {
        if (isClickActionComplete != null)
        {
            await isClickActionComplete.Task;
        }
        else
        {
            Debug.LogError("isClickActionComplete is NULL");
        }
    }

    public async Task WaitForItemClick()
    {
        if (isClickActionComplete != null)
        {
            await isItemClicked.Task;
        }
        else
        {
            Debug.LogError("isItemClicked is NULL");
        }
    }

    public async Task MoveToTargetAsync(Transform target, CancellationToken cancellationToken)
    {
        // Check if the target transform is set
        if (target == null)
        {
            Debug.LogError("Target transform is not set.");
            isClickActionComplete?.SetResult(false);
            return;
        }

        isMoving = true;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;
        Vector3 targetScale = target.localScale;

        while (elapsed < moveDuration)
        {

            // Check for cancellation request
            if (cancellationToken.IsCancellationRequested)
            {
                return; // Exit the method if canceled
            }


            // Interpolate position, rotation, and scale
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / moveDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / moveDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Yield control
        }

        // Set final position, rotation, and scale
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = targetScale;

        isMoving = false;

        InteractionAudioManager.Instance.PlayAudio(item_PlaceorDropSound);

        // Optionally disable collider instead of destroying it
        Objcollider.enabled = false;
        isClickActionComplete?.SetResult(true);
    }

    public void Initialize(GameInteractionHandler.InteractionData data, HighlightEffect _highlighterEffect, GameInteractionHandler handler, GameObject collider)
    {
        interactionData = data;
        targetTransform = data.click_ReachTransform;
        interactionHandler = handler;
        highlightEffect = _highlighterEffect;
        Objcollider = collider.GetComponent<Collider>();
        Objcollider.enabled = true;
        // Audio
        item_PickorClickSound = handler.item_PickorClickSound;
        item_PlaceorDropSound = handler.item_PlaceorDropSound;
    }

    private void OnApplicationQuit()
    {
        // Cancel the camera animation task
        itemAnimationCancellationTokenSource?.Cancel();

        // Reset the camera position and rotation on application quit
        transform.position = initialitemPosition;
        transform.rotation = initialitemRotation;

        if (gameObject.GetComponent<BoxCollider>() == null)
        {
            Destroy(this);
            Destroy(gameObject.GetComponent<BoxCollider>());
        }
    }
}
