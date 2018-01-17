using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public BoxCollider Container;
    public Transform BoxHandle;
    public Transform BoxLid;
    public GameObject[] ContainedItems;

    private void Start()
    {
        foreach (GameObject item in ContainedItems)
        {
            ContainedItemScript script = item.AddComponent<ContainedItemScript>();
            script.Container = Container;
        }
    }

    private void Update()
    {
        PlaceParts();
    }

    private void PlaceParts()
    {
        float minX = Container.transform.localPosition.x - Container.transform.localScale.x / 2;
        float maxX = Container.transform.localPosition.x + Container.transform.localScale.x / 2;
        float boxHandleX = Mathf.Clamp(BoxHandle.localPosition.x, minX, maxX);
        BoxHandle.localPosition = new Vector3(boxHandleX, -Container.transform.localScale.y / 2, 0);
        BoxLid.localPosition = new Vector3(boxHandleX / 2, 0, 0);
        BoxLid.localScale = new Vector3(boxHandleX, Container.transform.localScale.y, Container.transform.localScale.z);

        Shader.SetGlobalVector("_LidPlaneNormal", BoxLid.transform.forward);
        Shader.SetGlobalVector("_LidPlanePoint", BoxLid.transform.position);
    }
}
