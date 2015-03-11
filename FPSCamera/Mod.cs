using System;
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

    public class ModLoad : LoadingExtensionBase
    {
        private UIButton cameraModeButton;
        private UILabel cameraModeLabel;

        public override void OnLevelLoaded(LoadMode mode)
        {
            var uiView = GameObject.FindObjectOfType<UIView>();

            // Create a GameObject with a ColossalFramework.UI.UIButton component.
            var buttonObject = new GameObject();

            // Make the buttonObject a child of the uiView.
            buttonObject.transform.parent = uiView.transform;

            // Get the button component.
            cameraModeButton = buttonObject.AddComponent<UIButton>();

            // Set the text to show on the button.
            cameraModeButton.text = "Camera: Standard";

            // Set the button dimensions.
            cameraModeButton.width = 220;
            cameraModeButton.height = 30;

            // Style the button to look like a menu button.
            cameraModeButton.normalBgSprite = "ButtonMenu";
            cameraModeButton.disabledBgSprite = "ButtonMenuDisabled";
            cameraModeButton.hoveredBgSprite = "ButtonMenuHovered";
            cameraModeButton.focusedBgSprite = "ButtonMenuFocused";
            cameraModeButton.pressedBgSprite = "ButtonMenuPressed";
            cameraModeButton.textColor = new Color32(255, 255, 255, 255);
            cameraModeButton.disabledTextColor = new Color32(7, 7, 7, 255);
            cameraModeButton.hoveredTextColor = new Color32(7, 132, 255, 255);
            cameraModeButton.focusedTextColor = new Color32(255, 255, 255, 255);
            cameraModeButton.pressedTextColor = new Color32(30, 30, 44, 255);

            // Place the button.
            cameraModeButton.transformPosition = new Vector3(1.25f, 0.97f);

            // Respond to button click.
            cameraModeButton.eventClick += ButtonClick;

            var labelObject = new GameObject();
            labelObject.transform.parent = uiView.transform;

            cameraModeLabel = labelObject.AddComponent<UILabel>();
            cameraModeLabel.text = "Press (TAB) to exit first-person mode";
            cameraModeLabel.textColor = new Color32(255, 255, 255, 255);
            cameraModeLabel.transformPosition = new Vector3(1.15f, 0.90f);
            cameraModeLabel.Hide();

            FPSCamera.Initialize();
            FPSCamera.onCameraModeChanged = state =>
            {
                cameraModeButton.text = state ? "Camera: First Person" : "Camera: Standard";
                if (state)
                {
                    cameraModeLabel.Show();
                }
                else
                {
                    cameraModeLabel.Hide();
                }
            };
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            FPSCamera.SetMode(!FPSCamera.IsEnabled());
        }

    }

}
