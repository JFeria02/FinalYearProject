using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveEditCamera : MonoBehaviour
{
    private float zoomValue = 3800f;
    private bool moveView = false;
    private Vector3 previoiusMousePos;

    // Update is called once per frame
    void Update()
    {
        // if right click is held down
        // when first pressed get position
        // get difference from original and current position and rotate accordingly
        if (Input.GetButtonDown("Fire2"))
        {
            moveView = true;
            previoiusMousePos = Input.mousePosition;
        }

        if (moveView && Input.GetButton("Fire2"))
        {
            transform.Translate((previoiusMousePos - Input.mousePosition) * 500 * Time.deltaTime);
            previoiusMousePos = Input.mousePosition;
        }
        else if (!Input.GetButton("Fire2"))
        {
            moveView = false;
        }

        transform.LookAt(Vector3.zero); // Camera always faces 0,0,0

        if(Input.mouseScrollDelta.y > 0)
        {
            // Zoom in - set zoom value
            zoomValue -= 80;
        }
        else if(Input.mouseScrollDelta.y < 0)
        {
            // Zoom out - set zoom value
            zoomValue += 80;
        }

        float zoomDistance = Vector3.Distance(Vector3.zero, Camera.main.transform.position); // Used to ensure that mouse movement does not affect the zoom

        if(zoomDistance > zoomValue)
        {
            // Zoom in
            transform.Translate(Vector3.forward * (zoomDistance - zoomValue));
        }
        else if(zoomDistance < zoomValue)
        {
            // Zoom out
            transform.Translate(Vector3.back * (zoomValue - zoomDistance));
        }

    }
}
