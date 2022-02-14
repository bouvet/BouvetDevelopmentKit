using UnityEngine;
using TMPro;
using System;
#if WINDOWS_UWP
using Windows.Networking.Connectivity;
#endif

namespace Bouvet.DevelopmentKit.Tools.SystemInformation
{
    public class WifiControllerIconExample : MonoBehaviour
    {
        public SpriteRenderer icon;
        public TextMeshPro wifiNameText;
        public Sprite ConnectedIcon;
        public Sprite DisconnectedIcon;
        public Sprite CellularIcon;

        [Range(1f, 60f)]
        public float UpdateFrequency = 30f;

        public static event Action OnWifiLost;
        public static event Action OnWifiFound;

        private bool currentConnectionStatus;

#if WINDOWS_UWP
        private void Start()
        {
            currentConnectionStatus = IsConnected();
            InvokeRepeating(nameof(UpdateWifiStatus), 1f, UpdateFrequency);
        }

        private void UpdateWifiStatus()
        {
            icon.sprite = IsConnected() ? ConnectedIcon : DisconnectedIcon;
            if (OnMeteredConnection())
                icon.sprite = CellularIcon;
            wifiNameText.text = GetWifiName();

            if (!IsConnected() && currentConnectionStatus)
            {
                currentConnectionStatus = false;
                OnWifiLost?.Invoke();
            }
            else if (IsConnected() && !currentConnectionStatus)
            {
                currentConnectionStatus = true;
                OnWifiFound?.Invoke();
            }
        }

        private bool IsConnected()
        {
            ConnectionProfile connection = NetworkInformation.GetInternetConnectionProfile();
            return connection != null && connection.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
        }

        public bool OnMeteredConnection()
        {
            ConnectionProfile connection = NetworkInformation.GetInternetConnectionProfile();
            ConnectionCost dataplan = connection.GetConnectionCost();
            return dataplan.Roaming;
        }

        public string GetWifiName()
        {
            ConnectionProfile connection = NetworkInformation.GetInternetConnectionProfile();

            if (connection == null)
            {
                return "No wifi";
            }

            if (connection.IsWlanConnectionProfile)
            {
                return connection.WlanConnectionProfileDetails.GetConnectedSsid();
            }

            if (connection.IsWwanConnectionProfile)
            {
                return connection.WwanConnectionProfileDetails.AccessPointName;
            }

            if (!string.IsNullOrEmpty(connection.ProfileName))
            {
                return connection.ProfileName;
            }

            return "Unknown";
        }

        private int ConnectionStrength()
        {
            try
            {
                byte? value = NetworkInformation.GetInternetConnectionProfile()?.GetSignalBars();

                if (value.HasValue)
                {
                    // Value 5 and 1 is never used? Subtract value by one and use 0-4 instead
                    return Mathf.Clamp(Convert.ToInt32((byte)value), 0, 4);
                }
            }
            catch
            {
                // Empty
            }

            return 0;
        }
#endif
    }
}
