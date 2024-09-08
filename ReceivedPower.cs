using KSP.UI.Screens.DebugToolbar.Screens.Cheats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalResourceTransferV2
{
    internal class ReceivedPower
    {
        PlanetOcclusion planetOcclusion = new PlanetOcclusion();
        public double CalcRecvPower(
            //Recv variables
            double recvArea,
            double recvWavelength,
            double recvEfficiency,
            //Source variables
            double sourceWavelength,
            double sourceArea,
            double sourceEfficiency,
            double powerBeamed,
            //Vessels (for other info)
            Vessel sourceVessel,
            Vessel recvVessel,

            double wavelengthFalloff,

            //out parameters
            out bool isOccluded,
            out CelestialBody occluder,
            out double recvPower

            )
        {
            // Initialize received power
            recvPower = 0;

            // Calculate distance
            double Distance = Vector3d.Distance(sourceVessel.GetWorldPos3D(), recvVessel.GetWorldPos3D());

            // Calculate effective power density taking into account the inverse square law
            double powerDensity = (powerBeamed * sourceArea) / (4 * Math.PI * Math.Pow(Distance,2));

            // Apply wavelength falloff
            double wavelengthFalloffFactor = Math.Pow(Math.E, -wavelengthFalloff * (Math.Abs(recvWavelength - sourceWavelength) / recvWavelength));
            powerDensity *= wavelengthFalloffFactor;

            // Calculate received power considering the receiver's area and efficiency
            recvPower = powerDensity * recvArea * recvEfficiency;

            // Check for occlusion
            planetOcclusion.IsOccluded(sourceVessel.GetWorldPos3D(), recvVessel.GetWorldPos3D(), out occluder, out isOccluded);

            if (isOccluded)
            {
                recvPower = 0;
            }

            return recvPower;
        }
    }
}