// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using UnityEngine;
using UnityEngine.Events;

public class PropController : MonoBehaviour
{
    public UnityEvent<PropController> OnClick;

    public int UID {get;set;}
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        OnClick.Invoke(this);
    }
}
