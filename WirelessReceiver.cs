using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UniversalResourceTransferV2
{
    public class WirelessReceiver : PartModule
    {
        //Resource fields
        [KSPField(isPersistant = false)]
        public string OutputResource;

        [KSPField(isPersistant = false)]
        public string OutputResourceGUIName;

        [KSPField(isPersistant = false)]
        public int ConversionRate;

        //Receiver properties
        [KSPField(isPersistant = true)]
        public int recvArea;

        [KSPField(isPersistant =true)]
        public int recvEfficiency;

        [KSPField(isPersistant = false)]
        public double recvWavelength;
    }
}
