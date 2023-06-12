using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class IPFSImageDownloader
    {
        public static IEnumerator DownloadAndSetImageTexture(string url, Image image)
        {
            if (url.StartsWith("ipfs:"))
                url = url.Replace("ipfs://", "infura-ipfs.io/ipfs/");
            url = url.Insert(0, "https://");
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                DownloadHandler handle = webRequest.downloadHandler;
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        throw new Exception(webRequest.error);
                    case UnityWebRequest.Result.ProtocolError:
                        throw new Exception("Http Error: " + webRequest.error);
                    case UnityWebRequest.Result.Success:
                        try
                        {
                            Texture2D texture2d = DownloadHandlerTexture.GetContent(webRequest);

                            Sprite sprite = null;
                            sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), UnityEngine.Vector2.zero);

                            if (sprite != null)
                            {
                                image.sprite = sprite;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }

                        break;
                }
            }
        }
    }
}