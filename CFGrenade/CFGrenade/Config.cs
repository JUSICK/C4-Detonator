using System.ComponentModel;
using Exiled.API.Interfaces;
namespace CFGrenade
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        
        [Description("How many C4 charges can a player have?")]
        public int AvailableCharges { get; set; } = 3;
    }
}
