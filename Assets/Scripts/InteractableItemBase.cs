using HighlightPlus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInteractionHandlerBase;

public class InteractableItemBase : MonoBehaviour
{
    [HideInInspector] public InteractionData interactionData;
    [HideInInspector] public AudioClip item_PickorClickSound;
    [HideInInspector] public AudioClip item_PlaceorDropSound;


    [HideInInspector] public HighlightEffect targethighlightEffect;
    [HideInInspector] public HighlightEffect highlighterEffect;

    [HideInInspector] public GameInteractionHandler interactionHandler;


    public void GetAudio()
    {
        if (interactionData.item_PickorClickSound != null) item_PickorClickSound = interactionData.item_PickorClickSound;
        if (interactionData.item_PlaceorDropSound != null) item_PlaceorDropSound = interactionData.item_PlaceorDropSound;

        InteractionAudioManager.Instance.PlayAudio(item_PickorClickSound);
    }

}
