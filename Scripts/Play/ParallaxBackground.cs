using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform cam;
    private Vector3 startPosition;
    private Vector3 camStartPosition;

    [SerializeField] private float horizontalParallaxEffect;
    [SerializeField] private float verticalParallaxEffect;

    private float length;

void Awake()
{
     cam = Camera.main.transform;
        startPosition = transform.position;
        camStartPosition = cam.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
         UpdateParallax();
}
    void Start()
    {
       
        
    }

    void FixedUpdate()
    {
        UpdateParallax();
    }
    void UpdateParallax()
    {
        // Calculate the parallax effect for both horizontal and vertical movement
        float distanceToMoveX = (cam.position.x - camStartPosition.x) * horizontalParallaxEffect;
        float distanceToMoveY = (cam.position.y - camStartPosition.y) * verticalParallaxEffect;

        // Update the background's position 
        transform.position = new Vector3(startPosition.x + distanceToMoveX, startPosition.y + distanceToMoveY, transform.position.z);

        // Infinite horizontal parallax
        float tempX = cam.position.x * (1 - horizontalParallaxEffect);
        if (tempX > startPosition.x + length)
        {
            startPosition.x += length;
        }
        else if (tempX < startPosition.x - length)
        {
            startPosition.x -= length;
        }
    }
}
