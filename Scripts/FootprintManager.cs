using System.Collections.Generic;
using UnityEngine;

public class FootprintManager : MonoBehaviour
{
    [Header("Foot References")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Footprint Prefab")]
    public GameObject footprintPrefab;

    [Header("Settings")]
    public float groundY = 0.01f;
    public float footContactHeight = 0.08f;
    public float minDistanceBetweenPrints = 0.08f;
    public float minTimeBetweenPrints = 0.25f;

    private Vector3 lastLeftPrintPosition;
    private Vector3 lastRightPrintPosition;
    private float lastLeftPrintTime;
    private float lastRightPrintTime;

    private readonly List<GameObject> spawnedPrints = new List<GameObject>();

    private void Start()
    {
        lastLeftPrintPosition = Vector3.one * 999f;
        lastRightPrintPosition = Vector3.one * 999f;
    }

    private void Update()
    {
        TrySpawnFootprint(leftFoot, ref lastLeftPrintPosition, ref lastLeftPrintTime);
        TrySpawnFootprint(rightFoot, ref lastRightPrintPosition, ref lastRightPrintTime);
    }

    private void TrySpawnFootprint(Transform foot, ref Vector3 lastPrintPosition, ref float lastPrintTime)
    {
        if (foot == null || footprintPrefab == null)
        {
            return;
        }

        bool isNearGround = foot.position.y < footContactHeight;
        bool movedEnough = Vector3.Distance(foot.position, lastPrintPosition) > minDistanceBetweenPrints;
        bool timeEnough = Time.time - lastPrintTime > minTimeBetweenPrints;

        if (isNearGround && movedEnough && timeEnough)
        {
            Vector3 spawnPosition = new Vector3(foot.position.x, groundY, foot.position.z);

            Quaternion spawnRotation = Quaternion.Euler(
                0f,
                foot.eulerAngles.y,
                0f
            );

            GameObject print = Instantiate(footprintPrefab, spawnPosition, spawnRotation);
            spawnedPrints.Add(print);

            lastPrintPosition = foot.position;
            lastPrintTime = Time.time;
        }
    }

    public void ClearFootprints()
    {
        for (int i = 0; i < spawnedPrints.Count; i++)
        {
            if (spawnedPrints[i] != null)
            {
                Destroy(spawnedPrints[i]);
            }
        }

        spawnedPrints.Clear();

        lastLeftPrintPosition = Vector3.one * 999f;
        lastRightPrintPosition = Vector3.one * 999f;
        lastLeftPrintTime = 0f;
        lastRightPrintTime = 0f;
    }
}