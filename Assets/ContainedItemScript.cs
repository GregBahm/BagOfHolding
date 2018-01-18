using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ContainedItemScript : MonoBehaviour 
{
    private List<Renderer> _renderers;
    private List<Material> _mats;
    private BoxCollider _myCollider;
    public bool _isInContainer;
    public BoxCollider Container;

    public bool ShowItemInBox { get; internal set; }

    private void Start()
    {
        _isInContainer = true;
        _renderers = GetRenderers();
        _myCollider = GetComponent<BoxCollider>();
        _mats = GetMats();
    }

    private List<Renderer> GetRenderers()
    {
        List<Renderer> ret = new List<Renderer>();
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        ret.AddRange(skinnedRenderers);
        ret.AddRange(meshRenderers);
        return ret;
    }

    private List<Material> GetMats()
    {
        List<Material> ret = new List<Material>();
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            ret.AddRange(renderer.sharedMaterials);
        }
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            ret.AddRange(meshRenderer.sharedMaterials);
        }
        return ret;
    }

    private void Update()
    {
        if(_isInContainer)
        {
            if(IsOutOfContainer())
            {
                _isInContainer = false;
                transform.SetParent(null, true);
            }
            else
            {
                ContainWithinWalls();
            }
        }
        else
        {
            if(IsBackInContainer())
            {
                _isInContainer = true;
                transform.SetParent(Container.transform.parent, true);
            }
        }
        foreach (Material mat in _mats)
        {
            mat.SetFloat("_IsOutOfContainer", _isInContainer ? 0 : 1);
        }
        SetAvailability(!_isInContainer || ShowItemInBox);
    }

    private void SetAvailability(bool value)
    {
        _myCollider.enabled = value;
        foreach (Renderer renderer in _renderers)
        {
            renderer.enabled = value;
        }
    }

    private void ContainWithinWalls()
    {
        if(transform.parent != Container.transform.parent)
        {
            return;
        }
        float maxX = -transform.localScale.x / 2 + Container.transform.lossyScale.x / 2 + Container.transform.localPosition.x; ;
        float minX = transform.localScale.x / 2 - Container.transform.lossyScale.x / 2 + Container.transform.localPosition.x;

        float maxZ = -transform.localScale.z / 2 + Container.transform.lossyScale.z / 2 + Container.transform.localPosition.z;
        float minZ = transform.localScale.z / 2 - Container.transform.lossyScale.z / 2 + Container.transform.localPosition.z;

        float minY = transform.localScale.y / 2 - Container.transform.lossyScale.y / 2 + Container.transform.localPosition.y;

        float newX = Mathf.Clamp(transform.localPosition.x, minX, maxX);
        float newY = Mathf.Max(minY, transform.localPosition.y);
        float newZ = Mathf.Clamp(transform.localPosition.z, minZ, maxZ);
        transform.localPosition = new Vector3(newX, newY, newZ);
    }

    internal bool DoesContainCursor(Vector3 position)
    {
        Vector3 localPos = _myCollider.transform.InverseTransformPoint(position);
        return (Mathf.Abs(localPos.x) < 0.5f) && (Mathf.Abs(localPos.y) < 0.5f) && (Mathf.Abs(localPos.z) < 0.5f);
    }

    private bool IsBackInContainer()
    {
        Vector3 localPos = Container.transform.InverseTransformPoint(transform.position);
        return (Mathf.Abs(localPos.x) < 0.5f) && (Mathf.Abs(localPos.y) < 0.5f) && (Mathf.Abs(localPos.z) < 0.5f);
    }
    
    private bool IsOutOfContainer()
    {
        return transform.localPosition.y > (transform.localScale.y / 2);
    }
}
