using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private int lives = 3;

    [Header("Progression")]
    [SerializeField] private int killsToLevel2 = 20;
    [SerializeField] private string level2SceneName = "Level2";
    private int _killCount = 0;

    [Header("Drops - Shield")]
    [SerializeField, Range(0f, 1f)] private float shieldDropChance = 0.5f;
    [SerializeField] private GameObject shieldPickupPrefab;

    [Header("Drops - Weapons")]
    [SerializeField, Range(0f, 1f)] private float weaponDropChance = 0.45f;
    [SerializeField] private GameObject rpgPickupPrefab;
    [SerializeField] private GameObject flamethrowerPickupPrefab;
    [SerializeField] private GameObject riflePickupPrefab;

    [Header("Weapon Probabilitys")]
    [SerializeField] private float weightRifle = 0.5f; 
    [SerializeField] private float weightFlamethrower = 0.3f;
    [SerializeField] private float weightRpg = 0.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        lives = 3;
        _killCount = 0;
    }

    public void OnEnemyKilled(Vector3 enemyPos)
    {
        _killCount++;

        //cambio de nivel
        if (_killCount >= killsToLevel2)
        {
            LoadLevel2();
            return;
        }

        //drop de escudo
        if (shieldPickupPrefab != null && Random.value <= shieldDropChance)
        {
            Instantiate(shieldPickupPrefab, enemyPos, Quaternion.identity);
        }

        //drop de armas
        if (Random.value <= weaponDropChance)
        {
            GameObject pick = PickWeaponDrop();
            if (pick != null)
                Instantiate(pick, enemyPos, Quaternion.identity);
        }
    }
    public void AddKill()
    {
        _killCount++;

        Debug.Log($"Kills: {_killCount}/{killsToLevel2}");

        if (_killCount >= killsToLevel2)
        {
            SceneManager sm = FindFirstObjectByType<SceneManager>();
            if (sm != null) sm.LoadSceneByName(level2SceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(level2SceneName);
        }
    }

    private GameObject PickWeaponDrop()
    {
        float total = 0f;

        if (riflePickupPrefab != null) total += weightRifle;
        if (flamethrowerPickupPrefab != null) total += weightFlamethrower;
        if (rpgPickupPrefab != null) total += weightRpg;

        if (total <= 0f) return null;

        float roll = Random.value * total;

        if (riflePickupPrefab != null)
        {
            roll -= weightRifle;
            if (roll <= 0f) return riflePickupPrefab;
        }

        if (flamethrowerPickupPrefab != null)
        {
            roll -= weightFlamethrower;
            if (roll <= 0f) return flamethrowerPickupPrefab;
        }

        if (rpgPickupPrefab != null)
        {
            roll -= weightRpg;
            if (roll <= 0f) return rpgPickupPrefab;
        }

        return null;
    }

    private void LoadLevel2()
    {
        SceneManager sm = FindFirstObjectByType<SceneManager>();
        if (sm != null)
        {
            sm.LoadSceneByName(level2SceneName);
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(level2SceneName);
    }

    public void PlayerKilled()
    {
        lives--;

        if (lives <= 0)
        {
            GameOver();
            return;
        }

        SceneManager sm = FindFirstObjectByType<SceneManager>();
        if (sm != null) sm.ReloadCurrent();
        else UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void GameOver()
    {
        SceneManager sm = FindFirstObjectByType<SceneManager>();
        if (sm != null) sm.LoadSceneByName("Menu");
        else Debug.Log("GAME OVER");
    }
}