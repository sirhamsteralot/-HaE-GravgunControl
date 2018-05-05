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
    partial class Program
    {
        public class GravgunControl
        {
            HashSet<IMyGravityGenerator> gravityGenerators =  new HashSet<IMyGravityGenerator>();
            HashSet<IMyShipWelder> welders = new HashSet<IMyShipWelder>();
            HashSet<IMyLightingBlock> lights = new HashSet<IMyLightingBlock>();
            List<IMyShipMergeBlock> mergeblocks = new List<IMyShipMergeBlock>();
            IMyProjector projector;

            IEnumerator<bool> stateMachine;

            bool fullAutoFire = false;
            
            public GravgunControl(IMyBlockGroup gunGroup)
            {
                gunGroup.GetBlocksOfType<IMyGravityGenerator>(null, x => { gravityGenerators.Add(x); return false; });
                gunGroup.GetBlocksOfType<IMyShipWelder>(null, x => { welders.Add(x); return false; });
                gunGroup.GetBlocksOfType<IMyLightingBlock>(null, x => { lights.Add(x); return false; });
                gunGroup.GetBlocksOfType<IMyShipMergeBlock>(null, x => { mergeblocks.Add(x); return false; });

                var templist = new List<IMyProjector>();
                gunGroup.GetBlocksOfType(templist);
                if (templist.Count != 1)
                    throw new Exception($"No or too many projectors in {gunGroup.Name}");
                projector = templist[0];
            }

            public void Run()
            {
                if (stateMachine != null)
                {
                    if (!stateMachine.MoveNext())
                    {
                        stateMachine.Dispose();
                        stateMachine = null;
                    }
                } else if (fullAutoFire)
                {
                    stateMachine = FireSequence();
                }
            }

            public void ToggleAutoFire()
            {
                fullAutoFire = !fullAutoFire;
            }

            public void SingleFire()
            {
                fullAutoFire = false;
                
                if (stateMachine == null)
                    stateMachine = FireSequence();
            }

            private IEnumerator<bool> FireSequence()
            {
                if (!projector.Enabled)
                {
                    projector.Enabled = true;
                    yield return true;
                }

                if (!projector.IsProjecting)
                    yield return false;

                SetGravGens(false);

                SetBuildBlocks(true);

                projector.UpdateOffsetAndRotation(); //REEEEEEEEEEEEEEEEEEE


                for (int i = 0; i < 100; i++)
                { // fuck you keen
                    SetLights(new Color(100 - i, i, i), i / 10.0);
                    yield return true;
                }

                while (projector.RemainingBlocks != 0)
                {
                    projector.UpdateOffsetAndRotation(); //EEEEEEEEEEEEEEEEEEEEE
                    yield return true;
                }


                SetBuildBlocks(false);

                yield return true;

                SetGravGens(true);
                SetLights(Color.Cyan, 0);

                for (int i = 0; i < 75; i++)
                { // Wait 100 ticks inbetween shots
                    SetLights(new Color(i, 75 - i, 75 - i), (75 - i) / 7.5);
                    yield return true;
                }
            }

            private void SetLights(Color color, double intensity)
            {
                foreach (var light in lights)
                {
                    light.Intensity = (float)intensity;
                    light.Radius = (float)intensity;
                    light.Color = color;

                    if (!light.Enabled)
                        light.Enabled = true;
                }
            }

            private void SetGravGens(bool Enabled)
            {
                foreach (var gravGen in gravityGenerators)
                {
                    gravGen.Enabled = Enabled;
                }
            }

            private void SetBuildBlocks(bool Enabled)
            {
                foreach (var welder in welders)
                {
                    welder.Enabled = Enabled;
                }

                foreach (var mergeBlock in mergeblocks)
                {
                    mergeBlock.Enabled = Enabled;
                }
            }
        }
    }
}
