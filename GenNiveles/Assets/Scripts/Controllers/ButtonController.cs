using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    private Button button;
    public enum ButtonAction
    {
        MainMenuWin,
        MainMenuLose,
        MainMenuEnd,
        NextLevel,
        LoadSavedGame,
        PlayTryAgain,
        PlayGame
    }

    [SerializeField] private ButtonAction action;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleButtonClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClick);
        }
    }

    private void HandleButtonClick()
    {
        if (SceneController.instance == null)
        {
            Debug.LogError("SceneController instance not found!");
            return;
        }

        switch (action)
        {
            case ButtonAction.MainMenuWin:
                SceneController.instance.btn_mainMenuWin();
                break;
            case ButtonAction.MainMenuLose:
                SceneController.instance.btn_mainMenuLose();
                break;
            case ButtonAction.MainMenuEnd:
                SceneController.instance.btn_mainMenuEnd();
                break;
            case ButtonAction.NextLevel:
                SceneController.instance.btn_nextLevel();
                break;
            case ButtonAction.LoadSavedGame:
                SceneController.instance.btn_loadSavedGame();
                break;
            case ButtonAction.PlayTryAgain:
                SceneController.instance.btn_playtryAgain();
                break;
            case ButtonAction.PlayGame:
                SceneController.instance.btn_playGame();
                break;
        }
    }
}