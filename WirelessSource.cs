
using System;
using System.Collections.Generic;

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
        int InputResourceHash;

        [KSPField(isPersistant = false)]
        double ConversionRate;

        //Dish properties
        [KSPField(isPersistant = true)]
        public int SourceArea;

        [KSPField(isPersistant =false)]
        public double SourceEfficiency; 

        [KSPField(isPersistant = true)]
        public double Wavelength; //Wavelength in meters

        [KSPField(isPersistant = true)]
        public double BeamDivergence; //A multiplier applied to the wavelength derived beam divergence

        [KSPField(isPersistant = true)]
        public double BeamWaist; //Radius of the beam at its thinnest point, before it starts diverging

        
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

        [KSPField(isPersistant = true)]
        public double TargetID;

        [KSPField(isPersistant = true)]
        int Counter;

        [KSPField(isPersistant = true)]
        public double PowerToBeam;

        [KSPField(isPersistant = true)]
        public double RecvPower;



        int frames;
        bool isOccluded;
        CelestialBody occluder;

        //GUI names
        [KSPField(isPersistant = true, guiActive = false, guiName = "Occluded by ")]
        string OccludingBodyName;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Transmitting to: ")]
        public String TargetName;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Units Transmitted: ", guiFormat = "F0", guiUnits = "/s"), UI_FloatRange(minValue = 0, maxValue = 100000, stepIncrement = 1, scene = UI_Scene.Flight)]
        public double GUIPowerToBeam;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Units Received: ", guiFormat = "F0", guiUnits = "/s")]
        public double GUIRecvPower;


        public void Start()
        {

            //Set basic variables to 0
            PowerToBeam = 100;
            RecvPower = 0;

            if (HighLogic.LoadedSceneIsFlight)
            {
                RunFlightStartup();
            }


        }

        //Sets all target specific parameters
        private void SetTarget()
        {
            Target = receiverVessels[Counter];
            TargetName = Target.GetDisplayName();
        }

        private void RunFlightStartup()
        {
            //Load receiver data
            Utility.LoadReceiverVessels(out receiverVessels, out recvAreas, out receiverEfficiencies, out recvWavelengths);

            //Set GUI fields
            Fields["GUIRecvPower"].guiUnits = InputResourceGUIName;

            //Set resource hash
            InputResourceHash = PartResourceLibrary.Instance.GetDefinition(InputResource).id;
            //Target first receiver
            Counter = 0;
            SetTarget();
            frames = 0;
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (frames == 30)
                {
                    frames = 0;
                    RunFlightUpdate();
                }
                else frames++;
            }
        }

        private void RunFlightUpdate()
        {
            receivedPower.CalcRecvPower(recvAreas[Counter], recvWavelengths[Counter],
                receiverEfficiencies[Counter], Wavelength, SourceArea, SourceEfficiency,
                PowerToBeam, BeamWaist, BeamDivergence, this.part.vessel, receiverVessels[Counter],
                3, out isOccluded, out occluder, out RecvPower
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
            GUIRecvPower = RecvPower * ConversionRate;
            GUIPowerToBeam = PowerToBeam * ConversionRate;

            this.part.vessel.RequestResource(this.part, InputResourceHash, -GUIPowerToBeam*TimeWarp.fixedDeltaTime, true);
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Next Vessel")]
        private void ChangeTarget()
        {
            Counter++;
            Counter = (Counter % receiverVessels.Count);
            SetTarget();
        }
    }
}