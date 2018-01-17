using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class MRInputRaw
{
    static private MRInputRaw[] hand = new MRInputRaw[2];
    static public MRInputRaw Hand(int index) { if (hand[index] == null) hand[index] = new MRInputRaw(index==0); return hand[index]; }
    static public MRInputRaw Left() { return Hand(0); }
    static public MRInputRaw Right() { return Hand(1); }

    public bool IsLeft() { return handedness == InteractionSourceHandedness.Left; }
    public bool Active { get; private set; }

    public bool GripIsDown() { return Active && source.grasped; }
    public float SelectAmount() { return Active && source.selectPressed ? source.selectPressedAmount : 0.0f; }
    public bool MenuIsDown() { return Active && source.menuPressed; }
    public bool TouchpadTouched() { return Active && source.touchpadTouched; }
    public bool TouchpadIsDown() { return Active && source.touchpadPressed; }
    public bool ThumbstickIsDown() { return Active && source.thumbstickPressed; }
    public Vector2 TouchpadPosition() { return Active && source.touchpadTouched ? source.touchpadPosition : Vector2.zero; }
    public Vector2 ThumbstickPosition() { return Active ? source.thumbstickPosition : Vector2.zero; }
    public Vector3 GripPosition()
    {
        Vector3 position;
        if (!Active || !source.sourcePose.TryGetPosition(out position))
            return Vector3.zero;
        return position;
    }
    public Quaternion GripRotation()
    {
        Quaternion rotation;
        if (!Active || !source.sourcePose.TryGetRotation(out rotation))
            return Quaternion.identity;
        return rotation;
    }
    public Vector3 PointingPosition()
    {
        Vector3 position;
        if (!Active || !source.sourcePose.TryGetPosition(out position, InteractionSourceNode.Pointer))
            return Vector3.zero;
        return position;
    }
    public Quaternion PointingRotation()
    {
        Quaternion rotation;
        if (!Active || !source.sourcePose.TryGetRotation(out rotation, InteractionSourceNode.Pointer))
            return Quaternion.identity;
        return rotation;
    }

    public event Action OnUpdate;


    private InteractionSourceHandedness handedness;
    private InteractionSourceState source;

    private MRInputRaw(bool left)
    {
        Active = false;
        handedness = left ? InteractionSourceHandedness.Left : InteractionSourceHandedness.Right;

        if (XRDevice.isPresent)
        {
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        }
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
    {
        if (obj.state.source.handedness == handedness)
        {
            source = obj.state;
            Active = true;
            if (OnUpdate != null)
                OnUpdate();
        }
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
    {
        if (obj.state.source.handedness == handedness)
        {
            Active = false;
            if (OnUpdate != null)
                OnUpdate();
        }
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
    {
        if (obj.state.source.handedness == handedness)
        {
            source = obj.state;
            Active = true;
            if (OnUpdate != null)
                OnUpdate();
        }
    }
}
