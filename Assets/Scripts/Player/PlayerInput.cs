using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static readonly string MoveXAxis = "Horizontal";
    public static readonly string MoveYAxis = "Vertical";
    public static readonly string FireButton = "Fire1";

    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public bool Fire { get; private set; }
    public bool Attack { get; private set; }
    // public bool Explode { get; private set; }

    private void Update()
    {
        MoveX = Input.GetAxisRaw(MoveXAxis);
        MoveY = Input.GetAxisRaw(MoveYAxis);
        MousePosition = Input.mousePosition;
        Fire = Input.GetButton(FireButton);
        Attack = Input.GetKeyDown(KeyCode.Space);
        // Explode = Input.GetKeyDown(KeyCode.Q);
    }
}