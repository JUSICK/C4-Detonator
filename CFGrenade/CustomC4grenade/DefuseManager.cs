using System.Collections.Generic;
using Exiled.API.Features;
using UnityEngine;
using System;
using С4grenade.CustomC4grenade;
using CommandSystem;
using Mirror;

namespace CFGrenade.CustomC4grenade;

public static class DefuseManager
{
    public static Dictionary<Player, (ImpactDetect c4, int answer, string codeString)> ActiveSessions = new ();
    
    public static void ActivateSession(Player player, ImpactDetect c4, out string generatedCode)
    {
        int sum = 0;
        string codeString = "";
        for (int i = 0; i < CFGrenade.MainPlugin.Instance.Config.DigitsToSum; i++)
        {
            int num = UnityEngine.Random.Range(0, 10);
            sum += num;
            codeString += num + (i < CFGrenade.MainPlugin.Instance.Config.DigitsToSum-1 ? ", " : "");
        }
        
        generatedCode = codeString;
        ActiveSessions[player] = (c4, sum, codeString);
    }
}

[CommandHandler(typeof(ClientCommandHandler))]
    public class DefuseCommand : ICommand
    {
        public string Command { get; } = "defusec4";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Команда для знешкодження С4. Використання: '.defusec4' (сканування), '.defusec4 [число]' (введення коду), '.defusec4 cancel' (скидання сесії). Для деактивації потрібно ввести суму цифр отриманого коду";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Players only.";
                return false;
            }

            if (arguments.Count > 0)
            {
                string arg = arguments.At(0).ToLower();
                if (arg == "cancel")
                {
                    if (DefuseManager.ActiveSessions.Remove(player))
                    {
                        response = "Пристрій знешкодження успішно відключено. Систему перезавантажено, готово до роботи.";
                    }

                    response = "Пристрій наразі не підключений до жодної вибухівки.";
                    return false;
                }
                if (!int.TryParse(arguments.At(0), out int playerAnswer))
                {
                    response = "Помилка використання. Доступні варіації команди: 'defuse', 'defuse [сума]' або 'defuse cancel'.";
                    return false;
                }

                if (!DefuseManager.ActiveSessions.TryGetValue(player, out var session))
                {
                    response = "Пристрій ще не просканував заряд! Спочатку використайте команду 'defuse', щоб отримати код.";
                    return false;
                }

                ImpactDetect c4 = session.c4;
                int correctAnswer = session.answer;
                if (c4 == null)
                {
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "ПОМИЛКА: Зв’язок із зарядом втрачено. Вибухівку не знайдено.";
                    return false;
                }
                
                if (Vector3.Distance(player.Position, c4.transform.position) > 5.0f)
                {
                    response = "ПОМИЛКА: Сигнал надто слабкий. Ви знаходитесь задалеко від С4!";
                    return false;
                }
                
                if (c4.isDefusalLocked)
                {
                    response = "ПОМИЛКА: Деактивація неможлива. Протокол захисту заблокував доступ після невдалої спроби.";
                    return false;
                }
                
                if (playerAnswer == correctAnswer)
                {
                    c4.Defuse();
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "Вибухівку успішно знешкоджено! Заряд деактивовано.";
                    return true;
                }
                else
                {
                    c4.isDefusalLocked = true;
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "Помилка деактивації! Введено невірний код. Захисний протокол заблокував можливість подальшого втручання.";
                    return false;
                }
            }
            int layerMask = ~((1 << 13) | (1 << 20));
            if (Physics.SphereCast(player.CameraTransform.position, 0.2f, player.CameraTransform.forward, out RaycastHit hit, 4f, layerMask))
            {
                ImpactDetect c4Script = hit.collider.GetComponentInParent<ImpactDetect>();
                if (c4Script != null)
                {
                    
                    if (c4Script.isDefusalLocked)
                    {
                        response = "ПОМИЛКА: Неможливо підключитися. На пристрої активовано систему захисту від зламу.";
                        return false;
                    }

                    if (DefuseManager.ActiveSessions.TryGetValue(player, out var currentSession))
                    {
                        if (currentSession.c4 == c4Script)
                        {
                            response = $"З’єднання вже встановлено. Поточна комбінація: {currentSession.codeString}.";
                            return false;
                        }

                        response = "Знайдено інший пристрій. Для перепідключення спочатку скасуйте поточну сесію: '.defusec4 cancel'";
                        return false;
                    }
                    DefuseManager.ActivateSession(player, c4Script, out string generatedCode);
                    response = $"УСПІХ: З’єднання встановлено. Отримано дану комбінацію: {generatedCode}. (Введіть суму цифр для деактивації).";
                    return true;
                }
            }
            response = "У радіусі дії не виявлено жодного вибухового пристрою.";
            return false;
        }
    }