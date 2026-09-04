using _Project.Abilities.Scripts;
using Unity.Collections;
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





            foreach (Ability ability in database.allAbilities)
            {
                if (ability == null)
                    continue;
                BuyMenuItem card =
                    Instantiate(abilityCardPrefab, content);

                card.Setup(ability);


            }
        }

        private void ClearExistingCards()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }
        private Vector2 PositionGenerator(int index)
        {
            int columns = 3;
        }
    }


}