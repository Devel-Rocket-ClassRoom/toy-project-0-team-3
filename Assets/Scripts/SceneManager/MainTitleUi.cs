using UnityEngine;
using UnityEngine.UI;

public class MainTitleUi : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        continueButton.onClick.AddListener(OnStartGame);
        newGameButton.onClick.AddListener(OnStartGame);
        exitButton.onClick.AddListener(OnQuit);
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
