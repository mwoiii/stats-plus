using RoR2;
using RoR2.Stats;
using BepInEx;
using R2API;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace StatsMod.Awards
{
    public abstract class Award
    {
        public readonly string awardName = "unnamedAward";  // Name of the award
        public readonly string awardDesc = "I'm so proud/disappointed in you!";  // Hover over award and view a small description?
        public readonly string playerName = null;  // Player who won the award
        public readonly bool soloCompatible = true;
        protected float scoreWeight = 1;

        public abstract (PlayerCharacterMasterController, float) CalculateWinner();
    }

}