using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public BoxCollider Container;
    public Transform BoxHandle;
    public Transform BoxLid;
    public GameObject[] FunItems;
    public GameObject[] SeriousItems;
    public Transform FrameJoint;

    private List<ContainedItemScript> _allItems;
    public IEnumerable<ContainedItemScript> AllItems
    {
        get
        {
            return _allItems;
        }
    }
    public int ContainedObjectLayer;
    public Transform BoxOrientationTarget;
    public bool ShowProductive;

    private void Start()
    {
        _allItems = new List<ContainedItemScript>();
        foreach (GameObject item in FunItems.Concat(SeriousItems))
        {
            item.layer = ContainedObjectLayer;
            ContainedItemScript script = item.AddComponent<ContainedItemScript>();
            script.Container = Container;
            _allItems.Add(script);
        }
    }

    private void Update()
    {
        PlaceParts();
        transform.LookAt(BoxOrientationTarget, BoxOrientationTarget.up);
        foreach (GameObject item in FunItems)
        {
            item.GetComponent<ContainedItemScript>().ShowItemInBox = !ShowProductive;
        }
        foreach (GameObject item in SeriousItems)
        {
            item.GetComponent<ContainedItemScript>().ShowItemInBox = ShowProductive;
        }
        Shader.SetGlobalFloat("_SwapColors", ShowProductive ? 1 : 0);
    }

    private void PlaceParts()
    {
        float minZ = Container.transform.localPosition.z - Container.transform.localScale.z / 2;
        float maxZ = Container.transform.localPosition.z + Container.transform.localScale.z / 2;
        float boxHandleZ = Mathf.Clamp(BoxHandle.localPosition.z, minZ, maxZ);
        BoxHandle.localPosition = new Vector3(0, -Container.transform.localScale.y / 2, boxHandleZ);
        BoxLid.localPosition = new Vector3(0, 0, boxHandleZ / 2);
        BoxLid.localScale = new Vector3(Container.transform.localScale.x, boxHandleZ, 0);
        FrameJoint.localPosition = new Vector3(-boxHandleZ / 10, 0, 0);

        Shader.SetGlobalVector("_LidPlaneNormal", BoxLid.transform.forward);
        Shader.SetGlobalVector("_LidPlanePoint", BoxLid.transform.position);

        Container.gameObject.SetActive(boxHandleZ > (minZ + .0001f));
    }
}
