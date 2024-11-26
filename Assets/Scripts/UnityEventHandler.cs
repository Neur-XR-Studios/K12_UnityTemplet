using UnityEngine;

public class UnityEventHandler : MonoBehaviour
{
    public  void IsUnityEventCompleted(bool val)
    {
        GameInteractionHandler.isUnityEventCompleted = val;
    }
}
