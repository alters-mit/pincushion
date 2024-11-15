using UnityEngine;
using Pincushion;
using UnityEngine.UI;


public class ApplyMask : MonoBehaviour
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
        instance.applyMask = true;
        instance.maskFactor = value;
        // Apply the mask.
        instance.SetMask();
    }
}
