using UnityEngine;


public class PlayerInteractive : MonoBehaviour
{
    public static PlayerInteractive Instance { get; private set; }
    public PlayerInput playerInput;
    public GameObject targetObject;
    public float interactableRange = 1f;
    private bool isInRange = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, targetObject.transform.position) < interactableRange)
        {
            isInRange = true;
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (targetObject.TryGetComponent<ChestOpening>(out ChestOpening chestOpening))
                {
                    chestOpening.OpenLid();
                }
            }
        }
    }
}

