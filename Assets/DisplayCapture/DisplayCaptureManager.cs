using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Anaglyph.DisplayCapture {
    [DefaultExecutionOrder(-1000)]
    public class DisplayCaptureManager : MonoBehaviour {
        public static DisplayCaptureManager Instance { get; private set; }

        public bool startScreenCaptureOnStart = true;
        public bool flipTextureOnGPU;
        public RawImage previewImage;
        private bool captureSingleFrame = false;
        

        [SerializeField] private Vector2Int textureSize = new(1024, 1024);
        public Vector2Int Size => textureSize;
        

        public Texture2D ScreenCaptureTexture { get; private set; } // THIS IS WHAT WE WANT
        public Texture2D PhotoTexture { get; private set; } // THIS IS WHAT WE WANT

        private RenderTexture flipTexture;

        public Matrix4x4 ProjectionMatrix { get; private set; }

        public UnityEvent<Texture2D> onTextureInitialized = new();
        public UnityEvent onStarted = new();
        public UnityEvent onPermissionDenied = new();
        public UnityEvent onStopped = new();
        public UnityEvent onNewFrame = new();

        private unsafe sbyte* imageData;
        private int bufferSize;

        private class AndroidInterface {
            private readonly AndroidJavaClass androidClass;
            private readonly AndroidJavaObject androidInstance;

            public AndroidInterface(GameObject messageReceiver, int textureWidth, int textureHeight) {
                androidClass = new AndroidJavaClass("com.trev3d.DisplayCapture.DisplayCaptureManager");
                androidInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
                androidInstance.Call("setup", messageReceiver.name, textureWidth, textureHeight);
            }

            public void RequestCapture() {
                androidInstance.Call("requestCapture");
            }

            public void StopCapture() {
                androidInstance.Call("stopCapture");
            }

            public unsafe sbyte* GetByteBuffer() {
                var byteBuffer = androidInstance.Call<AndroidJavaObject>("getByteBuffer");
                return AndroidJNI.GetDirectBufferAddress(byteBuffer.GetRawObject());
            }
        }

        private AndroidInterface androidInterface;

        private void Awake() {
            Instance = this;
            androidInterface = new AndroidInterface(gameObject, Size.x, Size.y);
            ScreenCaptureTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, 1, false);
            PhotoTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, 1, false);
        }

        private void Start() {
            flipTexture = new RenderTexture(Size.x, Size.y, 1, RenderTextureFormat.ARGB32, 1);
            flipTexture.Create();

            // onTextureInitialized.Invoke(ScreenCaptureTexture);
            onTextureInitialized.Invoke(PhotoTexture);

            if (startScreenCaptureOnStart) StartScreenCapture();
            bufferSize = Size.x * Size.y * 4; // RGBA_8888 format: 4 bytes per pixel
        }

        public void StartScreenCapture() {
            androidInterface.RequestCapture();
        }

        public void StopScreenCapture() {
            androidInterface.StopCapture();
        }

        public void TakeScreenCapture() {
            captureSingleFrame = true;
        }

        public void AddtextureToImage(Texture2D texture) {
            if (texture == null) Debug.Log("texture is null");
            previewImage.texture = texture;
        }

        private void SaveFrame(Texture2D texture) {
            var bytes = texture.EncodeToPNG();
            var path = $"{Application.persistentDataPath}/CapturedFrame.png";
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Frame saved to: {path}");
        }
        

        // Messages sent from Android

#pragma warning disable IDE0051 // Remove unused private members
        private unsafe void OnCaptureStarted() {
            onStarted.Invoke();
            imageData = androidInterface.GetByteBuffer();
        }

        private void OnPermissionDenied() {
            onPermissionDenied.Invoke();
        }

        private unsafe void OnNewFrameAvailable() {
            if (imageData == default) return;

            // Create a temporary Texture2D for flipping
            Texture2D tempTexture = new Texture2D(Size.x, Size.y, TextureFormat.RGBA32, false);

            // Load raw data into the temporary texture
            tempTexture.LoadRawTextureData((IntPtr)imageData, bufferSize);
            tempTexture.Apply();

            // Flip the texture on GPU if required
            if (flipTextureOnGPU) {
                Graphics.Blit(tempTexture, flipTexture, new Vector2(1, -1), Vector2.zero);

                // Read the flipped texture back into tempTexture
                RenderTexture.active = flipTexture;
                tempTexture.ReadPixels(new Rect(0, 0, flipTexture.width, flipTexture.height), 0, 0);
                tempTexture.Apply();
                RenderTexture.active = null;

                Debug.Log("Flipped Temp Texture with ReadPixels");
            }

            // Handle single-frame capture using the flipped temporary texture
            if (captureSingleFrame) {
                Debug.Log("Capturing single frame");
                Graphics.CopyTexture(tempTexture, PhotoTexture);
                PhotoTexture.Apply();
                AddtextureToImage(PhotoTexture);
                SaveFrame(PhotoTexture);
                captureSingleFrame = false;
            }

            // Cleanup temporary texture (optional, if it's not reused)
            Destroy(tempTexture);

            // Notify listeners that a new frame is available
            onNewFrame.Invoke();
        }


        private void OnCaptureStopped() {
            onStopped.Invoke();
        }
#pragma warning restore IDE0051 // Remove unused private members

        public String convertCapturetoBase64() {
            if (PhotoTexture != null) {
                byte[] pngPhoto = PhotoTexture.EncodeToPNG();
                return Convert.ToBase64String(pngPhoto);
            }
            print("PhotoTexture is null");
            return "";
        }
    }
}