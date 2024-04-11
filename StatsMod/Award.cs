using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace StatsMod
{
    public class Award
    {
        public readonly string awardName;  // Name of the award
        public readonly string awardDesc;  // Hover over award and view a small description?
        public readonly string playerName;  // Player who won the award
        public Award(string awardName, string awardDesc, string playerName) 
        {
            this.awardName = awardName;
            this.awardDesc = awardDesc;
            this.playerName = playerName;
        }
    }

}