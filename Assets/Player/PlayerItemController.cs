using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemController : EntityItemController
{
    void OnCycle(InputValue inputValue) {
        float dirFloat = inputValue.Get<float>();
        if(dirFloat == 0) return;
        int dir = dirFloat > 0 ? 1 : -1;
        Cycle(dir);
    }

    void OnClick(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            ActiveItem.Use(true);
        }
        else
        {
            ActiveItem.Use(false);
        }
    }
}
