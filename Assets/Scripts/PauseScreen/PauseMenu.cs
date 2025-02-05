using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    PlayerInput playerInput;
    public DataPersistenceManager dataPersistenceManager;

    public static PauseMenu instance;

    public static bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        playerInput = GameObject.Find("PlayerInput").GetComponent<PlayerInput>();

        pauseMenu.SetActive(false);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        playerInput.SwitchCurrentActionMap("UI");
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        playerInput.SwitchCurrentActionMap("Gameplay");
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToBase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Base_Scene");
        isPaused = false;
    }

    public void LvlSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
        isPaused = false;
    }

    public void ExitGame()
    {
        SaveGame();
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    public void SaveGame()
    {
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.SaveGame();
            Debug.Log("Game Saved");
        }
        else
        {
            Debug.LogError("DataPersistenceManager instance not found");
        }
    }

}
