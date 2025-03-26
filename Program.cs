// #region Attempt V5
// using Gtk;
// using System.Diagnostics;
// using EvDevSharp;
// using Newtonsoft.Json;
// using System.Reflection.Metadata;


// //TODO: Currently can't do simultaneous key presses with pen and tablet buttons.


// public class KeybindInterface
// {
//     static EvDevDevice tablet;
//     static EvDevDevice keyboard;
//     static EvDevDevice pen;
//     static DeviceConfig config;
//     private static Dictionary<string, bool> inputStates = new Dictionary<string, bool>();

//     public static void Mains()
//     {
//         Application.Init();
//         var window = new Window("APE KEYBINDER");
//         window.SetDefaultSize(400, 300);
//         window.Destroyed += (sender, args) => Application.Quit();

//         tablet = GetTablet();
//         keyboard = GetKeyboard();
//         pen = GetPen();

//         config = LoadConfig("deviceConfig.json");

//         var vbox = new VBox();
//         vbox.PackStart(new Label("Tablet Mappings"), false, false, 5);
//         foreach (var mapping in config.TabletMappings)
//         {
//             var hbox = new HBox();
//             var buttonLabel = new Label(mapping.ButtonId);

//             var actionComboBox = new ComboBoxText();
//             var actions = new string[] { "None", "Tab", "Return", "Escape", "BackSpace", "space", "Left", "Right", "Up", "Down", "Home", "End", "Page_Up", "Page_Down", "Insert", "Delete", "ctrl", "alt", "shift", "super", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" };
//             foreach (var action in actions)
//             {
//                 actionComboBox.AppendText(action);
//             }
//             actionComboBox.Active = Array.IndexOf(actions, mapping.Action);

//             var modifierComboBox = new ComboBoxText();
//             var modifiers = new string[] { "None", "ctrl", "alt", "shift", "super" };
//             foreach (var modifier in modifiers)
//             {
//                 modifierComboBox.AppendText(modifier);
//             }
//             modifierComboBox.Active = Array.IndexOf(modifiers, mapping.Modifier);

//             var saveButton = new Button("Save");

//             saveButton.Clicked += (sender, args) =>
//             {
//                 mapping.Action = actionComboBox.ActiveText;
//                 mapping.Modifier = modifierComboBox.ActiveText;
//                 SaveConfig("deviceConfig.json", config);
//             };

//             hbox.PackStart(buttonLabel, false, false, 5);
//             hbox.PackStart(actionComboBox, true, true, 5);
//             hbox.PackStart(modifierComboBox, true, true, 5);
//             hbox.PackStart(saveButton, false, false, 5);
//             vbox.PackStart(hbox, false, false, 5);
//         }

//         vbox.PackStart(new Label("Pen Mappings"), false, false, 5);
//         foreach (var mapping in config.PenMappings)
//         {
//             var hbox = new HBox();
//             var buttonLabel = new Label(mapping.ButtonId);

//             var actionComboBox = new ComboBoxText();
//             var actions = new string[] { "None", "click", "rightclick", "middleclick", "doubleclick" };
//             foreach (var action in actions)
//             {
//                 actionComboBox.AppendText(action);
//             }
//             actionComboBox.Active = Array.IndexOf(actions, mapping.Action);

//             var saveButton = new Button("Save");

//             saveButton.Clicked += (sender, args) =>
//             {
//                 mapping.Action = actionComboBox.ActiveText;
//                 SaveConfig("deviceConfig.json", config);
//             };

//             hbox.PackStart(buttonLabel, false, false, 5);
//             hbox.PackStart(actionComboBox, true, true, 5);
//             hbox.PackStart(saveButton, false, false, 5);
//             vbox.PackStart(hbox, false, false, 5);
//         }

//         window.Add(vbox);
//         window.ShowAll();

//         PenInput(pen, config);
//         TabletInput(tablet, config);

//         //StartAllMonitoring(tablet, pen, config);
//         //HandleInput(tablet, pen, config);

//         Application.Run();
//     }

//     private static EvDevDevice GetTablet()
//     {
//         var tablet = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Tablet Monitor Pad")).FirstOrDefault();
//         if (tablet == null)
//         {
//             Console.WriteLine("Tablet not found");
//         }
//         else
//         {
//             Console.WriteLine($"Tablet found: {tablet.Name}");
//         }
//         return tablet;
       
//     }

//     private static EvDevDevice GetKeyboard()
//     {
//         var keyboard = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Keyboard")).FirstOrDefault();
//         if (keyboard == null)
//         {
//             Console.WriteLine("Keyboard not found");
//         }
//         else
//         {
//             Console.WriteLine($"Keyboard found: {keyboard.Name}");
//         }
//         return keyboard;
//     }

//     private static EvDevDevice GetPen()
//     {
//         var pen = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Pen")).FirstOrDefault();
//         if (pen == null)
//         {
//             Console.WriteLine("Pen not found");
//         }
//         else
//         {
//             Console.WriteLine($"Pen found: {pen.Name}");
//         }
//         foreach (var keys in pen.Keys)
//         {
//             Console.WriteLine($"Key: {keys}");
//         }
//         return pen;
//     }

