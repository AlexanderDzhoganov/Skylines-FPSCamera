using System;
using System.Resources;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace FPSCamera
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get { return "First Person Camera"; }
        }

        public string Description
        {
            get { return "See your city from a different perspective"; }
        }

    }

    public class ModTerrainUtil : TerrainExtensionBase
    {

        private static ITerrain terrain = null;

        public static float GetHeight(float x, float z)
        {
            if (terrain == null)
            {
                return 0.0f;
            }

            return terrain.SampleTerrainHeight(x, z);
        }

        public override void OnCreated(ITerrain _terrain)
        {
            terrain = _terrain;
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
       
        public override void OnLevelLoaded(LoadMode mode)
        {
            FPSCamera.Initialize(mode);
        }

        public override void OnLevelUnloading()
        {
            FPSCamera.Deinitialize();
        }

    }

}
