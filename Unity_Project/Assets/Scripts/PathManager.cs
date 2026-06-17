using UnityEngine;

public class PathManager : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public Transform currentTargetMarker;

    [Header("Settings")]
    public float reachDistance = 0.20f;

    private int currentIndex = 0;

    public void ResetPath()
    {
        currentIndex = 0;
        UpdateMarker();
    }

    public Vector3 GetCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return transform.position;
        }

        return waypoints[currentIndex].position;
    }

    public float GetDistanceToCurrent(Vector3 agentPosition)
    {
        Vector3 target = GetCurrentWaypoint();
        target.y = agentPosition.y;

        return Vector3.Distance(agentPosition, target);
    }

    public Vector3 GetDirectionToCurrent(Vector3 agentPosition)
    {
        Vector3 target = GetCurrentWaypoint();
        target.y = agentPosition.y;

        Vector3 direction = target - agentPosition;

        if (direction.magnitude < 0.001f)
        {
            return Vector3.zero;
        }

        return direction.normalized;
    }

    public bool IsCurrentWaypointReached(Vector3 agentPosition)
    {
        return GetDistanceToCurrent(agentPosition) < reachDistance;
    }

    public bool MoveToNextWaypoint()
    {
        if (currentIndex < waypoints.Length - 1)
        {
            currentIndex++;
            UpdateMarker();
            return false;
        }

        return true;
    }

    public float GetProgress01()
    {
        if (waypoints == null || waypoints.Length <= 1)
        {
            return 0f;
        }

        return (float)currentIndex / (waypoints.Length - 1);
    }

    private void UpdateMarker()
    {
        if (currentTargetMarker != null && waypoints != null && waypoints.Length > 0)
        {
            currentTargetMarker.position = waypoints[currentIndex].position;
        }
    }
}