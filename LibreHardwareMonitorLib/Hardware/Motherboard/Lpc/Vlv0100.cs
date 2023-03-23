// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;

namespace LibreHardwareMonitor.Hardware.Motherboard.Lpc
{
    internal class Vlv0100 : ISuperIO
    {
        public Vlv0100(Chip chip)
        {
            Chip = chip;

            Voltages = new float?[0];
            Temperatures = new float?[1];
            Fans = new float?[2];
            Controls = new float?[0];

            InpOut.Open();
        }

        public Chip Chip { get; }

        public float?[] Controls { get; }

        public float?[] Fans { get; }

        public float?[] Temperatures { get; }

        public float?[] Voltages { get; }

        public byte? ReadGpio(int index)
        {
            return null;
        }

        public void WriteGpio(int index, byte value)
        { }

        public void SetControl(int index, byte? value)
        {
        }

        public string GetReport()
        {
            StringBuilder r = new();
            return r.ToString();
        }

        public void Update()
        {
            if (!Ring0.WaitIsaBusMutex(10))
                return;

#if TRUE
            var data = InpOut.ReadMemory(new IntPtr(0xFE700B00 + 0x92), 2);
            if (data is not null)
                Fans[0] = (float)BitConverter.ToUInt16(data, 0);
            else
                Fans[0] = -1;

            data = InpOut.ReadMemory(new IntPtr(0xFE700300 + 0xB0), 2);
            if (data is not null)
                Fans[1] = (float)BitConverter.ToUInt16(data, 0);
            else
                Fans[1] = -1;

            data = InpOut.ReadMemory(new IntPtr(0xFE700400 + 0x6E), 2);
            if (data is not null)
                Temperatures[0] = (float)(BitConverter.ToUInt16(data.Reverse().ToArray(), 0) - 0x0AAC) / 10.0f;
            else
                Temperatures[0] = -1;
#elif FALSE
            byte[] xecb = InpOut.ReadMemory(new IntPtr(0xFE700B00), 0x0100);
            byte[] xec3 = InpOut.ReadMemory(new IntPtr(0xFE700300), 0x0100);
            byte[] XECC = InpOut.ReadMemory(new IntPtr(0xFE700C00+0x4C), 0x0100- 0x4C);
            ushort fanSetSpeed = (ushort)(xecb[0x92] + (xecb[0x93] << 8));
            ushort fanSpeed = (ushort)(xec3[0xB0] + (xec3[0xB1] << 8));
            byte fanCheck = xec3[0x9F];
            Fans[0] = (float)fanSetSpeed;
            Fans[1] = (float)fanSpeed;
#else
            ushort fanSetSpeed = 0;
            ushort fanSpeed = 0;

            if (Ring0.ReadMemory(0xFE700B00 + 0x92, ref fanSetSpeed))
                Fans[0] = (float)fanSetSpeed;
            else
                Fans[0] = -1;

            if (Ring0.ReadMemory(0xFE700300 + 0xB0, ref fanSpeed))
                Fans[1] = (float)fanSpeed;
            else
                Fans[1] = -1;
            // Ring0.ReadMemory(0xFE700300 + 0x9F, ref fanCheck);
#endif
            Ring0.ReleaseIsaBusMutex();
        }

        // ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles


        // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles
    }
}
