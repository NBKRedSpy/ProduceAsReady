using HarmonyLib;
using MGSC;
using ProduceAsReady_Bootstrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ProduceAsReady
{
    public class Plugin : BootstrapMod
    {

        public static ConfigDirectories ConfigDirectories = new ConfigDirectories();

        public static Logger Logger = new Logger();

        public Plugin(HookEvents hookEvents, bool isBeta) : base(hookEvents, isBeta)
        {
            new Harmony("NBKRedSpy_" + ConfigDirectories.ModAssemblyName).PatchAll();
        }
     
    }
}
