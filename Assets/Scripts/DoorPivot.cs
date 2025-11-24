using UnityEngine;
using UnityEngine.InputSystem;
    public class AnimationManager : MonoBehaviour
    {
    // Singleton (simple pattern)
    public static AnimationManager Instance;
    public Animator doorAnimator;
    private void Awake()
    {if(doorAnimator ==null)
    {
    Debug.LogError("DoorAnimator: no Animator found.");
    }
    // Enforce singleton
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    // If you want this manager to persist across scenes uncomment:
    // DontDestroyOnLoad(gameObject);
    }
    [ContextMenu("OpenDoor")]
    public void OpenDoor()
    {
    doorAnimator.SetBool("isOpen", true);
    }
    [ContextMenu("CloseDoor")]
    public void CloseDoor()
    {
    doorAnimator.SetBool("isOpen", false);
    }
// Alternative Approach
    public void ToggleDoor()
    {
        bool current = doorAnimator.GetBool("isOpen");
        doorAnimator.SetBool("isOpen", !current);
    }
}