using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    [SerializeField] private string pubScene = "Prototype_1_Pub";
    public void Interact()
    {
        SceneManager.LoadScene(pubScene);
    }

}
