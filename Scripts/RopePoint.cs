using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopePoint : MonoBehaviour
{
    
    public GameObject idleObj, activeObj;

    [Header("Active Slow motion to guide player")]
    public bool slowMotion = false;
    public GameObject activeHelperObj;
    public GameObject showReleaseTutObj;
    // Start is called before the first frame update
    void Start()
    {
        if (activeHelperObj)
            activeHelperObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.instance.currentAvailableRope != null && Player.instance.currentAvailableRope.gameObject == gameObject)
        {
            idleObj.SetActive(false);
            activeObj.SetActive(true);

            if (slowMotion)
            {
                if (activeHelperObj)
                    activeHelperObj.SetActive(true);

                if (showReleaseTutObj && !showReleaseTutObj.activeInHierarchy)
                    if (Player.instance.bIsGrabingRope && Player.instance.transform.position.x > transform.position.x && Player.instance.transform.position.y > (transform.position.y - (Vector2.Distance(transform.position, Player.instance.transform.position) * 0.5f)))
                    {
                        showReleaseTutObj.SetActive(true);
                        Time.timeScale = 0.1f;
                    }
            }
        }
        else
        {
            idleObj.SetActive(true);
            activeObj.SetActive(false);

            if (slowMotion)
            {
                if (activeHelperObj)
                    activeHelperObj.SetActive(false);
                if (showReleaseTutObj)
                    showReleaseTutObj.SetActive(false);
            }
        }
    }
}
