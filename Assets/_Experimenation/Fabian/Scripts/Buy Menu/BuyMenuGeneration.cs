using System.Collections;
using _Experimenation.K.Game_Manager.Scripts;
using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuGeneration : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private AbilityDatabase database;

        [Space, Header("UI")]
        [SerializeField] private RectTransform content;
        [SerializeField] private BuyMenuItem abilityCardPrefab;
        [SerializeField] private TextMeshProUGUI timeLeftText;

        [Space, Header("Spawn Locations")]
        [SerializeField] private Transform p1BuyMenuLocation;
        [SerializeField] private Transform p2BuyMenuLocation;
        
        private readonly WaitForSeconds _1S = new(1);

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GameData.Instance);
            var spawnLocation = GameData.Instance.HasStateAuthority ? p1BuyMenuLocation : p2BuyMenuLocation;
            transform.SetPositionAndRotation(spawnLocation.position, spawnLocation.rotation);
            
            GenerateAbilityCards();
            StartCoroutine(ShowTimeLeft());
        }

        private IEnumerator ShowTimeLeft()
        {
            for (var i = GameData.Instance.roundDuration * 0.25; i > -1; i--)
            {
                timeLeftText.SetText($"Time until Start: {(int)i}");
                yield return _1S;
            }
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

            for (var index = 0; index < database.allAbilities.Count; index++)
            {
                var ability = database.allAbilities[index];
                if (ability == null)
                    continue;

                var card = Instantiate(abilityCardPrefab, content);
                card.Setup(ability, index);
            }
        }

        private void ClearExistingCards()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }
    }


}