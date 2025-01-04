using Gtk;
using System.Diagnostics;
using EvDevSharp;
using Newtonsoft.Json;


namespace TabletRemapper
{
    // TODO: add input for pen nib (shift drag for brush size)
    
    class TabletRemapper
    {
        private DeviceConfig config;
        private static EvDevDevice tablet;
        private static EvDevDevice pen;

        static void Main(string[] args)
        {
            
            var remapper = new TabletRemapper();
            remapper.Run();
        }

        private void Run()
        {
            Application.Init();
            var window = new Window("Tablet Remapper");
            window.SetDefaultSize(400, 600);
            window.DeleteEvent += delegate { Application.Quit(); };

            config = LoadConfig("deviceConfig.json");
            tablet = GetTablet();
            pen = GetPen();
            
            BuildUI(window, config);
            SetupDeviceMonitoring(pen, tablet);            

            List<string> commandList = new List<string>();

            tablet.OnKeyEvent += (sender, e) => HandleEvent(e, "Tablet", commandList);
            pen.OnKeyEvent += (sender, e) => HandleEvent(e, "Pen", commandList);
            
            tablet.StartMonitoring();
            pen.StartMonitoring();

            window.ShowAll();
            Application.Run();
        }


#region UI

        private void BuildUI(Window window, DeviceConfig config)
        {
            var scrollWindow = new ScrolledWindow();
            var mainVBox = new VBox();
            
            // Tablet mappings
            mainVBox.PackStart(new Label("Tablet Mappings"), false, false, 5);
            foreach (var mapping in config.TabletMappings)
            {
                var hbox = new HBox();
                var buttonLabel = new Label(mapping.ButtonId);

                var actionComboBox = new ComboBoxText();
                var actions = new string[] { "None", "Tab", "Return", "Escape", "BackSpace", "space", 
                    "Left", "Right", "Up", "Down", "Home", "End", "Page_Up", "Page_Down", "Insert", 
                    "Delete", "ctrl", "alt", "shift", "super", "a", "b", "c", "d", "e", "f", "g", 
                    "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", 
                    "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", 
                    "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", 
                    
                    "click", "rightclick", "middleclick", "doubleclick" };
                foreach (var action in actions)
                {
                    actionComboBox.AppendText(action);
                }
                actionComboBox.Active = Array.IndexOf(actions, mapping.Action);

                var modifierComboBox = new ComboBoxText();
                var modifiers = new string[] { "None", "ctrl", "alt", "shift", "super" };
                foreach (var modifier in modifiers)
                {
                    modifierComboBox.AppendText(modifier);
                }
                modifierComboBox.Active = Array.IndexOf(modifiers, mapping.Modifier);

                var saveButton = new Button("Save");
                saveButton.Clicked += (sender, args) =>
                {
                    mapping.Action = actionComboBox.ActiveText;
                    mapping.Modifier = modifierComboBox.ActiveText;
                    SaveConfig("deviceConfig.json", config);
                };

                hbox.PackStart(buttonLabel, false, false, 5);
                hbox.PackStart(new Label("      "), false, false, 5);  // Spacer 
                hbox.PackStart(actionComboBox, true, true, 5);
                hbox.PackStart(modifierComboBox, true, true, 5);
                hbox.PackStart(new Label("    "), false, false, 5);  // Spacer before save button
                hbox.PackStart(saveButton, false, false, 5);
                hbox.PackStart(new Label("  "), false, false, 5);    // Right margin
                mainVBox.PackStart(hbox, false, false, 5);
            }

            // Pen mappings
            mainVBox.PackStart(new Label("Pen Mappings"), false, false, 5);
            foreach (var mapping in config.PenMappings)
            {
                var hbox = new HBox();
                var buttonLabel = new Label(mapping.ButtonId);

                var actionComboBox = new ComboBoxText();
                var actions = new string[] { "None", "click", "rightclick", "middleclick", "doubleclick" };
                foreach (var action in actions)
                {
                    actionComboBox.AppendText(action);
                }
                actionComboBox.Active = Array.IndexOf(actions, mapping.Action);

                var saveButton = new Button("Save");
                saveButton.Clicked += (sender, args) =>
                {
                    mapping.Action = actionComboBox.ActiveText;
                    SaveConfig("deviceConfig.json", config);
                };

                hbox.PackStart(buttonLabel, false, false, 5);
                hbox.PackStart(new Label("      "), false, false, 5);  // Spacer 
                hbox.PackStart(actionComboBox, true, true, 5);
                hbox.PackStart(new Label("    "), false, false, 5);  // Spacer before save button
                hbox.PackStart(saveButton, false, false, 5);
                hbox.PackStart(new Label("  "), false, false, 5);    // Right margin
                mainVBox.PackStart(hbox, false, false, 5);  
            }

            scrollWindow.AddWithViewport(mainVBox);
            scrollWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollWindow.KineticScrolling = true;
            window.Add(scrollWindow);
        }

#endregion

#region Mapping

