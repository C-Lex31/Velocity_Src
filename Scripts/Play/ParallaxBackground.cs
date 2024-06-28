
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{


    private GameObject cam;
    private Vector3 startPosition;
    private float length;
    private Vector3 startCameraPosition;
    [SerializeField] private float multiplier;
    [SerializeField] private bool calculateInfiniteVerticalPosition;
    [SerializeField] private bool calculateInfiniteHorisontalPosition;
    [SerializeField] private bool horizontalOnly;



    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera");
        startPosition = transform.position;
        startCameraPosition = cam.transform.position;
        CalculateStartPosition();
        
    }

    void CalculateStartPosition()
    {
        float distX = (cam.transform.position.x - transform.position.x) * multiplier;
        float distY = (cam.transform.position.y - transform.position.y) * multiplier;
        Vector3 tmp = new Vector3(startPosition.x, startPosition.y);
        if (calculateInfiniteHorisontalPosition)
            tmp.x = transform.position.x + distX;
        if (calculateInfiniteVerticalPosition)
            tmp.y = transform.position.y + distY;
        startPosition = tmp;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }


    void LateUpdate()
    {
        Vector3 position = startPosition;
        if (horizontalOnly)
            position.x += multiplier * (cam.transform.position.x - startCameraPosition.x);
        else
            position += multiplier * (cam.transform.position - startCameraPosition);
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        float tmp = cam.transform.position.x * (1 - multiplier);
        if (tmp > startPosition.x + length)
            startPosition.x += length;
        else if (tmp < startPosition.x - length)
            startPosition.x = length;
    }
}