using HighlightPlus;
using K12.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class GameInteractionHandler : GameInteractionHandlerBase
{
    public static bool isUnityEventCompleted = false;
    private int StepCount = -1;
    public Camera MainCam;
    private async void Start()
    {
        // Cache the camera’s initial position and rotation at the start
        initialCameraPosition = MainCam.transform.position;
        initialCameraRotation = MainCam.transform.rotation;

        await StartAsync();

    }

    public async Task StartAsync()
    {
        await CallUnityEvent(EventBeforeInteractionStart);

        if (executeAllInteraction)
            await InitializeInteractionsAsync();

        await CallUnityEvent(EventAfterInteractionComplete);
    }

    private async Task InitializeInteractionsAsync()
    {
        foreach (var interaction in interactions)
        {
            await HandleInteractionComplete(interaction);
        }
    }

    public async void CallInteraction(int index)
    {
        StepCount++;
        if (StepCount >= 1)
        {
            if (StepCount - 1 < interactions.Count)
            {
                await HandleInteractionComplete(interactions[StepCount - 1], true);
            }
        }
        //await HandleInteractionComplete(interactions[index], true);

    }
    public async void CallInteraction()
    {
        StepCount++;
        if (StepCount >= 1)
        {
            if (StepCount - 1 < interactions.Count)
            {
                await HandleInteractionComplete(interactions[StepCount - 1], true);
            }
        }
    }
    public async Task HandleInteractionComplete(InteractionData interactionData, bool isCalledInteractionFromEvent = false)
    {
        await CallUnityEvent(interactionData.OnInteractionStartEvent);

        // Execute actions based on defined order in the inspector
        foreach (var step in interactionData.executionOrder)
        {
            switch (step)
            {
                case ExecutionStep.ItemInteraction:
                    {
                        if (interactionData.UISetup.hasInteractableItem)
                        {

                            if (interactionData.interactionType == InteractionType.ClickAndDrag)
                            {
                                if (interactionData.InteractableItem != null && interactionData.dragAction == DragAction.PlaceInSlot)
                                {
                                    SetupDraggableObject(interactionData);
                                }

                                if (interactionData.dragAction == DragAction.PlaceInSlot && interactionData.item_DropTransform != null)
                                {
                                    await PlaceObjectInSlot(interactionData.InteractableItem);
                                }
                            }
                            else if (interactionData.interactionType == InteractionType.ClickItem)
                            {

                                if (interactionData.InteractableItem != null)
                                {

                                    SetupClickableObject(interactionData.InteractableItem, interactionData);
                                }
                                if (interactionData.clickAction == ClickAction.MoveObject && interactionData.click_ReachTransform != null)
                                {
                                    await WaitForClickActionComplete(interactionData.InteractableItem);
                                }
                                else if (interactionData.clickAction == ClickAction.ClickEvent && interactionData.clickEvent.GetPersistentEventCount() != 0)
                                {
                                    await WaitForItemClick(interactionData.InteractableItem);
                                    await CallUnityEvent(interactionData.clickEvent);
                                }
                            }
                            else if (interactionData.interactionType == InteractionType.ClickAndRotate)
                            {
                                if (interactionData.InteractableItem != null)
                                {
                                    SetupRotatableObject(interactionData.InteractableItem, interactionData);
                                    await RotateItemToTarget(interactionData.InteractableItem);
                                }
                            }

                            await CallUnityEvent(interactionData.OnItemInteractionComplete);

                            await Task.Delay(interactionData.interactableItemDelay);
                        }
                        break;
                    }

                case ExecutionStep.CameraAction:
                    {
                        if (interactionData.UISetup.hasCameraAction)
                        {
                            if (interactionData.cameraAction == CameraAction.MoveCamera)
                            {
                                await ExecuteCameraAction(interactionData);
                            }

                            await CallUnityEvent(interactionData.OnCameraActionComplete);

                            await Task.Delay(interactionData.cameraActionDelay);
                        }
                        break;
                    }

                case ExecutionStep.AnimationAction:
                    {
                        if (interactionData.UISetup.hasAnimationProperty)
                        {
                            if (interactionData.animationType == AnimationType.Timeline && interactionData.playableDirector != null)
                            {
                                interactionData.playableDirector.Play();
                                await WaitForTimelineToComplete(interactionData.playableDirector);
                            }
                            else if (interactionData.animationType == AnimationType.Animation && interactionData.animation != null && interactionData.animationName != string.Empty)
                            {
                                await WaitForAnimationComplete(interactionData.animation, interactionData.animationName);
                            }

                            await CallUnityEvent(interactionData.OnTimelineAnimationComplete);

                            await Task.Delay(interactionData.animationDelay);
                        }
                        break;
                    }
            }
        }

        await CallUnityEvent(interactionData.OnInteractionEndEvent);

        if (isCalledInteractionFromEvent)
            isUnityEventCompleted = true;

    }
    int count = 0;
    public async  void Test()
    {
        count++;
        Debug.Log("Test" + count);
    }

    private void SetupRotatableObject(GameObject interactableItem, InteractionData interactionData)
    {
        RotateableObject rotateableObject = interactableItem.GetComponent<RotateableObject>();

        Collider collider = interactableItem.GetComponent<Collider>();

        if (collider == null)
        {
            interactableItem.AddComponent<BoxCollider>();
        }
        if (rotateableObject == null)
        {
          
            rotateableObject = interactableItem.AddComponent<RotateableObject>();
        }

        HighlightEffect highlightEffect = SetUpHighlighter(interactionData.InteractableItem, yellow, true);

        SetUpHighlighter(interactionData.item_DropTransform.gameObject, yellow, false);

        rotateableObject.Initialize(interactionData, highlightEffect, this, interactableItem);
    }

    private void SetupClickableObject(GameObject interactableItem, InteractionData interactionData)
    {
        ClickableObject clickable = interactableItem.GetComponent<ClickableObject>();

        Collider collider = interactableItem.GetComponent<Collider>();
        if (collider == null)
        {
            interactableItem.AddComponent<BoxCollider>();
        }
       
        if (clickable == null)
        {
          

            clickable = interactableItem.AddComponent<ClickableObject>();
        }

        HighlightEffect highlighterEffect = SetUpHighlighter(interactableItem, yellow, true);

        clickable.Initialize(interactionData, highlighterEffect, this, interactableItem);
    }

    private HighlightEffect SetUpHighlighter(GameObject interactableItem, HighlightProfile color, bool enableScript)
    {
        HighlightEffect highlighterEffect = interactableItem.GetComponent<HighlightEffect>();

        if (highlighterEffect == null)
        {
            highlighterEffect = interactableItem.AddComponent<HighlightEffect>();
        }

        highlighterEffect.highlighted = true;
        highlighterEffect.ProfileLoad(color);
        highlighterEffect.enabled = enableScript;

        return highlighterEffect;
    }

    public async Task DestroyHighlighter(HighlightEffect highlighterEffect, HighlightProfile color)
    {
        highlighterEffect.ProfileLoad(color);
        await Task.Delay(500);
        Destroy(highlighterEffect);
    }

    private void SetupDraggableObject(InteractionData interactionData)
    {
        DraggableObject draggable = interactionData.InteractableItem.GetComponent<DraggableObject>();

        Collider collider = interactionData.InteractableItem.GetComponent<Collider>();
        if (collider == null)
        {
            interactionData.InteractableItem.AddComponent<BoxCollider>();
        }
      
        if (draggable == null)
        {
           
            draggable = interactionData.InteractableItem.AddComponent<DraggableObject>();
        }

        HighlightEffect highlightEffect = SetUpHighlighter(interactionData.InteractableItem, yellow, true);

        SetUpHighlighter(interactionData.item_DropTransform.gameObject, yellow, false);

        draggable.Initialize(interactionData, highlightEffect, this , interactionData.InteractableItem);
    }

    async Task CallUnityEvent(UnityEvent unityEvent)
    {
        if (unityEvent.GetPersistentEventCount() != 0)
        {
            await WaitForItemInteractionComplete(unityEvent);
            await WaitUntil(() => isUnityEventCompleted);
            isUnityEventCompleted = false;
        }
    }

    private async Task WaitUntil(System.Func<bool> condition)
    {
        // Wait until the condition is true
        while (!condition())
        {
            await Task.Yield(); // Yield control back to Unity
        }
    }

    // New method to invoke the event and wait for its completion
    public async Task WaitForItemInteractionComplete(UnityEvent unityEvent)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        // Subscribe to the Unity Event
        UnityAction listener = () =>
        {
            tcs.SetResult(true); // Signal that the event has completed
        };

        unityEvent.AddListener(listener);

        // Invoke the Unity Event
        unityEvent.Invoke();

        unityEvent.RemoveListener(listener); // Remove the listener to prevent memory leaks

        // Wait for the event to complete
        await tcs.Task; // This will yield until SetResult(true) is called
    }

    private async Task PlaceObjectInSlot(GameObject item)
    {
        DraggableObject draggableObject = item.GetComponent<DraggableObject>();
        TaskCompletionSource<bool> isObjectPlacedInSlot = new TaskCompletionSource<bool>();
        if (draggableObject != null)
        {
            await draggableObject.ReachedDragEndPosition(isObjectPlacedInSlot);
        }
    }

    private async Task RotateItemToTarget(GameObject item)
    {
        RotateableObject RotateableObject = item.GetComponent<RotateableObject>();
        TaskCompletionSource<bool> isObjectReachedTargetRotation = new TaskCompletionSource<bool>();
        if (RotateableObject != null)
        {
            await RotateableObject.ReachedTargetRotation(isObjectReachedTargetRotation);
        }
    }

    private async Task MoveObject(GameObject target, Transform targetTransform)
    {
        ClickableObject clickableObject = target.GetComponent<ClickableObject>();

        itemAnimationCancellationTokenSource = new CancellationTokenSource();

        if (clickableObject != null)
        {
            await clickableObject.MoveToTargetAsync(targetTransform, itemAnimationCancellationTokenSource.Token);
        }
    }

    private async Task WaitForClickActionComplete(GameObject target)
    {
        ClickableObject clickableObject = target.GetComponent<ClickableObject>();
        if (clickableObject != null)
        {
            await clickableObject.WaitForClickActionComplete();
        }
    }

    private async Task WaitForItemClick(GameObject target)
    {
        ClickableObject clickableObject = target.GetComponent<ClickableObject>();
        if (clickableObject != null)
        {
            await clickableObject.WaitForItemClick();
        }
    }

    public async Task ExecuteCameraAction(InteractionData interaction)
    {
        if (interaction.cameraInteractionData.cameraStartTransform == null || interaction.cameraInteractionData.cameraEndTransform == null) return;

        interaction.cameraInteractionData.cameraStartTransform.position = MainCam.transform.position;
        interaction.cameraInteractionData.cameraStartTransform.rotation = MainCam.transform.rotation;

        Vector3 cacheCameraStartPos = interaction.cameraInteractionData.cameraStartTransform.position;
        Quaternion cacheCameraStartRotation = interaction.cameraInteractionData.cameraStartTransform.rotation;
        isAnimatingCamera = true;
        cameraAnimationCancellationTokenSource = new CancellationTokenSource();

        try
        {
            if (MainCam != null)
            {
                await MoveAndRotateCamera(interaction.cameraInteractionData.cameraStartTransform, interaction.cameraInteractionData.cameraEndTransform, interaction.cameraInteractionData.cameraMoveDuration, cameraAnimationCancellationTokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, do nothing special
        }
        finally
        {
            isAnimatingCamera = false;
        }


        await Task.Delay((int)(interaction.cameraInteractionData.cameraReturnDuration * 1000));

        if (!interaction.cameraInteractionData.lockCamera)
        {
            try
            {
                if (MainCam != null)
                {
                    await MoveAndRotateCameraBack(cacheCameraStartPos, cacheCameraStartRotation, interaction.cameraInteractionData.cameraMoveDuration, cameraAnimationCancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, do nothing special
            }
            finally
            {
                isAnimatingCamera = false;
            }
        }

    }

    private async Task WaitForTimelineToComplete(PlayableDirector director)
    {
        while (director.state == PlayState.Playing)
        {
            await Task.Yield();
        }
    }

    public async Task WaitForAnimationComplete(Animation animationComponent, string animationName)
    {
        if (animationComponent == null || !animationComponent[animationName])
        {
            Debug.LogError("Animation clip not found or animation component not assigned!");
            return;
        }

        // Play the animation
        animationComponent.Play(animationName);

        // Wait while the animation is playing
        while (animationComponent.isPlaying)
        {
            await Task.Yield(); // Continue waiting until the animation stops
        }
    }

    private async Task MoveAndRotateCamera(Transform startTransform, Transform endTransform, float duration, CancellationToken cancellationToken)
    {
        Camera mainCamera = MainCam;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found");
            return;
        }

        float elapsed = 0f;
        Vector3 startPosition = startTransform.position;
        Quaternion startRotation = startTransform.rotation;
        Vector3 endPosition = endTransform.position;
        Quaternion endRotation = endTransform.rotation;

        mainCamera.transform.position = startPosition;
        mainCamera.transform.rotation = startRotation;

        while (elapsed < duration)
        {
            // Check for cancellation request
            if (cancellationToken.IsCancellationRequested)
            {
                return; // Exit the method if canceled
            }

            mainCamera.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        mainCamera.transform.position = endPosition;
        mainCamera.transform.rotation = endRotation;
    }

    private async Task MoveAndRotateCameraBack(Vector3 targetPosition, Quaternion targetRotation, float duration, CancellationToken cancellationToken)
    {
        Camera mainCamera = MainCam;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found");
            return;
        }

        float elapsed = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        while (elapsed < duration)
        {
            // Check for cancellation request
            if (cancellationToken.IsCancellationRequested)
            {
                return; // Exit the method if canceled
            }

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }

    private void OnApplicationQuit()
    {
        // Cancel the camera animation task
        cameraAnimationCancellationTokenSource?.Cancel();
        itemAnimationCancellationTokenSource?.Cancel();

        // Reset the camera position and rotation on application quit
        MainCam.transform.position = initialCameraPosition;
        MainCam.transform.rotation = initialCameraRotation;
    }

    public void MoveToNextStep()
    {
        EventManager.Broadcast(EVENTS.NEXT_STEP);
    }

    #region Event Subscription Handler
    //subribed on SPAW event
    private void OnEnable()
    {
        EventManager.AddHandler(EVENTS.STEP, CallInteraction);
    }
    //unsubribed from SPAW event
    private void OnDisable()
    {
        EventManager.RemoveHandler(EVENTS.STEP, CallInteraction);
    }
    #endregion
}
