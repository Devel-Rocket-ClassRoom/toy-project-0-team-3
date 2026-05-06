using UnityEngine;


public class PlayerInteractive : MonoBehaviour
{
    public static PlayerInteractive Instance { get; private set; }
    [Header("References")]
    public PlayerInput playerInput;

    [Header("Interaction Settings")]
    public float interactableRange = 1f;
    public LayerMask interactableLayer; // 상호작용 레이어 설정

    private GameObject currentTarget; // 범위 내 상호작용 대상
    private bool isInRange = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        DetectInteractable();

        if (currentTarget != null && playerInput.InteractKey)
        {
            TryInteract(currentTarget);
        }
    }


    // 플레이어 지정된 범위내에 상호작용 가능 대상이 있는지 확인
    private void DetectInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactableRange, interactableLayer);

        float closestDistance = float.MaxValue;
        currentTarget = null;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = hit.gameObject;
            }
        }
    }

    // 범위내에 있는 상호작용 가능 오브젝트와 상호작용
    private void TryInteract(GameObject target)
    {
        switch (target.tag)
        {
            case "Chest":
                ChestOpening chest = target.GetComponent<ChestOpening>();
                if (chest != null)   chest.OpenLid();
                Debug.Log("Chest Opened");
                break;
        }
    }

    // 기즈모
    private void OnDrawGizmoSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (transform.position, interactableRange);
    }
}

