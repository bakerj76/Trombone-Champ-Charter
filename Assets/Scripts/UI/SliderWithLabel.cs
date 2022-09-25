using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class SliderWithLabel : MonoBehaviour
{
	public Slider Slider;
	public TMP_Text ValueText;

	// Start is called before the first frame update
	void Start()
    {
		Slider.onValueChanged.AddListener(v => UpdateValueText(v));
	}

	private void UpdateValueText(float value) {
		ValueText.text = $"{(value * 100).ToString("G3")}%";
	}
}
