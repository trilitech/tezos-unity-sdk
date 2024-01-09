namespace TezosSDK.MarketplaceSample.MarketplaceExample.Core
{

	public interface IItemModel
	{
		public ItemType Type { get; }
		public string ResourcePath { get; }
		public string Name { get; }
		public StatParams Stats { get; }
		public int ID { get; }
		public float Price { get; }
		public string Owner { get; }
	}

	public enum ItemType
	{
		Head,
		Torso,
		Legs,
		Feet,
		Hand,
		Accessory,
		Consumable
	}

	public class StatParams
	{
		public StatParams()
		{
		}

		public StatParams(float damage, float armor, float attackSpeed, float healthPoints, float manaPoints)
		{
			Damage = damage;
			Armor = armor;
			AttackSpeed = attackSpeed;
			HealthPoints = healthPoints;
			ManaPoints = manaPoints;
		}

		public float Damage { get; set; }
		public float Armor { get; set; }
		public float AttackSpeed { get; set; }
		public float HealthPoints { get; set; }
		public float ManaPoints { get; set; }
	}

}