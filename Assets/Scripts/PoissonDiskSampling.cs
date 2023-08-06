using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiskSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30, int seed = 1123, List<Vector2> spawnPoints = null)
    {
        // Setup stuff
        Random.InitState(seed);
        float cellSize = radius / Mathf.Sqrt(2);
        Vector2 offset = sampleRegionSize / 2;

        // Make our grid for storing and searching for points
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        // Make our list of points
        List<Vector2> points = new List<Vector2>();

        // This is for customization in the editor
        // But the spawn points are the subset of points we're still trying to spawn from
        if (spawnPoints == null)
        {
            spawnPoints = new List<Vector2>();
            spawnPoints.Add(sampleRegionSize / 2);
        }

        // While we still have valid spawn points...
        while (spawnPoints.Count > 0)
        {
            // Choose a random spawn point from our list
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            // Find the coordinates for that point, and get ready to try to place a new one
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            // As many times as we've specified in our variable for it...
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                // Generate a new point around the current one, between r and 2r away
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + dir * Random.Range(radius, 2 * radius);

                // If it's valid...
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate); // Add to our list of points
                    spawnPoints.Add(candidate); // Add it to where we can spawn from, too
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count; // Update the grid
                    candidateAccepted = true; // Show we found something so we can use this point again

                    break;
                }
            }

            // If we couldn't find any new points from here
            if (!candidateAccepted)
            {
                // remove "here" as an option
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        // If the candidate is within out region in the first place...
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            // Get our location in terms of cells
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            // Identify our search grid, making sure to only include cells that exist
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            // Begin searching through the cells...
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;

                    // If we found a point...
                    if (pointIndex != -1)
                    {
                        // Get the distance
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;

                        // If it's too close...
                        if (sqrDst < radius * radius)
                        {
                            // It's not a valid candidate
                            return false;
                        }
                    }
                }
            }
            // There's no point conflict
            return true;
        }
        // It was off the grid
        return false;
    }
}