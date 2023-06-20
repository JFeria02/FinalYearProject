using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LiveUI : MonoBehaviour
{
    public TextMeshProUGUI resText;
    public Slider resSlider;
    public TextMeshProUGUI scaleText;
    public TextMeshProUGUI radiusText;
    public TextMeshProUGUI maxLevelText;
    public Button switchLayerButton;
    public Toggle enableSecondLayer;
    public Button generatebutton;
    public Button resetResButton;

    public GameObject baseLayerDisplay;
    public GameObject secondLayerDisplay;

    private NoiseLayer baseLayerSettings;
    private NoiseLayer secondLayerSettings;

    private static NoiseLayer[] finalNoiseLayers;

    private static int res;
    private static int radius;
    private static int maxLevel;


    // Start is called before the first frame update
    void Start()
    {
        bool isCursorLocked = false;
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            isCursorLocked = true;
        }
        Cursor.lockState = isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isCursorLocked;

        switchLayerButton.onClick.AddListener(SwitchLayers);
        generatebutton.onClick.AddListener(GeneratePlanet);
        resetResButton.onClick.AddListener(NewPlanet);

        baseLayerSettings = new NoiseLayer();
        secondLayerSettings = new NoiseLayer();

        GameObject.Find("PlanetQuadtreeMain").GetComponent<PlanetGenInit>().BeginGen();
    }

    // Update is called once per frame
    void Update()
    {
        resText.text = (resSlider.value*2).ToString();


        //update variables
        res = (int)resSlider.value * 2;
        radius = int.Parse(radiusText.text.Remove(radiusText.text.Length - 1));
        maxLevel = int.Parse(maxLevelText.text.Remove(maxLevelText.text.Length - 1));


        // Get noise settings from base and second layer
        // Send values to PlanetQuadtreeMain object to update live
        
        if (enableSecondLayer.isOn)
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[2];
            if(secondLayerSettings.noiseSettings == null)
            {
                secondLayerSettings.noiseSettings = new NoiseSettings();
            }
            noiseLayers[0] = baseLayerSettings;
            noiseLayers[1] = secondLayerSettings;
            GameObject.Find("PlanetQuadtreeMain").GetComponent<PlanetGenInit>().noiseLayers = noiseLayers;
            NoiseGenerator.SetNoiseLayers(noiseLayers);

        }
        else
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[1];
            noiseLayers[0] = baseLayerSettings;
            GameObject.Find("PlanetQuadtreeMain").GetComponent<PlanetGenInit>().noiseLayers = noiseLayers;
            NoiseGenerator.SetNoiseLayers(noiseLayers);

        }

    }

    public void UpdateLayer(int layer, NoiseSettings noiseSettings, NoiseType noiseType)
    {
        // baseLayer = 0, secondLayer = 1
        if(layer == 0)
        {
            baseLayerSettings.noiseType = noiseType;
            baseLayerSettings.noiseSettings = noiseSettings;
        }
        else if(layer == 1)
        {
            secondLayerSettings.noiseType = noiseType;
            secondLayerSettings.noiseSettings = noiseSettings;
        }
    }

    private void SwitchLayers()
    {
        if(baseLayerDisplay.activeInHierarchy)
        {
            baseLayerDisplay.SetActive(false);
            secondLayerDisplay.SetActive(true);
        }
        else
        {
            secondLayerDisplay.SetActive(false);
            baseLayerDisplay.SetActive(true);
        }
    }

    private void DestroyPlanet()
    {
        Destroy(GameObject.Find("PlanetQuadtreeMain"));
    }


    private void NewPlanet()  // Regenerate the planet with values given
    {
        DestroyPlanet();
        GameObject newPlanet = new GameObject("PlanetQuadtreeMain");
        newPlanet.layer = LayerMask.NameToLayer("PlanetTerrain");
        PlanetGenInit gen = newPlanet.AddComponent<PlanetGenInit>();
        gen.res = res;
        gen.radius = radius;
        gen.maxLevel = -1;
        if (enableSecondLayer.isOn)
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[2];
            if (secondLayerSettings.noiseSettings == null)
            {
                secondLayerSettings.noiseSettings = new NoiseSettings();
            }
            noiseLayers[0] = baseLayerSettings;
            noiseLayers[1] = secondLayerSettings;
            gen.noiseLayers = noiseLayers;

        }
        else
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[1];
            noiseLayers[0] = baseLayerSettings;
            gen.noiseLayers = noiseLayers;

        }
        gen.BeginGen();
        TriggerUpdate();
    }

    public void TriggerUpdate()
    {
        GameObject.Find("PlanetQuadtreeMain").GetComponent<PlanetGenInit>().liveUpdate = true;
    }

    private static void SetFinalLayers(bool isSecondLayerEnabled, NoiseLayer baseLayer, NoiseLayer secondLayer)
    {
        if (isSecondLayerEnabled)
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[2];
            if (secondLayer.noiseSettings == null)
            {
                secondLayer.noiseSettings = new NoiseSettings();
            }
            noiseLayers[0] = baseLayer;
            noiseLayers[1] = secondLayer;
            finalNoiseLayers = noiseLayers;

        }
        else
        {
            NoiseLayer[] noiseLayers = new NoiseLayer[1];
            noiseLayers[0] = baseLayer;
            finalNoiseLayers = noiseLayers;

        }

    }

    private void GeneratePlanet()
    {
        SetFinalLayers(enableSecondLayer.isOn, baseLayerSettings, secondLayerSettings);
        SceneManager.LoadScene("MainScene");
    }

    public static NoiseLayer[] GetFinalNoiseLayers()
    {
        return finalNoiseLayers;
    }

    public static int GetRes()
    {
        return res;
    }

    public static int GetRadius()
    {
        return radius;
    }

    public static int GetMaxLevel()
    {
        return maxLevel;
    }
}
