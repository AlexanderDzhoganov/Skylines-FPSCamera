using UnityEngine;

namespace FPSCamera
{
    interface IFollowCamera
    {
        Vector3 UserOffset { get; set; }
        bool Following { get; set; }

        void SetFollowInstance(uint instance);
        void StopFollowing();
    }
}
