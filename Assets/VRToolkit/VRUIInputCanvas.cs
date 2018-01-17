using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class VRUIInputCanvas : MonoBehaviour {

    protected void Start () {
        GetComponent<Canvas>().worldCamera = VRUIInputModule.Instance.RaycastCamera;
	}

    protected void OnValidate()
    {
        Canvas c = GetComponent<Canvas>();
        Debug.Assert(c.renderMode == RenderMode.WorldSpace, "VR UI Canvas system only works with Worldspace Canvas rendering");
    }

}
