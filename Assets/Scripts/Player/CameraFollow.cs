using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 초기 offset Tranform(0f, 9f, -9f)
    //             Rotation(40f, 0f, 0f)
    public Transform target;
    private Vector3 offset;

    private void Awake()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
