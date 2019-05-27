﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AzIoTHubDeviceStreams
{
    
    public class DeviceStreamClient
    {
        private DeviceClient _deviceClient;
        //public ActionReceivedText OnRecvdText = null;
        public ActionReceivedTextIO OnRecvdTextIO = null;

        //private DeviceClient _deviceClient;

        public DeviceStreamClient()
        {
            _deviceClient = null;
        }

        public DeviceStreamClient(DeviceClient deviceClient, ActionReceivedTextIO _OnRecvdText)
        {
            _deviceClient = deviceClient;
            OnRecvdTextIO = _OnRecvdText;
        }

        public async Task RunClientAsync()
        {
            await RunClientAsync(true).ConfigureAwait(false);
        }

        public async Task RunClientAsync(bool acceptDeviceStreamingRequest)
        {
            byte[] buffer = new byte[1024];

            try
            {
                System.Diagnostics.Debug.WriteLine("Client-1");
                using (var cancellationTokenSource = new CancellationTokenSource(DeviceStreamingCommon._Timeout))
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Client-2");
                        Microsoft.Azure.Devices.Client.DeviceStreamRequest streamRequest = await _deviceClient.WaitForDeviceStreamRequestAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                        System.Diagnostics.Debug.WriteLine("Client-3");
                        if (streamRequest != null)
                        {
                            if (acceptDeviceStreamingRequest)
                            {
                                System.Diagnostics.Debug.WriteLine("Client-4");
                                await _deviceClient.AcceptDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                                System.Diagnostics.Debug.WriteLine("Client-5");
                                using (ClientWebSocket webSocket = await DeviceStreamingCommon.GetStreamingClientAsync(streamRequest.Url, streamRequest.AuthorizationToken, cancellationTokenSource.Token).ConfigureAwait(false))
                                {
                                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), cancellationTokenSource.Token).ConfigureAwait(false);
                                    string msgIn = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                                    System.Diagnostics.Debug.WriteLine(string.Format("Client Received stream data: {0}", msgIn));
                                    string msgOut = msgIn;
                                    if (OnRecvdTextIO != null)
                                        msgOut = OnRecvdTextIO(msgIn);
                                    byte[] sendBuffer = Encoding.UTF8.GetBytes(msgOut);

                                    await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length), WebSocketMessageType.Binary, true, cancellationTokenSource.Token).ConfigureAwait(false);
                                    System.Diagnostics.Debug.WriteLine(string.Format("Client Sent stream data: {0}", Encoding.UTF8.GetString(sendBuffer, 0, sendBuffer.Length)));

                                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancellationTokenSource.Token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await _deviceClient.RejectDeviceStreamRequestAsync(streamRequest, cancellationTokenSource.Token).ConfigureAwait(false);
                            }
                        }

                        await _deviceClient.CloseAsync().ConfigureAwait(false);
                    }
                    catch (Microsoft.Azure.Devices.Client.Exceptions.IotHubCommunicationException)
                    {
                        System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): Hub connection failure");
                    }
                    catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceNotFoundException)
                    {
                        System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): Device not found");
                    }
                    catch (TaskCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): Task canceled");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): Operation canceled");
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Timeout"))
                            System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): " + ex.Message);
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("1 Error RunClientAsync(): Timeout");
                        }
                    }
                }
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.IotHubCommunicationException)
            {
                System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): Hub connection failure");
            }
            catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): Device not found");
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): Task canceled");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): Operation canceled");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Timeout"))
                    System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): " + ex.Message);
                else
                {
                    System.Diagnostics.Debug.WriteLine("2 Error RunClientAsync(): Timeout");
                }
            }
        }

        private static TransportType s_transportType = TransportType.Amqp;
        public static async void RunClient(string device_cs, ActionReceivedTextIO _OnRecvText)
        {
            try
            {
                using (DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(device_cs, s_transportType))
                {
                    if (deviceClient == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to create DeviceClient!");
                        //return null;
                    }

                    var sample = new DeviceStreamClient(deviceClient, _OnRecvText);
                    if (sample == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to create DeviceStreamClient!");
                        //return null;
                    }

                    try
                    {
                        await sample.RunClientAsync();//.GetAwaiter().GetResult();
                    }
                    catch (Microsoft.Azure.Devices.Client.Exceptions.IotHubCommunicationException)
                    {
                        System.Diagnostics.Debug.WriteLine("3 Error RunClient(): Hub connection failure");
                    }
                    catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceNotFoundException)
                    {
                        System.Diagnostics.Debug.WriteLine("3 Error RunClient(): Device not found");
                    }
                    catch (TaskCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("3 Error RunClient(): Task canceled");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine("3 Error RunClient(): Operation canceled");
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("Timeout"))
                            System.Diagnostics.Debug.WriteLine("3 Error RunClient(): " + ex.Message);
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("3 Error RunClient(): Timeout");
                        }
                    }

                }
                //return null;
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.IotHubCommunicationException)
            {
                System.Diagnostics.Debug.WriteLine("4 Error RunClient(): Hub connection failure");
            }
            catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("4 Error RunClient(): Device not found");
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("4 Error RunClient(): Task canceled");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("4 Error RunClient(): Operation canceled");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Timeout"))
                    System.Diagnostics.Debug.WriteLine("4 Error RunClient(): " + ex.Message);
                else
                {
                    System.Diagnostics.Debug.WriteLine("4 Error RunClient(): Timeout");
                }
            }
        }
    }
}
