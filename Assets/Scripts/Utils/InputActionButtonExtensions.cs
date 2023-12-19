using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


//This isn't my code it's just something that has been floating around the unity forums and is super useful. I can't credit one person because of how many people post it.
public static class InputActionButtonExtensions //custom class that allows to do the follwing special checks that aren't included in the new input system.
{
    public static bool GetButton(this InputAction action) => action.ReadValue<float>() > 0;
    public static bool GetButtonDown(this InputAction action) => action.triggered && action.ReadValue<float>() > 0;
    public static bool GetButtonUp(this InputAction action) => action.triggered && action.ReadValue<float>() == 0;
}
