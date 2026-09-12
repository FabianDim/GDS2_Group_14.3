using System.Linq;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuItem : MonoBehaviour
    {
        [SerializeField] private Image abilityImage;
        [SerializeField] private TMP_Text[] abilityName;
        [SerializeField] private TMP_Text[] description;
        [SerializeField] private TMP_Text price;
        [SerializeField] private TMP_Text keyBind;
        private Ability _ability;

        private void Awake()
        {
            if (abilityImage == null)
                abilityImage = GetComponentInChildren<Image>();
        }

        public void Setup(Ability ability)
        {
            if (!ability)
                return;
            _ability = ability;
            if (abilityImage == null)
                abilityImage = GetComponentInChildren<Image>();

            if (abilityImage != null)
                abilityImage.sprite = ability.abilitySprite;
            else
            {
                Debug.LogWarning("abilityImage is null – no Image component found on this BuyMenuItem.");
            }

            foreach (TMP_Text item in abilityName)
            {
                item.SetText(ability.abilityName);
            }
            foreach (TMP_Text item in description)
            {
                item.SetText(ability.abilityDescription);
            }
            price.SetText($"{ability.abilityPrice}");
        }

        public void BuyItem()
        {
            foreach(var effect in _ability.effects)
                effect.ApplyEffect(FindObjectsByType<Player>().First(p => p.HasInputAuthority));
        }
    }
}