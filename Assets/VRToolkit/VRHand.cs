using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class VRHand : MonoBehaviour
{
    static public VRHand[] Hands = new VRHand[2];
    static public VRHand Left() { return Hands[0]; }
    static public VRHand Right() { return Hands[1]; }

    [SerializeField]
    protected bool left;
    [SerializeField]
    protected GameObject cursor;
    [SerializeField]
    protected GameObject controller;

    private VRInput input;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        Hands[left ? 0 : 1] = this;
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        input = left ? VRInput.Left() : VRInput.Right();
    }

    void Update()
    {
        if (input==null || !input.Active())
        {
            controller.SetActive(false);
            cursor.SetActive(false);
            lineRenderer.enabled = false;
            return;
        }

        controller.SetActive(true);

        transform.localPosition = input.GripPosition();
        transform.localRotation = input.GripRotation();

        UpdateTeleportControls();
        if (teleporting)
            UpdateTeleportState();
        else if(draggable == null)
            UpdateIdleState();

        if (draggable)
            UpdateDragState();
    }

    bool teleporting;

    void UpdateIdleState()
    {
        Vector3 pointingStart = transform.parent.TransformPoint(input.PointingPosition());
        Vector3 pointingDir = (transform.parent.rotation * input.PointingRotation()) * Vector3.forward;

        RaycastHit hitInfo;
        bool hit = Physics.Raycast(pointingStart, pointingDir, out hitInfo, 10.0f);

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, pointingStart);
        if (hit)
            lineRenderer.SetPosition(1, hitInfo.point);
        else
            lineRenderer.SetPosition(1, pointingStart + pointingDir * 10.0f);
        lineRenderer.material.color = Color.white;

        cursor.SetActive(hit);
        if (hit)
        {
            cursor.transform.position = hitInfo.point;
            cursor.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
        }

        Quaternion lookRotation = Quaternion.LookRotation(pointingDir);
        if(VRUIInputModule.Instance != null)
        {
            VRUIInputModule.Instance.SetControllerStatus(left ? 0 : 1, input.Trigger.IsDown(), pointingStart, lookRotation);
        }
    }

    bool waitForStickReset = false;

    void UpdateTeleportControls()
    {
        if (teleporting)
            return;

        if (input.ThumbstickPosition().sqrMagnitude < 0.6f * 0.6f)
        {
            if (input.ThumbstickPosition().sqrMagnitude > 0.3f * 0.3f)
            {
                Vector2 inputVal = input.ThumbstickPosition().normalized;
                Vector3 dir = Camera.main.transform.TransformDirection(new Vector3(inputVal.x, 0.0f, inputVal.y));
                Camera.main.transform.parent.position += dir * Time.deltaTime * 0.5f;
            }
            waitForStickReset = false;
            return;
        }

        if (waitForStickReset)
            return;

        float angle = Vector2.SignedAngle(Vector2.up, input.ThumbstickPosition());
        float absAngle = Mathf.Abs(angle);

        if (absAngle < 45)
        {
            teleporting = true;
        }
        else if (absAngle > 135)
        {
            Vector3 dest = Camera.main.transform.TransformPoint(Vector3.forward * -2.0f);
            dest.y = Camera.main.transform.parent.position.y;
            RaycastHit hitInfo;
            if (Physics.Raycast(dest + Vector3.up, -Vector3.up, out hitInfo, 2.0f))
            {
                TeleportTo(hitInfo.point);
                waitForStickReset = true;
            }
        }
        else
        {
            float turn = -Mathf.Sign(angle) * 45;
            waitForStickReset = true;
            Camera.main.transform.parent.rotation = Quaternion.Euler(0.0f, turn, 0.0f) * Camera.main.transform.parent.rotation;
        }
    }

    bool GenerateCurve(Vector3 start, Vector3 dir, ref List<Vector3> points, out RaycastHit hitInfo)
    {
        Vector3 step = dir * 0.5f;
        const float fall = 0.03f;
        const int maxPoints = 40;
        points.Add(start);

        for (int i = 0; i < maxPoints; i++)
        {
            if (Physics.Raycast(start, step.normalized, out hitInfo, step.magnitude))
            {
                points.Add(hitInfo.point);
                return true;
            }
            points.Add(start + step * 0.5f + Vector3.up * fall * 0.25f);
            start += step;
            step.y -= fall;
            points.Add(start);
        }

        hitInfo = new RaycastHit();
        hitInfo.point = start;

        return false;
    }

    void UpdateTeleportState()
    {
        Vector3 pointingStart = transform.parent.TransformPoint(input.PointingPosition());
        Vector3 pointingDir = (transform.parent.rotation * input.PointingRotation()) * (Vector3.forward + Vector3.up * 0.1f).normalized;

        RaycastHit hitInfo;
        List<Vector3> points = new List<Vector3>(20);
        bool hit = GenerateCurve(pointingStart, pointingDir, ref points, out hitInfo);
        bool valid = hit && hitInfo.normal.y > 0.7f;

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth =  0.04f;
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.material.color = valid ? Color.blue : Color.red;

        cursor.SetActive(valid);
        if (hit)
        {
            cursor.transform.position = hitInfo.point;
            cursor.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
        }

        if (input.ThumbstickPosition().sqrMagnitude < 0.5f * 0.5f)
        {
            teleporting = false;
            if(valid)
            {
                TeleportTo(hitInfo.point);
            }
        }
    }

    void TeleportTo(Vector3 dest)
    {
        dest.x -= Camera.main.transform.localPosition.x;
        dest.z -= Camera.main.transform.localPosition.z;
        Camera.main.transform.parent.position = dest;
    }

    VRDraggable draggable;

    public void StartDrag(VRDraggable newDraggable)
    {
        draggable = newDraggable;
        draggable.InitDrag(cursor.transform.position, transform);
    }

    private void UpdateDragState()
    {
        if (!input.Trigger.IsDown())
        {
            draggable = null;
            return;
        }
        draggable.UpdateDrag(transform);
        Vector3 cursorPos = draggable.GetCursorPos();
        cursor.transform.position = cursorPos;

        Vector3 pointingStart = transform.parent.TransformPoint(input.PointingPosition());
        Vector3 pointingDir = (transform.parent.rotation * input.PointingRotation()) * Vector3.forward;

        float length = Vector3.Distance(pointingStart, cursorPos);
        Vector3[] points = new Vector3[10];
        for (int i = 0; i < 10; i++)
        {
            float lerp = (float)i / (10 - 1);
            points[i] = Vector3.Lerp(pointingStart + pointingDir * length * lerp, Vector3.Lerp(pointingStart, cursorPos, lerp), lerp);
        }

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 10;
        lineRenderer.SetPositions(points);
        lineRenderer.material.color = Color.green;
    }
}
