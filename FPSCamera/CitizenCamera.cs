using UnityEngine;

namespace FPSCamera
{

    public class CitizenCamera : MonoBehaviour
    {
        private uint followInstance;
        public bool following = false;
        public bool inVehicle = false;

        private CameraController cameraController;
        private Camera camera;

        private CitizenManager cManager;

        private float cameraOffsetForward = 0.2f;
        private float cameraOffsetUp = 1.5f;

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            camera.nearClipPlane = 0.1f;
            cameraController.enabled = false;
            FPSCamera.onCameraModeChanged(true);
        }

        public void StopFollowing()
        {
            following = false;
            cameraController.enabled = true;
            camera.nearClipPlane = 1.0f;
            FPSCamera.onCameraModeChanged(false);
        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            cManager = CitizenManager.instance;
        }

        void Update()
        {
            if (following)
            {
                var citizen = cManager.m_citizens.m_buffer[followInstance];
                var i = citizen.m_instance;

                if ((cManager.m_instances.m_buffer[i].m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                CitizenInstance c = cManager.m_instances.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                c.GetSmoothPosition((ushort)i, out position, out orientation);

                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                camera.transform.position = position +
                                            forward*cameraOffsetForward +
                                            up*cameraOffsetUp;
                Vector3 lookAt = position + (orientation * Vector3.forward) * 64.0f;
                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime*2.0f);
            }
        }

    }

}
