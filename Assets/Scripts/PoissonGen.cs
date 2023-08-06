using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class PoissonGen : MonoBehaviour
{
    [System.Serializable]
    public enum RadiusMode { Fixed, Dynamic };

    [HideInInspector]
    public RadiusMode radiusMode = RadiusMode.Fixed;
    [HideInInspector]
    public float radius;
    [HideInInspector]
    public float radiusMin;
    [HideInInspector]
    public float radiusMax;
    [HideInInspector]
    public float displayMultiplier;
    [HideInInspector]
    public Vector2 regionSize;
    [HideInInspector]
    public int rejectionSamples;
    [HideInInspector]
    public int seed = 1123;
    [HideInInspector]
    public List<Vector2> spawnPoints;
    [HideInInspector]
    public bool autoRun = false;
    public AnimationCurve curve;
    // Add auto run bool
    // Add editor support like a manual run button
    // Add variable radius support, according to a probability curve using an animation curve
    // Add an ability to assign prefabs depending on the chosen radius range

    public List<Vector2> points;

    public List<Vector2> GetPoints()
    {
        List<Vector2> newPoints;
        List<Vector2> spawnPointCopy = null;

        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            spawnPointCopy = new List<Vector2>(spawnPoints);
        }

        newPoints = PoissonDiskSampling.GeneratePoints(radius, regionSize, rejectionSamples, seed, spawnPointCopy);
        for (int i = 0; i < newPoints.Count; i++)
        {
            Vector2 temp = newPoints[i];
            temp -= regionSize / 2;
            newPoints[i] = temp;
        }

        return newPoints;
    }

    void OnValidate()
    {
        int calcEstimate = Mathf.CeilToInt(regionSize.x * regionSize.y / radius * rejectionSamples);
        if (calcEstimate > 100000)
        {
            Debug.LogWarning("Too many calculations: safety engaged");
        }
        else
        {
            // points = GetPoints();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(regionSize.x, 0, regionSize.y));

        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), radius * displayMultiplier / 2);
            }
        }
    }
}
