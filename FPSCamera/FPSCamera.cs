using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamera
{
    public class FPSCamera : MonoBehaviour
    {

        public delegate void OnCameraModeChanged(bool state);

        public static OnCameraModeChanged onCameraModeChanged;

        public static void Initialize()
        {
            var controller = GameObject.FindObjectOfType<CameraController>();
            instance = controller.gameObject.AddComponent<FPSCamera>();
            instance.controller = controller;
            instance.camera = controller.GetComponent<Camera>();
        }

        public static FPSCamera instance;

        public static readonly string configPath = "FPSCameraConfig.xml";
        public Configuration config;

        private bool fpsModeEnabled = false;
        private CameraController controller;
        private Camera camera;
        float rotationY = 0f;

        private bool showUI = false;
        private Rect configWindowRect = new Rect(Screen.width - 400 - 128, 100, 400, 300);

        private bool waitingForHotkey = false;

        private Vector3 mainCameraPosition;
        private Quaternion mainCameraOrientation;

        private Vector3 fpsCameraPosition;
        private Quaternion fpsCameraOrientation;

        private TerrainManager terrainManager;
        private NetManager netManager;

        private bool initPositions = false;

        private bool firstFpsMode = false;

        private Texture2D bgTexture;
        private GUISkin skin;

        void Awake()
        {
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
            }

            SaveConfig();

            mainCameraPosition = gameObject.transform.position;
            mainCameraOrientation = gameObject.transform.rotation;

            fpsCameraPosition = gameObject.transform.position;
            fpsCameraOrientation = gameObject.transform.rotation;

            terrainManager = Singleton<TerrainManager>.instance;
            netManager = Singleton<NetManager>.instance;

            bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, Color.grey);
            bgTexture.Apply();
        }

        void SaveConfig()
        {
            Configuration.Serialize(configPath, config);
        }

        void OnGUI()
        {
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<GUISkin>();
                skin.box = new GUIStyle(GUI.skin.box);
                skin.button = new GUIStyle(GUI.skin.button);
                skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                skin.label = new GUIStyle(GUI.skin.label);
                skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                skin.textArea = new GUIStyle(GUI.skin.textArea);
                skin.textField = new GUIStyle(GUI.skin.textField);
                skin.toggle = new GUIStyle(GUI.skin.toggle);
                skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                skin.window = new GUIStyle(GUI.skin.window);
                skin.window.normal.background = bgTexture;
                skin.window.onNormal.background = bgTexture;
            }

            if (showUI)
            {
                var oldSkin = GUI.skin;
                GUI.skin = skin;
                configWindowRect = GUI.Window(21521, configWindowRect, DoConfigWindow, "FPS Camera configuration");
                GUI.skin = oldSkin;
            }
        }

        void DoConfigWindow(int wnd)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Hotkey to toggle first-person:");
            GUILayout.FlexibleSpace();

            string label = config.toggleFPSCameraHotkey.ToString();
            if (waitingForHotkey)
            {
                label = "Waiting";

                if (Event.current.type == EventType.KeyDown)
                {
                    waitingForHotkey = false;
                    config.toggleFPSCameraHotkey = Event.current.keyCode;
                }
            }

            if (GUILayout.Button(label, GUILayout.Width(128)))
            {
                if (!waitingForHotkey)
                {
                    waitingForHotkey = true;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Field of view: ");
            config.fieldOfView = GUILayout.HorizontalSlider(config.fieldOfView, 30.0f, 120.0f, GUILayout.Width(200));
            Camera.main.fieldOfView = config.fieldOfView;
            GUILayout.Label(config.fieldOfView.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Movement speed: ");
            config.cameraMoveSpeed = GUILayout.HorizontalSlider(config.cameraMoveSpeed, 1.0f, 128.0f, GUILayout.Width(200));
            GUILayout.Label(config.cameraMoveSpeed.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Sensitivity: ");
            config.cameraRotationSensitivity = GUILayout.HorizontalSlider(config.cameraRotationSensitivity, 0.1f, 2.0f, GUILayout.Width(200));
            GUILayout.Label(config.cameraRotationSensitivity.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Snap to ground: ");
            config.snapToGround = GUILayout.Toggle(config.snapToGround, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset from ground: ");
            config.groundOffset = GUILayout.HorizontalSlider(config.groundOffset, 0.25f, 32.0f, GUILayout.Width(200));
            GUILayout.Label(config.groundOffset.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prevent ground clipping: ");
            config.preventClipGround = GUILayout.Toggle(config.preventClipGround, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Animated transitions: ");
            config.animateTransitions = GUILayout.Toggle(config.animateTransitions, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Animation speed: ");
            config.animationSpeed = GUILayout.HorizontalSlider(config.animationSpeed, 0.1f, 4.0f, GUILayout.Width(200));
            GUILayout.Label(config.animationSpeed.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Save configuration"))
            {
                SaveConfig();
            }

            if (GUILayout.Button("Reset configuration"))
            {
                config = new Configuration();
                SaveConfig();
            }
        }

        private bool inModeTransition = false;
        private Vector3 transitionTargetPosition = Vector3.zero;
        private Quaternion transitionTargetOrientation = Quaternion.identity;
        private Vector3 transitionStartPosition = Vector3.zero;
        private Quaternion transitionStartOrientation = Quaternion.identity;
        private float transitionT = 0.0f;

        public void SetMode(bool fpsMode)
        {
            instance.fpsModeEnabled = fpsMode;

            if (instance.fpsModeEnabled)
            {
                if (!firstFpsMode)
                {
                    fpsCameraPosition = mainCameraPosition;
                    fpsCameraOrientation = mainCameraOrientation;
                }

                firstFpsMode = true;

                instance.controller.enabled = false;
                Cursor.visible = false;
                if (!config.animateTransitions)
                {
                    instance.rotationY = -instance.transform.localEulerAngles.x;
                }
            }
            else
            {
                if (!config.animateTransitions)
                {
                    instance.controller.enabled = true;
                }
                Cursor.visible = true;
            }

            if (onCameraModeChanged != null)
            {
                onCameraModeChanged(fpsMode);
            }
        }

        public static void ToggleUI()
        {
            instance.showUI = !instance.showUI;
        }

        public static KeyCode GetToggleUIKey()
        {
            return instance.config.toggleFPSCameraHotkey;
        }

        public static bool IsEnabled()
        {
            return instance.fpsModeEnabled;
        }

        private Vector3 CalcFPSCameraTargetPosition()
        {
            return fpsCameraPosition;
        }

        private Quaternion CalcFPSCameraTargetOrientiation()
        {
            return fpsCameraOrientation;
        }

        void Update()
        {
            if (!initPositions)
            {
                mainCameraPosition = gameObject.transform.position;
                mainCameraOrientation = gameObject.transform.rotation;

                fpsCameraPosition = gameObject.transform.position;
                fpsCameraOrientation = gameObject.transform.rotation;

                rotationY = -instance.transform.localEulerAngles.x;
                initPositions = true;
            }

            if (Input.GetKeyDown(config.toggleFPSCameraHotkey))
            {
                if (config.animateTransitions)
                {
                    inModeTransition = true;
                    transitionT = 0.0f;

                    transitionStartPosition = gameObject.transform.position;
                    transitionStartOrientation = gameObject.transform.rotation;

                    if (fpsModeEnabled)
                    {
                        transitionTargetPosition = mainCameraPosition;
                        transitionTargetOrientation = mainCameraOrientation;
                    }
                    else
                    {
                        transitionTargetPosition = CalcFPSCameraTargetPosition();
                        transitionTargetOrientation = CalcFPSCameraTargetOrientiation();
                    }
                }

                SetMode(!fpsModeEnabled);
            }

            if (config.animateTransitions && inModeTransition)
            {
                transitionT += Time.deltaTime * config.animationSpeed;

                gameObject.transform.position = Vector3.Slerp(transitionStartPosition, transitionTargetPosition, transitionT);
                gameObject.transform.rotation = Quaternion.Slerp(transitionStartOrientation, transitionTargetOrientation, transitionT);

                if (transitionT >= 1.0f)
                {
                    inModeTransition = false;

                    if (!fpsModeEnabled)
                    {
                        instance.controller.enabled = true;
                    }
                }
            }
            else if (fpsModeEnabled)
            {
                var pos = gameObject.transform.position;
                float terrainY = terrainManager.SampleDetailHeight(gameObject.transform.position);
                float waterY = terrainManager.WaterLevel(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
                terrainY = Mathf.Max(terrainY, waterY);

                if (config.snapToGround)
                {
                    Segment3 ray = new Segment3(gameObject.transform.position + new Vector3(0f, 1.5f, 0f), gameObject.transform.position + new Vector3(0f, -1000f, 0f));

                    Vector3 hitPos;
                    ushort nodeIndex;
                    ushort segmentIndex;
                    Vector3 hitPos2;

                    if (netManager.RayCast(ray, 0f, ItemClass.Service.Road, ItemClass.Service.PublicTransport, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos, out nodeIndex, out segmentIndex)
                        | netManager.RayCast(ray, 0f, ItemClass.Service.Beautification, ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.SubService.None, ItemClass.Layer.Default, ItemClass.Layer.None, NetNode.Flags.None, NetSegment.Flags.None, out hitPos2, out nodeIndex, out segmentIndex))
                    {
                        terrainY = Mathf.Max(terrainY, Mathf.Max(hitPos.y, hitPos2.y));
                    }

                    gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(pos.x, terrainY + config.groundOffset, pos.z), 0.9f);
                }

                float speedFactor = 1.0f;
                if (config.limitSpeedGround)
                {
                    speedFactor *= Mathf.Sqrt(terrainY);
                    speedFactor = Mathf.Clamp(speedFactor, 1.0f, 256.0f);
                }

                if (Input.GetKey(KeyCode.W))
                {
                    gameObject.transform.position += gameObject.transform.forward * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    gameObject.transform.position -= gameObject.transform.forward * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    gameObject.transform.position -= gameObject.transform.right * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    gameObject.transform.position += gameObject.transform.right * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    gameObject.transform.position -= gameObject.transform.up * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    gameObject.transform.position += gameObject.transform.up * config.cameraMoveSpeed * speedFactor * Time.deltaTime;
                }

                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * config.cameraRotationSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * config.cameraRotationSensitivity;
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

                if (config.preventClipGround)
                {
                    if (transform.position.y < terrainY + config.groundOffset)
                    {
                        transform.position = new Vector3(transform.position.x, terrainY + config.groundOffset, transform.position.z);
                    }
                }

                camera.fieldOfView = config.fieldOfView;
                camera.nearClipPlane = 1.0f;

                fpsCameraPosition = gameObject.transform.position;
                fpsCameraOrientation = gameObject.transform.rotation;
            }
            else
            {
                mainCameraPosition = gameObject.transform.position;
                mainCameraOrientation = gameObject.transform.rotation;
            }
        }

    }

}
