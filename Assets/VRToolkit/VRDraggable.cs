using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRDraggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        VRHand.Hands[VRUIInputModule.Instance.LastInvokedID].StartDrag(this);
    }

    Vector3 localCursorPos;
    Vector3 posOffset;
    Quaternion rotOffset;
    float startOffset;

    public void InitDrag(Vector3 cursorPos, Transform hand)
    {
        localCursorPos = transform.InverseTransformPoint(cursorPos);
        posOffset = hand.InverseTransformPoint(transform.position);
        rotOffset = Quaternion.Inverse(hand.rotation) * transform.rotation;

        startOffset = Camera.main.transform.InverseTransformPoint(hand.position).z;
    }

    public void UpdateDrag(Transform hand)
    {
        Vector3 destPos = hand.TransformPoint(posOffset);
        float newOffset = Camera.main.transform.InverseTransformPoint(hand.position).z;
        Vector3 localOffset = Camera.main.transform.InverseTransformPoint(destPos);
        localOffset.z *= newOffset / startOffset;
        destPos = Camera.main.transform.TransformPoint(localOffset);

        transform.position = Vector3.Lerp(transform.position, destPos, 0.1f);
        Quaternion rotDest = hand.rotation * rotOffset;
        Vector3 srcAngles = transform.rotation.eulerAngles;
        Vector3 angles = rotDest.eulerAngles;
        rotDest = Quaternion.Euler(srcAngles.x, angles.y, srcAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotDest, 0.1f);
    }

    public Vector3 GetCursorPos()
    {
        return transform.TransformPoint(localCursorPos);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BEGIN DRAG");
    }
}
