using System;
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

		private void OnAccountDisconnected(string obj)
		{
			gameObject.SetActive(true);
		}

		private void OnAccountConnectionFailed(string message)
		{
			throw new Exception("Account connection failed!");
		}

		private void OnAccountConnected(string message)
		{
			gameObject.SetActive(false);
		}

		private void SetQrCode(string handshake)
		{
			if (encoded)
			{
				return;
			}

			var uri = "tezos://?type=tzip10&data=" + handshake;
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