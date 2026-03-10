using System.ComponentModel;
using Exiled.API.Interfaces;
namespace CFGrenade
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        
        [Description("Скільки зарядів С4 матиме гравець?")]
        public int AvailableCharges { get; set; } = 3;
        [Description("How many 'levels' of defense does the c4 have? For RP: 10; For Yamato: 2")]
        public int DigitsToSum { get; set; } = 10;
    }
}
