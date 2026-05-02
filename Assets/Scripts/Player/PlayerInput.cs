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
    public bool SkillQ { get; private set; }
    public bool SkillW { get; private set; }
    public bool SkillE { get; private set; }
    public bool SkillR { get; private set; }

    private void Update()
    {
        MoveX = Input.GetAxisRaw(MoveXAxis);
        MoveY = Input.GetAxisRaw(MoveYAxis);
        MousePosition = Input.mousePosition;
        Fire = Input.GetButton(FireButton);
        Attack = Input.GetKeyDown(KeyCode.Space);
        SkillQ = Input.GetKeyDown(KeyCode.Q);
        SkillW = Input.GetKeyDown(KeyCode.W);
        SkillE = Input.GetKeyDown(KeyCode.E);
        SkillR = Input.GetKeyDown(KeyCode.R);
    }
}