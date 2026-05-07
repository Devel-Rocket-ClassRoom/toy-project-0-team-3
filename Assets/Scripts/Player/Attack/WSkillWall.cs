using System.Collections.Generic;
using UnityEngine;

public class WSkillWall : MonoBehaviour
{
    [SerializeField] private WSkill _wSkill;
    [SerializeField] private Collider _wCollider;
    private List<Monster> _pushedMonsters = new List<Monster>();

    private void Awake()
    {
        DisableWall();
    }

    public void EnableWall()
    {
        _wCollider.enabled = true;
    }

    public void DisableWall()
    {
        _pushedMonsters.Clear();
        _wCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Monster>(out var monster))
        {
            monster.DisableNavMesh();
            _pushedMonsters.Add(monster);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Monster>(out var monster))
        {
            monster.EnableNavMesh();
            _pushedMonsters.Remove(monster);
        }
    }

    private void FixedUpdate()
    {
        foreach (var monster in _pushedMonsters)
        {
            if (monster == null) continue;
            Rigidbody rb = monster.GetComponent<Rigidbody>();
            rb.MovePosition(rb.position + _wSkill.Direction.normalized * _wSkill.DashSpeed * Time.fixedDeltaTime);
        }
    }
}