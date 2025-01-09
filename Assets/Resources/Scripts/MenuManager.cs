using UnityEngine;
using UnityEngine.UI;
public class MenuManager : MonoBehaviour
{
    [SerializeField] SceneInit sceneInit;
    [SerializeField] Button startBtn;
    [SerializeField] Button exitBtn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startBtn.onClick.AddListener(() => StartClicked());
        exitBtn.onClick.AddListener(() => ExitClicked());
    }


    private void StartClicked()
    {
        sceneInit.ChangeScene("GameScene");
    }

    private void ExitClicked()
    {
        Debug.Log("Game closed");
        Application.Quit();
    }
}
