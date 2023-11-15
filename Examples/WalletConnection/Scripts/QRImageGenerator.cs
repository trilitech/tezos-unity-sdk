using System;
using TezosSDK.Beacon;
using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace TezosSDK.Examples.WalletConnection.Scripts
{

	public class QRImageGenerator : MonoBehaviour
	{
		[SerializeField] private RawImage qrImage;
		
		private Texture2D texture;
		private bool encoded;

		private void Start()
		{
			TezosManager.Instance.MessageReceiver.HandshakeReceived += SetQrCode;
			TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
			TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
			TezosManager.Instance.MessageReceiver.AccountConnectionFailed += OnAccountConnectionFailed;
		}

		private void OnAccountDisconnected(AccountInfo account_info)
		{
			gameObject.SetActive(true);
		}

		private void OnAccountConnectionFailed(ErrorInfo error_info)
		{
			throw new Exception("Account connection failed!");
		}

		private void OnAccountConnected(AccountInfo account_info)
		{
			gameObject.SetActive(false);
		}

		private void SetQrCode(HandshakeData handshake_data)
		{
			if (encoded)
			{
				return;
			}

			var uri = "tezos://?type=tzip10&data=" + handshake_data.PairingData;
			EncodeTextToQrCode(uri);
		}

		private void EncodeTextToQrCode(string text)
		{
			if (texture == null)
			{
				texture = new Texture2D(256, 256);
				qrImage.texture = texture;
				texture.filterMode = FilterMode.Point;
			}

			var colors = Encode(text, texture.width, texture.height);
			texture.SetPixels32(colors);
			texture.Apply();

			encoded = true;
		}

		private Color32[] Encode(string text, int width, int height)
		{
			var options = new QrCodeEncodingOptions
			{
				Width = width,
				Height = height,
				PureBarcode = true
			};
			
			var writer = new BarcodeWriter
			{
				Format = BarcodeFormat.QR_CODE,
				Options = options
			};
			
			return writer.Write(text);
		}
	}

}