using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuyMenuItem : MonoBehaviour
{
    [SerializeField] private Sprite abilitySprite;
    [SerializeField] private Image abilityImage;
    [SerializeField] private TMP_Text[] abilityName;
    [SerializeField] private TMP_Text[] description;
    [SerializeField] private TMP_Text price;
    [SerializeField] private TMP_Text KeyBind;

    public void Setup(Ability ability)
    {
        abilitySprite = ability.abilityImage;
        abilityImage.sprite = abilitySprite;

        foreach (TMP_Text item in abilityName)
        {
            item.SetText(ability.abilityName);
        }
        foreach (TMP_Text item in description)
        {
            item.SetText(ability.abilityDescription);
        }
        price.SetText($"{ability.AbilityPrice}");
    }
}