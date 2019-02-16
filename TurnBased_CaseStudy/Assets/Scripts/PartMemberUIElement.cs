using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartMemberUIElement : MonoBehaviour
{
	private CharacterBase m_character;

	[SerializeField, Required]
	private TextMeshProUGUI nameText;
	[SerializeField, Required]
	private Slider healthSlider;

	[SerializeField, Required]
	private Image image;

	[SerializeField]
	private Color highLightColor = Color.cyan;
	[SerializeField]
	private Color defaultColor = Color.white;
	[SerializeField]
	private Color disableColor = Color.grey;

	
	
	public void Init(CharacterBase Character)
	{
		m_character = Character;

		gameObject.name = m_character.characterName + "_PartyUI";
		nameText.text = m_character.characterName;

		healthSlider.value = healthSlider.maxValue = m_character.StartingHealth;
		healthSlider.minValue = 0;
		

		UpdateUI();

	}

	public void UpdateUI()
	{
		healthSlider.value = m_character.CurrentHealth;
	}

	public void Highlight(bool state)
	{
		image.color = state ? highLightColor : defaultColor;
	}

	public void SetActive(bool state)
	{
		image.color = !state ? disableColor : defaultColor;
	}
}
