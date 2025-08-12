// DroneComponents.cs

using Unity.Entities;

/// <summary>
/// A tag component to identify entities that are drones.
/// </summary>
public struct DroneTag : IComponentData { }

/// <summary>
/// A component that holds the state for an entity following a spline.
/// </summary>
public struct SplineFollower : IComponentData
{
    /// <summary>
    /// The index of the spline to follow within the BlobArray of splines.
    /// </summary>
    public int SplineIndex;

    /// <summary>
    /// The speed at which the entity moves along the spline (in units per second).
    /// </summary>
    public float Speed;

    /// <summary>
    /// The current normalized progress along the spline, from 0 (start) to 1 (end).
    /// </summary>
    public float Progress;
}