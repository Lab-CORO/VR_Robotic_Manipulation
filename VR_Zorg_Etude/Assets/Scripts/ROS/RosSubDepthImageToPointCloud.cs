using System;
using System.Collections;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

/// <summary>
/// Subscribe to depth image and camera info from ROS, convert to 3D point cloud using GPU compute shader,
/// and render using VFX Graph for high performance (30+ FPS for 1M points)
/// </summary>
public class RosSubDepthImageToPointCloud : MonoBehaviour
{
    #region ROS Configuration

    private ROSConnection _rosConnection;

    [Header("ROS Topics")]
    [Tooltip("Depth image topic (sensor_msgs/Image, encoding: 32FC1 or 16UC1)")]
    public string depthTopicName = "/camera/depth/image_raw";

    [Tooltip("Camera info topic (sensor_msgs/CameraInfo)")]
    public string cameraInfoTopicName = "/camera/depth/camera_info";

    [Tooltip("RGB image topic for future color support (sensor_msgs/CompressedImage)")]
    public string rgbTopicName = "/camera/rgb/image_raw/compressed";

    #endregion

    #region GPU Resources

    [Header("GPU Resources")]
    [Tooltip("Compute shader for depth to point cloud conversion")]
    public ComputeShader computeShader;

    private Texture2D _depthTexture;
    private Texture2D _rgbTexture;
    private GraphicsBuffer _positionBuffer;
    private GraphicsBuffer _colorBuffer;

    private int _kernelGeneratePointCloud;
    private int _kernelApplyRGBColors;

    private VisualEffect _visualEffect;

    #endregion

    #region Camera Intrinsics

    [Serializable]
    private struct CameraIntrinsics
    {
        public float fx;         // Focal length X
        public float fy;         // Focal length Y
        public float cx;         // Principal point X
        public float cy;         // Principal point Y
        public int width;        // Image width
        public int height;       // Image height
        public bool isValid;     // Have we received camera info?
    }

    private CameraIntrinsics _intrinsics;

    [Header("Mock Camera Intrinsics (if CameraInfo not available)")]
    [Tooltip("Use mock intrinsics if no CameraInfo received")]
    public bool useMockIntrinsics = false;
    public float mockFocalLength = 600f;
    public int mockWidth = 1024;
    public int mockHeight = 1024;

    #endregion

    #region Configuration

    [Header("Point Cloud Settings")]
    [Tooltip("Determine if the point cloud is visible")]
    [SerializeField] private bool isOpen = false;

    [Tooltip("Transform where the point cloud should be positioned")]
    [SerializeField] private Transform attachPoint;

    [Tooltip("Point size in world units")]
    [SerializeField, Range(0.001f, 0.1f)] private float pointSize = 0.01f;

    [Tooltip("Depth scale factor (convert depth units to meters)")]
    [SerializeField] private float depthScale = 1.0f;

    [Tooltip("Minimum valid depth (meters)")]
    [SerializeField] private float minDepth = 0.1f;

    [Tooltip("Maximum valid depth (meters)")]
    [SerializeField] private float maxDepth = 10.0f;

    [Tooltip("Downsampling factor (1=full res, 2=half res, 4=quarter res)")]
    [SerializeField, Range(1, 4)] private int downsampleFactor = 1;

    [Tooltip("Enable RGB color support (requires RGB topic)")]
    [SerializeField] private bool enableRGBColors = false;

    [Header("Debug Visualization")]
    [Tooltip("RawImage UI to display depth map for debugging")]
    [SerializeField] private RawImage depthDebugImage;

    [Tooltip("Show debug depth visualization")]
    [SerializeField] private bool showDepthDebug = false;

    #endregion

    #region State Management

    private bool _depthMessageReceived = false;
    private bool _depthMessageProcessing = false;
    private bool _rgbMessageReceived = false;
    private bool _needsVFXReinit = true;

    private int _pointCount;
    private int _currentWidth;
    private int _currentHeight;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Validate compute shader
        if (computeShader == null)
        {
            Debug.LogError("[PointCloud] Compute Shader not assigned!");
            enabled = false;
            return;
        }

        // Get VFX component
        _visualEffect = GetComponent<VisualEffect>();
        if (_visualEffect == null)
        {
            Debug.LogError("[PointCloud] VisualEffect component not found!");
            enabled = false;
            return;
        }

