using TezosSDK.Beacon;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

namespace TezosSDK.View
{
    public class QRCodeView : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        private Texture2D _texture;

        void Start()
        {
            if (_texture != null) return;

            _rawImage.texture = _texture = new Texture2D(256, 256);
            _texture.filterMode = FilterMode.Point;
        }

        public void SetQrCode(HandshakeData handshakeData)
        {
            var uri = "tezos://?type=tzip10&data=" + handshakeData.PairingData;
            EncodeTextToQrCode(uri);
        }

        public void EncodeTextToQrCode(string text)
        {
            if (_texture == null)
            {
                _rawImage.texture = _texture = new Texture2D(256, 256);
                _texture.filterMode = FilterMode.Point;
            }

            var colors = Encode(text, _texture.width, _texture.height);
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