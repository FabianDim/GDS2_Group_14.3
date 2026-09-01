using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class BuyMenuItem : MonoBehaviour
{
    [SerializeField] private Sprite abilityImage;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text price;
    [SerializeField] private TMP_Text K;

    public void Setup(Ability ability)
    {
        abilityName.SetText(ability.abilityName);
        description.SetText(ability.abilityDescription);
        price.SetText($"{ability.AbilityPrice}");
    }
}