        // Initialize compute shader kernels
        _kernelGeneratePointCloud = computeShader.FindKernel("GeneratePointCloud");
        _kernelApplyRGBColors = computeShader.FindKernel("ApplyRGBColors");

        // Connect to ROS
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<ImageMsg>(depthTopicName, ReceivedDepthImage);
        _rosConnection.Subscribe<CameraInfoMsg>(cameraInfoTopicName, ReceivedCameraInfo);

        if (enableRGBColors)
        {
            _rosConnection.Subscribe<CompressedImageMsg>(rgbTopicName, ReceivedRGBImage);
        }

        Debug.Log($"[PointCloud] Subscribed to depth: {depthTopicName}, info: {cameraInfoTopicName}");

        // Use mock intrinsics if specified
        if (useMockIntrinsics)
        {
            InitializeMockIntrinsics();
        }

        // Start closed
        ClosePointCloud();
    }

    private void LateUpdate()
    {
        if (!isOpen || !gameObject.activeSelf) return;

        // Check if we have valid camera intrinsics
        if (!_intrinsics.isValid)
        {
            if (useMockIntrinsics)
            {
                InitializeMockIntrinsics();
            }
            else
            {
                return;
            }
        }

        // Check if we have new depth data
        if (!_depthMessageReceived) return;

        // Mark as processed so we don't re-dispatch every frame
        _depthMessageReceived = false;

        // Initialize GPU resources if needed
        if (_positionBuffer == null)
        {
            InitializeGPUResources();
        }

        // Dispatch compute shader to generate point cloud
        DispatchPointCloudGeneration();

        // Update VFX Graph
        UpdateVFXGraph();

        // Update transform
        if (attachPoint != null)
        {
            transform.position = attachPoint.position;
            transform.rotation = attachPoint.rotation;
        }
    }

    private void OnDestroy()
    {
        ReleaseGPUResources();
    }

    #endregion

    #region ROS Message Handlers

    private void ReceivedDepthImage(ImageMsg msg)
    {
        if (!isOpen || _depthMessageProcessing || !gameObject.activeSelf) return;

        _depthMessageProcessing = true;
        StartCoroutine(ProcessDepthImage(msg));
    }

    private void ReceivedCameraInfo(CameraInfoMsg msg)
    {
        if (_intrinsics.isValid) return;

        // Camera matrix K: [fx 0 cx; 0 fy cy; 0 0 1]
        _intrinsics.fx = (float)msg.k[0];
        _intrinsics.fy = (float)msg.k[4];
        _intrinsics.cx = (float)msg.k[2];
        _intrinsics.cy = (float)msg.k[5];
        _intrinsics.width = (int)msg.width;
        _intrinsics.height = (int)msg.height;
        _intrinsics.isValid = true;

        Debug.Log($"[PointCloud] Camera intrinsics: {_intrinsics.width}x{_intrinsics.height}, fx={_intrinsics.fx:F1}, fy={_intrinsics.fy:F1}");
    }

    private void ReceivedRGBImage(CompressedImageMsg msg)
    {
        if (!isOpen || !enableRGBColors) return;
        StartCoroutine(ProcessRGBImage(msg.data));
    }

    #endregion

    #region Image Processing Coroutines

    private IEnumerator ProcessDepthImage(ImageMsg msg)
    {
        yield return null;

        // Create texture if needed
        if (_depthTexture == null || _depthTexture.width != msg.width || _depthTexture.height != msg.height)
        {
            int width = (int)msg.width / downsampleFactor;
            int height = (int)msg.height / downsampleFactor;

            _depthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
            _depthTexture.filterMode = FilterMode.Point;
            _depthTexture.wrapMode = TextureWrapMode.Clamp;

            _currentWidth = width;
            _currentHeight = height;
            _pointCount = width * height;

            Debug.Log($"[PointCloud] Depth texture: {width}x{height} ({_pointCount} points)");

            ReleaseGPUResources();
        }

        if (msg.encoding == "32FC1")
        {
            _depthTexture.LoadRawTextureData(msg.data);
            _depthTexture.Apply();
        }
        else if (msg.encoding == "16UC1")
        {
            ConvertUInt16ToFloat(msg.data, msg.width, msg.height);
        }
        else
        {
            Debug.LogWarning($"[PointCloud] Unsupported depth encoding: {msg.encoding}");
        }

        yield return null;

        _depthMessageReceived = true;
        _depthMessageProcessing = false;

        UpdateDepthDebugImage();
    }

    private IEnumerator ProcessRGBImage(byte[] imageData)
    {
        yield return null;

        if (_rgbTexture == null)
        {
            _rgbTexture = new Texture2D(2, 2);
        }

        _rgbTexture.LoadImage(imageData);

        yield return null;

        _rgbMessageReceived = true;
    }

    #endregion

    #region GPU Processing

    private void InitializeGPUResources()
    {
        if (_pointCount == 0)
        {
            _pointCount = _currentWidth * _currentHeight;
        }

        _positionBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _pointCount,
            3 * sizeof(float)
        );

        if (enableRGBColors)
        {
            _colorBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                _pointCount,
                4 * sizeof(float)
            );
        }

        Debug.Log($"[PointCloud] GPU buffers created: {_pointCount} points ({_pointCount * 12 / 1024 / 1024}MB)");
    }

    private void DispatchPointCloudGeneration()
    {
        if (_depthTexture == null || _positionBuffer == null) return;

        float fxVal = _intrinsics.fx / downsampleFactor;
        float fyVal = _intrinsics.fy / downsampleFactor;
        float cxVal = _intrinsics.cx / downsampleFactor;
        float cyVal = _intrinsics.cy / downsampleFactor;

        computeShader.SetTexture(_kernelGeneratePointCloud, "_DepthTexture", _depthTexture);
        computeShader.SetBuffer(_kernelGeneratePointCloud, "_PositionBuffer", _positionBuffer);
        computeShader.SetFloat("_FocalLengthX", fxVal);
        computeShader.SetFloat("_FocalLengthY", fyVal);
        computeShader.SetFloat("_PrincipalPointX", cxVal);
        computeShader.SetFloat("_PrincipalPointY", cyVal);
        computeShader.SetFloat("_DepthScale", depthScale);
        computeShader.SetInt("_Width", _currentWidth);
        computeShader.SetInt("_Height", _currentHeight);
        computeShader.SetFloat("_MinDepth", minDepth);
        computeShader.SetFloat("_MaxDepth", maxDepth);

        int threadGroupsX = Mathf.CeilToInt(_currentWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(_currentHeight / 8.0f);
        computeShader.Dispatch(_kernelGeneratePointCloud, threadGroupsX, threadGroupsY, 1);

        if (enableRGBColors && _rgbMessageReceived && _colorBuffer != null && _rgbTexture != null)
        {
            computeShader.SetTexture(_kernelApplyRGBColors, "_RGBTexture", _rgbTexture);
            computeShader.SetBuffer(_kernelApplyRGBColors, "_ColorBuffer", _colorBuffer);
            computeShader.Dispatch(_kernelApplyRGBColors, threadGroupsX, threadGroupsY, 1);
        }
    }

    private void UpdateVFXGraph()
    {
        if (_visualEffect == null || _positionBuffer == null) return;

        _visualEffect.SetGraphicsBuffer("PositionBuffer", _positionBuffer);
        _visualEffect.SetInt("ParticleCount", _pointCount);
        _visualEffect.SetFloat("PointSize", pointSize);

        if (enableRGBColors && _colorBuffer != null)
        {
            _visualEffect.SetGraphicsBuffer("ColorBuffer", _colorBuffer);
        }

        // Reinit VFX after first buffer setup so particles read actual data
        if (_needsVFXReinit)
        {
            _needsVFXReinit = false;
            StartCoroutine(DelayedVFXReinit());
        }
    }

    private IEnumerator DelayedVFXReinit()
    {
        yield return null;

        if (_visualEffect != null)
        {
            _visualEffect.SetGraphicsBuffer("PositionBuffer", _positionBuffer);
            _visualEffect.SetInt("ParticleCount", _pointCount);
            _visualEffect.SetFloat("PointSize", pointSize);
            _visualEffect.Reinit();
            Debug.Log($"[PointCloud] VFX initialized with {_pointCount} points");
        }
    }

    private void ReleaseGPUResources()
    {
        _positionBuffer?.Release();
        _positionBuffer = null;

        _colorBuffer?.Release();
        _colorBuffer = null;
    }

    #endregion

    #region Utility Methods

    private void ConvertUInt16ToFloat(byte[] data, uint width, uint height)
    {
        int pixelCount = (int)(width * height);
        float[] floatData = new float[pixelCount];

        for (int i = 0; i < pixelCount; i++)
        {
            ushort depthMM = BitConverter.ToUInt16(data, i * 2);
            floatData[i] = depthMM / 1000.0f;
        }

        byte[] byteData = new byte[pixelCount * sizeof(float)];
        Buffer.BlockCopy(floatData, 0, byteData, 0, byteData.Length);

        _depthTexture.LoadRawTextureData(byteData);
        _depthTexture.Apply();
    }

    private void InitializeMockIntrinsics()
    {
        _intrinsics.fx = mockFocalLength;
        _intrinsics.fy = mockFocalLength;
        _intrinsics.cx = mockWidth / 2.0f;
        _intrinsics.cy = mockHeight / 2.0f;
        _intrinsics.width = mockWidth;
        _intrinsics.height = mockHeight;
        _intrinsics.isValid = true;
    }

    [ContextMenu("Generate Mock Depth Data")]
    public void GenerateMockDepthData()
    {
        if (!useMockIntrinsics)
        {
            Debug.LogWarning("[PointCloud] Enable 'Use Mock Intrinsics' first!");
            return;
        }

        InitializeMockIntrinsics();

        _currentWidth = mockWidth;
        _currentHeight = mockHeight;
        _pointCount = mockWidth * mockHeight;

        _depthTexture = new Texture2D(mockWidth, mockHeight, TextureFormat.RFloat, false);

        for (int y = 0; y < mockHeight; y++)
        {
            for (int x = 0; x < mockWidth; x++)
            {
                float dx = (x - mockWidth / 2.0f) / (mockWidth / 2.0f);
                float dy = (y - mockHeight / 2.0f) / (mockHeight / 2.0f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float depth = dist < 0.5f ? 2.0f - dist * 2.0f : 0f;

                _depthTexture.SetPixel(x, y, new Color(depth, 0, 0, 1));
            }
        }

        _depthTexture.Apply();
        _depthMessageReceived = true;

        UpdateDepthDebugImage();

        Debug.Log("[PointCloud] Mock depth data generated");

        OpenPointCloud();
    }

    #endregion

    #region Debug Visualization

    private void UpdateDepthDebugImage()
    {
        if (!showDepthDebug || depthDebugImage == null || _depthTexture == null)
            return;

        int w = _depthTexture.width;
        int h = _depthTexture.height;

        Texture2D debugTex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float d = _depthTexture.GetPixel(x, y).r;
                if (d > 0)
                {
                    minVal = Mathf.Min(minVal, d);
                    maxVal = Mathf.Max(maxVal, d);
                }
            }
        }

        float range = maxVal - minVal;
        if (range < 0.001f) range = 1f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float d = _depthTexture.GetPixel(x, y).r;
                if (d <= 0)
                {
                    debugTex.SetPixel(x, y, Color.black);
                }
                else
                {
                    float normalized = (d - minVal) / range;
                    debugTex.SetPixel(x, y, new Color(normalized, normalized, normalized, 1f));
                }
            }
        }

        debugTex.Apply();
        depthDebugImage.texture = debugTex;
        depthDebugImage.gameObject.SetActive(true);
    }

    #endregion

    #region UI Integration

    private void OpenPointCloud()
    {
        isOpen = true;
        gameObject.SetActive(true);

        if (_visualEffect != null)
        {
            _visualEffect.enabled = true;
        }
    }

    private void ClosePointCloud()
    {
        isOpen = false;
        gameObject.SetActive(false);

        if (_visualEffect != null)
        {
            _visualEffect.enabled = false;
        }
    }

    public void TogglePointCloud(Toggle toggle)
    {
        ManagePointCloud(toggle.isOn);
    }

    public void ManagePointCloud(bool state)
    {
        if (state)
        {
            OpenPointCloud();
        }
        else
        {
            ClosePointCloud();
        }
    }

    #endregion
}
