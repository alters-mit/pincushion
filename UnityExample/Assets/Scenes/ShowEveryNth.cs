using UnityEngine;
using Pincushion;
using UnityEngine.UI;


public class ShowEveryNth : MonoBehaviour
{
    private Slider slider;
    
    
    private void Start()
    {
        slider = FindObjectOfType<Slider>();
        slider.onValueChanged.AddListener(Resample);
    }


    private void Resample(float value)
    {
        PincushionManager instance = PincushionManager.Instance;
        // Set the factor.
        instance.showEveryNth = true;
        instance.nthFactor = value;
        // Resample.
        instance.ShowEveryNthPoint();
    }
}
