using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace TezosSDK.Helpers
{

	public class QrCodeGenerator : MonoBehaviour
	{
		[SerializeField] private RawImage rawImage;
		private bool _encoded;
		private Texture2D _texture;

		private void Start()
		{
			if (_texture != null)
			{
				return;
			}

			rawImage.texture = _texture = new Texture2D(256, 256);
			_texture.filterMode = FilterMode.Point;
		}

		public void SetQrCode(HandshakeData handshakeData)
		{
			if (_encoded)
			{
				return;
			}

			var uri = "tezos://?type=tzip10&data=" + handshakeData.PairingData;
			EncodeTextToQrCode(uri);
			_encoded = true;
		}

		private Color32[] Encode(string text, int width, int height)
		{
			var writer = new BarcodeWriter
			{
				Format = BarcodeFormat.QR_CODE,
				Options = new QrCodeEncodingOptions
				{
					Width = width,
					Height = height,
					PureBarcode = true
				}
			};

			return writer.Write(text);
		}

		private void EncodeTextToQrCode(string text)
		{
			if (_texture == null)
			{
				rawImage.texture = _texture = new Texture2D(256, 256);
				_texture.filterMode = FilterMode.Point;
			}

			var colors = Encode(text, _texture.width, _texture.height);
			_texture.SetPixels32(colors);
			_texture.Apply();
		}
	}

}