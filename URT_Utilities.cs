using CommNet.Occluders;
using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalResourceTransferV2
{
    internal class URT_Utilities
    {
        public double LoadTransmitterData(Vessel ReceiverVessel, out double TotalRecvPower)
        {
            List<int >TransmitterAreas = new List<int>();
            List<Vessel> Transmitters = new List<Vessel>();
            List<Double> TransmitterReceivedPowers = new List<Double>();
            List<String> TransmitterTargets = new List<String>();
            List<float> TransmitterWavelengths = new List<float>();
            List<Guid> TargetIDs = new List<Guid>();
            List<Vessel> TransmittersTargettingReceiver = new List<Vessel>();
            TotalRecvPower = 0;

            List<float> ReceiverAreas = new List<float>();
            List<float> Receivers = new List<float>();   //Fill transmitter list
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                foreach (ProtoPartSnapshot protoPartSnapshot in v.protoVessel.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protoPartModuleSnapshot in protoPartSnapshot.modules)
                    {
                        if (protoPartModuleSnapshot.moduleName == "WirelessSource")
                        {
                            ConfigNode moduleValues = protoPartModuleSnapshot.moduleValues;
                            Guid guid = new Guid(moduleValues.GetValue("TargetID"));
                            Transmitters.Add(v);
                            TransmitterReceivedPowers.Add(Convert.ToDouble(moduleValues.GetValue("RecvPower")));
                            TargetIDs.Add(guid);
                        }
                    }
                }
            }
            var i = 0;
            foreach (Guid TargetID in TargetIDs)
            {
                i += 1;
                if (TargetID == ReceiverVessel.id)
                {
                    TransmittersTargettingReceiver.Add(Transmitters[i]);
                    TotalRecvPower += TransmitterReceivedPowers[i];
                }
            }

            return TotalRecvPower;

        }

        public void LoadReceiverVessels(
            out List<Vessel> ReceiverVessels,//List of all receiver vessels, from which any other data can be extracted
            out List<double> VesselAreas, //Total recv area of all vessels with receivers
            out List<double> VesselEfficiencies, //Efficiency of all vessels with receivers
            out List<double> VesselWavelengths //Wavelengths of all vessels with receivers
            )
        {
            ReceiverVessels = new List<Vessel>();
            VesselAreas = new List<double>();
            VesselEfficiencies = new List<double>();
            VesselWavelengths = new List<double>();

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                foreach (VesselModule vesselModule in vessel.vesselModules)
                {
                    if (vesselModule.GetType().Name == "URT_ReceiverVesselModule")
                    {
                        ReceiverVessels.Add(vessel);
                        VesselAreas.Add(Convert.ToDouble(vesselModule.Fields.GetValue("TotalRecvArea")));
                        VesselEfficiencies.Add(Convert.ToDouble(vesselModule.Fields.GetValue("AverageEfficiency")));
                        VesselWavelengths.Add(Convert.ToDouble(vesselModule.Fields.GetValue("currentWavelength")));
                    }
                }
            }
        }

        public void UpdateReceivers()
        {
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                foreach (VesselModule vesselModule in vessel.vesselModules)
                {
                    if (vesselModule.GetType().Name == "URT_ReceiverVesselModule")
                    {
                        vesselModule.Fields.SetValue("toReset", true);
                    }
                }
            }
        }
    }
    internal class PlanetOcclusion
    {
        // checks for occlusion by each celestial body
        public void IsOccluded(Vector3d source, Vector3d dest, out CelestialBody celestialBody, out bool occluded)
        {
            bool planetocclusion = HighLogic.CurrentGame.Parameters.CustomParams<BPSettings>().planetOcclusion;
            Transform transform2; double radius2; celestialBody = new CelestialBody(); occluded = new bool();

            if (planetocclusion)
            {
                for (int x = 0; x < FlightGlobals.Bodies.Count; x++)
                {
                    transform2 = FlightGlobals.Bodies[x].transform;
                    radius2 = FlightGlobals.Bodies[x].Radius;
                    celestialBody = FlightGlobals.Bodies[x];

                    OccluderHorizonCulling occlusion = new OccluderHorizonCulling(transform2, radius2, radius2, radius2);
                    occlusion.Update();
                    occluded = occlusion.Raycast(source, dest);
                    if (occluded == true)
                    {
                        break;
                    }
                }
            }
            else
            {
                occluded = false;
            }
        }
    }

    internal class RelativisticEffects
    {
        bool relativistic; const float c = 299792452; Vector3d prevPos = Vector3d.zero;
        string exceeded_c = Localizer.Format("#LOC_BeamedPower_ExceededC");

        public double RedOrBlueShift(Vessel source, Vessel dest, string state, out string status)
        {
            relativistic = HighLogic.CurrentGame.Parameters.CustomParams<BPSettings>().relativistic; double powerMult;
            double v = Vector3d.Magnitude(source.orbit.GetWorldSpaceVel() - dest.orbit.GetWorldSpaceVel());
            v *= Math.Cos(Vector3d.Angle((source.GetWorldPos3D() - dest.GetWorldPos3D()),
                (source.orbit.GetWorldSpaceVel() - dest.orbit.GetWorldSpaceVel())));

            if (relativistic)
            {
                if (v >= c - 1)
                {
                    powerMult = 0;
                    status = exceeded_c;
                }
                else
                {
                    powerMult = Math.Sqrt((1 + v / c) / (1 - v / c));
                    status = state;
                }
            }
            else
            {
                powerMult = 1;
                status = state;
            }
            return powerMult;
        }

        internal bool WarpDriveEngaged(Part part)
        {
            relativistic = HighLogic.CurrentGame.Parameters.CustomParams<BPSettings>().relativistic;
            bool warping = false;

            if (relativistic)
            {
                Vector3d position = part.vessel.GetWorldPos3D();
                double displacement = Vector3d.Distance(position, prevPos);
                double v = displacement / TimeWarp.fixedDeltaTime;
                if (v > part.vessel.orbit.GetWorldSpaceVel().magnitude * 2d)
                {
                    warping = true;
                }
                prevPos = position;
            }
            return warping;
        }
    }
}


