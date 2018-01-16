using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour 
{
    public Transform BoxHandle;
    public Transform BoxLid;

    private void Update()
    {
        float boxHandleX = Mathf.Clamp(BoxHandle.position.x, -4, 0);
        BoxHandle.localPosition = new Vector3(boxHandleX, -.5f, 0);
        BoxLid.localPosition = new Vector3(boxHandleX / 2, 0, 0);
        BoxLid.localScale = new Vector3(boxHandleX, 1, 1);
        
        Shader.SetGlobalVector("_LidPlaneNormal", BoxLid.transform.forward);
        Shader.SetGlobalVector("_LidPlanePoint", BoxLid.transform.position);
    }
}
