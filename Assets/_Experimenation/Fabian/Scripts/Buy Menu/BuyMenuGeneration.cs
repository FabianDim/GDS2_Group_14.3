using _Project.Abilities.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuGeneration : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private AbilityDatabase database;

        [Header("UI")]
        [SerializeField] private RectTransform content;
        [SerializeField] private BuyMenuItem abilityCardPrefab;


        private void Start()
        {
            GenerateAbilityCards();
        }

        private void GenerateAbilityCards()
        {
            if (database == null)
            {
                Debug.LogError("BuyMenuGeneration: AbilityDatabase is not assigned.");
                return;
            }

            if (content == null)
            {
                Debug.LogError("BuyMenuGeneration: Content transform is not assigned.");
                return;
            }

            if (abilityCardPrefab == null)
            {
                Debug.LogError("BuyMenuGeneration: Ability card prefab is not assigned.");
                return;
            }

            ClearExistingCards();





            for (int i = 0; i < database.allAbilities.Count; i++)
            {
                Ability ability = database.allAbilities[i];
                if (ability == null)
                    continue;
                BuyMenuItem card =
                    Instantiate(abilityCardPrefab, content);

                card.Setup(ability);

                PositionCard(card, i);

            }
        }

        private void ClearExistingCards()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }

        [SerializeField] private int columnWidthModifier = 2;

        [SerializeField] private int columnHeightModifier = 4;
        [SerializeField] private Vector2 cardPivot = new Vector2(-1.5f, 1.2f);
        private void PositionCard(BuyMenuItem card, int index)
        {
            if (content == null || card == null)
            {
                return;
            }
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogError("BuyMenuItem must be attached to a UI object with a RectTransform.");
                return;
            }
            int columns = 3;
            int column = index % columns;

            float cardWidth = cardRect.rect.width;
            float cardHeight = cardRect.rect.height;
            float contentWidth = content.rect.width;
            float columnWidth = (cardWidth / columns) / columnWidthModifier;

            float x = -contentWidth / 2f
                      + columnWidth * column
                      + columnWidth / 2f;

            float row = Mathf.CeilToInt(index / columns);

            float y = -(cardHeight * row) / columnHeightModifier;




            cardRect.anchorMin = new Vector2(0f, 1f);
            cardRect.anchorMax = new Vector2(0f, 1f);

            cardRect.pivot = cardPivot;
            cardRect.anchoredPosition = new Vector2(x, y);
        }
    }


}