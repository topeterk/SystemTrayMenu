﻿// <copyright file="JoystickHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Reflection.Metadata;
    using System.Threading;
    using System.Windows.Input;
    using SharpDX.DirectInput;
    using Key = System.Windows.Input.Key;

    public class JoystickHelper : IDisposable
    {
        private readonly System.Timers.Timer timerReadJoystick = new();
        private readonly object lockRead = new();
        private Joystick joystick;
        private Key pressingKey;
        private int pressingKeyCounter;
        private bool joystickHelperEnabled;

        public JoystickHelper()
        {
            timerReadJoystick.Interval = 80;
            timerReadJoystick.Elapsed += ReadJoystickLoop;
            timerReadJoystick.Enabled = false;
            if (Properties.Settings.Default.SupportGamepad)
            {
                timerReadJoystick.Start();
            }
        }

        ~JoystickHelper() // the finalizer
        {
            Dispose(false);
        }

        public event Action<Key, ModifierKeys> KeyPressed;

        public void Enable()
        {
            joystickHelperEnabled = true;
        }

        public void Disable()
        {
            joystickHelperEnabled = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timerReadJoystick.Elapsed -= ReadJoystickLoop;
                timerReadJoystick.Dispose();
                joystick?.Dispose();
            }
        }

        private static Key ReadKeyFromState(JoystickUpdate state)
        {
            Key keys = Key.None;
            switch (state.Offset)
            {
                case JoystickOffset.PointOfViewControllers0:
                    switch (state.Value)
                    {
                        case 0:
                            keys = Key.Up;
                            break;
                        case 9000:
                            keys = Key.Right;
                            break;
                        case 18000:
                            keys = Key.Down;
                            break;
                        case 27000:
                            keys = Key.Left;
                            break;
                        default:
                            break;
                    }

                    break;
                case JoystickOffset.Buttons0:
                    if (state.Value == 128)
                    {
                        keys = Key.Enter;
                    }

                    break;
                default:
                    break;
            }

            return keys;
        }

        private void ReadJoystickLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (joystickHelperEnabled)
            {
                lock (lockRead)
                {
                    timerReadJoystick.Stop();
                    if (joystick == null)
                    {
                        Thread.Sleep(3000);
                        InitializeJoystick();
                    }
                    else
                    {
                        ReadJoystick();
                    }

                    timerReadJoystick.Start();
                }
            }
        }

        private void ReadJoystick()
        {
            try
            {
                joystick.Poll();
                JoystickUpdate[] datas = joystick.GetBufferedData();
                foreach (JoystickUpdate state in datas)
                {
                    if (state.Value < 0)
                    {
                        pressingKey = Key.None;
                        pressingKeyCounter = 0;
                        continue;
                    }

                    Key key = ReadKeyFromState(state);
                    if (key != Key.None)
                    {
                        KeyPressed?.Invoke(key, ModifierKeys.None);
                        if (state.Offset == JoystickOffset.PointOfViewControllers0)
                        {
                            pressingKeyCounter = 0;
                            pressingKey = key;
                        }
                    }
                }

                if (pressingKey != Key.None)
                {
                    pressingKeyCounter += 1;
                    if (pressingKeyCounter > 1)
                    {
                        KeyPressed?.Invoke(pressingKey, ModifierKeys.None);
                    }
                }
            }
            catch
            {
                joystick?.Dispose();
                joystick = null;
            }
        }

        private void InitializeJoystick()
        {
            // Initialize DirectInput
            DirectInput directInput = new();

            // Find a Joystick Guid
            Guid joystickGuid = Guid.Empty;

            foreach (DeviceInstance deviceInstance in directInput.GetDevices(
                DeviceType.Gamepad,
                DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
            {
                foreach (DeviceInstance deviceInstance in directInput.GetDevices(
                    DeviceType.Joystick,
                    DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                }
            }

            // If Joystick found
            if (joystickGuid != Guid.Empty)
            {
                // Instantiate the joystick
                joystick = new Joystick(directInput, joystickGuid);

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                var handle = Process.GetCurrentProcess().MainWindowHandle;
                joystick.SetCooperativeLevel(handle, CooperativeLevel.NonExclusive | CooperativeLevel.Background);

                // Acquire the joystick
                joystick.Acquire();
            }
        }
    }
}
