// ImageCue.cs
using UnityEngine;

/// <summary>
/// A simple data structure to hold a texture and the time it should appear.
/// This [System.Serializable] tag allows us to edit it in the Unity Inspector.
/// </summary>
[System.Serializable]
public struct ImageCue
{
    [Tooltip("The Decal Material that will be projected.")]
    public Material imageMaterial; // Changed variable type and name

    [Tooltip("The time in seconds since the performance started when this material will appear.")]
    public float displayTime;
}