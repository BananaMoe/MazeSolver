/**
 * This code is based on the Maze Generation Algorithm implementation for Unity found at:
 * https://github.com/EvanatorM/Maze-Generation-Algorithm-Unity
 * 
 * Original Author: EvanatorM
 * Accessed on: 02.12.2023
 * 
 * Modifications made:
 * - Added a `playerPrefab` field to instantiate a player within the maze.
 * - Added `HasGeneratedMaze` property to track if the maze has been generated.
 * - Added `OnMazeGenerated` and `OnPathHighlighted` events.
 * - Added `GenerateNewMaze` method to allow generation of new mazes with a specified size.
 * - Modified `GenerateMaze` method to clear existing nodes before generating new ones.
 * - Added `FindPath` method to determine the shortest path from the start to the goal node using Bredth-first search.
 * - Added `HighlightPath` and `HighlightPathFromPlayer` methods for visualizing paths.
 * - Added neighbor tracking to nodes during maze generation for pathfinding purposes.
 * - Adjusted camera FOV dynamically based on the size of the maze to ensure the maze fits within the camera view.
 * - Added methods for player management (`PlayerExists`, `RemovePlayer`) and maze clearing (`ClearMaze`). 
 * - Added theme packs for Maze Generation
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] MazeNode nodePrefab;
    [SerializeField] MazeNode nodePrefabCandy;
    [SerializeField] Vector2Int mazeSize;
    [SerializeField] float nodeSize;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerPrefabCandy;
    private GameObject playerInstance;
    private List<MazeNode> nodes = new List<MazeNode>();
    public int mazeTheme = 0;

    public bool HasGeneratedMaze { get; private set; }
    public event Action OnMazeGenerated;
    public event Action OnPathHighlighted;
    public bool IsHighlightingPath { get; private set; }

    public void GenerateNewMaze(int size)
    {
        mazeSize = new Vector2Int(size, size);
        StartCoroutine(GenerateMaze(mazeSize));
    }

    IEnumerator GenerateMaze(Vector2Int size)
    {
        nodes.Clear();

        AdjustCameraFOV(Camera.main, mazeSize, nodeSize);

        // Create nodes
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3 nodePos = new Vector3(x - (size.x / 2f), 0, y - (size.y / 2f));
                if(mazeTheme == 0)
                {
                    MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                    nodes.Add(newNode);
                }
                else
                {
                    MazeNode newNode = Instantiate(nodePrefabCandy, nodePos, Quaternion.identity, transform);
                    nodes.Add(newNode);
                }

                

                yield return null;
            }
        }

        List<MazeNode> currentPath = new List<MazeNode>();
        List<MazeNode> completedNodes = new List<MazeNode>();

        // Choose starting node
        currentPath.Add(nodes[UnityEngine.Random.Range(0, nodes.Count)]);
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
                int chosenDirection = UnityEngine.Random.Range(0, possibleDirections.Count);
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

        HasGeneratedMaze = true;
        OnMazeGenerated?.Invoke();
        yield break;
    }


    public void PlacePlayer()
    {
        if (!HasGeneratedMaze)
            return;

        if (playerInstance != null)
            Destroy(playerInstance);

        if (mazeTheme == 0)
        {
            playerInstance = Instantiate(playerPrefab, nodes[0].transform.position, Quaternion.identity);
            playerInstance.GetComponent<PlayerController>().SetCurrentNode(nodes[0]);
        }
        else if (mazeTheme == 1)
        {
            playerInstance = Instantiate(playerPrefabCandy, nodes[0].transform.position, Quaternion.identity);
            playerInstance.GetComponent<PlayerController>().SetCurrentNode(nodes[0]);
        }
        
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

    IEnumerator HighlightPath(List<MazeNode> path)
    {

        IsHighlightingPath = true;

        MazeNode lastNode = null;
        if (path.Count > 0)
        {
            lastNode = path[path.Count - 1];
        }

        foreach (MazeNode node in path)
        {
            node.SetState(NodeState.Highlighted);
            yield return new WaitForSeconds(0.1f);
            node.SetState(NodeState.Completed);
        }
        OnPathHighlighted?.Invoke();
        IsHighlightingPath = false;
    }

    public void HighlightPathFromPlayer()
    {
        if (!HasGeneratedMaze || playerInstance == null)
            return;

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController == null)
            return;

        MazeNode playerNode = playerController.CurrentNode;
        if (playerNode == null)
            return;

        List<MazeNode> path = FindPath(playerNode, nodes[nodes.Count - 1]);

        if (path.Count > 1)
            path.RemoveAt(path.Count - 1);

        StartCoroutine(HighlightPath(path));
    }


    List<MazeNode> GetNeighbors(MazeNode node)
    {
        return node.neighbors;
    }

    private void AdjustCameraFOV(Camera camera, Vector2Int mazeSize, float nodeSize)
    {
        // Determine the scale of the maze compared to a 20x20 maze.
        float scale = 20f / Mathf.Max(mazeSize.x, mazeSize.y);

        // Calculate the base distance for a 20x20 maze.
        float baseDistance = 200f;

        // Calculate the new distance required to make the smaller maze appear the same size.
        float distance = baseDistance / scale;

        // Calculate a subtle FOV modifier that slightly increases FOV for smaller mazes.
        // The '+1' ensures that we don't have a logarithm of zero and the modifier is always positive.
        float fovModifier = Mathf.Log(mazeSize.x * mazeSize.y + 1, 2);

        // Calculate the required FOV to achieve the new distance and apply the modifier.
        float fov = 2f * Mathf.Atan(0.5f * mazeSize.y * nodeSize / (distance + fovModifier)) * Mathf.Rad2Deg;

        float additionalFOV = Mathf.Lerp(3f, 1f, (mazeSize.y - 6f) / (20f - 6f));

        // Set the camera's FOV to this new value.
        camera.fieldOfView = fov + additionalFOV;

        // Adjust the camera's position to account for the new FOV.
        Vector3 cameraPosition = camera.transform.position;
        
        // Raise the camera slightly for smaller mazes.
        cameraPosition.y = distance + fovModifier; 
        
        camera.transform.position = cameraPosition;
    }

    public bool PlayerExists()
    {
        if (playerInstance != null)
            return true;
        else
            return false;
    }

    public void RemovePlayer()
    {
        if (playerInstance != null)
            Destroy(playerInstance.gameObject);
    }

    public void ClearMaze()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
        foreach (MazeNode node in nodes)
        {
            Destroy(node.gameObject);
        }
        nodes.Clear();
    }

    public void SetThemeToJungle()
    {
        mazeTheme = 0; 
    }

    public void SetThemeToCandy()
    {
        mazeTheme = 1;
    }
}
