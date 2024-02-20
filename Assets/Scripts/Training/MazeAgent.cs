using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MazeAgent : Agent
{
    public TrainingMazeGenerator mazeGenerator;
    public Transform target;
    public float moveSpeed = 1f;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void OnEpisodeBegin()
    {

        transform.localPosition = Vector3.zero;

        //mazeGenerator.GenerateMaze(new Vector2Int(6, 6));

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        Vector3 toTarget = (target.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toTarget);

        /*
        MazeNode currentNode = GetCurrentMazeNode();
        if (currentNode != null)
        {
            for (int i = 0; i < currentNode.walls.Length; i++)
            {
                sensor.AddObservation(currentNode.walls[i] != null ? 1 : 0);
            }
        }
        */

        /*
        MazeNode currentNode = GetCurrentMazeNode();
        if (currentNode != null)
        {
            for (int i = 0; i < currentNode.walls.Length; i++)
            {
                sensor.AddObservation(currentNode.walls[i] != null ? 1 : 0);
            }
        }
        */
    }

    MazeNode GetCurrentMazeNode()
    {
        return mazeGenerator.GetNodeFromWorldPosition(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector2 move = Vector2.zero;
        move.x = actionBuffers.DiscreteActions[0] - 1; 
        move.y = actionBuffers.DiscreteActions[1] - 1;
        transform.localPosition += new Vector3(move.x, 0, move.y) * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.localPosition, target.localPosition) < 1.0f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else
        {
            AddReward(-0.01f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)Input.GetAxisRaw("Horizontal") + 1;
        discreteActionsOut[1] = (int)Input.GetAxisRaw("Vertical") + 1;
    }
}
