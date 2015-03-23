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

        public void SetFollowInstance(ushort instance)
        {
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
                
                camera.transform.position = position +
                    forward * v.Info.m_attachOffsetFront +
                    forward * cameraOffsetForward +
                    up * cameraOffsetUp +
                    (v.Info.m_isLargeVehicle ? forward * cameraOffsetForwardLargeVehicle : Vector3.zero);
                Vector3 lookAt = position + (orientation * Vector3.forward) * 64.0f;

                var currentOrientation = camera.transform.rotation;
                camera.transform.LookAt(lookAt, Vector3.up);
                camera.transform.rotation = Quaternion.Slerp(currentOrientation, camera.transform.rotation,
                    Time.deltaTime * 3.0f);
            }
        }

    }

}
