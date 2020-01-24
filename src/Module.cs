using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;

namespace UnturnedConsoleModule
{
    public class Module : IModuleNexus
    {
        public void initialize()
        {
            Dedicator.commandWindow.setIOHandler(new CommandInputOutput());
        }

        public void shutdown()
        {
        }
    }
}