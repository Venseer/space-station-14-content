﻿using Content.Server.GameObjects.Components.Power;
using SS14.Shared.GameObjects.System;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.GameObjects.EntitySystems
{
    public class PowerSystem : EntitySystem
    {
        public List<Powernet> Powernets = new List<Powernet>();

        private IComponentManager componentManager;

        private int _lastUid = 0;

        public override void Initialize()
        {
            base.Initialize();
            componentManager = IoCManager.Resolve<IComponentManager>();
        }

        public int NewUid()
        {
            return ++_lastUid;
        }

        public override void Update(float frametime)
        {
            for (int i = 0; i < Powernets.Count; i++)
            {
                var powernet = Powernets[i];
                if (powernet.Dirty)
                {
                    //Tell all the wires of this net to be prepared to create/join new powernets
                    foreach (var wire in powernet.WireList)
                    {
                        wire.Regenerating = true;
                    }

                    foreach (var wire in powernet.WireList)
                    {
                        //Only a few wires should pass this if check since each will create and take all the others into its powernet
                        if (wire.Regenerating)
                            wire.SpreadPowernet();
                    }

                    //At this point all wires will have found/joined new powernet, all capable nodes will have joined them as well and removed themselves from nodelist
                    powernet.DirtyKill();
                    i--;
                }
            }

            foreach (var powernet in Powernets)
            {
                powernet.Update(frametime);
            }

            // Draw power for devices not connected to anything.
            foreach (var device in componentManager.GetComponents<PowerDeviceComponent>())
            {
                device.ProcessInternalPower(frametime);
            }
        }
    }
}