//     private static DeviceConfig LoadConfig(string filePath)
//     {
//         string json = File.ReadAllText(filePath);
//         return JsonConvert.DeserializeObject<DeviceConfig>(json);
//     }

//     private static void SaveConfig(string filePath, DeviceConfig config)
//     {
//         string json = JsonConvert.SerializeObject(config, Formatting.Indented);
//         File.WriteAllText(filePath, json);
//     }

//     public static void StartAllMonitoring (EvDevDevice tablet, EvDevDevice pen, DeviceConfig config)
//     {
//         Task.Run(() => TabletInput(tablet, config));
//         Task.Run(() => PenInput(pen, config));
//     }

//     private static void HandleInput(EvDevDevice tablet, EvDevDevice pen, DeviceConfig config)
//     {
//         tablet.OnKeyEvent += (sender, e) =>
//         {
//             Console.WriteLine($"Key event triggered: {e.Key}");
//             var buttonId = e.Key.ToString();
//             var mapping = config.TabletMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Mapped Action: {mapping.Action}, Modifier: {mapping.Modifier}");
//                 if (EvDevKeyValue.KeyDown == e.Value)
//                 {

//                     TriggerInput(mapping.Action, mapping.Modifier);
//                 }
//             }
//         };

//         pen.OnKeyEvent += (sender, e) =>
//         {
//             Console.WriteLine($"Key event triggered: {e.Key}");
//             var buttonId = e.Key.ToString();
//             var mapping = config.PenMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Mapped Action: {mapping.Action}");
//                 if (EvDevKeyValue.KeyDown == e.Value)
//                 {
//                     TriggerInput(mapping.Action, mapping.Modifier);
//                 }
//             }
//         };

//         pen.StartMonitoring();
//         tablet.StartMonitoring();
//     }

//     // private static void TabletInput(EvDevDevice tablet, DeviceConfig config)
//     // {
//     //     Console.WriteLine("Setting up tablet key event...");
//     //     tablet.OnKeyEvent += async (sender, e) =>
//     //     {
//     //         Console.WriteLine($"Tablet key event triggered: {e.Key}");
//     //         var buttonId = e.Key.ToString();

//     //         inputStates[buttonId] = e.Value == EvDevKeyValue.KeyDown;
//     //         var mapping = config.TabletMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//     //         if (mapping != null)
//     //         {
//     //             Console.WriteLine($"Mapped Action: {mapping.Action}, Modifier: {mapping.Modifier}");
//     //             if (EvDevKeyValue.KeyDown == e.Value)
//     //             {                    
//     //                 await Task.Run(() => TriggerKeyboardInput(mapping.Action, mapping.Modifier));
//     //                 // await Task.Run(() => TriggerInput(mapping.Action, mapping.Modifier));

//     //             }
//     //         }
//     //     };

//     //     tablet.StartMonitoring();
//     // }

//     // private static void PenInput(EvDevDevice pen, DeviceConfig config)
//     // {
//     //     Console.WriteLine("Setting up pen key event...");
//     //     pen.OnKeyEvent += async (sender, e) =>
//     //     {
//     //         Console.WriteLine($"Pen key event triggered: {e.Key}");
//     //         var buttonId = e.Key.ToString();
//     //         inputStates[buttonId] = e.Value == EvDevKeyValue.KeyDown;


//     //         var mapping = config.PenMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//     //         if (mapping != null)
//     //         {
//     //             Console.WriteLine($"Mapped Action: {mapping.Action}");
//     //             if (EvDevKeyValue.KeyDown == e.Value)
//     //             {
//     //                 await Task.Run(() => TriggerMouseInput(mapping.Action));
//     //                 //await Task.Run(() => TriggerInput(mapping.Action, mapping.Modifier));
//     //             }
//     //         }
//     //     };
//     //     pen.StartMonitoring();
//     // }

//     private static void TriggerKeyboardInput(string action, string modifier)
//     {
//         string command = "xdotool key";
//         if (!string.IsNullOrEmpty(modifier) && modifier != "None")
//         {
//             command += $" {modifier}+{action}";
//         }
//         else
//         {
//             command += $" {action}";
//         }

//         Console.WriteLine($"Executing command: {command}");
//         Process.Start("sh", $"-c \"{command}\"");
//     }

//     private static void TriggerMouseInput(string action)
//     {
//         string command = "xdotool click";
//         string normalizedAction = action.Trim().ToLowerInvariant();
//         switch (normalizedAction)
//         {
//             case "click":
//                 command += " 1";
//                 break;
//             case "rightclick":
//                 command += " 3";
//                 break;
//             case "middleclick":
//                 command += " 2";
//                 break;
//             case "doubleclick":
//                 command += " --repeat 2 1";
//                 break;
//             default:
//                 Console.WriteLine($"Unknown mouse action: {action}");
//                 return;
//         }

