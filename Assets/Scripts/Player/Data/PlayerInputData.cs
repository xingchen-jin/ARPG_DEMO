using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct PlayerInputData
{
    public Vector2 moveInput;
    public Vector2 lookInput;
    
    public bool jumpInput;
    public bool crouchInput;
    public bool attackInput;
    public bool RifleInput;
    public bool aimInput;
    public bool fireInput;
    public bool reloadInput;

    public bool radialMenuInput;

}
