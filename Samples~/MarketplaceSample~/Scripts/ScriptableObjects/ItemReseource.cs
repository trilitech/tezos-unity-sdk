using UnityEngine;

namespace TezosSDK.Samples.MarketplaceSample.ScriptableObjects
{

	[CreateAssetMenu(fileName = "ItemResources", menuName = "ScriptableObjects/ItemResources", order = 1)]
	public class ItemReseource : ScriptableObject
	{
		public string Name;
		public Sprite ItemSprite;
	}

}