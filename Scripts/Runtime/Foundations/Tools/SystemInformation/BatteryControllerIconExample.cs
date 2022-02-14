using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Devices.Power;
using WindowsBatteryStatus = Windows.System.Power.BatteryStatus;
#endif

namespace Bouvet.DevelopmentKit.Tools.SystemInformation
{
    public class BatteryControllerIconExample : MonoBehaviour
    {
        public SpriteRenderer icon;
        public TextMeshPro batteryPercentage;
        public Sprite FullIcon;
        public Sprite ChargingIcon;
        public Sprite AlertIcon;

        [Range(1f, 60f)]
        public float UpdateFrequency = 30f;

#if WINDOWS_UWP
        private void Start()
        {
            InvokeRepeating(nameof(UpdateBatteryState), 1f, UpdateFrequency);
        }

        private void UpdateBatteryState()
        {
            int charge = GetPercentage();
            if (IsCharging())
                icon.sprite = ChargingIcon;
            else if (charge < 20)
                icon.sprite = AlertIcon;
            else
                icon.sprite = FullIcon;
            batteryPercentage.text = charge + "%";
        }

        private bool IsCharging()
        {
            return Battery.AggregateBattery.GetReport().Status == WindowsBatteryStatus.Charging;
        }

        private int GetPercentage()
        {
            BatteryReport report = Battery.AggregateBattery.GetReport();

            float fullChargeMwh = report.FullChargeCapacityInMilliwattHours ?? 0;
            int remainingCapacityMwh = report.RemainingCapacityInMilliwattHours ?? 0;

            if (Mathf.Abs(fullChargeMwh) < 0.01f || Math.Abs(remainingCapacityMwh) < 0.01f)
            {
                return 100;
            }

            return Mathf.Clamp(Mathf.CeilToInt(remainingCapacityMwh / fullChargeMwh * 100), 1, 100);
        }
#endif
    }
}
