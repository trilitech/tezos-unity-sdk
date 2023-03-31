using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

#if UNITY_ANDROID || UNITY_IOS
using BeaconSDK;
#endif

public class QRCodeView : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    private Texture2D _texture;
    
    void Start()
    {
        _rawImage.texture = _texture = new Texture2D(256, 256);
        _texture.filterMode = FilterMode.Point;
    }
    
    public void SetQrCode(string handshake)
    {
        var uri = "tezos://?type=tzip10&data=" + handshake;
        EncodeTextToQrCode(uri);
    }
    
    public void EncodeTextToQrCode(string text)
    {
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
