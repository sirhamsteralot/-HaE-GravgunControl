using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Names
        INISerializer nameSerializer = new INISerializer("Blocknames");
        string GravgunGroupTag { get { return (string)nameSerializer.GetValue("GravgunGroupTag"); } }
        #endregion

        List<GravgunControl> gravGuns = new List<GravgunControl>();
        bool initialized = false;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            #region serializer
            nameSerializer.AddValue("GravgunGroupTag", x => x, "[Gravgun]");

            string customDat = Me.CustomData;
            nameSerializer.FirstSerialization(ref customDat);
            Me.CustomData = customDat;
            nameSerializer.DeSerialize(Me.CustomData);
            #endregion

            var blockGroups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(blockGroups, x => x.Name.Contains(GravgunGroupTag));

            foreach (var blockGroup in blockGroups)
            {
                gravGuns.Add(new GravgunControl(blockGroup));
            }
        }
        
        public void Init()
        {
            List<IMyBlockGroup> gunGroups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(gunGroups, x => x.Name.Contains(GravgunGroupTag));
        }


        int waitForInit = 0;
        public void Main(string argument, UpdateType updateSource)
        {
            if (!initialized)
            {
                if (waitForInit++ == 10)
                {
                    Init();
                    initialized = true;
                }
                return;
            }
                

            switch (argument)
            {
                case "SingleFire":
                    foreach (var gun in gravGuns)
                        gun.SingleFire();
                    break;
                case "ToggleFullAuto":
                    foreach (var gun in gravGuns)
                        gun.ToggleAutoFire();
                    break;
            }

            foreach (var gun in gravGuns)
                gun.Run();
        }
    }
}