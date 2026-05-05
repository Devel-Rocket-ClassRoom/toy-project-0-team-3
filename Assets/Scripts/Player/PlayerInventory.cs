using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private PlayerInput _playerInput;

    public GameObject _inventory;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inventory.SetActive(false);
    }

    private void Update()
    {
        if (_playerInput.TabKey)
        {
            _inventory.SetActive(!_inventory.activeSelf);
        }
    }
}
