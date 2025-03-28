using UnityEngine;

public class PlayerControllerManager : MonoBehaviour
{
    [SerializeField] private PlayerController startPlayerController;
    
    public PlayerController ActivePlayerController { get; private set; }

    /// <summary>
    /// Whether the input is enabled for the active player controller.
    /// Note: this is inherited from the previous PlayerController
    /// when switching player controllers. Thus, always set this
    /// back to true when no longer needed.
    /// </summary>
    public bool InputEnabled
    {
        get => ActivePlayerController.InputEnabled;
        set => ActivePlayerController.InputEnabled = value;
    }
    
    private void Start()
    {
        ActivePlayerController = startPlayerController;
        ActivePlayerController.enabled = true;
        ActivePlayerController.InputEnabled = true;
    }
    
    public void SwitchPlayerController(PlayerController playerController)
    {
        playerController.InputEnabled = ActivePlayerController.InputEnabled;
        
        ActivePlayerController.enabled = false;
        ActivePlayerController = playerController;
        ActivePlayerController.enabled = true;
    }
}
