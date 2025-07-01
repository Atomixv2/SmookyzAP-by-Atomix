using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SmookyzAP_by_Atomix
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const int NICKNAME_ADDR = 0x010DF5D8;
        const int MAP_ADDR = 0x010D856C;

        struct PlayerStatus
        {
            public int hpValue, hpMax, spValue, spMax;
        }

        class Buffs
        {
            public bool aspd, gloom, quag, sun, fire, water, wind,
                        str, dex, agi, vit, luk, intell, drowsiness, resentment, speed,
                        negativeStatus,
                        gloria, truesight, abrasive, autoguard, reflectshield, defender;
        }

        class ServerInfo
        {
            public string WindowTitle { get; set; }
            public int BaseAddress { get; set; }
        }

        class Config
        {
            public int aspdKey = -1, gloomKey = -1, sunKey = -1, spKey = -1, hpKey = -1,
                       fireKey = -1, waterKey = -1, windKey = -1,
                       dexKey = -1, agiKey = -1, vitKey = -1, lukKey = -1, intellKey = -1,
                       resentmentKey = -1, drowsinessKey = -1, speedKey = -1,
                       strKey = -1, statusRecoveryKey = -1, gloriaKey = -1,
                       truesightKey = -1, abrasiveKey = -1, autoguardKey = -1,
                       reflectshieldKey = -1, defenderKey = -1,
                       triggerKey = -1, poemKey = -1, riffKey = -1, appleKey = -1,
                       guitarKey = -1;
            public double spThreshold = 40.0;
            public double hpThreshold = 80.0;
            public int pauseKey = 0x24;
            public string windowTitle = "HoneyRO ~";
            public int autoBuffDelay = 30;
            public int chainMacroDelay = 1;
            public List<int> skillSpamKeys = new List<int>();
            public int skillSpamDelay = 1;
            public bool isChainMacroActive = false;
            public int currentMacroIndex = 0;
            public DateTime lastMacroKeyTime = DateTime.MinValue;
            public List<int> macroKeys = new List<int>();
        }

        static readonly Dictionary<string, int> virtualKeyMap = new()
        {
            { "F1", 0x70 }, { "F2", 0x71 }, { "F3", 0x72 }, { "F4", 0x73 },
            { "F5", 0x74 }, { "F6", 0x75 }, { "F7", 0x76 }, { "F8", 0x77 },
            { "F9", 0x78 }, { "F10", 0x79 }, { "F11", 0x7A }, { "F12", 0x7B },
            { "f1", 0x70 }, { "f2", 0x71 }, { "f3", 0x72 }, { "f4", 0x73 },
            { "f5", 0x74 }, { "f6", 0x75 }, { "f7", 0x76 }, { "f8", 0x77 },
            { "f9", 0x78 }, { "f10", 0x79 }, { "f11", 0x7A }, { "f12", 0x7B },
            { "0", 0x30 }, { "1", 0x31 }, { "2", 0x32 }, { "3", 0x33 }, { "4", 0x34 },
            { "5", 0x35 }, { "6", 0x36 }, { "7", 0x37 }, { "8", 0x38 }, { "9", 0x39 },
            { "NUM0", 0x60 }, { "NUM1", 0x61 }, { "NUM2", 0x62 }, { "NUM3", 0x63 },
            { "NUM4", 0x64 }, { "NUM5", 0x65 }, { "NUM6", 0x66 }, { "NUM7", 0x67 },
            { "NUM8", 0x68 }, { "NUM9", 0x69 },
            { "A", 0x41 }, { "B", 0x42 }, { "C", 0x43 }, { "D", 0x44 }, { "E", 0x45 },
            { "F", 0x46 }, { "G", 0x47 }, { "H", 0x48 }, { "I", 0x49 }, { "J", 0x4A },
            { "K", 0x4B }, { "L", 0x4C }, { "M", 0x4D }, { "N", 0x4E }, { "O", 0x4F },
            { "P", 0x50 }, { "Q", 0x51 }, { "R", 0x52 }, { "S", 0x53 }, { "T", 0x54 },
            { "U", 0x55 }, { "V", 0x56 }, { "W", 0x57 }, { "X", 0x58 }, { "Y", 0x59 }, { "Z", 0x5A },
            { "a", 0x41 }, { "b", 0x42 }, { "c", 0x43 }, { "d", 0x44 }, { "e", 0x45 },
            { "f", 0x46 }, { "g", 0x47 }, { "h", 0x48 }, { "i", 0x49 }, { "j", 0x4A },
            { "k", 0x4B }, { "l", 0x4C }, { "m", 0x4D }, { "n", 0x4E }, { "o", 0x4F },
            { "p", 0x50 }, { "q", 0x51 }, { "r", 0x52 }, { "s", 0x53 }, { "t", 0x54 },
            { "u", 0x55 }, { "v", 0x56 }, { "w", 0x57 }, { "x", 0x58 }, { "y", 0x59 }, { "z", 0x5A },
            { "HOME", 0x24 }, { "END", 0x23 }, { "INSERT", 0x2D }, { "DELETE", 0x2E },
            { "PAGEUP", 0x21 }, { "PAGEDOWN", 0x22 }
        };

        static double Percent(double val1, double val2) => (val2 == 0) ? 0 : (val1 / val2) * 100.0;

        static void PressKey(IntPtr hWnd, int key, int delay)
        {
            if (key != -1)
            {
                SetForegroundWindow(hWnd);
                PostMessage(hWnd, WM_KEYDOWN, key, 0);
                Thread.Sleep(5);
                PostMessage(hWnd, WM_KEYUP, key, 0);
                Thread.Sleep(delay); // No aplicar delay adicional para SkillSpammer
            }
        }

        static void PressHPKey(IntPtr hWnd, int key)
        {
            if (key != -1)
            {
                SetForegroundWindow(hWnd);
                PostMessage(hWnd, WM_KEYDOWN, key, 0);
                Thread.Sleep(5);
                PostMessage(hWnd, WM_KEYUP, key, 0);
            }
        }

        static void PressSPKey(IntPtr hWnd, int key)
        {
            if (key != -1)
            {
                SetForegroundWindow(hWnd);
                PostMessage(hWnd, WM_KEYDOWN, key, 0);
                Thread.Sleep(5);
                PostMessage(hWnd, WM_KEYUP, key, 0);
            }
        }

        static void MouseFlick()
        {
            mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, 0);
            Thread.Sleep(1);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(1);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(1);
            mouse_event(MOUSEEVENTF_MOVE, 0, 4294967295, 0, 0);
        }

        static string ReadStringFromMemory(IntPtr hProcess, int address, int maxLength)
        {
            byte[] buffer = new byte[maxLength];
            if (ReadProcessMemory(hProcess, (IntPtr)address, buffer, buffer.Length, out _))
            {
                int nullTerminatorIndex = Array.IndexOf(buffer, (byte)0);
                if (nullTerminatorIndex == -1) nullTerminatorIndex = buffer.Length;
                return Encoding.ASCII.GetString(buffer, 0, nullTerminatorIndex).Trim();
            }
            return "Unknown";
        }

        static List<ServerInfo> LoadServers()
        {
            const string file = "servers.ini";
            var servers = new List<ServerInfo>();

            if (!File.Exists(file))
            {
                File.WriteAllText(file, """
                [Servers]
                HoneyRO ~=010DCE10
                """);
            }

            var lines = File.ReadAllLines(file);
            string section = "";
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#")) continue;
                if (line.StartsWith("[") && line.EndsWith("]")) { section = line[1..^1]; continue; }

                if (section == "Servers")
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string windowTitle = parts[0].Trim();
                        if (int.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.HexNumber, null, out int baseAddress))
                        {
                            servers.Add(new ServerInfo { WindowTitle = windowTitle, BaseAddress = baseAddress });
                        }
                    }
                }
            }

            return servers;
        }

        static void LoadOrCreateConfig(Config config)
        {
            const string file = "config.ini";
            if (!File.Exists(file))
            {
                File.WriteAllText(file, """
                [Hotkeys]
                hpKey=F9
                spKey=F8
                statusRecoveryKey=F7
                pauseKey=HOME

                [Autobuffs]
                aspdKey=
                gloomKey=
                sunKey=
                fireKey=
                waterKey=
                windKey=
                strKey=
                dexKey=
                agiKey=
                vitKey=
                lukKey=
                intellKey=
                resentmentKey=
                drowsinessKey=
                speedKey=
                gloriaKey=
                truesightKey=
                abrasiveKey=
                autoguardKey=
                reflectshieldKey=
                defenderKey=

                [Settings]
                spThreshold=40
                hpThreshold=80
                windowTitle=HoneyRO ~
                autoBuffDelay=30

                [SkillSpammer]
                skillSpamKeys=
                skillSpamDelay=1

                [ChainMacros]
                triggerKey=F1
                poemKey=F2
                riffKey=F3
                appleKey=F4
                guitarKey=F1
                chainMacroDelay=1
                """);
                return;
            }

            var lines = File.ReadAllLines(file);
            string section = "";
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#")) continue;
                if (line.StartsWith("[") && line.EndsWith("]")) { section = line[1..^1]; continue; }

                var parts = line.Split('=');
                if (parts.Length != 2) continue;
                string key = parts[0].Trim(), val = parts[1].Trim();

                if (section == "Hotkeys" || section == "Autobuffs" || section == "ChainMacros")
                {
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        typeof(Config).GetField(key)?.SetValue(config, -1);
                    }
                    else if (virtualKeyMap.TryGetValue(val, out int code))
                    {
                        typeof(Config).GetField(key)?.SetValue(config, code);
                    }
                }
                else if (section == "Settings")
                {
                    if (key == "spThreshold" && double.TryParse(val, out double spVal))
                        config.spThreshold = spVal;
                    else if (key == "hpThreshold" && double.TryParse(val, out double hpVal))
                        config.hpThreshold = hpVal;
                    else if (key == "windowTitle")
                        config.windowTitle = val;
                    else if (key == "autoBuffDelay" && int.TryParse(val, out int delayVal))
                        config.autoBuffDelay = delayVal;
                }
                else if (section == "SkillSpammer")
                {
                    if (key == "skillSpamKeys")
                    {
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            var keys = val.Split(',').Select(k => k.Trim()).ToList();
                            foreach (var k in keys)
                            {
                                if (virtualKeyMap.TryGetValue(k, out int code))
                                    config.skillSpamKeys.Add(code);
                            }
                        }
                    }
                    else if (key == "skillSpamDelay" && int.TryParse(val, out int delay))
                        config.skillSpamDelay = Math.Max(1, delay);
                }
                else if (section == "ChainMacros")
                {
                    if (key == "chainMacroDelay" && int.TryParse(val, out int delay))
                        config.chainMacroDelay = Math.Max(1, delay);
                }
            }
        }

        static void DisplayConfig(Config config, string nickname, string map)
        {
            bool autopotEnabled = config.hpKey != -1 || config.spKey != -1 || config.statusRecoveryKey != -1;

            bool autobuffEnabled = config.aspdKey != -1 || config.gloomKey != -1 || config.sunKey != -1 ||
                                   config.fireKey != -1 || config.waterKey != -1 || config.windKey != -1 ||
                                   config.strKey != -1 || config.dexKey != -1 || config.agiKey != -1 ||
                                   config.vitKey != -1 || config.lukKey != -1 || config.intellKey != -1 ||
                                   config.resentmentKey != -1 || config.drowsinessKey != -1 || config.speedKey != -1 ||
                                   config.gloriaKey != -1 || config.truesightKey != -1 || config.abrasiveKey != -1 ||
                                   config.autoguardKey != -1 || config.reflectshieldKey != -1 || config.defenderKey != -1;

            string skillSpammerStatus = config.skillSpamKeys.Count > 0
                ? string.Join(", ", config.skillSpamKeys.Select(k => virtualKeyMap.FirstOrDefault(x => x.Value == k).Key))
                : "Disabled";

            bool chainMacroEnabled = config.triggerKey != -1;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server Found!!");
            Console.WriteLine("Atomix [ON]");
            Console.ResetColor();

            Console.Write("Nickname: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(nickname);
            Console.ResetColor();

            Console.Write("Map: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(map);
            Console.ResetColor();

            Console.Write("Autopot: ");
            Console.ForegroundColor = autopotEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(autopotEnabled ? "Enabled" : "Disabled");
            Console.ResetColor();

            Console.Write("Autobuff: ");
            Console.ForegroundColor = autobuffEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(autobuffEnabled ? "Enabled" : "Disabled");
            Console.ResetColor();

            Console.Write("Skill Spammer: ");
            Console.ForegroundColor = config.skillSpamKeys.Count > 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(skillSpammerStatus);
            Console.ResetColor();

            Console.Write("ClownMacro: ");
            Console.ForegroundColor = chainMacroEnabled ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(chainMacroEnabled ? "Enabled" : "Disabled");
            Console.ResetColor();
        }

        static void ReadHpOnly(IntPtr hProcess, int addr, ref PlayerStatus status)
        {
            byte[] buffer = new byte[8];
            if (ReadProcessMemory(hProcess, (IntPtr)addr, buffer, buffer.Length, out _))
            {
                status.hpValue = BitConverter.ToInt32(buffer, 0);
                status.hpMax = BitConverter.ToInt32(buffer, 4);
            }
        }

        static void ReadSp(IntPtr hProcess, int spAddr, ref PlayerStatus status)
        {
            byte[] buffer = new byte[8];
            if (ReadProcessMemory(hProcess, (IntPtr)spAddr, buffer, 8, out _))
            {
                status.spValue = BitConverter.ToInt32(buffer, 0);
                status.spMax = BitConverter.ToInt32(buffer, 4);
            }
        }

        static void CheckBuffs(IntPtr hProcess, int addr, Buffs buffs)
        {
            int bufferSize = 33;
            byte[] buffer = new byte[4 * bufferSize];
            if (!ReadProcessMemory(hProcess, (IntPtr)addr, buffer, buffer.Length, out _))
                return;

            for (int i = 0; i < bufferSize; i++)
            {
                int buffId = BitConverter.ToInt32(buffer, i * 4);
                if (buffId == -1) break;
                switch (buffId)
                {
                    case 883:
                    case 884:
                    case 885:
                    case 886:
                    case 887:
                        buffs.negativeStatus = true;
                        break;
                    case 3: buffs.gloom = true; break;
                    case 8: buffs.quag = true; break;
                    case 21: buffs.gloria = true; break;
                    case 37: case 38: case 39: buffs.aspd = true; break;
                    case 41: buffs.speed = true; break;
                    case 58: buffs.autoguard = true; break;
                    case 59: buffs.reflectshield = true; break;
                    case 62: buffs.defender = true; break;
                    case 115: buffs.truesight = true; break;
                    case 184: buffs.sun = true; break;
                    case 295: buffs.abrasive = true; break;
                    case 908: buffs.water = true; break;
                    case 910: buffs.fire = true; break;
                    case 911: buffs.wind = true; break;
                    case 241: buffs.str = true; break;
                    case 244: buffs.dex = true; break;
                    case 242: buffs.agi = true; break;
                    case 243: buffs.vit = true; break;
                    case 246: buffs.luk = true; break;
                    case 245: buffs.intell = true; break;
                    case 150: buffs.resentment = true; break;
                    case 151: buffs.drowsiness = true; break;
                }
            }
        }

        static void HandleActions(IntPtr hProcess, IntPtr hWnd, PlayerStatus status, Buffs buffs, double spThreshold, double hpThreshold, Config config, bool paused)
        {
            if (paused) return;

            if (config.statusRecoveryKey != -1 && buffs.negativeStatus)
            {
                PressKey(hWnd, config.statusRecoveryKey, 15);
                return;
            }

            if (config.resentmentKey != -1 && !buffs.resentment)
            {
                PressKey(hWnd, config.resentmentKey, config.autoBuffDelay);
                return;
            }

            if (config.drowsinessKey != -1 && !buffs.drowsiness)
            {
                PressKey(hWnd, config.drowsinessKey, config.autoBuffDelay);
                return;
            }

            if (!buffs.quag)
            {
                if (config.gloomKey != -1 && !buffs.gloom)
                {
                    PressKey(hWnd, config.gloomKey, config.autoBuffDelay);
                    return;
                }
            }
            if (config.aspdKey != -1 && !buffs.aspd)
            {
                PressKey(hWnd, config.aspdKey, config.autoBuffDelay);
                return;
            }
            if (config.sunKey != -1 && !buffs.sun)
            {
                PressKey(hWnd, config.sunKey, config.autoBuffDelay);
                return;
            }
            if (config.speedKey != -1 && !buffs.speed)
            {
                PressKey(hWnd, config.speedKey, config.autoBuffDelay);
                return;
            }
            if (config.fireKey != -1 && !buffs.fire)
            {
                PressKey(hWnd, config.fireKey, config.autoBuffDelay);
                return;
            }
            if (config.waterKey != -1 && !buffs.water)
            {
                PressKey(hWnd, config.waterKey, config.autoBuffDelay);
                return;
            }
            if (config.windKey != -1 && !buffs.wind)
            {
                PressKey(hWnd, config.windKey, config.autoBuffDelay);
                return;
            }
            if (config.strKey != -1 && !buffs.str)
            {
                PressKey(hWnd, config.strKey, config.autoBuffDelay);
                return;
            }
            if (config.dexKey != -1 && !buffs.dex)
            {
                PressKey(hWnd, config.dexKey, config.autoBuffDelay);
                return;
            }
            if (config.agiKey != -1 && !buffs.agi)
            {
                PressKey(hWnd, config.agiKey, config.autoBuffDelay);
                return;
            }
            if (config.vitKey != -1 && !buffs.vit)
            {
                PressKey(hWnd, config.vitKey, config.autoBuffDelay);
                return;
            }
            if (config.lukKey != -1 && !buffs.luk)
            {
                PressKey(hWnd, config.lukKey, config.autoBuffDelay);
                return;
            }
            if (config.intellKey != -1 && !buffs.intell)
            {
                PressKey(hWnd, config.intellKey, config.autoBuffDelay);
                return;
            }
            if (config.gloriaKey != -1 && !buffs.gloria)
            {
                PressKey(hWnd, config.gloriaKey, config.autoBuffDelay);
                return;
            }
            if (config.truesightKey != -1 && !buffs.truesight)
            {
                PressKey(hWnd, config.truesightKey, config.autoBuffDelay);
                return;
            }
            if (config.abrasiveKey != -1 && !buffs.abrasive)
            {
                PressKey(hWnd, config.abrasiveKey, config.autoBuffDelay);
                return;
            }
            if (config.autoguardKey != -1 && !buffs.autoguard)
            {
                PressKey(hWnd, config.autoguardKey, config.autoBuffDelay);
                return;
            }
            if (config.reflectshieldKey != -1 && !buffs.reflectshield)
            {
                PressKey(hWnd, config.reflectshieldKey, config.autoBuffDelay);
                return;
            }
            if (config.defenderKey != -1 && !buffs.defender)
            {
                PressKey(hWnd, config.defenderKey, config.autoBuffDelay);
                return;
            }
        }

        static void HandleChainMacros(IntPtr hWnd, Config config, bool paused)
        {
            if (paused || config.triggerKey == -1) return;

            // Iniciar la macro si se presiona triggerKey y no está activa
            if (!config.isChainMacroActive && (GetAsyncKeyState(config.triggerKey) & 0x8000) != 0)
            {
                config.isChainMacroActive = true;
                config.currentMacroIndex = 0;
                config.macroKeys.Clear();
                // Secuencia: guitar, apple, dagger, guitar, riff, dagger, guitar, poem, dagger
                config.macroKeys.AddRange(new[]
                {
                    config.guitarKey,
                    config.poemKey,
                    config.guitarKey,
                    config.riffKey,
                    config.guitarKey,
                    config.appleKey,
                    config.guitarKey,
                    config.poemKey,
                });
                config.lastMacroKeyTime = DateTime.Now;
            }

            // Ejecutar la siguiente tecla de la macro si está activa
            if (config.isChainMacroActive && config.currentMacroIndex < config.macroKeys.Count)
            {
                if ((DateTime.Now - config.lastMacroKeyTime).TotalMilliseconds >= config.chainMacroDelay)
                {
                    int key = config.macroKeys[config.currentMacroIndex];
                    if (key != -1)
                    {
                        SetForegroundWindow(hWnd);
                        PostMessage(hWnd, WM_KEYDOWN, key, 0);
                        Thread.Sleep(10);
                        PostMessage(hWnd, WM_KEYUP, key, 0);
                    }
                    config.currentMacroIndex++;
                    config.lastMacroKeyTime = DateTime.Now;
                }
            }

            // Finalizar la macro si se completó la secuencia
            if (config.isChainMacroActive && config.currentMacroIndex >= config.macroKeys.Count)
            {
                config.isChainMacroActive = false;
                config.currentMacroIndex = 0;
                config.macroKeys.Clear();
            }
        }

        static void Main()
        {
            Console.Title = "Atomix";
            var config = new Config();
            LoadOrCreateConfig(config);

            var servers = LoadServers();
            var selectedServer = servers.Find(s => s.WindowTitle == config.windowTitle);
            if (selectedServer == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: No se encontró el servidor '{config.windowTitle}' en servers.ini");
                Console.WriteLine("Por favor, agrega el servidor al archivo servers.ini y verifica el windowTitle.");
                Console.ResetColor();
                Console.WriteLine("Presiona cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }

            IntPtr hWnd = IntPtr.Zero;
            IntPtr hProcess = IntPtr.Zero;
            int pid = 0;

            while (hWnd == IntPtr.Zero || hProcess == IntPtr.Zero)
            {
                hWnd = FindWindow(null, config.windowTitle);
                if (hWnd != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hWnd, out pid);
                    hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
                }

                if (hWnd == IntPtr.Zero || hProcess == IntPtr.Zero)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Waiting for Ragnarok window...");
                    Thread.Sleep(1000);
                }
            }

            int baseAddr = selectedServer.BaseAddress;
            int hpAddr = baseAddr;
            int spAddr = baseAddr + 8;
            int buffAddr = baseAddr + 0x474;

            bool paused = false;
            bool hpUsed = false, spUsed = false;
            PlayerStatus status = new();
            Buffs buffs = new();
            int debounceDelayMs = 600;
            DateTime lastPauseToggle = DateTime.MinValue;
            DateTime lastInfoCheck = DateTime.MinValue;
            string currentNickname = ReadStringFromMemory(hProcess, NICKNAME_ADDR, 24);
            string currentMap = ReadStringFromMemory(hProcess, MAP_ADDR, 32);

            DisplayConfig(config, currentNickname, currentMap);

            while (true)
            {
                // Manejo de la pausa
                if ((GetAsyncKeyState(config.pauseKey) & 0x8000) != 0)
                {
                    if ((DateTime.Now - lastPauseToggle).TotalMilliseconds > debounceDelayMs)
                    {
                        paused = !paused;
                        Console.WriteLine(paused ? "Paused" : "Resumed");
                        lastPauseToggle = DateTime.Now;
                    }
                }

                if (paused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                // Autopot (HP/SP) - Prioridad máxima
                ReadHpOnly(hProcess, hpAddr, ref status);
                ReadSp(hProcess, spAddr, ref status);
                double hpPercent = Percent(status.hpValue, status.hpMax);
                double spPercent = Percent(status.spValue, status.spMax);

                if (config.hpKey != -1 && hpPercent < config.hpThreshold && !hpUsed && hpPercent > 0)
                {
                    PressHPKey(hWnd, config.hpKey);
                    hpUsed = true;
                    Thread.Sleep(100);
                }
                else if (hpPercent >= config.hpThreshold)
                {
                    hpUsed = false;
                }

                if (config.spKey != -1 && spPercent < config.spThreshold && !spUsed && spPercent > 0)
                {
                    PressSPKey(hWnd, config.spKey);
                    spUsed = true;
                    Thread.Sleep(100);
                }
                else if (spPercent >= config.spThreshold)
                {
                    spUsed = false;
                }

                // Buffs y actualización de mapa (verificar cada 1000ms)
                if ((DateTime.Now - lastInfoCheck).TotalMilliseconds >= 1000)
                {
                    buffs = new Buffs();
                    CheckBuffs(hProcess, buffAddr, buffs);
                    HandleActions(hProcess, hWnd, status, buffs, config.spThreshold, config.hpThreshold, config, paused);

                    string newMap = ReadStringFromMemory(hProcess, MAP_ADDR, 32);
                    if (newMap != currentMap)
                    {
                        currentMap = newMap;
                        Console.Clear();
                        DisplayConfig(config, currentNickname, currentMap);
                    }

                    lastInfoCheck = DateTime.Now;
                }

                // SkillSpammer
                bool anyKeyPressed = false;
                foreach (int key in config.skillSpamKeys)
                {
                    if (key != -1 && (GetAsyncKeyState(key) & 0x8000) != 0)
                    {
                        anyKeyPressed = true;
                        PressKey(hWnd, key, 0);
                        MouseFlick();
                        Thread.Sleep(config.skillSpamDelay);
                    }
                }

                // ChainMacros
                HandleChainMacros(hWnd, config, paused);

                // Retraso para reducir uso de CPU
                Thread.Sleep(anyKeyPressed ? 1 : 10);
            }
        }
    }
}