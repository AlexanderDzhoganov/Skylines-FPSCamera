using System;
using ColossalFramework.UI;
using UnityEngine;

namespace FPSCamera
{

    public class FPSCameraUI : MonoBehaviour
    {

        public static FPSCameraUI instance;

        public static FPSCameraUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FPSCamera.instance.gameObject.AddComponent<FPSCameraUI>();
                }

                return instance;
            }
        }

        private UIPanel panel;
        private UIButton cameraModeButton;
        private UILabel cameraModeLabel;

        private UIButton hotkeyToggleButton;
        private UIButton hotkeyShowMouseButton;
        private UIButton hotkeyGoFasterButton;

        private bool waitingForChangeCameraHotkey = false;
        private bool waitingForShowMouseHotkey = false;
        private bool waitingForGoFasterHotkey = false;

        private FPSCameraUI()
        {
            var uiView = FindObjectOfType<UIView>();
            var fullscreenContainer = uiView.FindUIComponent("FullScreenContainer");

            cameraModeButton = uiView.AddUIComponent(typeof(UIButton)) as UIButton;

            cameraModeButton.name = "FPSCameraConfigurationButton";
            cameraModeButton.gameObject.name = "FPSCameraConfigurationButton";
            cameraModeButton.width = 36;
            cameraModeButton.height = 36;

            cameraModeButton.pressedBgSprite = "OptionBasePressed";
            cameraModeButton.normalBgSprite = "OptionBase";
            cameraModeButton.hoveredBgSprite = "OptionBaseHovered";
            cameraModeButton.disabledBgSprite = "OptionBaseDisabled";

            cameraModeButton.normalFgSprite = "InfoPanelIconFreecamera";
            cameraModeButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            cameraModeButton.scaleFactor = 1.0f;

            cameraModeButton.tooltip = "FPS Camera configuration";
            cameraModeButton.tooltipBox = uiView.defaultTooltipBox;

            UIComponent escbutton = uiView.FindUIComponent("Esc");
            cameraModeButton.relativePosition = new Vector2
            (
                escbutton.relativePosition.x + escbutton.width / 2.0f - cameraModeButton.width / 2.0f - escbutton.width - 8.0f,
                escbutton.relativePosition.y + escbutton.height / 2.0f - cameraModeButton.height / 2.0f
            );

            cameraModeButton.eventClick += (component, param) => { panel.isVisible = !panel.isVisible; };

            var labelObject = new GameObject();
            labelObject.transform.parent = uiView.transform;

            cameraModeLabel = labelObject.AddComponent<UILabel>();
            cameraModeLabel.textColor = new Color32(255, 255, 255, 255);
            cameraModeLabel.Hide();

            FPSCamera.onCameraModeChanged = state =>
            {
                if (state)
                {
                    cameraModeLabel.text = String.Format("Press ({0}) to exit first-person mode", FPSCamera.GetToggleUIKey());
                    cameraModeLabel.color = new Color32(255, 255, 255, 255);
                    cameraModeLabel.AlignTo(cameraModeButton, UIAlignAnchor.BottomRight);
                    cameraModeLabel.relativePosition += new Vector3(-38.0f, -8.0f);
                    cameraModeLabel.Show();
                }
                else
                {
                    cameraModeLabel.Hide();
                }
            };

            FPSCamera.onUpdate = () =>
            {
                if (cameraModeLabel.color.a > 0)
                {
                    var c = cameraModeLabel.color;
                    cameraModeLabel.color = new Color32(c.r, c.g, c.b, (byte)(c.a - 1));
                }
            };

            panel = fullscreenContainer.AddUIComponent<UIPanel>();
            panel.size = new Vector2(400, 700);
            panel.isVisible = false;
            panel.backgroundSprite = "SubcategoriesPanel";
            panel.relativePosition = new Vector3(cameraModeButton.relativePosition.x - panel.size.x, cameraModeButton.relativePosition.y + 60.0f);
            panel.name = "FPSCameraConfigPanel";

            var titleLabel = panel.AddUIComponent<UILabel>();
            titleLabel.name = "Title";
            titleLabel.text = "First-person camera configuration";
            titleLabel.autoSize = false;
            titleLabel.size = new Vector2(panel.size.x, 24.0f);
            titleLabel.AlignTo(panel, UIAlignAnchor.TopLeft);
            titleLabel.relativePosition = new Vector3(titleLabel.relativePosition.x, titleLabel.relativePosition.y + 2.0f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;

            float y = 48.0f;

            var hotkeyToggleLabel = panel.AddUIComponent<UILabel>();
            hotkeyToggleLabel.name = "ToggleFirstpersonLabel";
            hotkeyToggleLabel.text = "Hotkey to toggle first-person";
            hotkeyToggleLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyToggleLabel.textScale = 0.8f;

            hotkeyToggleButton = MakeButton(panel, "ToggleFirstpersonButton",
                FPSCamera.instance.config.toggleFPSCameraHotkey.ToString(), y,
                () =>
                {
                    if (!waitingForChangeCameraHotkey)
                    {
                        waitingForChangeCameraHotkey = true;
                        waitingForShowMouseHotkey = false;
                        waitingForGoFasterHotkey = false;
                        hotkeyToggleButton.text = "Waiting";
                    }
                });

            y += 28.0f;

            var hotkeyShowMouseLabel = panel.AddUIComponent<UILabel>();
            hotkeyShowMouseLabel.name = "ShowMouseLabel";
            hotkeyShowMouseLabel.text = "Hotkey to show cursor (hold)";
            hotkeyShowMouseLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyShowMouseLabel.textScale = 0.8f;

            hotkeyShowMouseButton = MakeButton(panel, "ShowMouseButton",
                FPSCamera.instance.config.showMouseHotkey.ToString(), y,
                () =>
                {
                    if (!waitingForChangeCameraHotkey)
                    {
                        waitingForChangeCameraHotkey = false;
                        waitingForShowMouseHotkey = true;
                        waitingForGoFasterHotkey = false;
                        hotkeyShowMouseButton.text = "Waiting";
                    }
                });

            y += 28.0f + 16.0f;

            var hotkeyGoFasterLabel = panel.AddUIComponent<UILabel>();
            hotkeyGoFasterLabel.name = "GoFasterLabel";
            hotkeyGoFasterLabel.text = "Hotkey to go faster (hold)";
            hotkeyGoFasterLabel.relativePosition = new Vector3(4.0f, y);
            hotkeyGoFasterLabel.textScale = 0.8f;

            hotkeyGoFasterButton = MakeButton(panel, "GoFasterButton",
                FPSCamera.instance.config.goFasterHotKey.ToString(), y,
                () =>
                {
                    if (!waitingForGoFasterHotkey)
                    {
                        waitingForChangeCameraHotkey = false;
                        waitingForShowMouseHotkey = false;
                        waitingForGoFasterHotkey = true;
                        hotkeyGoFasterButton.text = "Waiting";
                    }
                });

            y += 28.0f;

            MakeSlider(panel, "GoFasterMultiplier", "\"Go faster\" speed multiplier", y,
                FPSCamera.instance.config.goFasterSpeedMultiplier, 2.0f, 20.0f,
                value =>
                {
                    FPSCamera.instance.config.goFasterSpeedMultiplier = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f + 16.0f;

            if (FPSCamera.instance.hideUIComponent != null)
            {
                MakeCheckbox(panel, "HideUI", "HideUI integration", y, FPSCamera.instance.config.integrateHideUI,
                    value =>
                    {
                        FPSCamera.instance.config.integrateHideUI = value;
                        FPSCamera.instance.SaveConfig();
                    });

                y += 28.0f + 8.0f;
            }

            MakeSlider(panel, "FieldOfView", "Field of view", y,
                FPSCamera.instance.config.fieldOfView, 30.0f, 120.0f,
                value =>
                {
                    FPSCamera.instance.SetFieldOfView(value);
                });

            y += 28.0f;

            MakeSlider(panel, "MovementSpeed", "Movement speed", y,
                FPSCamera.instance.config.cameraMoveSpeed, 0.25f, 128.0f,
                value =>
                {
                    FPSCamera.instance.config.cameraMoveSpeed = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f + 16.0f;

            MakeSlider(panel, "Sensitivity", "Sensitivity", y,
                FPSCamera.instance.config.cameraRotationSensitivity, 0.25f, 3.0f,
                value =>
                {
                    FPSCamera.instance.config.cameraRotationSensitivity = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f;

            MakeCheckbox(panel, "InvertYAxis", "Invert Y-Axis", y, FPSCamera.instance.config.invertYAxis,
                value =>
                {
                    FPSCamera.instance.config.invertYAxis = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f + 16.0f;

            MakeCheckbox(panel, "SnapToGround", "Snap to ground", y, FPSCamera.instance.config.snapToGround,
               value =>
               {
                   FPSCamera.instance.config.snapToGround = value;
                   FPSCamera.instance.SaveConfig();
               });

            y += 28.0f;

            MakeSlider(panel, "GroundDistance", "Ground distance", y,
                FPSCamera.instance.config.groundOffset, 0.25f, 32.0f,
                value =>
                {
                    FPSCamera.instance.config.groundOffset = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f;

            MakeCheckbox(panel, "PreventGroundClipping", "Prevent ground clipping", y, FPSCamera.instance.config.preventClipGround,
               value =>
               {
                   FPSCamera.instance.config.preventClipGround = value;
                   FPSCamera.instance.SaveConfig();
               });

            y += 28.0f + 16.0f;

            MakeCheckbox(panel, "AnimatedTransitions", "Animated transitions", y, FPSCamera.instance.config.animateTransitions,
               value =>
               {
                   FPSCamera.instance.config.animateTransitions = value;
                   FPSCamera.instance.SaveConfig();
               });

            y += 28.0f;

            MakeSlider(panel, "TransitionSpeed", "Transition speed", y,
                FPSCamera.instance.config.animationSpeed, 0.1f, 4.0f,
                value =>
                {
                    FPSCamera.instance.config.animationSpeed = value;
                    FPSCamera.instance.SaveConfig();
                });

            y += 28.0f + 16.0f;

            if (!FPSCamera.editorMode)
            {
                MakeCheckbox(panel, "AllowMovementVehicleMode", "Allow movement in vehicle/ citizen mode", y, FPSCamera.instance.config.allowUserOffsetInVehicleCitizenMode,
                   value =>
                   {
                       FPSCamera.instance.config.allowUserOffsetInVehicleCitizenMode = value;
                       FPSCamera.instance.SaveConfig();
                   });

                y += 28.0f;

                MakeCheckbox(panel, "ManualWalkthrough", "Manual switching in walkthrough- mode", y, FPSCamera.instance.config.walkthroughModeManual,
                   value =>
                   {
                       FPSCamera.instance.config.walkthroughModeManual = value;
                       FPSCamera.instance.SaveConfig();
                   });

                y += 28.0f;

                MakeSlider(panel, "StayDuration", "Walkthrough stay duration", y,
                    FPSCamera.instance.config.walkthroughModeTimer, 10.0f, 60.0f,
                    value =>
                    {
                        FPSCamera.instance.config.walkthroughModeTimer = value;
                        FPSCamera.instance.SaveConfig();
                    });

                y += 28.0f;

                var walkthroughModeButton = MakeButton(panel, "WalkthroughModeButton", "Enter walkthrough mode", y,
                    () =>
                    {
                        FPSCamera.instance.EnterWalkthroughMode();
                    });
                walkthroughModeButton.relativePosition = new Vector3(2.0f, y - 6.0f);
                walkthroughModeButton.size = new Vector2(200.0f, 24.0f);

                y += 28.0f + 16.0f;
            }

            var resetConfig = MakeButton(panel, "ResetConfigButton", "Reset configuration", y,
                    () =>
                    {
                        FPSCamera.instance.ResetConfig();
                    });

            resetConfig.relativePosition = new Vector3(2.0f, y);
            resetConfig.size = new Vector2(200.0f, 24.0f);
        }

        public void Show()
        {
            panel.isVisible = true;
        }

        public void Hide()
        {
            panel.isVisible = false;
        }

        private delegate void ButtonClicked();

        private static UIButton MakeButton(UIPanel panel, string name, string text, float y, ButtonClicked clicked)
        {
            var button = panel.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.relativePosition = new Vector3(200.0f, y - 6.0f);
            button.size = new Vector2(100.0f, 24.0f);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.eventClick += (component, param) =>
            {
                clicked();
            };

            return button;
        }

        private delegate void CheckboxSetValue(bool value);

        private static UICheckBox MakeCheckbox(UIPanel panel, string name, string text, float y, bool value,
            CheckboxSetValue setValue)
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.relativePosition = new Vector3(4.0f, y);
            label.textScale = 0.8f;

            var checkbox = panel.AddUIComponent<UICheckBox>();
            checkbox.AlignTo(label, UIAlignAnchor.TopLeft);
            checkbox.relativePosition = new Vector3(checkbox.relativePosition.x + 332.0f, checkbox.relativePosition.y - 6.0f);
            checkbox.size = new Vector2(20.0f, 20.0f);
            checkbox.isVisible = true;
            checkbox.canFocus = true;
            checkbox.isInteractive = true;

            checkbox.eventCheckChanged += (component, newValue) =>
            {
                setValue(newValue);
            };

            var uncheckSprite = checkbox.AddUIComponent<UISprite>();
            uncheckSprite.size = new Vector2(20.0f, 20.0f);
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            var checkSprite = checkbox.AddUIComponent<UISprite>();
            checkSprite.size = new Vector2(20.0f, 20.0f);
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            checkbox.isChecked = value;
            checkbox.checkedBoxObject = checkSprite;
            return checkbox;
        }

        private delegate void SliderSetValue(float value);

        private static UISlider MakeSlider(UIPanel panel, string name, string text, float y, float value, float min, float max, SliderSetValue setValue)
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = name + "Label";
            label.text = text;
            label.relativePosition = new Vector3(4.0f, y);
            label.textScale = 0.8f;

            var slider = panel.AddUIComponent<UISlider>();
            slider.name = name + "Slider";
            slider.minValue = min;
            slider.maxValue = max;
            slider.stepSize = 0.25f;
            slider.value = value;
            slider.relativePosition = new Vector3(200.0f, y);
            slider.size = new Vector2(158.0f, 16.0f);

            var thumbSprite = slider.AddUIComponent<UISprite>();
            thumbSprite.name = "Thumb";
            thumbSprite.spriteName = "SliderBudget";

            slider.backgroundSprite = "ScrollbarTrack";
            slider.thumbObject = thumbSprite;
            slider.orientation = UIOrientation.Horizontal;
            slider.isVisible = true;
            slider.enabled = true;
            slider.canFocus = true;
            slider.isInteractive = true;

            var valueLabel = panel.AddUIComponent<UILabel>();
            valueLabel.name = name + "ValueLabel";
            valueLabel.text = slider.value.ToString("0.00");
            valueLabel.relativePosition = new Vector3(362.0f, y);
            valueLabel.textScale = 0.8f;

            slider.eventValueChanged += (component, f) =>
            {
                setValue(f);
                valueLabel.text = slider.value.ToString("0.00");
            };

            return slider;
        }

        void OnDestroy()
        {
            Destroy(panel.gameObject);   
        }

        private void OnGUI()
        {

            if (Event.current.type == EventType.KeyDown)
            {
                if (waitingForChangeCameraHotkey)
                {
                    waitingForChangeCameraHotkey = false;
                    var keycode = Event.current.keyCode;
                    FPSCamera.instance.config.toggleFPSCameraHotkey = keycode;
                    hotkeyToggleButton.text = keycode.ToString();
                    FPSCamera.instance.SaveConfig();
                }
                else if (waitingForShowMouseHotkey)
                {
                    waitingForShowMouseHotkey = false;
                    var keycode = Event.current.keyCode;
                    FPSCamera.instance.config.showMouseHotkey = keycode;
                    hotkeyShowMouseButton.text = keycode.ToString();
                    FPSCamera.instance.SaveConfig();
                }
                else if (waitingForGoFasterHotkey)
                {
                    waitingForGoFasterHotkey = false;
                    var keycode = Event.current.keyCode;
                    FPSCamera.instance.config.goFasterHotKey = keycode;
                    hotkeyGoFasterButton.text = keycode.ToString();
                    FPSCamera.instance.SaveConfig();
                }
            }
            
        }
    }

}
