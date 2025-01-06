using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunController : MonoBehaviour
{
    [SerializeField] Gun gun;

    void OnClick(InputValue inputValue)
    {
        if(!gun.gameObject.activeSelf) return;
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
