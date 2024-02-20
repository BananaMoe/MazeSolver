using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingMazeGenerator : MonoBehaviour
{
    [SerializeField] MazeNode nodePrefab;
    [SerializeField] Vector2Int mazeSize;
    [SerializeField] float nodeSize;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance;

    private void Start()
    {
        //GenerateMazeInstant(mazeSize);
        StartCoroutine(GenerateMaze(mazeSize));
    }


    IEnumerator GenerateMaze(Vector2Int size)
    {
        List<MazeNode> nodes = new List<MazeNode>();

        // Create nodes
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(newNode);

                yield return null;
            }
        }

        List<MazeNode> currentPath = new List<MazeNode>();
        List<MazeNode> completedNodes = new List<MazeNode>();

        // Choose starting node
        currentPath.Add(nodes[Random.Range(0, nodes.Count)]);
        currentPath[0].SetState(NodeState.Current);

        while (completedNodes.Count < nodes.Count)
        {
            // Check nodes next to the current node
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirections = new List<int>();

            int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if (currentNodeX < size.x - 1)
            {
                // Check node to the right of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1);
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }
            if (currentNodeX > 0)
            {
                // Check node to the left of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }
            if (currentNodeY < size.y - 1)
            {
                // Check node above the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }
            if (currentNodeY > 0)
            {
                // Check node below the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }

            // Choose next node
            if (possibleDirections.Count > 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                MazeNode chosenNode = nodes[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                if (!chosenNode.neighbors.Contains(currentPath[currentPath.Count - 1]))
                {
                    chosenNode.neighbors.Add(currentPath[currentPath.Count - 1]);
                }
                if (!currentPath[currentPath.Count - 1].neighbors.Contains(chosenNode))
                {
                    currentPath[currentPath.Count - 1].neighbors.Add(chosenNode);
                }

                currentPath.Add(chosenNode);
                chosenNode.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }

            yield return new WaitForSeconds(0.01f);
        }

        nodes[0].SetState(NodeState.Start);
        nodes[nodes.Count - 1].SetState(NodeState.Goal);

        List<MazeNode> path = FindPath(nodes[0], nodes[nodes.Count - 1]);

        if (playerInstance != null) Destroy(playerInstance);
        playerInstance = Instantiate(playerPrefab, nodes[0].transform.position, Quaternion.identity);
        playerInstance.GetComponent<PlayerController>().SetCurrentNode(nodes[0]);
    }

    private List<MazeNode> FindPath(MazeNode startNode, MazeNode goalNode)
    {
        Queue<MazeNode> frontier = new Queue<MazeNode>();
        frontier.Enqueue(startNode);

        Dictionary<MazeNode, MazeNode> cameFrom = new Dictionary<MazeNode, MazeNode>();
        cameFrom[startNode] = null;

        while (frontier.Count > 0)
        {
            MazeNode current = frontier.Dequeue();

            if (current == goalNode)
            {
                break;
            }

            foreach (var next in GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<MazeNode> path = new List<MazeNode>();
        MazeNode temp = goalNode;
        while (temp != startNode)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Add(startNode);
        path.Reverse();

        return path;
    }

    List<MazeNode> GetNeighbors(MazeNode node)
    {
        return node.neighbors;
    }

    public MazeNode GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        // Calculate the local position of the point relative to the maze's origin
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        // Assuming the maze is centered on the origin and nodes are 1 unit apart
        int xIndex = Mathf.RoundToInt(localPos.x + mazeSize.x / 2);
        int zIndex = Mathf.RoundToInt(localPos.z + mazeSize.y / 2);

        // Check if the indices are within the bounds of the maze
        if (xIndex < 0 || xIndex >= mazeSize.x || zIndex < 0 || zIndex >= mazeSize.y)
        {
            return null; // Out of bounds
        }

        // Convert 2D grid indices to 1D list index
        int nodeIndex = xIndex * mazeSize.y + zIndex;

        /*

        // Check if the index is within the bounds of the node list
        if (nodeIndex >= 0 && nodeIndex < nodes.Count)
        {
            return nodes[nodeIndex];
        }

        */

        return null;
    }


}
