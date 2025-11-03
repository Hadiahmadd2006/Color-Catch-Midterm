using UnityEngine;
using UnityEngine.UI;               
using UnityEngine.SceneManagement;  
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;        
    public TMP_Text targetColorText;  
    public TMP_Text timerText;        

    [Header("Game Over UI")]
    public GameObject gameOverPanel;  
    public Button restartButton;
    public MonoBehaviour[] toDisable; 
    public TMP_Text finalScoreGO; 


    [Header("Audio")]
    public AudioSource sfxAudioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public AudioClip timeUpSFX;

    [Header("Game Settings")]
    public int score = 0;
    public float startTimeSeconds = 60f;
    public CoinColor currentTarget;

    [Header("Level Complete UI (Optional)")]
    public GameObject levelCompletePanel;
    public Button nextLevelButton;
    public Button exitButtonOnWin;
    public TMP_Text finalScoreNL;

    private float timeLeft;
    private bool isRunning = true;

    private float levelCheckDelay = 1.0f;
    private float levelCheckTimer = 0f;
    private bool levelComplete = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timeLeft = Mathf.Max(0f, startTimeSeconds);
        UpdateScoreUI();
        UpdateTimerUI();
        PickNewTargetColor();

        if (gameOverPanel) gameOverPanel.SetActive(false);

        if (restartButton) restartButton.onClick.AddListener(Restart);

        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (nextLevelButton) nextLevelButton.onClick.AddListener(NextLevelDefault);
        if (exitButtonOnWin) exitButtonOnWin.onClick.AddListener(QuitGame);

        OnThemeSelected(0);

        Time.timeScale = 1f;
        levelCheckTimer = 0f;
        levelComplete = false;

        if (startPanel && startPanel.activeInHierarchy)
        {
            isRunning = false;
            Time.timeScale = 0f;
        }
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerUI();
            OnTimeUp();
            return;
        }
        UpdateTimerUI();
    }

    void LateUpdate()
    {
        if (!isRunning || levelComplete) return;

        levelCheckTimer += Time.deltaTime;
        if (levelCheckTimer < levelCheckDelay) return;

        if (AllCoinsGone())
        {
            OnLevelComplete();
        }
    }

    public void OnCoinCollected(Coin coin)
    {
        if (!isRunning || coin == null) return;

        if (coin.coinColor == currentTarget)
        {
            score += 1;
            PlaySFX(correctSFX);
        }
        else
        {
            score -= 1;
            PlaySFX(wrongSFX);
        }

        UpdateScoreUI();
        PickNewTargetColor();
    }

    void PickNewTargetColor()
    {
        var colorsInScene = GetActiveCoinColors();
        if (colorsInScene.Count > 0)
        {
            var arr = new List<CoinColor>(colorsInScene);
            currentTarget = arr[Random.Range(0, arr.Count)];
        }
        else
        {
            currentTarget = (CoinColor)Random.Range(0, 4);
        }

        EnsureAtLeastOneTargetCoin();

        if (targetColorText)
        {
            targetColorText.text = "Catch: " + currentTarget.ToString().ToUpper();
            targetColorText.color = Coin.FromCoinColor(currentTarget);
        }
    }

    HashSet<CoinColor> GetActiveCoinColors()
    {
        var set = new HashSet<CoinColor>();
        var gos = GameObject.FindGameObjectsWithTag("Coin");
        foreach (var go in gos)
        {
            if (!go.activeInHierarchy) continue;
            var c = go.GetComponent<Coin>();
            if (c != null) set.Add(c.coinColor);
        }
        return set;
    }

    void EnsureAtLeastOneTargetCoin()
    {
        var gos = GameObject.FindGameObjectsWithTag("Coin");
        Coin firstActive = null;
        foreach (var go in gos)
        {
            if (!go.activeInHierarchy) continue;
            var c = go.GetComponent<Coin>();
            if (!c) continue;
            if (firstActive == null) firstActive = c;
            if (c.coinColor == currentTarget) return;
        }
        if (firstActive != null)
        {
            firstActive.coinColor = currentTarget;
            firstActive.ApplyColor();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;
        int secs = Mathf.CeilToInt(timeLeft);
        int m = secs / 60;
        int s = secs % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

    void OnTimeUp()
    {
        if (!isRunning) return;
        isRunning = false;

        PlaySFX(timeUpSFX);

        if (toDisable != null)
            foreach (var mb in toDisable) if (mb) mb.enabled = false;

        Time.timeScale = 0f;

        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (targetColorText) targetColorText.text = "TIME'S UP!";
        if (finalScoreGO) finalScoreGO.text = "Final Score: " + score;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource && clip) sfxAudioSource.PlayOneShot(clip);
    }

    public void NextLevel(int increment = 2)
    {
        Time.timeScale = 1f;
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);

        var spawners = FindObjectsByType<CoinSpawner>(FindObjectsSortMode.None);
        foreach (var sp in spawners)
        {
            if (sp == null) continue;
            int newCount = Mathf.Max(0, sp.count + increment);
            sp.Respawn(newCount);
        }

        timeLeft = Mathf.Max(0f, startTimeSeconds);
        UpdateTimerUI();

        PickNewTargetColor();

        isRunning = true;
        levelComplete = false;
        levelCheckTimer = 0f;
    }

    public void NextLevelDefault() => NextLevel();

    bool AllCoinsGone()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (var c in coins)
        {
            if (c.activeInHierarchy) return false;
        }
        return true;
    }

    void OnLevelComplete()
    {
        levelComplete = true;
        isRunning = false;

        Time.timeScale = 0f;

        if (targetColorText) targetColorText.text = "LEVEL COMPLETE!";

        if (!levelCompletePanel)
            levelCompletePanel = GameObject.Find("NextLevelPanel");

        if (levelCompletePanel) levelCompletePanel.SetActive(true);
        else Debug.LogWarning("[GameManager] Level complete panel not found. Assign 'levelCompletePanel' in Inspector or name it 'NextLevelPanel'.");

        if (levelCompletePanel) levelCompletePanel.SetActive(true);
        if (finalScoreNL) finalScoreNL.text = "Final Score: " + score;
    }

    public void OnPlayerCaught()
    {
        if (!isRunning) return;
        isRunning = false;

        if (toDisable != null)
            foreach (var mb in toDisable) if (mb) mb.enabled = false;

        if (targetColorText) targetColorText.text = "CAUGHT!";
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreGO) finalScoreGO.text = "Final Score: " + score;

        Time.timeScale = 0f;
    }

    [Header("Start Menu")]
    public GameObject startPanel;       
    public GameObject optionsPanel;     
    public Color[] availablePlayerColors = new Color[] { Color.white, Color.red, Color.green, Color.blue };

    public void OnPlayPressed()
    {
        if (startPanel) startPanel.SetActive(false);
        Time.timeScale = 1f;
        isRunning = true;
    }

    public void OnOptionsPressed()
    {
        if (optionsPanel) optionsPanel.SetActive(true);
        if (startPanel) startPanel.SetActive(false);
    }

    public void OnCloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (startPanel) startPanel.SetActive(true);
    }

    public void OnMuteToggle()
    {
        AudioListener.pause = !AudioListener.pause;
    }

    [System.Serializable]
    public class CharacterTheme
    {
        public string themeName;
        public GameObject playerPrefab;
        public GameObject enemyPrefab;
    }

    [Header("Character Themes")]
    public CharacterTheme[] themes;          
    private int currentThemeIndex = 0;

    public Transform playerSpawnPoint;       
    public Transform enemySpawnPoint;        

    private GameObject currentPlayer;
    private GameObject currentEnemy;

    public void OnThemeSelected(int index)
    {
        if (themes == null || themes.Length == 0) return;
        index = Mathf.Clamp(index, 0, themes.Length - 1);
        currentThemeIndex = index;

        if (currentPlayer) Destroy(currentPlayer);
        if (currentEnemy) Destroy(currentEnemy);

        currentPlayer = Instantiate(themes[index].playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        currentEnemy   = Instantiate(themes[index].enemyPrefab,  enemySpawnPoint.position,  enemySpawnPoint.rotation);

        var ef = currentEnemy.GetComponent<EnemyFollow>();
        if (ef) ef.target = currentPlayer.transform;

        Debug.Log("Theme switched to: " + themes[index].themeName);
    }
}