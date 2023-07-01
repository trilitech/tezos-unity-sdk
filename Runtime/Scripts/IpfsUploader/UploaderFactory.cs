using UnityEngine;

namespace TezosSDK.Scripts.IpfsUploader
{
    public static class UploaderFactory
    {
        public static IFileUploader GetUploader()
        {
            IFileUploader uploader = null;
#if UNITY_WEBGL
            uploader = WebUploaderHelper.InitWebFileLoader();
#elif UNITY_EDITOR
            var editorUploaderGameObject = GameObject.Find(nameof(EditorUploader));
            uploader = editorUploaderGameObject != null
                ? editorUploaderGameObject.GetComponent<EditorUploader>()
                : new GameObject(nameof(EditorUploader)).AddComponent<EditorUploader>();
#endif
            return uploader;
        }
    }
}