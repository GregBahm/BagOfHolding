using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class VRControlScript : MonoBehaviour
{
    public Transform RightHand;

    private MainScript _mainScript;
    private bool _wasHeld;

    private void Start()
    {
        _mainScript = GetComponent<MainScript>();
    }

    private void HandleOpening()
    {
        bool rightTriggerHeld = VRInput.Right().Trigger.IsHeld();
        if (rightTriggerHeld && !_wasHeld)
        {
            transform.position = RightHand.position;
            transform.rotation = RightHand.rotation;
        }
        if (rightTriggerHeld)
        {
            _mainScript.BoxHandle.position = RightHand.position;
        }
        _wasHeld = rightTriggerHeld;
    }
    
    void Update ()
    {
        HandleOpening();
    }
}
