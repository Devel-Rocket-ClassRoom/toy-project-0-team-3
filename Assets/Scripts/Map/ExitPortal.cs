using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    [SerializeField]
    private string bossScene = "Prototype_1_BossRoom";

    public void Interact()
    {
        SceneManager.LoadScene(bossScene);
    }
}
