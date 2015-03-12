using ColossalFramework;
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
        }

        public static FPSCamera instance;

        public static readonly string configPath = "FPSCameraConfig.xml";
        public Configuration config;

        private bool fpsModeEnabled = false;
        private CameraController controller;
        float rotationY = 0f;

        private bool showUI = false;
        private Rect configWindowRect = new Rect(Screen.width - 400 - 128, 100, 400, 190);

        private bool waitingForHotkey = false;

        void Awake()
        {
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
            }

            SaveConfig();
        }

        void SaveConfig()
        {
            Configuration.Serialize(configPath, config);
        }

        void OnGUI()
        {
            if (showUI)
            {
                configWindowRect = GUI.Window(21521, configWindowRect, DoConfigWindow, "FPS Camera configuration");
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
            config.groundOffset = GUILayout.HorizontalSlider(config.groundOffset, 2.0f, 32.0f, GUILayout.Width(200));
            GUILayout.Label(config.groundOffset.ToString("0.00"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Save configuration"))
            {
                SaveConfig();
            }
        }

        public static void SetMode(bool fpsMode)
        {
            instance.fpsModeEnabled = fpsMode;

            if (instance.fpsModeEnabled)
            {
                instance.controller.enabled = false;
                Cursor.visible = false;
                instance.rotationY = -instance.transform.localEulerAngles.x;
            }
            else
            {
                instance.controller.enabled = true;
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

        void Update()
        {
            if (fpsModeEnabled)
            {
                if (Input.GetKeyDown(config.toggleFPSCameraHotkey))
                {
                    SetMode(false);
                    return;
                }

                if (config.snapToGround)
                {
                    var pos = gameObject.transform.position;
                    float y = ModTerrainUtil.GetHeight(pos.x, pos.z);
                    gameObject.transform.position = new Vector3(pos.x, y + config.groundOffset, pos.z);
                }

                float speedFactor = 1.0f;
                if (config.limitSpeedGround)
                {
                    var pos = gameObject.transform.position;
                    float y = ModTerrainUtil.GetHeight(pos.x, pos.z);
                    speedFactor *= Mathf.Sqrt(y);
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

                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * config.cameraRotationSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * config.cameraRotationSensitivity;
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

                Camera.main.fieldOfView = config.fieldOfView;
                Camera.main.nearClipPlane = 1.0f;
            }

            if (Input.GetKeyDown(config.toggleFPSCameraHotkey))
            {
                SetMode(true);
            }
        }

    }

}
