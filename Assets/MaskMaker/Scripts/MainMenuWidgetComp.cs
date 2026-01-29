using System;
using UnityEngine;

public class MainMenuWidgetComp : MonoBehaviour
{
    public event Action Opened;
    public event Action GameContinued;

    public void Open()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        Opened?.Invoke();
    }

    public void ContinueGame()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        GameContinued?.Invoke();
    }

    public void OpenSettings()
    {
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
