
---

# 💣 C4 Detonator (SCP:SL Plugin)

An interactive Custom Item plugin for SCP: Secret Laboratory that introduces remote-detonated C4 explosives and a high-stakes defusal mini-game for RP.

## ✨ Features

* **Sticky Explosives:**.
* **Remote Detonation:**.
* **Interactive Defusal:** Players can look at a planted C4 and use the `.defusec4` command to initiate a defusal session.
* **Math-Based Mini-game:** Defusing requires calculating the sum of a generated code.
* **Anti-Tamper Mechanism:** Entering the wrong code permanently locks the C4, preventing any future defusal attempts!
* **Immersive Visuals & Audio:** Features warning lights and custom sounds/schematics.

---

## 🎮 Player Commands

Players can use these commands in the client console (`~`) to interact with planted explosives.

| Command | Description |
| --- | --- |
| `.defusec4` | Scans the C4 you are looking at and generates the defusal code. |
| `.defusec4 [sum]` | Submits your mathematical answer to attempt a defuse. |
| `.defusec4 cancel` | Safely disconnects your active defusal session so you can scan a different explosive. |

---

## 🛠️ Installation

1. Make sure you have the [EXILED framework](https://github.com/Exiled-Team/EXILED) installed on your SCP:SL server.
2. Download the latest `C4Detonator.dll` from the **Releases** tab.
3. Place the `.dll` file into your server's `EXILED/Plugins` folder.
4. Restart your server. The necessary configurations will be generated automatically.

---

