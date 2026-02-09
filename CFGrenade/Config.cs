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
        [Description("Скільки цифр в комбінації гравець має порахувати для деактивації заряду? Рекомендовано: 10")]
        public int DigitsToSum { get; set; } = 10;
    }
}
