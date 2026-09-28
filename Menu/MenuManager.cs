using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject Menu;
    public GameObject pauseMenu;

    [Header("Shop")]
    public WeaponShopUI shopUI;

    [Header("Options")]
    public bool startWithMainMenu = true;
    public KeyCode pauseKey = KeyCode.Escape;

    private bool gameStarted = false;
    private bool isPaused = false;
    private Animator animator;
    public bool GameStarted => gameStarted;
    public bool IsPaused => isPaused;

    /// <summary>
    /// Истина, если геймплей сейчас должен быть заморожен
    /// (открыто главное меню или пауза).
    /// </summary>
    public static bool GameplayBlocked =>
        Instance != null && (!Instance.gameStarted || Instance.isPaused);


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (startWithMainMenu) ShowMainMenu();
        else StartGame();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!gameStarted) return;
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ---------------- Состояния ----------------
    public IEnumerator ieOpenShop() 
    {
        Time.timeScale = 1;
        if (shopUI != null) shopUI.OpenShop();
        animator.SetTrigger("ToShop");
        yield return new WaitForSeconds(0.5f);
        if (Menu != null) Menu.SetActive(false);
        Debug.Log("nigger shop is open");
        Time.timeScale = 0;

    }
    public void OpenShop()
    {

        StartCoroutine(ieOpenShop());
        // курсор уже разлочен в главном меню, Time.timeScale = 0 — трогать не надо
    }

    public IEnumerator ieCloseShop()
    {
        Time.timeScale = 1;
        animator.SetTrigger("ToMenu");
        if (Menu != null) Menu.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (shopUI != null) shopUI.CloseShop();
        Time.timeScale = 0;

    }
    public void CloseShop()
    {
       StartCoroutine(ieCloseShop());
    }


    public void ShowMainMenu()
    {
        gameStarted = false;
        isPaused = false;

        if (mainMenu != null) mainMenu.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(false);

        Time.timeScale = 0f;
        SetCursor(true);
    }

    public void StartGame()
    {
        gameStarted = true;
        isPaused = false;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);

        Time.timeScale = 1f;
        SetCursor(false);
    }

    public void PauseGame()
    {
        if (!gameStarted || isPaused) return;
        isPaused = true;

        if (pauseMenu != null) pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        SetCursor(true);
    }

    public void ResumeGame()
    {
        if (!gameStarted) return;
        isPaused = false;

        if (pauseMenu != null) pauseMenu.SetActive(false);

        Time.timeScale = 1f;
        SetCursor(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetCursor(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}