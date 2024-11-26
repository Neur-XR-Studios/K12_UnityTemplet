using UnityEngine;
using System.Threading.Tasks;
using static GameInteractionHandlerBase;
using HighlightPlus;
using System.Drawing;

[RequireComponent(typeof(Collider))]
public class DraggableObject : InteractableItemBase
{
    public Transform dropTransform;
    public float proximityThreshold = 2f;

    // Limits for movement along each axis
    public Vector2 xAxisLimits = new Vector2(-5f, 5f); // Min and Max values for X-axis
    public Vector2 yAxisLimits = new Vector2(-5f, 5f); // Min and Max values for Y-axis
    public Vector2 zAxisLimits = new Vector2(-5f, 5f); // Min and Max values for Z-axis

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 initialPosition;

    private TaskCompletionSource<bool> isReachedDragEndPosition;
    Collider objCollider;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();

        highlighterEffect.ProfileLoad(interactionHandler.blue);

        if(targethighlightEffect == null)
        targethighlightEffect = interactionData.item_DropTransform.GetComponent<HighlightEffect>();


        if (targethighlightEffect != null)
        {
            targethighlightEffect.enabled = true;
        }

        GetAudio();
    }


    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + offset;

            // Apply axis constraints to the target position
            targetPosition.x = Mathf.Clamp(targetPosition.x, xAxisLimits.x, xAxisLimits.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, yAxisLimits.x, yAxisLimits.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zAxisLimits.x, zAxisLimits.y);

            transform.position = targetPosition;

            if (dropTransform != null && Vector3.Distance(transform.position, dropTransform.position) <= proximityThreshold)
            {
                MoveToTargetAsync(dropTransform).Forget();
                StopDragging();
            }
        }
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            StopDragging();
            if (Vector3.Distance(transform.position, dropTransform.position) > proximityThreshold)
            {
                ResetToInitialPositionAsync().Forget();
            }
        }
    }

    private void StopDragging()
    {
        isDragging = false;
        if (targethighlightEffect != null)
        {
            targethighlightEffect.enabled = false;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private async Task ResetToInitialPositionAsync()
    {
        float resetDuration = 0.2f;
        float elapsedTime = 0f;
        Vector3 currentPos = transform.position;

        while (elapsedTime < resetDuration)
        {
            transform.position = Vector3.Lerp(currentPos, initialPosition, elapsedTime / resetDuration);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        transform.position = initialPosition;
        highlighterEffect.ProfileLoad(interactionHandler.yellow);
    }

    public async Task MoveToTargetAsync(Transform targetTransform, float duration = 0.2f)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetTransform.position, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        isReachedDragEndPosition?.SetResult(true);
    }

    public async Task ReachedDragEndPosition(TaskCompletionSource<bool> taskCompletionSource)
    {
        isReachedDragEndPosition = taskCompletionSource;
        await isReachedDragEndPosition.Task;
        InteractionAudioManager.Instance.PlayAudio(item_PlaceorDropSound);

        objCollider.enabled = false;
        _ = interactionHandler.DestroyHighlighter(highlighterEffect, interactionHandler.green);
        if (targethighlightEffect != null)
        {
            Destroy(targethighlightEffect);
        }

        Destroy(this);
    }

    public void Initialize(InteractionData _interactionData, HighlightEffect _highlighterEffect, GameInteractionHandler handler , GameObject _collider)
    {
        dropTransform = _interactionData.item_DropTransform;
        proximityThreshold = _interactionData.proximityThreshold;
        interactionData = _interactionData;
        interactionHandler = handler;
        xAxisLimits = _interactionData.xAxisLimits;
        yAxisLimits = _interactionData.yAxisLimits;
        zAxisLimits = _interactionData.zAxisLimits;
        highlighterEffect = _highlighterEffect;
        objCollider = _collider.GetComponent<Collider>();
        objCollider.enabled = true;
        // Audio
        item_PickorClickSound = handler.item_PickorClickSound;
        item_PlaceorDropSound = handler.item_PlaceorDropSound;
    }

    private void OnApplicationQuit()
    {
        if (gameObject.GetComponent<BoxCollider>() == null)
        {
            Destroy(this);
            Destroy(gameObject.GetComponent<BoxCollider>());
        }
    }
}

// Extension method to allow fire-and-forget tasks
public static class TaskExtensions
{
    public static void Forget(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
        });
    }
}
