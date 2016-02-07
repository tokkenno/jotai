/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jotai.Hardware.CPU
{

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AMDCPU : GenericCPU
    {

        public enum Microarchitecture
        {
            Unknown,
            K5,
            K6,
            K6_2,
            Sharptooth,
            Athlon,
            Hammer,
            K10,
            Fusion,
            Bobcat,
            Jaguar,
            Puma,
            Bulldozer,
            Piledriver,
            Steamroller,
            Excavator,
            Zen
        }

        private const byte PCI_BUS = 0;
        private const byte PCI_BASE_DEVICE = 0x18;
        private const byte DEVICE_VENDOR_ID_REGISTER = 0;
        private const ushort AMD_VENDOR_ID = 0x1022;

        private readonly Microarchitecture microarchitecture;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public Microarchitecture MicroArchitecture { get { return this.microarchitecture; } }

        public AMDCPU(int processorIndex, CPUID[][] cpuid, ISettings settings) : base(processorIndex, cpuid, settings)
        {
            this.microarchitecture = Microarchitecture.Unknown;
            System.Console.WriteLine(this.family);
            switch (this.family)
            {
                case 0xC:
                    this.microarchitecture = Microarchitecture.K10;
                    break;
                case 0xE:
                    this.microarchitecture = Microarchitecture.Bobcat;
                    break;
                case 0x10:
                    switch (this.model)
                    {
                        case 0x2:
                            this.microarchitecture = Microarchitecture.Jaguar;
                            break;
                        case 0x3:
                            this.microarchitecture = Microarchitecture.Puma;
                            break;
                    }
                    break;
                case 0x15:
                    switch (this.model)
                    {
                        case 0x1:
                            this.microarchitecture = Microarchitecture.Bulldozer;
                            break;
                        case 0x2:
                            this.microarchitecture = Microarchitecture.Piledriver;
                            break;
                        case 0x3:
                            this.microarchitecture = Microarchitecture.Steamroller;
                            break;
                        case 0x4:
                            this.microarchitecture = Microarchitecture.Excavator;
                            break;
                    }
                    break;
            }
        }

        protected uint GetPciAddress(byte function, ushort deviceId)
        {

            // assemble the pci address
            uint address = Ring0.GetPciAddress(PCI_BUS,
              (byte)(PCI_BASE_DEVICE + processorIndex), function);

            // verify that we have the correct bus, device and function
            uint deviceVendor;
            if (!Ring0.ReadPciConfig(
              address, DEVICE_VENDOR_ID_REGISTER, out deviceVendor))
                return Ring0.InvalidPciAddress;

            if (deviceVendor != (deviceId << 16 | AMD_VENDOR_ID))
                return Ring0.InvalidPciAddress;

            return address;
        }

    }
}
