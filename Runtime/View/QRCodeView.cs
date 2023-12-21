#region

using System;
using TezosSDK.Beacon;
using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

#endregion

namespace TezosSDK.View
{

	public class QRCodeView : MonoBehaviour
	{
		[SerializeField] private RawImage rawImage;
		private Texture2D _texture;
		private bool encoded;

		private void Start()
		{
			if (_texture != null)
			{
				return;
			}

			rawImage.texture = _texture = new Texture2D(256, 256);
			_texture.filterMode = FilterMode.Point;
		}

		private void OnEnable()
		{
			if (TezosManager.Instance.Tezos != null && TezosManager.Instance.Tezos.Wallet.HandshakeData != null)
			{
				SetQrCode(TezosManager.Instance.Tezos.Wallet.HandshakeData);
			}
		}

		public void SetQrCode(HandshakeData handshakeData)
		{
			if (encoded)
			{
				return;
			}
			
			encoded = true;
			var uri = "tezos://?type=tzip10&data=" + handshakeData.PairingData;
			EncodeTextToQrCode(uri);
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