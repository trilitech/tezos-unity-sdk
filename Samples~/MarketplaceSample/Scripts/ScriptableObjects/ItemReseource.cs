using UnityEngine;

namespace TezosSDK.MarketplaceSample
{

	[CreateAssetMenu(fileName = "ItemResources", menuName = "ScriptableObjects/ItemResources", order = 1)]
	public class ItemReseource : ScriptableObject
	{
		public string Name;
		public Sprite ItemSprite;
	}

}