        public class DeviceConfig
        {
            public List<KeyMapping> TabletMappings { get; set; }
            public List<KeyMapping> PenMappings { get; set; }
        }

        public class KeyMapping
        {
            public string ButtonId { get; set; }
            public string Action { get; set; }
            public string Modifier { get; set; }
        }

#endregion

#region Config
        private DeviceConfig LoadConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DeviceConfig>(json);
        }

        public  void SaveConfig(string filePath, DeviceConfig config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
#endregion

#region Devices
        private void SetupDeviceMonitoring(EvDevDevice tablet, EvDevDevice pen) 
        {
            if(tablet == null && pen == null)
            {
                Console.WriteLine("No devices found. Exiting...");
                Environment.Exit(1);
            }
            if (tablet != null) tablet.StartMonitoring();
            if (pen != null) pen.StartMonitoring();
        }

        private EvDevDevice GetTablet()
        {
            var device = EvDevDevice.GetDevices().FirstOrDefault(d => d.Name.Contains("Pad"));
            if (device != null) Console.WriteLine($"Tablet found: {device.Name}");
            return device;
        }

        private EvDevDevice GetPen()
        {
            var device = EvDevDevice.GetDevices().FirstOrDefault(d => d.Name.Contains("Pen"));
            if (device != null) Console.WriteLine($"Pen found: {device.Name}");
            return device;
        }

#endregion


#region Input


        private void HandleEvent(OnKeyEventArgs e, string deviceType, List<string> commandList)
        {
            string buttonId = e.Key.ToString();
            var mappings = deviceType == "Tablet" ? config.TabletMappings : config.PenMappings;
            var mapping = mappings.FirstOrDefault(m => m.ButtonId == buttonId);

            if (mapping != null)
            {
                if (e.Value == EvDevKeyValue.KeyDown) // Button pressed
                {
                    if (mapping.Modifier != "None")
                    {
                        commandList.Add($"keydown {mapping.Modifier.ToLower()}");
                    }

                    // Handle pen-specific actions (e.g., clicks)
                    if (deviceType == "Pen" && IsClickAction(mapping.Action))
                    {
                        string translatedCommand = TranslatePenInput(mapping.Action);
                        if (translatedCommand != "None")
                        {
                            commandList.Add(translatedCommand);
                        }
                    }
                    else
                    {
                        commandList.Add($"keydown {mapping.Action.ToLower()}");
                    }
                }
                else if (e.Value == EvDevKeyValue.KeyUp) // Button released
                {
                    if (mapping.Modifier != "None")
                    {
                        commandList.Add($"keyup {mapping.Modifier.ToLower()}");
                    }

                    // Only send keyup for non-click actions
                    if (!IsClickAction(mapping.Action))
                    {
                        commandList.Add($"keyup {mapping.Action.ToLower()}");
                    }
                }

                // If the list contains commands, send to XDoTool
                if (commandList.Count > 0)
                {
                    string command = string.Join(" ", commandList);
                    RunXdotool(command);
                    commandList.Clear(); // Clear the command list after execution
                }
            }
        }

        private bool IsClickAction(string action)
        {
            var clickActions = new List<string> { "leftclick", "middleclick", "rightclick", "doubleclick" };
            return clickActions.Contains(action.ToLower());
        }        
        private string TranslatePenInput(string action)
        {
            string command = "click ";
            var clickMap = new Dictionary<string, string>
            {
                { "leftclick", "1" },
                { "middleclick", "2" },
                { "rightclick", "3" },
                { "doubleclick", "--repeat 2 1" }
            };
            if (clickMap.ContainsKey(action.ToLower()))
            {
                command += $"{clickMap[action.ToLower()]}";
                // Console.WriteLine($"Translated pen input Command: {command}");
                return command;
            }
            else
            {
                Console.WriteLine($"Unknown click action: {action}");
                return "None";
            }

        } 

#endregion

#region XDoTool

        private void RunXdotool(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sh",
                    Arguments = $"-c \"xdotool {command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error executing command '{command}': {error}");
            }
            else
            {
                Console.WriteLine($"Command '{command}' executed successfully: {output}");
            }
        }



#endregion

    }
}
