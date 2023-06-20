using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LiveUINoiseSettings : MonoBehaviour
{
    public int layer;
    public LiveUI mainUI;
    public TextMeshProUGUI seedText;
    public TextMeshProUGUI xOriginText;
    public TextMeshProUGUI yOriginText;
    public TextMeshProUGUI zOriginText;
    public TextMeshProUGUI octaveText;
    public Slider octaveSlider;
    public TextMeshProUGUI amplitudeControlText;
    public Slider amplitudeControlSlider;
    public TextMeshProUGUI frequencyControlText;
    public Slider frequencyControlSlider;
    public TextMeshProUGUI minValueText;
    public Slider minValueSlider;
    public TextMeshProUGUI strengthText;
    public Slider strengthSlider;
    public TextMeshProUGUI roughnessText;
    public Slider roughnessSlider;
    public TMP_Dropdown noiseTypeSelection;
    public TextMeshProUGUI scaleText;

    private NoiseType noiseType;
    private int seed;
    private float xOrigin;
    private float yOrigin;
    private float zOrigin;
    private int octaves;
    private float amplitudeControl;
    private float frequencyControl;
    private float minValue;
    private float strength;
    private float roughness;
    private int scale;

    private NoiseSettings noiseSettings;

    // Start is called before the first frame update
    void Start()
    {
        noiseSettings = new NoiseSettings();
    }

    // Update is called once per frame
    void Update()
    {

        octaveText.text = octaveSlider.value.ToString();
        amplitudeControlText.text = amplitudeControlSlider.value.ToString();
        frequencyControlText.text = frequencyControlSlider.value.ToString();
        minValueText.text = minValueSlider.value.ToString();
        strengthText.text = strengthSlider.value.ToString();
        roughnessText.text = roughnessSlider.value.ToString();

        // update variables
        if (noiseTypeSelection.value == 0)
        {
            noiseType = NoiseType.Original;
        }
        else if (noiseTypeSelection.value == 1)
        {
            noiseType = NoiseType.Ridge;
        }

        seed = int.Parse(seedText.text.Remove(seedText.text.Length - 1));
        xOrigin = int.Parse(xOriginText.text.Remove(xOriginText.text.Length - 1));
        yOrigin = int.Parse(yOriginText.text.Remove(yOriginText.text.Length - 1));
        zOrigin = int.Parse(zOriginText.text.Remove(zOriginText.text.Length - 1));
        octaves = (int)octaveSlider.value;
        amplitudeControl = amplitudeControlSlider.value;
        frequencyControl = frequencyControlSlider.value;
        minValue = minValueSlider.value;
        strength = strengthSlider.value;
        roughness = roughnessSlider.value;
        scale = int.Parse(scaleText.text.Remove(scaleText.text.Length - 1));


        noiseSettings.seed = seed;
        noiseSettings.xOrigin = xOrigin;
        noiseSettings.yOrigin = yOrigin;
        noiseSettings.zOrigin = zOrigin;
        noiseSettings.octaves = octaves;
        noiseSettings.amplitudeControl = amplitudeControl;
        noiseSettings.frequencyControl = frequencyControl;
        noiseSettings.minValue = minValue;
        noiseSettings.strength = strength;
        noiseSettings.roughness = roughness;
        noiseSettings.scale = scale;


        // update main UI class
        if(layer == 0)
        {
            mainUI.UpdateLayer(0, noiseSettings, noiseType);
        }
        else if(layer == 1)
        {
            mainUI.UpdateLayer(1, noiseSettings, noiseType);
        }


    }
}
