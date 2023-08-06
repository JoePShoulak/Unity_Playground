using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoissonGen))]
public class PoissonGenEditor : Editor
{
    PoissonGen.RadiusMode radiusMode = PoissonGen.RadiusMode.Fixed;
    float radius = 1f;
    float radiusMin = 0.5f;
    float radiusMax = 1.5f;
    float displayMultiplier = 1f;

    Vector2 regionSize = Vector2.one * 5;
    int rejectionSamples = 30;
    int seed = 1123;
    List<Vector2> spawnPoints;

    bool autoRun = false;

    public float GetRadius()
    {
        radius = EditorGUILayout.FloatField("Radius", radius);
        radius = Mathf.Max(radius, 0.01f);
        return radius;
    }

    public float[] GetRadiusMinMax()
    {
        radiusMin = EditorGUILayout.FloatField("RadiusMin", radiusMin);
        radiusMax = EditorGUILayout.FloatField("RadiusMax", radiusMax);

        radiusMin = Mathf.Max(radiusMin, 0.01f);
        radiusMax = Mathf.Max(radiusMax, radiusMin);

        return new float[] { radiusMin, radiusMax };
    }

    public PoissonGen.RadiusMode GetRadiusMode(PoissonGen script)
    {
        radiusMode = (PoissonGen.RadiusMode)EditorGUILayout.EnumPopup("Radius Mode", radiusMode);
        return radiusMode;
    }

    public float GetDisplayMultiplier()
    {
        displayMultiplier = EditorGUILayout.FloatField("Display Multiplier", displayMultiplier);
        displayMultiplier = Mathf.Clamp(displayMultiplier, 0.001f, 1000);

        return displayMultiplier;
    }

    public Vector2 GetRegionSize()
    {
        regionSize = EditorGUILayout.Vector2Field("Region Size", regionSize);
        regionSize.x = Mathf.Max(regionSize.x, 0f);
        regionSize.y = Mathf.Max(regionSize.y, 0f);

        return regionSize;
    }

    public int GetSeed()
    {
        seed = EditorGUILayout.IntField("Seed", seed);
        return seed;
    }

    private SerializedProperty spawnPointsProperty;

    private void OnEnable()
    {
        // Find the serialized property for the spawnPoints
        spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
    }

    public void GetAndSetSpawnPoints(PoissonGen script)
    {
        EditorGUILayout.PropertyField(spawnPointsProperty, true);
        spawnPoints = script.spawnPoints;
        if (spawnPoints != null)
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Vector2 temp = spawnPoints[i];
                temp.x = Mathf.Clamp(spawnPoints[i].x, 0f, regionSize.x);
                temp.y = Mathf.Clamp(spawnPoints[i].y, 0f, regionSize.y);
                spawnPoints[i] = temp;
            }
        }
        if (spawnPoints != null && spawnPoints.Count == 0)
        {
            spawnPoints = null;
        }
        script.spawnPoints = spawnPoints;
        serializedObject.ApplyModifiedProperties();
    }

    public bool GetAutoRun()
    {
        autoRun = EditorGUILayout.Toggle("Auto Run", autoRun);
        return autoRun;
    }

    public void Draw(PoissonGen script)
    {
        script.points = script.GetPoints();
        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI()
    {
        PoissonGen script = (PoissonGen)target;
        serializedObject.Update();
        DrawDefaultInspector();

        EditorGUILayout.LabelField("Radius Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        script.radiusMode = GetRadiusMode(script);
        bool fixedMode = (script.radiusMode == PoissonGen.RadiusMode.Fixed);

        EditorGUI.BeginDisabledGroup(!fixedMode);
        script.radius = GetRadius();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(fixedMode);
        float[] radiusMinMax = GetRadiusMinMax();
        script.radiusMin = radiusMinMax[0];
        script.radiusMax = radiusMinMax[1];
        EditorGUI.EndDisabledGroup();

        script.displayMultiplier = GetDisplayMultiplier();

        EditorGUILayout.EndVertical();

        script.regionSize = GetRegionSize();

        rejectionSamples = EditorGUILayout.IntSlider("Rejection Samples", rejectionSamples, 1, 50);
        script.rejectionSamples = rejectionSamples;

        script.seed = GetSeed();

        GetAndSetSpawnPoints(script);

        script.autoRun = GetAutoRun();

        if (GUILayout.Button("Run"))
        {
            Draw(script);
        }

        if (script.autoRun)
        {
            Draw(script);
        }
    }
}
