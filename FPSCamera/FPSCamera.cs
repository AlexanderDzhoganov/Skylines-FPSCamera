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

        public float cameraMoveSpeed = 128.0f;
        private bool fpsModeEnabled = false;
        private CameraController controller;
        float rotationY = 0f;

        private static void ShowHideUI(bool show)
        {
        }

        public static void SetMode(bool fpsMode)
        {
            instance.fpsModeEnabled = fpsMode;
            ShowHideUI(!instance.fpsModeEnabled);

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

        public static bool IsEnabled()
        {
            return instance.fpsModeEnabled;
        }

        void Update()
        {
            if (fpsModeEnabled)
            {
                if(Input.GetKeyDown(KeyCode.Tab))
                {
                    SetMode(false);
                    return;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    gameObject.transform.position += gameObject.transform.forward * cameraMoveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    gameObject.transform.position -= gameObject.transform.forward * cameraMoveSpeed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    gameObject.transform.position -= gameObject.transform.right * cameraMoveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    gameObject.transform.position += gameObject.transform.right * cameraMoveSpeed * Time.deltaTime;
                }

                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") ;
                rotationY += Input.GetAxis("Mouse Y");
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
        }

    }

}
