using UnityEngine;
using UnityEngine.SceneManagement;

public class MagicSquare : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;

    private ParticleSystem portalParticle;

    private bool isActivated = false;
    private Vector3 _portalLifter = new Vector3(0f, 1.5f, 0f);

    [SerializeField] private string NextScene = "Prototype_1_BossRoom";


    private void Awake()
    {
        portalParticle = portalPrefab.GetComponent<ParticleSystem>();

    }
    public void Interact()
    {
        if (isActivated)
        {
            SceneManager.LoadScene(NextScene);
        }
        else
        {
            isActivated = true;

            //portalParticle.Play();

            Instantiate(portalPrefab, transform.position + _portalLifter, Quaternion.identity);
        }

    }
}