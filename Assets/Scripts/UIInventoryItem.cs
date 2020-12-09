// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIInventoryItem : MonoBehaviour, IPointerClickHandler
{
    public Image selectionHighlight;

    public UnityEvent<UIInventoryItem> OnClick;

    public string NAME {get;set;}
    public int UID {get;set;}
    
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clear()
    {
        NAME = null;
        UID = -1;
        image.sprite = null;
        image.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        Select(false);
    }

    public void Set(int uid, string name, Sprite sprite)
    {
        NAME = name;
        UID = uid;
        image.sprite = sprite;
        image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public void Select(bool on)
    {
        selectionHighlight.gameObject.SetActive(on);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(this);
    }

}
