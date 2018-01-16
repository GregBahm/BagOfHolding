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
            ContainWithinContainerWalls();
            _isInContainer = !IsOutOfContainer();
        }
        else
        {
            _isInContainer = IsBackInContainer();
        }
        _mat.SetFloat("_IsOutOfContainer", _isInContainer ? 0 : 1);
    }

    private void ContainWithinContainerWalls()
    {
        float leftObjectSide = transform.localPosition.x + transform.localScale.x / 2;
        float rightObjectSide = transform.localPosition.x - transform.localScale.x / 2;
        float leftWall = 0;
        float rightWall = -Container.transform.lossyScale.x;
        if (leftObjectSide > leftWall)
        {
            transform.localPosition = new Vector3(leftWall - transform.localScale.x / 2, transform.localPosition.y, transform.localPosition.z);
        }
        if (rightObjectSide < rightWall)
        {
            transform.localPosition = new Vector3(rightWall + transform.localScale.x / 2, transform.localPosition.y, transform.localPosition.z);
        }


        float frontObjectSide = transform.localPosition.z + transform.localScale.z / 2;
        float backObjectSide = transform.localPosition.z - transform.localScale.z / 2;
        float frontWall = Container.transform.lossyScale.z / 2;
        float backWall = -Container.transform.lossyScale.z / 2;
        if (frontObjectSide > frontWall)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, frontWall - transform.localScale.z / 2);
        }
        if (backObjectSide < backWall)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, backWall + transform.localScale.z / 2);
        }

        float bottomObjectSide = transform.localPosition.y - transform.localScale.y / 2;
        float bottomWall = -Container.transform.lossyScale.y;
        if (bottomObjectSide < bottomWall)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, bottomWall + transform.localScale.y / 2, transform.localPosition.z);
        }
    }

    private bool IsBackInContainer()
    {
        return false;
    }
    
    private bool IsOutOfContainer()
    {
        return transform.localPosition.y > transform.localScale.y / 2;
    }
}
