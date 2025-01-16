using UnityEngine;

namespace TezosSDK.FileUploaders
{

	public abstract class BaseUploader : MonoBehaviour
	{
		public string SupportedFileExtensions
		{
			get => ".jpg, .jpeg, .png";
		}

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
		}
	}

}