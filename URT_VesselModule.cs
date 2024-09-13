using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Unity;
using UnityEngine;
namespace UniversalResourceTransferV2
{
    public class URT_ReceiverVesselModule : VesselModule
    {
        //Resource variables
        List<String> OutputResources = new List<string>();
        List<String> OutputResourceGUINames = new List<string>(); 
        List<int> ConversionRates = new List<int>();
        List<int> ResourceHashes = new List<int>();
        int currentOutputResourceNum;

        //Vessel values
        [KSPField(isPersistant = true)]
        float AverageEfficiency = 0;
        
        [KSPField(isPersistant =true)]
        float TotalrecvArea;

        List<double> Wavelengths = new List<double>();

        //Current Values
        [KSPField(isPersistant = true)]
        bool toReset;

        [KSPField(isPersistant = true)]
        int currentWavelengthNum;

        [KSPField(isPersistant = true)]
        double currentWavelength;

        [KSPField(isPersistant = true)]
        double TotalRecvPower;

        URT_Utilities Utility = new URT_Utilities();
        public void Start()
        {
            LoadInternalReceiverData();
            Utility.LoadTransmitterData(this.Vessel,out TotalRecvPower);
        }
        
        //Compiles all the receivers on the vessel
        private void LoadInternalReceiverData()
        {
            //Empty lists/variables
            OutputResources.Clear();
            OutputResourceGUINames.Clear();
            ConversionRates.Clear();
            ResourceHashes.Clear();
            AverageEfficiency = 0;
            TotalrecvArea = 0;
            currentOutputResourceNum = 0;
            currentWavelength = 0;
            currentWavelengthNum = 0;

            //Initialise vessel lists            
            List<WirelessReceiver>VesselPartModules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<WirelessReceiver>();
            
            //Loop through each WirelessReceiver on the vessel
            
            foreach (WirelessReceiver WirelessReceiver in VesselPartModules)
            {
                //Increase diameter
                TotalrecvArea += WirelessReceiver.recvArea;

                //add resources
                if (!OutputResources.Contains(WirelessReceiver.OutputResource))
                {
                    OutputResources.Add(WirelessReceiver.OutputResource);
                    OutputResourceGUINames.Add(WirelessReceiver.OutputResourceGUIName);
                    ConversionRates.Add(WirelessReceiver.ConversionRate);
                    ResourceHashes.Add(PartResourceLibrary.Instance.GetDefinition(WirelessReceiver.OutputResource).id);
                }
                if (!Wavelengths.Contains(WirelessReceiver.recvWavelength))
                {
                    Wavelengths.Add(WirelessReceiver.recvWavelength);
                }
                //Increase sum of weighted values
                AverageEfficiency += WirelessReceiver.recvEfficiency * WirelessReceiver.recvArea;
            }
            //Get weighted efficiency
            AverageEfficiency /= TotalrecvArea;
        }

        public void ReloadData()
        {
            LoadInternalReceiverData();
            Utility.LoadTransmitterData(this.vessel,out TotalRecvPower);
        }

        public void FixedUpdate()
        {
            this.vessel.RequestResource(this.vessel.rootPart, ResourceHashes[1], -TotalRecvPower * TimeWarp.fixedDeltaTime, true);
        }
    }
}
