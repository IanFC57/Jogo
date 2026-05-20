using UnityEngine;

public struct MobileTouchSample
{
    public MobileTouchSample(int fingerId, Vector2 position, TouchPhase phase)
    {
        FingerId = fingerId;
        Position = position;
        Phase = phase;
    }

    public int FingerId { get; private set; }
    public Vector2 Position { get; private set; }
    public TouchPhase Phase { get; private set; }
}
