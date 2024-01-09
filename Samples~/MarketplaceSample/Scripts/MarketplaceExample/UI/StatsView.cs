using System.Text.RegularExpressions;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class StatsView : MonoBehaviour
	{
		[SerializeField] private StatTextView statTextPrefab;

		public void Clear()
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}
		}

		public void DisplayStats(StatParams stats)
		{
			Clear();

			var pis = stats.GetType().GetProperties();

			foreach (var pi in pis)
			{
				var newStat = Instantiate(statTextPrefab, transform);
				// convert name from camel case to sentence case
				var sentName = Regex.Replace(pi.Name, @"\p{Lu}", m => " " + m.Value);
				newStat.SetStatName(sentName);
				newStat.SetStatValue(pi.GetValue(stats).ToString());
			}
		}
	}

}