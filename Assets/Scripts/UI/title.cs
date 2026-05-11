using UnityEngine;
using UnityEngine.UI;

public class title : MonoBehaviour
{
    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private Button newGameButton;

    [SerializeField]
    private Button loadGameButton;

    [SerializeField]
    private Button optionsButton;

    [SerializeField]
    private Button QuitButton;

    private void Start()
    {
        continueButton.onClick.AddListener(OnStartGame);
        newGameButton.onClick.AddListener(OnStartGame);
        QuitButton.onClick.AddListener(OnQuit);
    }

    private void OnStartGame()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.GameScene.Pub);
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
