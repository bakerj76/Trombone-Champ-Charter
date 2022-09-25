using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	public Slider RedSlider;
	public Slider GreenSlider;
	public Slider BlueSlider;

	public Image ColorPreview;

	public delegate void ValueChanged(Color value);
	public ValueChanged OnValueChanged;

	public Color Color {
		get { return new Color(RedSlider.value, GreenSlider.value, BlueSlider.value); }
	}

	void Start() {
		RedSlider.onValueChanged.AddListener(v => UpdateColor());
		GreenSlider.onValueChanged.AddListener(v => UpdateColor());
		BlueSlider.onValueChanged.AddListener(v => UpdateColor());
	}

	public void SetColor(float red, float green, float blue) {
		RedSlider.value = red;
		GreenSlider.value = green;
		BlueSlider.value = blue;

		UpdateColor();
	}

	public void UpdateColor() {
		ColorPreview.color = Color;
		OnValueChanged?.Invoke(Color);
	}
}
