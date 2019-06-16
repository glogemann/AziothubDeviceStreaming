﻿using AzIoTHubDeviceStreams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPXamlApp
{
    sealed partial class MainPage : Page
    {
        private string OnDeviceRecvText(string msgIn)
        {
            //Perform device side processing here. Eg read sensors.
            string msgOut = msgIn.ToUpper();

            Task.Run(async () => {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    tbDeviceMsgIn.Text = msgIn;
                    tbDeiceMsgOut.Text = msgOut;
                });
            });
            return msgOut;
        }

        private void OnDeviceStatusUpdate(string msgIn)
        {
            Task.Run(async () => {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    tbDeviceStatus.Text = msgIn;
                });
            });
        }

        private void ButtonCanceLDevice_Click(object sender, RoutedEventArgs e)
        {
            DeviceStream_Device.deviceStream_Device?.Cancel();
        }

        private async void Button_Click_Device(object sender, RoutedEventArgs e)
        {
            bool useCustomClass = (chkUseCustomClassDevice.IsChecked == true);           
            await Task.Run(() =>
            {
                try
                {
                    if (!useCustomClass)
                        DeviceStream_Device.RunDevice(device_cs, OnDeviceRecvText, OnDeviceStatusUpdate, KeepDeviceListening).GetAwaiter().GetResult();
                    else
                        DeviceStream_Device.RunDevice(device_cs, OnDeviceRecvText, OnDeviceStatusUpdate, KeepDeviceListening, new DeviceSvcCurrentSettings_Example()).GetAwaiter().GetResult();
                }
                catch (TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("0 Error App.RunClient(): Task cancelled");
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("0 Error App.RunClient(): Operation cancelled");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("0 Error App.RunClient(): " + ex.Message);
                }
            });
        }

        bool keepDeviceListening = false;
        public bool KeepDeviceListening
        {
            get
            {
                return keepDeviceListening;
            }
            set
            {
                keepDeviceListening = value;
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains("KeepDeviceListening"))
                {
                    if (localSettings.Values["KeepDeviceListening"] is bool)
                        localSettings.Values["KeepDeviceListening"] = keepDeviceListening;
                    else
                        localSettings.Values.Remove("KeepDeviceListening");
                }
                if (!localSettings.Values.Keys.Contains("KeepDeviceListening"))
                    localSettings.Values.Add("KeepDeviceListening", keepDeviceListening);
            }
        }
        private void ChKeepDeviceListening_Checked(object sender, RoutedEventArgs e)
        {
            KeepDeviceListening = (bool)((CheckBox)sender)?.IsChecked;
        }


        bool autoStartDevice = false;
        bool AutoStartDevice
        {
            get { return autoStartDevice; }
            set {
                autoStartDevice = value;
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.Keys.Contains("AutoStartDevice"))
                {
                    if (localSettings.Values["AutoStartDevice"] is bool)
                        localSettings.Values["AutoStartDevice"] = value;
                    else
                        localSettings.Values.Remove("AutoStartDevice");
                }
                if (!localSettings.Values.Keys.Contains("AutoStartDevice"))
                    localSettings.Values.Add("AutoStartDevice",value);
            }
        }
        private void ChAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            AutoStartDevice = (bool)((CheckBox)sender)?.IsChecked;
        }
    }
}
