using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainSceneStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlanetGenInit gen = gameObject.GetComponent<PlanetGenInit>();
        gen.res = LiveUI.GetRes();
        gen.radius = LiveUI.GetRadius();
        gen.maxLevel = LiveUI.GetMaxLevel();
        gen.noiseLayers = LiveUI.GetFinalNoiseLayers();

        gameObject.GetComponent<UIDistance>().radius = gen.radius;

        gen.BeginGen();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("LiveEditScene");
        }
    }


}
