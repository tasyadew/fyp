using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using System.Collections.Generic;
using System.Linq;
using TMPro;


public class QrCodeRecenter : MonoBehaviour
{

    [SerializeField]
    private ARSession session;
    [SerializeField]
    private XROrigin sessionOrigin;
    [SerializeField]
    private ARCameraManager cameraManager;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private TargetHandler targetHandler;

    [SerializeField]
    private GameObject qrCodeScanningPanel;

    private Texture2D cameraImageTexture;
    private IBarcodeReader reader = new BarcodeReader(); // create a barcode reader instance
    private bool scanningEnabled = false;
    public bool hasScanned = false;

    [SerializeField]
    private TMP_Text debugText;

    [SerializeField]
    private GameObject infoTextBox;
    private TMP_Text infoText;

    private void Start()
    {
        // Hide the tip for user after 5 seconds
        infoText = infoTextBox.transform.GetChild(0).GetComponent<TMP_Text>();
        Invoke("DisableInfoText", 5);
    }

    private void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {

        if (!scanningEnabled)
        {
            return;
        }

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        // Detect and decode the barcode inside the bitmap
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        // Do something with the result
        if (result != null)
        {
            targetHandler.onSwapStartPointButtonClicked(result.Text);
            SetQrCodeRecenterTarget(result.Text, true);
            ToggleScanning();
            debugText.text = System.DateTime.Now.ToString("HH:mm:ss") + " => " + result.Text;
            result = null;

            if (!hasScanned)
            {
                targetHandler.setActivePathfinding();
                hasScanned = true;
            }
        }
    }

    private void DisableInfoText()
    {
        infoTextBox.gameObject.SetActive(false);
    }

    void Update()
    {
        //debugText.text = System.DateTime.Now.ToString("HH:mm:ss") + " => " + mainCamera.transform.rotation;
        debugText.text = System.DateTime.Now.ToString("HH:mm:ss") + " => " + sessionOrigin.CameraInOriginSpacePos + "\n" + sessionOrigin.OriginInCameraSpacePos;
    }

    public void SetQrCodeRecenterTarget(string targetText, bool isStartPoint)
    {
        TargetFacade currentTarget = targetHandler.GetCurrentTargetByTargetText(targetText);
        if (currentTarget != null)
        {
            if (isStartPoint)
            {
                session.Reset();
                sessionOrigin.transform.position = currentTarget.transform.position;
                sessionOrigin.transform.rotation = currentTarget.transform.rotation;

                // Cancel the previous invoke if there is any
                CancelInvoke("DisableInfoText");

                // Update the info text
                infoTextBox.gameObject.SetActive(true);
                if (targetText == "StartPoint1")
                {
                    infoText.text = "Starting in front of Wellness!";
                }
                else if (targetText == "StartPoint2")
                {
                    infoText.text = "Starting next to escalator!";
                }
                else
                {
                    infoText.text = "Starting at " + targetText + "!";
                }

                // Call function to hide the info text after waiting for 3 seconds
                Invoke("DisableInfoText", 3);
            }
            else
            {
                //sessionOrigin.transform.position = currentTarget.transform.position;
                sessionOrigin.MoveCameraToWorldLocation(currentTarget.transform.position);
                // Move ARSession back to the origin without resetting
                // session.GetComponent<Camera>().transform.position = currentTarget.transform.position;
            }
        }
    }

    public void ChangeActiveFloor(string floorEntrance)
    {
        SetQrCodeRecenterTarget(floorEntrance, true);
        if (!hasScanned)
        {
            targetHandler.setActivePathfinding();
            hasScanned = true;
        }
    }

    public void ToggleScanning()
    {
        scanningEnabled = !scanningEnabled;
        qrCodeScanningPanel.SetActive(scanningEnabled);
    }
}
