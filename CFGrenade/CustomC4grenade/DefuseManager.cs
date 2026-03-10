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
        public string Description { get; } = "Command to defuse C4. Usage: '.defusec4' (scan), '.defusec4 [number]' (enter code), '.defusec4 cancel' (reset session). To deactivate, you must enter the sum of the digits of the received code.";

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
                        response = "Defusal device successfully disconnected. System rebooted, ready to use.";
                    }

                    response = "The device is not currently connected to any explosives.";
                    return false;
                }
                if (!int.TryParse(arguments.At(0), out int playerAnswer))
                {
                    response = "Usage error. Available command variations: 'defuse', 'defuse [sum]' or 'defuse cancel'.";
                    return false;
                }

                if (!DefuseManager.ActiveSessions.TryGetValue(player, out var session))
                {
                    response = "The device has not scanned the charge yet! First, use the 'defuse' command to obtain the code.";
                    return false;
                }

                ImpactDetect c4 = session.c4;
                int correctAnswer = session.answer;
                if (c4 == null)
                {
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "ERROR: Connection to the charge lost. No explosives found.";
                    return false;
                }
                
                if (Vector3.Distance(player.Position, c4.transform.position) > 5.0f)
                {
                    response = "ERROR: Signal too weak. You are too far from the C4!";
                    return false;
                }
                
                if (c4.isDefusalLocked)
                {
                    response = "ERROR: Deactivation impossible. Security protocol has blocked access after a failed attempt.";
                    return false;
                }
                
                if (playerAnswer == correctAnswer)
                {
                    c4.Defuse();
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "Explosives successfully defused! Charge deactivated.";
                    return true;
                }
                else
                {
                    c4.isDefusalLocked = true;
                    DefuseManager.ActiveSessions.Remove(player);
                    response = "Deactivation error! Incorrect code entered. Security protocol has blocked any further interference.";
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
                        response = "ERROR: Unable to connect. The device's anti-tamper security system is active.";
                        return false;
                    }

                    if (DefuseManager.ActiveSessions.TryGetValue(player, out var currentSession))
                    {
                        if (currentSession.c4 == c4Script)
                        {
                            response = $"Connection already established. Current combination: {currentSession.codeString}";
                            return false;
                        }

                        response = "Another device found. To reconnect, first cancel the current session: '.defusec4 cancel'";
                        return false;
                    }
                    DefuseManager.ActivateSession(player, c4Script, out string generatedCode);
                    response = $"SUCCESS: Connection established. Received the following combination: {generatedCode}. (Enter the sum of the digits to deactivate).";
                    return true;
                }
            }
            response = "No explosive devices detected within range.";
            return false;
        }
    }