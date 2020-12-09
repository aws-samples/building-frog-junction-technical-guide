// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform relativeSprite;
    void LateUpdate()
    {
        transform.position = relativeSprite.position + (transform.up * -1.2f);
    }
}
