using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunController : MonoBehaviour
{
    [SerializeField] Gun gun;

    void OnClick(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            gun.Shooting = true;
        }
        else
        {
            gun.Shooting = false;
        }
    }
}
