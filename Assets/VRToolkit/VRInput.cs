using System;
using UnityEngine;

public class VRInput
{
    static private VRInput[] hand = new VRInput[2];
    static public VRInput Hand(int index) { if (hand[index] == null) hand[index] = new VRInput(index == 0); return hand[index]; }
    static public VRInput Left() { return Hand(0); }
    static public VRInput Right() { return Hand(1); }

    public Button Trigger { get; private set; }
    public Button Grip { get; private set; }
    public Button Touchpad { get; private set; }
    public Button TouchpadButton { get; private set; }
    public Button Menu { get; private set; }

    public Vector3 GripPosition() { return input.GripPosition(); }
    public Quaternion GripRotation() { return input.GripRotation(); }

    public Vector3 PointingPosition() { return input.PointingPosition(); }
    public Quaternion PointingRotation() { return input.PointingRotation(); }

    public Vector2 TouchpadPosition() { return input.TouchpadPosition(); }
    public Vector2 ThumbstickPosition() { return input.ThumbstickPosition(); }
    public Vector2 ThumbstickPosition(float deadZone)
    {
        Vector2 pos = input.ThumbstickPosition();
        if (pos.sqrMagnitude < deadZone * deadZone)
            return Vector2.zero;
        return pos.normalized * Mathf.Min(pos.magnitude - deadZone / (1.0f-deadZone), 1.0f);
    }


    private MRInputRaw input;

    public bool IsLeft() { return input.IsLeft(); }
    public bool Active() { return input.Active; }

    VRInput(bool left)
    {
        Trigger = new Button();
        Grip = new Button();
        Touchpad = new Button();
        TouchpadButton = new Button();
        Menu = new Button();
        input = left ? MRInputRaw.Left() : MRInputRaw.Right();
        input.OnUpdate += Input_OnUpdate;
        Input_OnUpdate();
    }

    private void Input_OnUpdate()
    {
        Trigger.Update(input.SelectAmount());
        Grip.Update(input.GripIsDown() ? 1 : 0);
        Touchpad.Update(input.TouchpadTouched() ? 1 : 0);
        TouchpadButton.Update(input.TouchpadIsDown() ? 1 : 0);
        Menu.Update(input.MenuIsDown() ? 1 : 0);
    }

    // =^..^=

    public class Button
    {
        private const float defaultMinHoldTime = 0.3f;
        private const float defaultMaxClickTime = 0.3f;
        private const float defaultMaxDoubleClickTime = 0.4f;
        public float Value { get; private set; }
        public float MinHoldTime = defaultMinHoldTime;
        public float MaxClickTime = defaultMaxClickTime;
        public float MaxDoubleClickTime = defaultMaxDoubleClickTime;

        private float downDuration;
        private float upDuration;

        public bool IsDown() { UpdateTime(); return Value > 0.0f; }
        public bool IsHeld() { UpdateTime(); return IsDown() && downDuration >= MinHoldTime; }
        public float GetDownDuration() { UpdateTime(); return IsDown() ? downDuration : 0.0f; }
        public float GetUpDuration() { UpdateTime(); return !IsDown() ? upDuration : 0.0f; }
        public bool WasPressed() { UpdateTime(); return IsDown() && downDuration == 0; }
        public bool WasReleased() { UpdateTime(); return !IsDown() && upDuration == 0; }
        public bool WasClicked() { UpdateTime(); return !IsDown() && downDuration <= MaxClickTime; }
        public bool WasDoubleClicked() { UpdateTime(); return IsDown() && downDuration == 0 && upDuration <= MaxDoubleClickTime; }

        public event Action OnDown;
        public event Action OnUp;
        public event Action OnClicked;
        public event Action OnDoubleClicked;
        public event Action OnHeldStarted;
        public event Action OnHeld;

        private float prevUpdateTime;
        private float newValue;

        private void UpdateTime()
        {
            if (prevUpdateTime == Time.time)
                return;
            float deltaTime = Time.time - prevUpdateTime;
            prevUpdateTime = Time.time;
            if (IsDown())
            {
                bool wasHeld = IsHeld();
                downDuration += deltaTime;
                Value = newValue;
                if (!IsDown())
                {
                    if (OnUp != null)
                        OnUp();
                    if (OnClicked != null && WasClicked())
                        OnClicked();
                    upDuration = 0;
                }
                if (IsHeld())
                {
                    if (!wasHeld && OnHeldStarted != null)
                        OnHeldStarted();
                    if (OnHeld != null)
                        OnHeld();
                }
            }
            else
            {
                upDuration += deltaTime;
                Value = newValue;
                if (IsDown())
                {
                    if (OnDown != null)
                        OnDown();
                    if (OnDoubleClicked != null && WasDoubleClicked())
                        OnDoubleClicked();
                    downDuration = 0;
                }
            }
        }

        public void Update(float value)
        {
            newValue = value;
            UpdateTime();
        }
    }
}
