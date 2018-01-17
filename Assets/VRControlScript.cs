using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class VRControlScript : MonoBehaviour
{
    public Transform RightHand;
    public Transform LeftHand;

    private GameObject _validationCube;

    private MainScript _mainScript;
    private bool _wasHeld;

    private void Start()
    {
        _mainScript = GetComponent<MainScript>();
        _validationCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _validationCube.name = "ValidationCube";
        Destroy(_validationCube.GetComponent<BoxCollider>());
        _validationCube.SetActive(false);
        _positionTarget = new GameObject("PositionTarget").transform;
        _positionTarget.SetParent(LeftHand);
    }

    private float _CloseMomentum;
    private Transform _positionTarget;

    private void HandleOpening()
    {
        
        bool rightTriggerHeld = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        if (rightTriggerHeld && !_wasHeld)
        {
            transform.position = RightHand.position;
            transform.rotation = RightHand.rotation;
        }
        if (rightTriggerHeld)
        {
            
            _mainScript.BoxHandle.position = RightHand.position;
            _CloseMomentum = 0;
        }
        else
        {
            _CloseMomentum += .01f;
            _mainScript.BoxHandle.localPosition = Vector3.Lerp(_mainScript.BoxHandle.localPosition, Vector3.zero, _CloseMomentum);
        }
        _wasHeld = rightTriggerHeld;
    }

    private Transform _selectedObject;
    
    void Update ()
    {
        HandleOpening();
        _validationCube.SetActive(false);
        if(_selectedObject == null)
        {
            HandlePotentialSelection();
        }
        else
        {
            HandleSelectedObject();
        }
    }

    private void HandlePotentialSelection()
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(LeftHand.position, LeftHand.forward, out hitInfo);
        if(hit)
        {
            DisplayValidationCube(hitInfo.point);
            bool leftTriggerHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
            if (leftTriggerHeld)
            {
                SelectObject(hitInfo.transform);
            }
        }
    }

    private void HandleSelectedObject()
    {
        bool leftTriggerHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
        if(leftTriggerHeld)
        {
            _selectedObject.transform.position = _positionTarget.position;
        }
        else
        {
            _selectedObject = null;
        }
    }

    private void SelectObject(Transform transform)
    {
        _positionTarget.position = transform.position;
        _positionTarget.rotation = transform.rotation;
        _selectedObject = transform;
    }

    private void DisplayValidationCube(Vector3 end)
    {
        Vector3 start = LeftHand.position;
        _validationCube.transform.position = (start + end) / 2;
        _validationCube.transform.LookAt(end);
        _validationCube.transform.localScale = new Vector3(0.005f, 0.005f, (start - end).magnitude);
        _validationCube.SetActive(true);
    }
}
