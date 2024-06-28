using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public float lerpSpeed = 3;
    public Vector2 offsetPlayer = new Vector2(1, 1);
    public float limitCameraUp = 26;     //set limit position for Y axis
    public float limitCameraBelow = -3;     //set limit position for Y axis

    private float limitUp = 0;     //set limit position for Y axis
    private float limitBelow = 0;     //set limit position for Y axis

    Vector3 offset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        limitUp = limitCameraUp - (Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z * -1)).y - transform.position.y);
        limitBelow = limitCameraBelow + (transform.position.y - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.z * -1)).y);
    }
    private void LateUpdate()
    {
        offset = offsetPlayer;
        offset.z = transform.position.z;

        var finalPos = Player.instance.transform.position + offset;
        finalPos.y = Mathf.Clamp(finalPos.y, limitBelow, limitUp);

        transform.position = Vector3.Lerp(transform.position, finalPos, 10 * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.up * limitCameraUp, Vector3.right * 1000);
        Gizmos.DrawRay(Vector3.up * limitCameraBelow, Vector3.right * 1000);

        if (!Application.isPlaying)
        {
            limitUp = limitCameraUp - (Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z * -1)).y - transform.position.y);
            limitBelow = limitCameraBelow + (transform.position.y - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.position.z * -1)).y);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(Vector3.up * limitUp, Vector3.right * 1000);
        Gizmos.DrawRay(Vector3.up * limitBelow, Vector3.right * 1000);
    }
}
