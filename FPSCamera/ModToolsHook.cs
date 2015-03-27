namespace FPSCamera
{
    class ModToolsHook : ModTools.IModEntryPoint
    {
        private ModLoad modLoad;

        public void OnModLoaded()
        {
            modLoad = new ModLoad();
            modLoad.OnLevelLoaded(ICities.LoadMode.NewMap);
        }

        public void OnModUnloaded()
        {
            modLoad.OnLevelUnloading();           
        }
    }
}