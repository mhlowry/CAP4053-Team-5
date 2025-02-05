using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] public bool[] levelsCompleted;
    [SerializeField] public bool[] heardDialogue;
    private int playerLevel = 1;
    private int curExp = 0;
    private int curGold = 0;
    private bool hasSpokenToShopkeeper = false;
    private string dialogueName;    

    [Range(0, 5)] public int lightAttackBoosts = 0;

    [Range(0, 5)] public int heavyAttackBoosts = 0;

    [Range(0, 5)] public int speedBoosts = 0;

    [Range(0, 5)] public int willpowerBoosts = 0;

    [Range(0, 5)] public int healthPointBoosts = 0;

    const int maxLevel = 7;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        levelsCompleted = new bool[10];
        heardDialogue = new bool[10];

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start() 
    {
        // InitializeLevelButtons();
    }

    // Called when base island is loaded
    public void InitializeLevelPortals()
    {
        for (int i = 0; i < levelsCompleted.Length; i++)
        {
            GameObject portal = GameObject.Find("Gate_Level " + (i + 1));
            if (portal)
            {
                Transform portalChild = FindPortalChild(portal);

                if (portalChild != null)
                {
                    // The first level's portal child is always active.
                    if (i == 0)
                    {
                        portalChild.gameObject.SetActive(true);
                    }
                    else
                    {
                        // For subsequent levels, activate only if the previous level is completed
                        portalChild.gameObject.SetActive(levelsCompleted[i - 1]);
                    }
                }
                else
                {
                    Debug.LogWarning("Portal child object for level " + (i + 1) + " not found!");
                }
            }
            else
            {
                Debug.LogWarning("Portal for level " + (i + 1) + " not found!");
            }
        }
    }

    private Transform FindPortalChild(GameObject portal)
    {
        foreach (Transform child in portal.transform)
        {
            if (child.name.StartsWith("Portal"))
            {
                return child;
            }
        }
        return null;
    }

    public void MarkLevelAsCompleted(int levelIndex)
    {
        Debug.Log("Marking level " + (levelIndex + 1) + " as completed");
        if (levelIndex < levelsCompleted.Length)
        {
            levelsCompleted[levelIndex] = true;

            // Save the game data immediately after marking a level as completed
            DataPersistenceManager.instance.SaveGame();
        }
        else
        {
            Debug.LogError("Level index out of bounds");
        }
    }

    public bool IsLevelCompleted(int levelIndex)
    {
        if (levelIndex < levelsCompleted.Length)
        {
            return levelsCompleted[levelIndex];
        }
        Debug.LogError("Level index out of bounds");
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Base_Scene")
        {
            InitializeLevelPortals();
            InitializeStorytellerDialogue();
            InitializeShopkeeperDialogue();
            
            Debug.Log("Base Scene setup complete");
        }
    }

    // This method will be called every time a scene is loaded
    private void InitializeStorytellerDialogue()
    {
        GameObject storyteller = GameObject.Find("Storyteller");
        if (storyteller != null)
        {
            DialogueActivator dialogueActivator = storyteller.GetComponent<DialogueActivator>();
            if (dialogueActivator != null)
            {
                UpdateStorytellerDialogue(dialogueActivator);
            }
            else
            {
                Debug.LogWarning("DialogueActivator component not found on Storyteller");
            }
        }
        else
        {
            Debug.LogWarning("Storyteller object not found in the scene");
        }
    }

    private void UpdateStorytellerDialogue(DialogueActivator dialogueActivator)
    {
        // Check if the dialogueActivator is for the Storyteller object
        if (dialogueActivator.gameObject.name != "Storyteller")
        {
            return; // Exit the method if not the Storyteller
        }

        int completedLevels = GetCompletedLevelsCount();

        string currentDialogueName;
        string nextDialogueName;

        // Find the newDialogue GameObject as a child of the Storyteller
        GameObject newDialogue = dialogueActivator.gameObject.transform.Find("newDialogue").gameObject;

        if (completedLevels > 0)
        {
            if (!heardDialogue[completedLevels - 1])
            {
                // Use first dialogue of the current level
                currentDialogueName = $"Level {completedLevels}-1";
                // Use default dialogue of the current level
                nextDialogueName = $"Level {completedLevels}-default";
                if (newDialogue != null) newDialogue.SetActive(true);
                heardDialogue[completedLevels - 1] = true;
            }
            else
            {
                currentDialogueName = $"Level {completedLevels}-default";
                nextDialogueName = $"Level {completedLevels}-default";
                // Set newDialogue to active if on main story dialogue
                if (newDialogue != null) newDialogue.SetActive(false);
            }
        }
        else
        {
            // If no levels have been completed, use Level 1 dialogues
            currentDialogueName = "Level 1-default";
            nextDialogueName = "Level 1-default";

            // Set newDialogue to inactive if on default dialogue
            if (newDialogue != null) newDialogue.SetActive(false);
        }

        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        DialogueObject currentDialogue = dialogueManager.GetDialogueByName(currentDialogueName);
        DialogueObject nextDialogue = dialogueManager.GetDialogueByName(nextDialogueName);

        Debug.Log("Current Dialogue Name: " + currentDialogueName);
        Debug.Log("Next Dialogue Name: " + nextDialogueName);

        // Additional logs for debugging
        if (currentDialogue != null) Debug.Log("Current Dialogue found: " + currentDialogue.name);
        else Debug.Log("Current Dialogue not found");

        if (nextDialogue != null) Debug.Log("Next Dialogue found: " + nextDialogue.name);
        else Debug.Log("Next Dialogue not found");

        dialogueActivator.UpdateDialogueObject(currentDialogue);
        dialogueActivator.SetNextDialogueObject(nextDialogue);
    }



    public void InitializeShopkeeperDialogue()
    {
        GameObject shopkeeper = GameObject.Find("Shopkeeper");
        if (shopkeeper != null)
        {
            DialogueActivator dialogueActivator = shopkeeper.GetComponent<DialogueActivator>();
            if (dialogueActivator != null)
            {
                UpdateShopkeeperDialogue(dialogueActivator);
            }
            else
            {
                Debug.LogWarning("DialogueActivator component not found on Shopkeeper");
            }
        }
        else
        {
            Debug.LogWarning("Shopkeeper object not found in the scene");
        }
    }

    public void UpdateShopkeeperDialogue(DialogueActivator shopkeeperActivator)
    {
        // Check if the shopkeeperActivator is null
        if (shopkeeperActivator == null)
        {
            Debug.LogError("ShopkeeperActivator is null");
            return;
        }

        // Find the DialogueManager in the scene
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found in the scene");
            return;
        }

        string dialogueName;

        // Check if the shopkeeper has been spoken to and set the dialogue name accordingly
        if (!hasSpokenToShopkeeper)
        {
            dialogueName = "FE 1-1"; // First encounter dialogue
            hasSpokenToShopkeeper = true;

            // Save the game state if DataPersistenceManager is available
            if (DataPersistenceManager.instance != null)
            {
                DataPersistenceManager.instance.SaveGame();
                Debug.Log("Game Saved");
            }
            else
            {
                Debug.LogError("DataPersistenceManager instance not found");
            }

            Debug.Log("First encounter dialogue set.");
        }
        else
        {
            int dialogueIndex = Random.Range(1, 9); // Randomly pick a number between 1 and 8
            dialogueName = "RT 1-" + dialogueIndex;
            Debug.Log("Random dialogue set: " + dialogueName);
        }

        // Get the dialogue object by name
        DialogueObject newDialogue = dialogueManager.GetDialogueByName(dialogueName);
        if (newDialogue == null)
        {
            Debug.LogError("DialogueObject not found for name: " + dialogueName);
            return;
        }

        // Update the dialogue activator with the new dialogue
        shopkeeperActivator.UpdateDialogueObject(newDialogue);
    }


    // Returns the count of completed levels
    private int GetCompletedLevelsCount()
    {
        int count = 0;
        foreach (bool levelCompleted in levelsCompleted)
        {
            if (levelCompleted)
            {
                count++;
            }
        }
        return count;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // setters and getters

    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
    }

    public void SetPlayerExp(int exp)
    {
        curExp = exp;
    }

    public int GetPlayerLevel()
    {
        return playerLevel;
    }

    public int GetPlayerExp()
    {
        return curExp;
    }

    public void SetPlayerGold(int gold)
    {
        curGold = gold;
    }

    public int GetPlayerGold()
    {
        return curGold;
    }
    public void SetHasSpokenToShopkeeper(bool hasSpoken)
    {
        hasSpokenToShopkeeper = hasSpoken;
    }

    public bool GetHasSpokenToShopkeeper()
    {
        return hasSpokenToShopkeeper;
    }

    public void SetLightAttackBoosts(int value)
    {
        lightAttackBoosts = value;
    }

    public int GetLightAttackBoosts()
    {
        return lightAttackBoosts;
    }

    public void SetHeavyAttackBoosts(int value)
    {
        heavyAttackBoosts = value;
    }

    public int GetHeavyAttackBoosts()
    {
        return heavyAttackBoosts;
    }

    public void SetSpeedBoosts(int value)
    {
        speedBoosts = value;
    }

    public int GetSpeedBoosts()
    {
        return speedBoosts;
    }

    public void SetWillpowerBoosts(int value)
    {
        willpowerBoosts = value;
    }

    public int GetWillpowerBoosts()
    {
        return willpowerBoosts;
    }

    public void SetHealthPointBoosts(int value)
    {
        healthPointBoosts = value;
    }

    public int GetHealthPointBoosts()
    {
        return healthPointBoosts;
    }

    public string GetDialogueName()
    {
        return dialogueName;
    }

    public void SetDialogueName(string name)
    {
        dialogueName = name;
    }

    public bool[] GetLevelsCompleted()
    {
        return levelsCompleted;
    }

    public void SetLevelsCompleted(bool[] completedLevels)
    {
        if (completedLevels.Length == levelsCompleted.Length)
        {
            levelsCompleted = completedLevels;
            Debug.Log("Levels completion status updated.");
        }
        else
        {
            Debug.LogError("Mismatch in levelsCompleted length.");
        }
    }

    public int GetLevelsCompletedCount()
    {
        int count = 0;
        foreach (bool levelCompleted in levelsCompleted)
        {
            if (levelCompleted)
            {
                count++;
            }
        }
        return count;
    }

    public bool[] GetHeardDialogue()
    {
        return heardDialogue;
    }

    public void SetHeardDialogue(bool[] heard)
    {
        if (heard.Length == heardDialogue.Length)
        {
            heardDialogue = heard;
            Debug.Log("Heard dialogue status updated.");
        }
        else
        {
            Debug.LogError("Mismatch in heardDialogue length.");
        }
    }
}
