// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using TMPro;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    public TextMeshPro nameLabel;
    public float speed;
    public SpriteRenderer hat;

    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private Vector2 motionVector;
    private float currentAngle;
    private float lastAngle = float.MinValue;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        motionVector = inputVector.normalized * speed;
        if (inputVector != Vector2.zero)
        {
            currentAngle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + motionVector * Time.fixedDeltaTime);
        if(currentAngle != lastAngle)
        {
            Vector3 curScale = transform.localScale;
            if(currentAngle < 90.0f && currentAngle > -90.0f)
            {
                if(curScale.x > 0.0f)
                {
                    curScale.x = -curScale.x;
                    transform.localScale = curScale;
                }
            }
            else if(currentAngle > 90.0f || currentAngle < -90.0f)
            {
                if(curScale.x < 0.0f)
                {
                    curScale.x = -curScale.x;
                    transform.localScale = curScale;
                }
            }
            lastAngle = currentAngle;
         }
    }

    public void SetName(string name)
    {
        nameLabel.text = name;
    }

    public void WearHat(Sprite newHat)
    {
        hat.enabled = true;
        hat.sprite = newHat;
    }

    public void RemoveHat()
    {
        hat.enabled = false;
    }

    public Vector3 GetPlaceItemPosition()
    {
        Vector3 vec = transform.position;
        if(currentAngle < 90.0f && currentAngle > -90.0f)
        {
            vec.x += 1.5f;
        }
        else
        {
            vec.x -= 1.5f;
        }
        return vec;
    }

}
