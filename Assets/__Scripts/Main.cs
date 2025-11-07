using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

[RequireComponent(typeof(BoundsCheck))]
public class Main : MonoBehaviour
{
    // ---------- Wave data ----------
    [System.Serializable]
    public class Wave
    {
        [Tooltip("Just for your reference")]
        public string waveName = "Wave";
        [Tooltip("When this wave should begin (seconds since Play)")]
        public float startTime = 0f;
        [Tooltip("Enemies to spawn in this wave (in order)")]
        public GameObject[] enemies;
        [Tooltip("Delay between spawns inside this wave")]
        public float spawnInterval = 0.5f;
    }

    // ---------- Static / global ----------
    static private Main S;                        // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;

    // ---------- Inspector (existing) ----------
    [Header("Inscribed")]
    public bool spawnEnemies = true;                   // used only if NOT using timed waves
    public GameObject[] prefabEnemies;                 // random mode only
    public float enemySpawnPerSecond = 0.5f;           // random mode only
    public float enemyInsetDefault = 1.5f;             // spawn padding
    public float gameRestartDelay = 2.0f;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {
        eWeaponType.blaster, eWeaponType.blaster,
        eWeaponType.spread,  eWeaponType.shield
    };

    [Header("Timed Level Progression (Wave System)")]
    public bool useTimedWaves = true;                  // turn on the wave system
    public bool loopWaves = true;                      // <— set true to loop forever
    public List<Wave> waves = new List<Wave>();        // define waves in Inspector

    // ---------- Private ----------
    private BoundsCheck bndCheck;

    // state for wave system
    private float levelStartTime;
    private int currentWave = 0;
    private int nextEnemyIndex = 0;
    private float nextSpawnAt = 0f;
    private bool wavesCompleted = false;

    void Awake()
    {
        S = this;

        // Cache BoundsCheck
        bndCheck = GetComponent<BoundsCheck>();

        // Initialize dictionary of weapon defs
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }

        // Choose spawn mode
        if (!useTimedWaves)
        {
            // Old randomized mode
            Invoke(nameof(SpawnEnemyRandom), 1f / enemySpawnPerSecond);
        }
        else
        {
            // Timed wave mode
            levelStartTime = Time.time;
            currentWave = 0;
            nextEnemyIndex = 0;
            nextSpawnAt = 0f;
            wavesCompleted = (waves == null || waves.Count == 0);
        }
    }

    void Update()
    {
        if (!useTimedWaves) return;
        if (wavesCompleted) return;

        HandleTimedWaves();
    }

    // ---------- Timed Waves ----------
    private void HandleTimedWaves()
    {
        // If we’ve reached the end of the list of waves…
        if (currentWave >= waves.Count)
        {
            if (loopWaves)
            {
                // restart from the top
                currentWave = 0;
                nextEnemyIndex = 0;
                levelStartTime = Time.time;
                nextSpawnAt = 0f;
                return;
            }
            else
            {
                wavesCompleted = true;
                return;
            }
        }

        Wave w = waves[currentWave];

        // Wait until it's time to start this wave
        float elapsed = Time.time - levelStartTime;
        if (elapsed < w.startTime) return;

        // Time to spawn enemies inside this wave
        if (nextEnemyIndex < (w.enemies?.Length ?? 0) && Time.time >= nextSpawnAt)
        {
            GameObject prefab = w.enemies[nextEnemyIndex];
            if (prefab != null) SpawnSpecificEnemy(prefab);
            nextEnemyIndex++;
            nextSpawnAt = Time.time + Mathf.Max(0f, w.spawnInterval);
        }

        // If this wave has finished spawning, advance to the next wave
        if (nextEnemyIndex >= (w.enemies?.Length ?? 0))
        {
            currentWave++;
            nextEnemyIndex = 0;
            nextSpawnAt = 0f; // will be recalculated when the next wave starts
        }
    }

    private void SpawnSpecificEnemy(GameObject enemyPrefab)
    {
        // Position the Enemy above the screen with a random x
        float enemyInset = enemyInsetDefault;
        var bc = enemyPrefab.GetComponent<BoundsCheck>();
        if (bc != null) enemyInset = Mathf.Abs(bc.radius);

        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;

        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    // ---------- Legacy random spawner (kept for convenience) ----------
    public void SpawnEnemyRandom()
    {
        if (!spawnEnemies)
        {
            Invoke(nameof(SpawnEnemyRandom), 1f / enemySpawnPerSecond);
            return;
        }

        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject prefab = prefabEnemies[ndx];
        SpawnSpecificEnemy(prefab);

        Invoke(nameof(SpawnEnemyRandom), 1f / enemySpawnPerSecond);
    }

    // ---------- Restart flow ----------
    void DelayedRestart()
    {
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart()
    {
        SceneManager.LoadScene("__Scene_0");
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();
    }

    // ---------- WeaponDefinition lookup ----------
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {
        if (WEAP_DICT.ContainsKey(wt)) return WEAP_DICT[wt];
        return new WeaponDefinition();
    }

    // ---------- PowerUp drop ----------
    static public void SHIP_DESTROYED(Enemy e)
    {
        if (Random.value <= e.powerUpDropChance)
        {
            int ndx = Random.Range(0, S.powerUpFrequency.Length);
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            GameObject go = Instantiate<GameObject>(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetType(pUpType);
            pUp.transform.position = e.transform.position;
        }
    }
}
