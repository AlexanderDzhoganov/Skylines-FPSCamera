using UnityEngine;

namespace FPSCamera
{

    public class VehicleCamera : MonoBehaviour
    {
        private ushort followInstance;
        public bool following = false;
        private CameraController cameraController;
        private Camera camera;

        private VehicleManager vManager;

        private float cameraOffsetForward = 2.75f;
        private float cameraOffsetForwardLargeVehicle = 4.0f;
        private float cameraOffsetUp = 1.5f;

        private Vehicle currentVehicle;

        private Vector3 GetCameraOffsetForVehicleType(Vehicle v, Vector3 forward, Vector3 up)
        {
            currentVehicle = v;

            var offset = forward * v.Info.m_attachOffsetFront +
                         forward * cameraOffsetForward +
                         up * cameraOffsetUp;

            if (v.m_leadingVehicle != 0)
            {
                offset += up*3.0f;
                offset -= forward*2.0f;
            }
            else if(v.Info.name == "Train Engine")
            {
                offset += forward * 2.0f;
            }


            return offset;
        }

        public void SetFollowInstance(ushort instance)
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

            if (FPSCamera.instance.hideUIComponent != null && FPSCamera.instance.config.integrateHideUI)
            {
                FPSCamera.instance.hideUIComponent.SendMessage("Show");
            }
        }

        void Awake()
        {
            cameraController = GetComponent<CameraController>();
            camera = GetComponent<Camera>();
            vManager = VehicleManager.instance;
        }

        void Update()
        {
            if(following)
            {
                var i = followInstance;

                if ((vManager.m_vehicles.m_buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    StopFollowing();
                    return;
                }

                if ((vManager.m_vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    StopFollowing();
                    return;
                }

                Vehicle v = vManager.m_vehicles.m_buffer[i];
                Vector3 position = Vector3.zero;
                Quaternion orientation = Quaternion.identity;
                v.GetSmoothPosition((ushort)i, out position, out orientation);

                Vector3 forward = orientation * Vector3.forward;
                Vector3 up = orientation * Vector3.up;

                camera.transform.position = position + GetCameraOffsetForVehicleType(v, forward, up);
                Vector3 lookAt = position + (orientation * Vector3.forward) * 64.0f;

                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime * 3.0f);
            }
        }

    }

}
