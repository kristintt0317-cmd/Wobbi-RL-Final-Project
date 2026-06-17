using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WobbiGaitAgent : Agent
{
    [Header("Main References")]
    public ArticulationBody rootBody;
    public Transform bodyTransform;

    [Header("Path Following")]
    public PathManager pathManager;

    [Header("Footprint Visualisation")]
    public FootprintManager footprintManager;

    [Header("8 DOF Joint Order")]
    public ArticulationBody[] joints = new ArticulationBody[8];

    [Header("Joint Angle Limits")]
    public Vector2[] jointLimits = new Vector2[8]
    {
        new Vector2(-8f, 8f),      // 0 Left Hip Roll
        new Vector2(-16f, 16f),    // 1 Left Hip Pitch
        new Vector2(0f, 35f),      // 2 Left Knee Pitch
        new Vector2(-10f, 10f),    // 3 Left Ankle Pitch

        new Vector2(-8f, 8f),      // 4 Right Hip Roll
        new Vector2(-16f, 16f),    // 5 Right Hip Pitch
        new Vector2(0f, 35f),      // 6 Right Knee Pitch
        new Vector2(-10f, 10f)     // 7 Right Ankle Pitch
    };

    [Header("Reward Settings")]
    public float uprightRewardScale = 0.0008f;
    public float survivalReward = 0.00015f;

    public float tiltPenalty = -0.006f;
    public float lowBodyPenalty = -0.006f;
    public float crawlingPenalty = -0.010f;
    public float standingStillPenalty = -0.003f;

    public float actionPenaltyScale = 0.00012f;
    public float timePenalty = -0.0002f;

    public float waypointProgressScale = 5.0f;
    public float velocityToWaypointScale = 0.03f;
    public float noProgressPenalty = -0.004f;

    public float waypointReachedReward = 1.0f;
    public float pathCompletedReward = 3.0f;
    public float fallPenalty = -1.0f;

    [Header("Speed Reward")]
    public float targetForwardSpeed = 0.12f;
    public float speedMatchRewardScale = 0.01f;
    public float slowMovementPenalty = -0.003f;

    [Header("Alternating Gait Reward")]
    public float antiPhaseGaitRewardScale = 0.002f;
    public float hipSwingRewardScale = 0.0008f;

    [Header("Standing Condition")]
    public float standingBodyHeight = 0.32f;
    public float standingUprightDot = 0.68f;

    [Header("Low Body / Fall Detection")]
    public float lowBodyHeight = 0.26f;
    public float minBodyHeight = 0.18f;
    public float minUprightDot = 0.30f;

    [Header("Movement Detection")]
    public float minMovementSpeed = 0.02f;
    public int maxNoProgressSteps = 100;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float previousDistanceToWaypoint;
    private int noProgressSteps = 0;

    public override void Initialize()
    {
        if (rootBody == null)
        {
            Debug.LogError("Root Body is not assigned.");
            return;
        }

        if (bodyTransform == null)
        {
            bodyTransform = rootBody.transform;
        }

        startPosition = rootBody.transform.position;
        startRotation = rootBody.transform.rotation;

        if (pathManager != null)
        {
            pathManager.ResetPath();
            previousDistanceToWaypoint = pathManager.GetDistanceToCurrent(bodyTransform.position);
        }
    }

    public override void OnEpisodeBegin()
    {
        rootBody.TeleportRoot(startPosition, startRotation);

        rootBody.linearVelocity = Vector3.zero;
        rootBody.angularVelocity = Vector3.zero;

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] != null)
            {
                joints[i].linearVelocity = Vector3.zero;
                joints[i].angularVelocity = Vector3.zero;
                SetJointTarget(joints[i], 0f);
            }
        }

        if (pathManager != null)
        {
            pathManager.ResetPath();
            previousDistanceToWaypoint = pathManager.GetDistanceToCurrent(bodyTransform.position);
        }

        if (footprintManager != null)
        {
            footprintManager.ClearFootprints();
        }

        noProgressSteps = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observation size = 35

        Vector3 relativePosition = bodyTransform.position - startPosition;
        sensor.AddObservation(relativePosition.x);
        sensor.AddObservation(relativePosition.y);
        sensor.AddObservation(relativePosition.z);

        sensor.AddObservation(bodyTransform.rotation.x);
        sensor.AddObservation(bodyTransform.rotation.y);
        sensor.AddObservation(bodyTransform.rotation.z);
        sensor.AddObservation(bodyTransform.rotation.w);

        sensor.AddObservation(rootBody.linearVelocity.x);
        sensor.AddObservation(rootBody.linearVelocity.y);
        sensor.AddObservation(rootBody.linearVelocity.z);

        sensor.AddObservation(rootBody.angularVelocity.x);
        sensor.AddObservation(rootBody.angularVelocity.y);
        sensor.AddObservation(rootBody.angularVelocity.z);

        Vector3 directionToWaypoint = Vector3.zero;
        float distanceToWaypoint = 0f;
        float pathProgress = 0f;

        if (pathManager != null)
        {
            directionToWaypoint = pathManager.GetDirectionToCurrent(bodyTransform.position);
            distanceToWaypoint = pathManager.GetDistanceToCurrent(bodyTransform.position);
            pathProgress = pathManager.GetProgress01();
        }

        sensor.AddObservation(directionToWaypoint.x);
        sensor.AddObservation(directionToWaypoint.y);
        sensor.AddObservation(directionToWaypoint.z);

        sensor.AddObservation(Mathf.Clamp(distanceToWaypoint / 3f, 0f, 1f));
        sensor.AddObservation(pathProgress);

        float upright = Vector3.Dot(bodyTransform.up, Vector3.up);
        sensor.AddObservation(upright);

        for (int i = 0; i < joints.Length; i++)
        {
            float jointPosition = 0f;

            if (joints[i] != null && joints[i].jointPosition.dofCount > 0)
            {
                jointPosition = joints[i].jointPosition[0];
            }

            sensor.AddObservation(Mathf.Clamp(jointPosition / Mathf.PI, -1f, 1f));
        }

        for (int i = 0; i < joints.Length; i++)
        {
            float jointVelocity = 0f;

            if (joints[i] != null && joints[i].jointVelocity.dofCount > 0)
            {
                jointVelocity = joints[i].jointVelocity[0];
            }

            sensor.AddObservation(Mathf.Clamp(jointVelocity / 10f, -1f, 1f));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float actionPenalty = 0f;

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null)
            {
                continue;
            }

            float actionValue = Mathf.Clamp(actions.ContinuousActions[i], -1f, 1f);
            float targetAngle = ConvertActionToAngle(actionValue, jointLimits[i]);

            SetJointTarget(joints[i], targetAngle);

            actionPenalty += Mathf.Abs(actionValue);
        }

        float upright = Vector3.Dot(bodyTransform.up, Vector3.up);

        bool isStanding =
            upright > standingUprightDot &&
            bodyTransform.position.y > standingBodyHeight;

        bool isLowBody =
            bodyTransform.position.y < lowBodyHeight;

        Vector3 horizontalVelocity = rootBody.linearVelocity;
        horizontalVelocity.y = 0f;

        float horizontalSpeed = horizontalVelocity.magnitude;

        // -----------------------------
        // Stability and posture rewards
        // -----------------------------

        AddReward(upright * uprightRewardScale);

        if (isStanding)
        {
            AddReward(survivalReward);
        }
        else
        {
            AddReward(crawlingPenalty);
        }

        if (upright < 0.55f)
        {
            AddReward(tiltPenalty);
        }

        if (isLowBody)
        {
            AddReward(lowBodyPenalty);
        }

        if (isStanding && horizontalSpeed < minMovementSpeed)
        {
            AddReward(standingStillPenalty);
        }

        AddReward(-actionPenalty * actionPenaltyScale);
        AddReward(timePenalty);

        // -----------------------------
        // Alternating gait reward
        // -----------------------------
        // Encourage left and right hip pitch joints to move in opposite directions.
        // This encourages a more walk-like left-right coordination pattern.

        float leftHipPitchAction = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float rightHipPitchAction = Mathf.Clamp(actions.ContinuousActions[5], -1f, 1f);

        if (isStanding)
        {
            float antiPhase = -leftHipPitchAction * rightHipPitchAction;
            AddReward(antiPhase * antiPhaseGaitRewardScale);

            if (antiPhase > 0f)
            {
                float hipSwingAmount = Mathf.Abs(leftHipPitchAction - rightHipPitchAction);
                AddReward(hipSwingAmount * hipSwingRewardScale);
            }
        }

        // -----------------------------
        // Waypoint following rewards
        // -----------------------------

        if (pathManager != null)
        {
            float currentDistance = pathManager.GetDistanceToCurrent(bodyTransform.position);
            float distanceImprovement = previousDistanceToWaypoint - currentDistance;

            Vector3 directionToWaypoint = pathManager.GetDirectionToCurrent(bodyTransform.position);

            float velocityTowardWaypoint = Vector3.Dot(horizontalVelocity, directionToWaypoint);
            velocityTowardWaypoint = Mathf.Clamp(velocityTowardWaypoint, -0.4f, 0.4f);

            if (isStanding)
            {
                // Reward improvement in distance to the waypoint.
                AddReward(distanceImprovement * waypointProgressScale);

                // Reward velocity specifically toward the waypoint.
                AddReward(velocityTowardWaypoint * velocityToWaypointScale);

                // -----------------------------
                // Target speed reward
                // -----------------------------
                // This avoids rewarding any random fast movement.
                // The agent is rewarded only for moving toward the waypoint
                // at a moderate target speed while remaining upright.

                float forwardSpeed = Mathf.Max(0f, velocityTowardWaypoint);

                float speedMatch =
                    1f - Mathf.Clamp01(Mathf.Abs(forwardSpeed - targetForwardSpeed) / targetForwardSpeed);

                AddReward(speedMatch * speedMatchRewardScale);

                if (forwardSpeed < minMovementSpeed)
                {
                    AddReward(slowMovementPenalty);
                }
            }
            else
            {
                // Crawling or sliding toward the waypoint should not be rewarded.
                AddReward(crawlingPenalty);
            }

            if (distanceImprovement < 0.001f || !isStanding)
            {
                noProgressSteps++;
                AddReward(noProgressPenalty);
            }
            else
            {
                noProgressSteps = 0;
            }

            if (noProgressSteps > maxNoProgressSteps)
            {
                AddReward(-0.5f);
                EndEpisode();
            }

            if (pathManager.IsCurrentWaypointReached(bodyTransform.position) && isStanding)
            {
                AddReward(waypointReachedReward);

                bool finishedPath = pathManager.MoveToNextWaypoint();

                if (finishedPath)
                {
                    AddReward(pathCompletedReward);
                    EndEpisode();
                }

                previousDistanceToWaypoint = pathManager.GetDistanceToCurrent(bodyTransform.position);
                noProgressSteps = 0;
            }
            else
            {
                previousDistanceToWaypoint = currentDistance;
            }
        }

        // -----------------------------
        // Fall condition
        // -----------------------------

        if (bodyTransform.position.y < minBodyHeight || upright < minUprightDot)
        {
            AddReward(fallPenalty);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;

        float t = Mathf.Sin(Time.time * 2f);

        actions[0] = 0.3f * t;             // Left Hip Roll
        actions[1] = t;                    // Left Hip Pitch
        actions[2] = Mathf.Max(0f, t);     // Left Knee Pitch
        actions[3] = -0.5f * t;            // Left Ankle Pitch

        actions[4] = -0.3f * t;            // Right Hip Roll
        actions[5] = -t;                   // Right Hip Pitch
        actions[6] = Mathf.Max(0f, -t);    // Right Knee Pitch
        actions[7] = 0.5f * t;             // Right Ankle Pitch
    }

    private float ConvertActionToAngle(float actionValue, Vector2 limits)
    {
        float normalised = (actionValue + 1f) * 0.5f;
        return Mathf.Lerp(limits.x, limits.y, normalised);
    }

    private void SetJointTarget(ArticulationBody joint, float targetAngle)
    {
        if (joint == null)
        {
            return;
        }

        ArticulationDrive drive = joint.xDrive;

        drive.target = targetAngle;
        drive.stiffness = 500f;
        drive.damping = 50f;
        drive.forceLimit = 1000f;

        joint.xDrive = drive;
    }
}