using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(VRUIInputModule))]
public class VRUIInputModule_Editor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ((VRUIInputModule)target).DrawInspector();
    }
}
#endif

public class VRUIInputModule : StandaloneInputModule
{
    public static VRUIInputModule Instance;

    private struct ControllerInput
    {
        public bool IsConnected;
        public bool IsButtonPressed;
        public PointerEventData.FramePressState ButtonStatus;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    /// <summary>
    /// The ID of the controller that was last invoked.
    /// This allows Unity UI buttons to query which controller was used
    /// </summary>
    public int LastInvokedID { get; private set; }

    /// <summary>
    /// Camera used for raycasting
    /// </summary>
    public Camera RaycastCamera { get; private set; }

    /// <summary>
    /// last position of the pointer for the input in 3D
    /// </summary>
    private readonly Dictionary<int, Vector3> lastMousePoint3D = new Dictionary<int, Vector3>();

    /// <summary>
    /// last input data for pointer
    /// </summary>
    private readonly Dictionary<int, ControllerInput> currentControllerReadings = new Dictionary<int, ControllerInput>();

    protected override void Awake()
    {
        base.Awake();

        RaycastCamera = gameObject.AddComponent<Camera>();
        RaycastCamera.cullingMask = 0;
        RaycastCamera.clearFlags = CameraClearFlags.Nothing;
        RaycastCamera.stereoTargetEye = StereoTargetEyeMask.None;
        RaycastCamera.nearClipPlane = 0.01f;

        gameObject.AddComponent<PhysicsRaycaster>();

        Debug.Assert(Instance == null, "There should only be one EventSystem and one VRInputModule");
        Instance = this;

//        OnValidate();
    }

