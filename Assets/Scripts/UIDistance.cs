using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDistance : MonoBehaviour
{
    public TextMeshProUGUI distanceValueText;
    public int radius;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, Vector3.zero) - radius;
        distanceValueText.text = distance.ToString();
    }
}
