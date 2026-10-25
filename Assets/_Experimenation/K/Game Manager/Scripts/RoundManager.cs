using System.Collections.Generic;
using UnityEngine;
namespace _Experimenation.K.Game_Manager.Scripts
{
	public class RoundManager : MonoBehaviour
	{
		private static readonly Color WonColor = Color.green;
		private static readonly Color LostOrPendingColor = Color.red;

		private GameObject _roundUnit;
		private readonly List<GameObject> _copies = new();
		private int _shownWon = -1;
		private int _shownTotal = -1;

		private void Awake() => _roundUnit = transform.GetChild(0).gameObject;

		private void Update()
		{
			var gameData = GameData.Instance;
			if (gameData == null)
				return;

			// P1 is the host (state authority) and P2 is the client, matching GameManager.
			var localData = gameData.HasStateAuthority ? gameData.P1Data : gameData.P2Data;
			Refresh(localData.Score, gameData.numberOfRounds);
		}

		private void Refresh(int roundsWon, int totalRounds)
		{
			if (roundsWon == _shownWon && totalRounds == _shownTotal)
				return;

			_shownWon = roundsWon;
			_shownTotal = totalRounds;

			for (var i = 0; i < totalRounds; i++)
			{
				var unit = GetUnit(i);
				unit.SetActive(true);
				SetColor(unit, i < roundsWon ? WonColor : LostOrPendingColor);
			}

			for (var i = totalRounds; i < _copies.Count + 1; i++)
			{
				var unit = i == 0 ? _roundUnit : _copies[i - 1];
				if (unit != null)
					unit.SetActive(false);
			}
		}

		// Index 0 is the original child; further indices are copies created on demand.
		private GameObject GetUnit(int index)
		{
			if (index == 0)
				return _roundUnit;

			while (_copies.Count < index)
				_copies.Add(Instantiate(_roundUnit, transform));

			return _copies[index - 1];
		}

		private static void SetColor(GameObject unit, Color color)
		{
			foreach (var renderer in unit.GetComponentsInChildren<Renderer>(true))
				renderer.material.color = color;
		}
	}
}
