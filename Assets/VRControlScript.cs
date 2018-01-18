using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class VRControlScript : MonoBehaviour
{
    public Transform RightHand;
    public Transform SpawnPoint;
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
        bool oneHeld = OVRInput.Get(OVRInput.Button.One);
        bool twoHeld = OVRInput.Get(OVRInput.Button.Two);
        bool buttonHeld = oneHeld || twoHeld;
        if (buttonHeld && !_wasHeld)
        {
            transform.position = SpawnPoint.position;
            transform.rotation = SpawnPoint.rotation;
        }
        if (buttonHeld)
        {
            
            _mainScript.BoxHandle.position = RightHand.position;
            _mainScript.BoxOrientationTarget.position = RightHand.position;
            _mainScript.BoxOrientationTarget.rotation = RightHand.rotation;
            _CloseMomentum = 0;
        }
        else
        {
            _CloseMomentum += .01f;
            _mainScript.BoxHandle.localPosition = Vector3.Lerp(_mainScript.BoxHandle.localPosition, Vector3.zero, _CloseMomentum);
        }
        _wasHeld = buttonHeld;
    }

    private Transform _selectedObject;
    
    void Update ()
    {
        HandleProductivityToggle();
        HandleOpening();
        _validationCube.SetActive(false);
        bool collided = false;
        if (_selectedObject == null)
        {
            collided = HandlePotentialSelection();
        }
        else
        {
            HandleSelectedObject();
        }
        Shader.SetGlobalFloat("_Collision", collided ? 1f : 0f);
    }

    private void HandleProductivityToggle()
    {
        bool showProductive = OVRInput.GetDown(OVRInput.Button.One);
        if(showProductive)
        {
            _mainScript.ShowProductive = true;
        }
        bool showUnproductive = OVRInput.GetDown(OVRInput.Button.Two);
        if(showUnproductive)
        {
            _mainScript.ShowProductive = false;
        }
    }

    private bool HandlePotentialSelection()
    {
        bool leftHandHeld = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        bool leftTriggerHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
        if(leftHandHeld || leftTriggerHeld)
        {
            ContainedItemScript interior = CheckInteriors();
            if(interior != null)
            {
                Shader.SetGlobalVector("_ColliderPosition", LeftHand.position);
                SelectObject(interior.transform);
                return true;
            }
        }

        RaycastHit hitInfo;
        int layerMask = (int)Mathf.Pow(_mainScript.ContainedObjectLayer, 2);
        bool hit = Physics.Raycast(LeftHand.position, LeftHand.forward, out hitInfo, 10000);
        if(hit)
        {
            DisplayValidationCube(hitInfo.point);
            if (leftTriggerHeld || leftHandHeld)
            {
                SelectObject(hitInfo.transform);
            }
        }
        return hit;
    }

    private ContainedItemScript CheckInteriors()
    {
        foreach (ContainedItemScript item in _mainScript.AllItems)
        {
            if(item.DoesContainCursor(LeftHand.position))
            {
                return item;
            }
        }
        return null;
    }

    private void HandleSelectedObject()
    {
        bool leftHandHeld = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        bool leftTriggerHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
        if (leftHandHeld || leftTriggerHeld)
        {
            _selectedObject.transform.rotation = _positionTarget.rotation;
            if (leftTriggerHeld && !leftHandHeld)
            {
                _selectedObject.transform.position = _positionTarget.position;
            }
            else
            {
                _positionTarget.position = _selectedObject.position;
            }
            _selectedObject.transform.localScale = _positionTarget.localScale;
            HandleScaling();
        }
        else
        {
            _selectedObject = null;
        }
    }

    private void HandleScaling()
    {
        float thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        float multiplier = (1 + thumbstick * .05f);
        float newX = _positionTarget.localScale.x * multiplier;
        float newY = _positionTarget.localScale.y * multiplier;
        float newZ = _positionTarget.localScale.z * multiplier;
        _positionTarget.localScale =  new Vector3(newX, newY, newZ);
    }

    private void SelectObject(Transform transform)
    {
        _positionTarget.position = transform.position;
        _positionTarget.rotation = transform.rotation;
        _positionTarget.localScale = transform.localScale;
        _selectedObject = transform;
    }

    private void DisplayValidationCube(Vector3 end)
    {
        Vector3 start = LeftHand.position;
        _validationCube.transform.position = (start + end) / 2;
        _validationCube.transform.LookAt(end);
        _validationCube.transform.localScale = new Vector3(0.005f, 0.005f, (start - end).magnitude);
        _validationCube.SetActive(true);
        Shader.SetGlobalVector("_ColliderPosition", end);
    }
}
