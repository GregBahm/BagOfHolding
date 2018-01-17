using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ContainedItemScript : MonoBehaviour 
{
    private MeshRenderer _meshRender;
    private Material _mat;
    private bool _isInContainer;
    public BoxCollider Container;

    private void Start()
    {
        _isInContainer = true;
        _meshRender = GetComponent<MeshRenderer>();
        _mat = _meshRender.material;
    }
    
    private void Update()
    {
        if(_isInContainer)
        {
            ContainWithinWalls();
            if(IsOutOfContainer())
            {
                _isInContainer = false;
                transform.SetParent(null, true);
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
        _mat.SetFloat("_IsOutOfContainer", _isInContainer ? 0 : 1);
    }

    private void ContainWithinWalls()
    {
        float maxX = -transform.localScale.x / 2;
        float minX = transform.localScale.x / 2  - Container.transform.localScale.x;

        float maxZ = -transform.localScale.z / 2 + Container.transform.lossyScale.z / 2;
        float minZ = transform.localScale.z / 2 - Container.transform.lossyScale.z / 2;
        
        float minY = transform.localScale.y / 2 - Container.transform.lossyScale.y;

        float newX = Mathf.Clamp(transform.localPosition.x, minX, maxX);
        float newY = Mathf.Max(minY, transform.localPosition.y);
        float newZ = Mathf.Clamp(transform.localPosition.z, minZ, maxZ);
        transform.localPosition = new Vector3(newX, newY, newZ);
    }

    private bool IsBackInContainer()
    {
        return Container.bounds.Contains(transform.position) ;
    }
    
    private bool IsOutOfContainer()
    {
        return transform.localPosition.y > transform.localScale.y / 2;
    }
}
