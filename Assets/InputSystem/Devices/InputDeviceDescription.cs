using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ISX
{
    /// <summary>
    /// Metadata for a device. Primarily used to find a matching template
    /// which we can then use to create a control setup for the device.
    /// </summary>
    [Serializable]
    public struct InputDeviceDescription
    {
        /// <summary>
        /// How we talk to the device; usually name of the underlying backend that feeds
        /// state for the device.
        /// </summary>
        /// <example>Examples: "HID", "XInput"</example>
        public string interfaceName;


        /// <summary>
        /// What the interface thinks the device classifies as.
        /// </summary>
        /// <remarks>
        /// If there is no template specifically matching a device description,
        /// the device class is used as as fallback. If, for example, this field
        /// is set to "Gamepad", the "Gamepad" template is used as a fallback.
        /// </remarks>
        public string deviceClass;

        // Who made the thing.
        public string manufacturer;
        // What they call it.
        public string product;
        public string serial;
        public string version;

        /// <summary>
        /// An optional JSON string listing device-specific capabilities.
        /// </summary>
        /// <remarks>
        /// The primary use of this field is to allow custom template constructors
        /// to create templates on the fly from in-depth device descriptions delivered
        /// by external APIs.
        ///
        /// In the case of HID, for example, this field contains a JSON representation
        /// of the HID descriptor as reported by the device driver. This descriptor
        /// contains information about all I/O elements on the device which can be used
        /// to determine the control setup and data format used by the device.
        /// </remarks>
        public string capabilities;

        public bool empty
        {
            get
            {
                return string.IsNullOrEmpty(interfaceName) &&
                    string.IsNullOrEmpty(deviceClass) &&
                    string.IsNullOrEmpty(manufacturer) &&
                    string.IsNullOrEmpty(product) &&
                    string.IsNullOrEmpty(serial) &&
                    string.IsNullOrEmpty(version) &&
                    string.IsNullOrEmpty(capabilities);
            }
        }

        public override string ToString()
        {
            var haveProduct = !string.IsNullOrEmpty(product);
            var haveManufacturer = !string.IsNullOrEmpty(manufacturer);

            if (haveProduct && haveManufacturer)
                return string.Format("{0} {1}", manufacturer, product);
            if (haveProduct)
                return product;

            if (!string.IsNullOrEmpty(deviceClass))
                return deviceClass;

            return string.Empty;
        }

        public bool Matches(InputDeviceDescription other)
        {
            return MatchPair(interfaceName, other.interfaceName)
                && MatchPair(deviceClass, other.deviceClass)
                && MatchPair(manufacturer, other.manufacturer)
                && MatchPair(product, other.product)
                // We don't match serials; seems nonsense to do that.
                && MatchPair(version, other.version)
                && MatchPair(capabilities, other.capabilities);
        }

        private static bool MatchPair(string left, string right)
        {
            if (string.IsNullOrEmpty(left))
                return true;
            if (string.IsNullOrEmpty(right))
                return false;
            if (!Regex.IsMatch(right, left, RegexOptions.IgnoreCase))
                return false;
            return true;
        }

        public string ToJson()
        {
            var data = new DeviceDescriptionJson
            {
                @interface = interfaceName,
                type = deviceClass,
                product = product,
                manufacturer = manufacturer,
                serial = serial,
                version = version,
                capabilities = capabilities
            };
            return JsonUtility.ToJson(data);
        }

        public static InputDeviceDescription FromJson(string json)
        {
            var data = JsonUtility.FromJson<DeviceDescriptionJson>(json);

            return new InputDeviceDescription
            {
                interfaceName = data.@interface,
                deviceClass = data.type,
                product = data.product,
                manufacturer = data.manufacturer,
                serial = data.serial,
                version = data.version,
                capabilities = data.capabilities
            };
        }

        ////FIXME: this format differs from the one used in templates; we should only have one format
        private struct DeviceDescriptionJson
        {
            public string @interface;
            public string type;
            public string product;
            public string serial;
            public string version;
            public string manufacturer;
            public string capabilities;
        }
    }
}
