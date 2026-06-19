using UnityEngine;

public class PonteJoystick : MonoBehaviour
{
    // Arraste o Dynamic Joystick da hierarquia para cá no Inspector
    public DynamicJoystick joystickVirtual; 
    
    // Arraste o seu personagem (Player) para cá no Inspector
    public PlayerController playerController; 

    void Update()
    {
        if (joystickVirtual != null && playerController != null)
        {
            // Injeta os valores do dedão (-1 a 1) direto nos campos do seu PlayerController
            playerController.joystickHorizontal = joystickVirtual.Horizontal;
            playerController.joystickVertical = joystickVirtual.Vertical;
        }
    }
}