    /// <summary>
    /// Draw the custom inspector visuals.
    /// Specifically, draw the hand cameras
    /// </summary>
    internal void DrawInspector()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            foreach (var pair in currentControllerReadings)
            {
                if (!pair.Value.IsConnected)
                {
                    continue;
                }
                SetCameraTransform(pair.Value);

                RenderTexture tempTexture = RenderTexture.GetTemporary(200, 200);
                RaycastCamera.cullingMask = -1;
                RaycastCamera.clearFlags = CameraClearFlags.SolidColor;
                RaycastCamera.targetTexture = tempTexture;
                RaycastCamera.Render();

                GUILayout.Label(pair.Key.ToString() + ":");
                GUILayout.Label(tempTexture);

                RaycastCamera.cullingMask = 0;
                RaycastCamera.clearFlags = CameraClearFlags.Nothing;
                RaycastCamera.targetTexture = null;

                tempTexture.Release();
            }
        }
#endif
    }

    /// <summary>
    /// Update the controller status and position for use in the Unity UI Event System
    /// </summary>
    public void SetControllerStatus(int id, bool buttonDown, Vector3 position, Quaternion rotation)
    {
        PointerEventData.FramePressState newStatus;
        if (currentControllerReadings.ContainsKey(id))
        {
            if (buttonDown && !currentControllerReadings[id].IsButtonPressed)
            {
                newStatus = PointerEventData.FramePressState.Pressed;
            }
            else if (!buttonDown && currentControllerReadings[id].IsButtonPressed)
            {
                newStatus = PointerEventData.FramePressState.Released;
            }
            else
            {
                newStatus = PointerEventData.FramePressState.NotChanged;
            }
        }
        else
        {
            newStatus = buttonDown ? PointerEventData.FramePressState.Pressed : PointerEventData.FramePressState.NotChanged;
        }

        currentControllerReadings[id] = new ControllerInput()
        {
            IsConnected = true,
            IsButtonPressed = buttonDown,
            ButtonStatus = newStatus,
            Position = position,
            Rotation = rotation,
        };
    }

    /// <summary>
    /// Remove the controller from execution after its disconnected
    /// </summary>
    public void ControllerDisconnected(int id)
    {
        PointerEventData.FramePressState newStatus;
        if (currentControllerReadings.ContainsKey(id) && currentControllerReadings[id].IsButtonPressed)
        {
            newStatus = PointerEventData.FramePressState.Released;
        }
        else
        {
            newStatus = PointerEventData.FramePressState.NotChanged;
        }

        currentControllerReadings[id] = new ControllerInput()
        {
            IsConnected = false,
            IsButtonPressed = false,
            ButtonStatus = newStatus,
            Position = Vector3.zero,
            Rotation = Quaternion.identity,
        };
    }


    /// <summary>
    /// Set the position and rotation of the fake input camera to the position of the input poining ray
    /// </summary>
    private void SetCameraTransform(ControllerInput reading)
    {
        Debug.Assert(reading.IsConnected, "Need transform to be able to SetCameraTransform", this);
        RaycastCamera.transform.position = reading.Position;
        RaycastCamera.transform.rotation = reading.Rotation;
    }

    /// <summary>
    /// Override StandaloneInputModule.cs Process function() to add VR input and disable mouse input
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/StandaloneInputModule.cs
    /// </summary>
    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent)
            {
                usedEvent |= SendMoveEventToSelectedObject();
            }

            if (!usedEvent)
            {
                SendSubmitEventToSelectedObject();
            }
        }

        // Process controllers
        var idList = currentControllerReadings.Keys.ToArray();
        foreach (var id in idList)
        {
            if (currentControllerReadings[id].IsConnected)
            {
                SetCameraTransform(currentControllerReadings[id]);
            }

            // Note: we want to process the events even for invalid controller since they may have button releases
            // This function calls ExecuteHierarchy and will execute the functions inline
            // https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/StandaloneInputModule.cs
            LastInvokedID = id;
            ProcessMouseEvent(id);

            // Clear the event data, since we consumed it
            var data = currentControllerReadings[id];
            data.ButtonStatus = PointerEventData.FramePressState.NotChanged;
            currentControllerReadings[id] = data;
        }
    }

    /// <summary>
    /// Duplicate and modify BaseInputModule.cs FindFirstRaycast() function to fix the bug with multiple canvases
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/BaseInputModule.cs
    /// </summary>
    private RaycastResult FindFirstRaycast_Internal(List<RaycastResult> candidates)
    {
        var modules = candidates.Select(hit => hit.module.gameObject).Distinct();
        var firstPerModule = modules.Select(module => candidates.First(candidate => module == candidate.module.gameObject));
        return firstPerModule.OrderBy(hitGameObject => hitGameObject.distance).FirstOrDefault();
    }

    /// <summary>
    /// Abstract out a portion of the GetMousePointerEventData() function that gathers the data for a pointer
    /// </summary>
    private PointerEventData GetPointerData_Internal(int id)
    {
        PointerEventData pointerData;
        GetPointerData(id, out pointerData, true);

        pointerData.Reset();

        if (!lastMousePoint3D.ContainsKey(id))
        {
            pointerData.position = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
        }
        else
        {
            // Calculate the last "mouse" position
            pointerData.position = (Vector3)RaycastCamera.WorldToScreenPoint(lastMousePoint3D[id]);
        }

        Vector2 pos = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
        pointerData.delta = pos - pointerData.position;
        pointerData.position = pos;
        pointerData.scrollDelta = Vector2.zero;
        pointerData.button = PointerEventData.InputButton.Left;
        return pointerData;
    }

    private readonly PointerInputModule.MouseState m_MouseState = new PointerInputModule.MouseState();

    /// <summary>
    /// Override PointerInputModule.cs function to hack in mouse position and velocity
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/PointerInputModule.cs
    /// </summary>
    protected override MouseState GetMousePointerEventData(int id)
    {
        // Populate the pointer data
        PointerEventData pointerData = GetPointerData_Internal(id);

        // Hack the PressPosition data since the cursor is locked
        if (currentControllerReadings[id].IsButtonPressed && currentControllerReadings[id].ButtonStatus == PointerEventData.FramePressState.NotChanged)
        {
            pointerData.pressPosition += pointerData.delta;
        }

        // Raycast into world
        if (currentControllerReadings[id].IsConnected)
        {
            // Enable the camera to raycast into 3D
            RaycastCamera.cullingMask = -1;

            // RaycastAll: https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/EventSystem.cs
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            var raycast = FindFirstRaycast_Internal(m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            // Set the camera flag back
            RaycastCamera.cullingMask = 0;
        }
        else
        {
            pointerData.pointerCurrentRaycast = new RaycastResult();
        }

        // Get the button state, if applicable
        PointerEventData.FramePressState state = currentControllerReadings[id].ButtonStatus;

        // Set the fake mouse buttons
        m_MouseState.SetButtonState(PointerEventData.InputButton.Left, state, pointerData);
        m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, pointerData);
        m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, pointerData);

        lastMousePoint3D[id] = RaycastCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f));

        return m_MouseState;
    }

    /// <summary>
    /// Force the module to be active all the time, since it doesn't get normal input changes
    /// </summary>
    public override bool ShouldActivateModule()
    {
        return true;
    }

    /// <summary>
    /// Override & modify from PointerInputModule.cs to allow input with locked cursor
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/PointerInputModule.cs
    /// </summary>
    protected override void ProcessMove(PointerEventData pointerEvent)
    {
        var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerEvent, targetGO);
    }

    /// <summary>
    /// Override & modify from PointerInputModule.cs to allow input with locked cursor
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/PointerInputModule.cs
    /// </summary>
    protected override void ProcessDrag(PointerEventData pointerEvent)
    {
        if (!pointerEvent.IsPointerMoving() || pointerEvent.pointerDrag == null)
        {
            return;
        }

        if (!pointerEvent.dragging
            && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
        {
            ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
            pointerEvent.dragging = true;
        }

        // Drag notification
        if (pointerEvent.dragging)
        {
            // Before doing drag we should cancel any pointer down state
            // And clear selection!
            if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
            {
                ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
            }
            ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
        }
    }

    /// <summary>
    /// Duplicate from PointerInputModule.cs since its private there
    /// Unmodified
    /// https://github.com/MattRix/UnityDecompiled/blob/2017.1.0f3/UnityEngine.UI/UnityEngine.EventSystems/PointerInputModule.cs
    /// </summary>
    private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
    {
        if (!useDragThreshold)
        {
            return true;
        }

        return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
    }

}
