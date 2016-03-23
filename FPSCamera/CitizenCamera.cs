using UnityEngine;

namespace FPSCamera
{

    public class CitizenCamera : MonoBehaviour , IFollowCamera
    {
        private uint followInstance;
        private bool following = false;
        public bool inVehicle = false;

        private CameraController cameraController;
        private Camera camera;

        private CitizenManager cManager;

        private float cameraOffsetForward = 0.2f;
        private float cameraOffsetUp = 1.5f;

        private Vector3 userOffset = Vector3.zero;

        public Vector3 UserOffset
        {
            get
            {
                return userOffset;
            }

            set
            {
                userOffset = value;
            }
        }

        public bool Following
        {
            get
            {
                return following;
            }

            set
            {
                following = value;
            }
        }

        public void SetFollowInstance(uint instance)
        {
            FPSCamera.instance.SetMode(false);
            followInstance = instance;
            following = true;
            camera.nearClipPlane = 0.1f;
            cameraController.enabled = false;
            camera.fieldOfView = FPSCamera.instance.config.fieldOfView;
            FPSCamera.onCameraModeChanged(true);
        }

        public void StopFollowing()
        {
            following = false;
            cameraController.enabled = true;
            camera.nearClipPlane = 1.0f;
            FPSCamera.onCameraModeChanged(false);
            userOffset = Vector3.zero;
            camera.fieldOfView = FPSCamera.instance.originalFieldOfView;

            if (FPSCamera.instance.hideUIComponent != null && FPSCamera.instance.config.integrateHideUI)
            {
                FPSCamera.instance.hideUIComponent.SendMessage("Show");
            }
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

                var flags = cManager.m_instances.m_buffer[i].m_flags;
                if ((flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) != CitizenInstance.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((flags & CitizenInstance.Flags.EnteringVehicle) != 0)
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

                var pos = position +
                          forward*cameraOffsetForward +
                          up*cameraOffsetUp;
                camera.transform.position = pos +
                                            userOffset;
                Vector3 lookAt = pos + (orientation * Vector3.forward) * 1.0f;
                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime*2.0f);
            }
        }

    }

}
