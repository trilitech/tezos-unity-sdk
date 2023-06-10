using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using Logger = Scripts.Helpers.Logger;

namespace Tezos.StarterSample
{
    public class StarterQRCodeSetter : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        private Texture2D _texture;

        private Coroutine _runningCoroutine;

        void Awake()
        {
            _texture = new Texture2D(256, 256);
            _rawImage.texture = _texture;
            _texture.filterMode = FilterMode.Point;
        }

        private void OnEnable()
        {
            GenerateQrCode();
        }

        private void OnDisable()
        {
            if (_runningCoroutine != null)
                StopCoroutine(_runningCoroutine);
        }

        private void GenerateQrCode()
        {
            if (_runningCoroutine != null)
            {
                StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }

            _runningCoroutine = StartCoroutine(IEGenerateQrCode());
        }

        private IEnumerator IEGenerateQrCode()
        {
            while (StarterTezosManager.Instance.Handshake == "")
            {
                Logger.LogError("Handshake is not set!");
                yield return null;
            }

            if (_texture == null)
            {
                _rawImage.texture = _texture = new Texture2D(256, 256);
                _texture.filterMode = FilterMode.Point;
            }

            var colors = Encode(StarterTezosManager.Instance.HandshakeURI, _texture.width, _texture.height);
            _texture.SetPixels32(colors);
            _texture.Apply();
        }

        private Color32[] Encode(string text, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions()
                {
                    Width = width,
                    Height = height,
                    PureBarcode = true
                }
            };
            return writer.Write(text);
        }
    }
}