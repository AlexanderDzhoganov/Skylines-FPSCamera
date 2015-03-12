using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera
{

    public class Configuration
    {

        public float cameraMoveSpeed = 128.0f;
        public float cameraRotationSensitivity = 1.0f;
        public bool snapToGround = false;
        public float groundOffset = 16.0f;
        public KeyCode toggleFPSCameraHotkey = KeyCode.Tab;
        public bool limitSpeedGround = false;
        public float fieldOfView = 45.0f;

        public void OnPreSerialize()
        {
        }

        public void OnPostDeserialize()
        {
        }

        public static void Serialize(string filename, Configuration config)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            using (var writer = new StreamWriter(filename))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
        }

        public static Configuration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(Configuration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (Configuration)serializer.Deserialize(reader);
                    config.OnPostDeserialize();
                    return config;
                }
            }
            catch { }

            return null;
        }
    }

}
