using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniversalResourceTransferV2
{
    public class WirelessSource : PartModule
    {
        //Resource fields
        [KSPField(isPersistant = false)]
        string InputResource;

        [KSPField(isPersistant = false)]
        string InputResourceGUIName;

        [KSPField(isPersistant = false)]
        string ConversionRate;

        //Dish properties
        [KSPField(isPersistant = true)]
        public int SourceArea;

        [KSPField(isPersistant =false)]
        public double SourceEfficiency;

        [KSPField(isPersistant = true)]
        public Single Wavelength;

        
        //Receiver data
        [KSPField(isPersistant = true)]
        List<Vessel> receiverVessels;

        [KSPField(isPersistant = true)]
        List<double> receiverEfficiencies;

        [KSPField(isPersistant = true)]
        List<double> recvAreas;

        [KSPField(isPersistant = true)]
        List<double> recvWavelengths;

        //Initialise utility classes
        URT_Utilities Utility = new URT_Utilities();
        ReceivedPower receivedPower = new ReceivedPower();

        //Current values
        [KSPField(isPersistant = true)]
        public Vessel Target;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Target")]
        public String TargetName;

        [KSPField(isPersistant = true)]
        int Counter;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Units Transmitted", guiFormat = "F0"), UI_FloatRange(minValue = 0, maxValue = 100000, stepIncrement = 1, scene = UI_Scene.Flight)]
        public double PowerToBeam;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Units Received", guiFormat = "F0")]
        public double RecvPower;

        [KSPField(isPersistant = true, guiActive = false, guiName = "Occluded by ")]
        string OccludingBodyName;

        bool isOccluded;
        CelestialBody occluder;

        public void Start()
        {

            //Set basic variables to 0
            PowerToBeam = 100;
            RecvPower = 0;
            
            //Load receiver data
            Utility.LoadReceiverVessels(out receiverVessels, out recvAreas, out receiverEfficiencies, out recvWavelengths);

            //Target first receiver
            Counter = 1;
            SetTarget();
        }

        //Sets all target specific parameters
        private void SetTarget()
        {
            Target = receiverVessels[Counter];
            TargetName = Target.GetDisplayName();

            receivedPower.CalcRecvPower(
                recvAreas[Counter], recvWavelengths[Counter], 
                receiverEfficiencies[Counter],SourceArea,Wavelength,SourceEfficiency,
                PowerToBeam,this.part.vessel,Target,4.6,out isOccluded,
                out occluder, out RecvPower
            );
            if (isOccluded)
            {
                OccludingBodyName = occluder.GetDisplayName();
                Fields["OccludingBodyName"].guiActive = true;
            }
            else
            {
                Fields["OccludingBodyName"].guiActive = false;
            }
        }
    }
}