//         Console.WriteLine($"Executing command: {command}");
//         Process.Start("sh", $"-c \"{command}\"");
//     }
//     private static void TriggerInput(string action, string modifier)
//     {
//         // If it’s a click action, handle as mouse event.
//     if (action.EndsWith("click", StringComparison.OrdinalIgnoreCase))
//     {
//         // Press modifier if present.
//         if (!string.IsNullOrEmpty(modifier))
//             RunXdotool($"keydown {modifier}");

//         var clickMap = new Dictionary<string,string>
//         {
//             { "leftclick", "1" },
//             { "middleclick", "2" },
//             { "rightclick", "3" },
//             { "doubleclick", "--repeat 2 1" }
//         };

//         if (clickMap.ContainsKey(action.ToLower()))
//             RunXdotool($"click {clickMap[action.ToLower()]}");
//         else
//             Console.WriteLine($"Unknown click action: {action}");

//         if (!string.IsNullOrEmpty(modifier))
//             RunXdotool($"keyup {modifier}");
//         }
//         else
//         {
//             // Otherwise treat the action as a key.
//             if (!string.IsNullOrEmpty(modifier))
//                 RunXdotool($"keydown {modifier}");

//             RunXdotool($"key {action}");

//             if (!string.IsNullOrEmpty(modifier))
//                 RunXdotool($"keyup {modifier}");
//         }
//     }

//     private static void RunXdotool(string command)
//     {
//         var process = new Process
//         {
//             StartInfo = new ProcessStartInfo
//             {
//                 FileName = "sh",
//                 Arguments = $"-c \"{command}\"",
//                 RedirectStandardOutput = true,
//                 RedirectStandardError = true,
//                 UseShellExecute = false,
//                 CreateNoWindow = true
//             }
//         };
//         process.Start();
//         process.WaitForExit();
//     }

//     public class DeviceConfig
//     {
//         public List<KeyMapping> TabletMappings { get; set; }
//         public List<KeyMapping> PenMappings { get; set; }
//     }

//     public class KeyMapping
//     {
//         public string ButtonId { get; set; }
//         public string Action { get; set; }
//         public string Modifier { get; set; }
//     }

//     #region Handling simultaneous inputStates




// private static void TabletInput(EvDevDevice tablet, DeviceConfig config)
// {
//     Console.WriteLine("Setting up tablet key event...");
//     tablet.OnKeyEvent += async (sender, e) =>
//     {
//         Console.WriteLine($"Tablet key event triggered: {e.Key}");
//         var buttonId = e.Key.ToString();
//         inputStates[buttonId] = e.Value == EvDevKeyValue.KeyDown;

//         var mapping = config.TabletMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//         if (mapping != null)
//         {
//             Console.WriteLine($"Mapped Action: {mapping.Action}, Modifier: {mapping.Modifier}");
//             if (EvDevKeyValue.KeyDown == e.Value)
//             {
//                 await Task.Run(() => CheckCombinedInput(config));
//             }
//         }
//     };

//     tablet.StartMonitoring();
// }

// private static void PenInput(EvDevDevice pen, DeviceConfig config)
// {
//     Console.WriteLine("Setting up pen key event...");
//     pen.OnKeyEvent += async (sender, e) =>
//     {
//         Console.WriteLine($"Pen key event triggered: {e.Key}");
//         var buttonId = e.Key.ToString();
//         inputStates[buttonId] = e.Value == EvDevKeyValue.KeyDown;

//         var mapping = config.PenMappings.FirstOrDefault(m => m.ButtonId == buttonId);
//         if (mapping != null)
//         {
//             Console.WriteLine($"Mapped Action: {mapping.Action}");
//             if (EvDevKeyValue.KeyDown == e.Value)
//             {
//                 await Task.Run(() => CheckCombinedInput(config));
//             }
//         }
//     };
//     pen.StartMonitoring();
// }

// private static void CheckCombinedInput(DeviceConfig config)
// {
//     var tabletButtonPressed = inputStates.Any(kvp => kvp.Key.StartsWith("Tablet") && kvp.Value);
//     var penButtonPressed = inputStates.Any(kvp => kvp.Key.StartsWith("Pen") && kvp.Value);

//     if (tabletButtonPressed && penButtonPressed)
//     {
//         var tabletMapping = config.TabletMappings.FirstOrDefault(m => inputStates[m.ButtonId]);
//         var penMapping = config.PenMappings.FirstOrDefault(m => inputStates[m.ButtonId]);

//         if (tabletMapping != null && penMapping != null)
//         {
//             Console.WriteLine($"Combined Action: {tabletMapping.Action} + {penMapping.Action}");
//             TriggerCombinedInput(tabletMapping.Action, penMapping.Action, tabletMapping.Modifier);
//         }
//     }
// }

// private static void TriggerCombinedInput(string tabletAction, string penAction, string modifier)
// {
//     string command = "xdotool key";
//     if (!string.IsNullOrEmpty(modifier) && modifier != "None")
//     {
//         command += $" {tabletAction}+{penAction}";
//     }
//     else
//     {
//         command += $" {tabletAction}+{modifier}";
//     }

//     Console.WriteLine($"Executing command: {command}");
//     Process.Start("sh", $"-c \"{command}\"");
// }

//     #endregion Handling simultaneous inputStates



// }


// #endregion Attempt V5



