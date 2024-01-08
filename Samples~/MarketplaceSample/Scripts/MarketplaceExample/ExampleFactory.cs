using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample
{

	public class ExampleFactory : MonoBehaviour
	{
		public static ExampleFactory Instance;
		private IExampleManager _exampleManager;

		private void Start()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}

			_exampleManager = new ExampleManager();
			_exampleManager.Init();
		}

		public IExampleManager GetExampleManager()
		{
			return _exampleManager;
		}
	}

}