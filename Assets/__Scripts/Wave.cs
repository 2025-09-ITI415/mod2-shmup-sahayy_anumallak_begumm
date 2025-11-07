using UnityEngine;

[System.Serializable]
public class Wave
{
    [Tooltip("Label for your own sanity in the Inspector")]
    public string waveName;

    [Tooltip("When this wave should begin (seconds since Play)")]
    public float startTime = 0f;

    [Tooltip("Enemy prefabs to spawn in this wave (from top, left->right random X)")]
    public GameObject[] enemies;

    [Tooltip("Delay between each enemy in this wave (seconds)")]
    public float spawnInterval = 0.4f;
}
