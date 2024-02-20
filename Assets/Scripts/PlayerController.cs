using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    private MazeNode currentNode;
    private MazeNode targetNode;
    public MazeNode CurrentNode => currentNode;
    public event Action OnReachedGoal;

    private void Update()
    {
        if (targetNode == null && currentNode != null)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h != 0 || v != 0)
            {
                Vector3 inputDirection = new Vector3(h, 0.0f, v).normalized;
                MazeNode nextNode = DetermineNextNode(inputDirection);

                if (nextNode != null)
                {
                    targetNode = nextNode;
                }
            }
        }

        if (targetNode != null)
        {
            MoveTowardsTargetNode();
        }
    }

    public void SetCurrentNode(MazeNode node)
    {
        currentNode = node;
        transform.position = node.transform.position;
    }

    private MazeNode DetermineNextNode(Vector3 direction)
    {
        foreach (var neighbor in currentNode.neighbors)
        {
            Vector3 toNeighbor = (neighbor.transform.position - transform.position).normalized;
            if (Vector3.Dot(toNeighbor, direction) > 0.9f)
            {
                return neighbor;
            }
        }
        return null;
    }

    private void MoveTowardsTargetNode()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetNode.transform.position) < 0.1f)
        {
            SetCurrentNode(targetNode);
            targetNode = null;
        }
        CheckIfAtGoal();
    }

    private void CheckIfAtGoal()
    {
        if (currentNode != null && currentNode.State == NodeState.Goal)
        {
            OnReachedGoal?.Invoke();
            HandleReachedGoal();
        }
    }
    public void HandleReachedGoal()
    {
        Destroy(this.gameObject);
    }

}
