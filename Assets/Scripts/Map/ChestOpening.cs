using System.Collections;
using UnityEngine;

public class ChestOpening : MonoBehaviour
{
    public GameObject lid;
    public GameObject body;
    private GameObject player;
    public float interactableRange = 1f;

    [Header("Lid Open Settings")]
    public Vector3 openAngle = new Vector3 (-90f, 0f, 0f); // 인스펙터에서 조절
    public float openDuration = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (isOpen || isAnimating) return;

        if (Vector3.Distance(transform.position, player.transform.position) < interactableRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenLid();
            }
        }
    }



    public void OpenLid()
    {
        // isOpen/isAnimating 으로 중복방지

        if (isOpen || isAnimating) return;
        StartCoroutine(RotateLid());
    }   


    private IEnumerator RotateLid()
    {
        isAnimating = true;

        Quaternion startRotation = lid.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(openAngle);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            lid.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, elapsed / openDuration);
            yield return null;
        }

        lid.transform.localRotation = endRotation;
        isOpen = true;
        isAnimating = false;
    }


}
