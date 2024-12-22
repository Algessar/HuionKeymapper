#region Attempt V5
using Gtk;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using EvDevSharp;
using Newtonsoft.Json;
using System.Net;

public class KeybindInterface
{
    static EvDevDevice tablet;
    static EvDevDevice keyboard;
    static EvDevDevice pen;
    static DeviceConfig config;

    public static void Main()
    {
        Application.Init();
        var window = new Window("APE KEYBINDER");
        window.SetDefaultSize(400, 300);
        window.Destroyed += (sender, args) => Application.Quit();

        tablet = GetTablet();
        keyboard = GetKeyboard();
        pen = GetPen();

        config = LoadConfig("deviceConfig.json");

        var vbox = new VBox();
        vbox.PackStart(new Label("Tablet Mappings"), false, false, 5);
        foreach (var mapping in config.TabletMappings)
        {
            var hbox = new HBox();
            var buttonLabel = new Label(mapping.ButtonId);

            var actionComboBox = new ComboBoxText();
            var actions = new string[] { "None", "Tab", "Return", "Escape", "BackSpace", "space", "Left", "Right", "Up", "Down", "Home", "End", "Page_Up", "Page_Down", "Insert", "Delete", "ctrl", "alt", "shift", "super", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" };
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
            hbox.PackStart(actionComboBox, true, true, 5);
            hbox.PackStart(modifierComboBox, true, true, 5);
            hbox.PackStart(saveButton, false, false, 5);
            vbox.PackStart(hbox, false, false, 5);
        }

        vbox.PackStart(new Label("Pen Mappings"), false, false, 5);
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
            hbox.PackStart(actionComboBox, true, true, 5);
            hbox.PackStart(saveButton, false, false, 5);
            vbox.PackStart(hbox, false, false, 5);
        }

        window.Add(vbox);
        window.ShowAll();

        StartAllMonitoring(tablet, pen, config);

        Application.Run();
    }

    private static EvDevDevice GetTablet()
    {
        var tablet = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Tablet Monitor Pad")).FirstOrDefault();
        if (tablet == null)
        {
            Console.WriteLine("Tablet not found");
        }
        else
        {
            Console.WriteLine($"Tablet found: {tablet.Name}");
        }
        return tablet;

        //Why isnt git working
    }

    private static EvDevDevice GetKeyboard()
    {
        var keyboard = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Keyboard")).FirstOrDefault();
        if (keyboard == null)
        {
            Console.WriteLine("Keyboard not found");
        }
        else
        {
            Console.WriteLine($"Keyboard found: {keyboard.Name}");
        }
        return keyboard;
    }

    private static EvDevDevice GetPen()
    {
        var pen = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Pen")).FirstOrDefault();
        if (pen == null)
        {
            Console.WriteLine("Pen not found");
        }
        else
        {
            Console.WriteLine($"Pen found: {pen.Name}");
        }
        foreach (var keys in pen.Keys)
        {
            Console.WriteLine($"Key: {keys}");
        }
        return pen;
    }

    private static DeviceConfig LoadConfig(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<DeviceConfig>(json);
    }

    private static void SaveConfig(string filePath, DeviceConfig config)
    {
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static void StartAllMonitoring (EvDevDevice tablet, EvDevDevice pen, DeviceConfig config)
    {
        Task.Run(() => TabletInput(tablet, config));
        Task.Run(() => PenInput(pen, config));
    }

    private static void TabletInput(EvDevDevice tablet, DeviceConfig config)
    {
        Console.WriteLine("Setting up tablet key event...");
        tablet.OnKeyEvent += async (sender, e) =>
        {
            Console.WriteLine($"Tablet key event triggered: {e.Key}");
            var buttonId = e.Key.ToString();
            var mapping = config.TabletMappings.FirstOrDefault(m => m.ButtonId == buttonId);
            if (mapping != null)
            {
                Console.WriteLine($"Mapped Action: {mapping.Action}, Modifier: {mapping.Modifier}");
                if (EvDevKeyValue.KeyDown == e.Value)
                {
                    //await Task.Run(() => TriggerKeyboardInput(mapping.Action, mapping.Modifier));
                    await Task.Run(() => TriggerInput(mapping.Action, mapping.Modifier));

                }
            }
        };

        tablet.StartMonitoring();
    }

    private static void PenInput(EvDevDevice pen, DeviceConfig config)
    {
        Console.WriteLine("Setting up pen key event...");
        pen.OnKeyEvent += async (sender, e) =>
        {
            Console.WriteLine($"Pen key event triggered: {e.Key}");
            var buttonId = e.Key.ToString();
            var mapping = config.PenMappings.FirstOrDefault(m => m.ButtonId == buttonId);
            if (mapping != null)
            {
                Console.WriteLine($"Mapped Action: {mapping.Action}");
                if (EvDevKeyValue.KeyDown == e.Value)
                {
                    //await Task.Run(() => TriggerMouseInput(mapping.Action));
                    await Task.Run(() => TriggerInput(mapping.Action, mapping.Modifier));
                }
            }
        };
        pen.StartMonitoring();
    }

    private static void TriggerKeyboardInput(string action, string modifier)
    {
        string command = "xdotool key";
        if (!string.IsNullOrEmpty(modifier) && modifier != "None")
        {
            command += $" {modifier}+{action}";
        }
        else
        {
            command += $" {action}";
        }

        Console.WriteLine($"Executing command: {command}");
        Process.Start("sh", $"-c \"{command}\"");
    }

    private static void TriggerMouseInput(string action)
    {
        string command = "xdotool click";
        switch (action)
        {
            case "click":
                command += " 1";
                break;
            case "rightclick":
                command += " 3";
                break;
            case "middleclick":
                command += " 2";
                break;
            case "doubleclick":
                command += " --repeat 2 1";
                break;
            default:
                Console.WriteLine($"Unknown mouse action: {action}");
                return;
        }

        Console.WriteLine($"Executing command: {command}");
        Process.Start("sh", $"-c \"{command}\"");
    }
    private static void TriggerInput(string action, string modifier)
    {
        // If it’s a click action, handle as mouse event.
    if (action.EndsWith("click", StringComparison.OrdinalIgnoreCase))
    {
        // Press modifier if present.
        if (!string.IsNullOrEmpty(modifier))
            RunXdotool($"keydown {modifier}");

        var clickMap = new Dictionary<string,string>
        {
            { "leftclick", "1" },
            { "middleclick", "2" },
            { "rightclick", "3" },
            { "doubleclick", "--repeat 2 1" }
        };

        if (clickMap.ContainsKey(action.ToLower()))
            RunXdotool($"click {clickMap[action.ToLower()]}");
        else
            Console.WriteLine($"Unknown click action: {action}");

        if (!string.IsNullOrEmpty(modifier))
            RunXdotool($"keyup {modifier}");
    }
    else
    {
        // Otherwise treat the action as a key.
        if (!string.IsNullOrEmpty(modifier))
            RunXdotool($"keydown {modifier}");

        RunXdotool($"key {action}");

        if (!string.IsNullOrEmpty(modifier))
            RunXdotool($"keyup {modifier}");
    }
    }

    private static void RunXdotool(string command)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xdotool",
                Arguments = command,
                UseShellExecute = false
            }
        };
        process.Start();
        process.WaitForExit();
    }

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
}

#endregion



#region Attempt V4


// using System;
// using System.Linq;
// using System.Threading;
// using EvDevSharp;
// using Newtonsoft.Json;

// class Program
// {
//     private static EvDevDevice tablet;
//     private static EvDevDevice keyboard;
//     private static Config config;

//     private static Config keyboardConfig;

//     static void Main(string[] args)
//     {
//         // Set up signal handlers
//         Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
//         AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

//         // Initialize devices and config
//         InitializeDevices();

//         // Main program logic
//         MonitorDevices();

//         // Keep the application running
//         Console.WriteLine("Application is running. Press Ctrl+C to exit.");
//         while (true)
//         {
//             Thread.Sleep(1000);
//         }
//     }

//     private static void InitializeDevices()
//     {
//         var devices = EvDevDevice.GetDevices().OrderBy(d => d.DevicePath).ToList();
//         tablet = devices.FirstOrDefault(d => d.Name.Contains("Tablet Monitor Pad"));
//         keyboard = devices.FirstOrDefault(d => d.Name.Contains("Keyboard"));

//         if (tablet == null)
//         {
//             Console.WriteLine("Tablet not found");
//             return;
//         }

//         if (keyboard == null)
//         {
//             Console.WriteLine("Keyboard not found");
//             return;
//         }

//         // Load config (assuming config is loaded here)
        
//         config = LoadConfig("tabletConfig.json");
//     }

//     private static void MonitorDevices()
//     {
//         tablet.OnKeyEvent += (sender, e) =>
//         {
//             var buttonId = e.Key.ToString(); // e.g., "KEY_A", "KEY_B", etc.

//             // Find the corresponding action from the config
//             var mapping = config.Mappings.FirstOrDefault(m => m.ButtonId == buttonId);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Button {buttonId} should trigger action: {mapping.Action} and modifier: {mapping.Modifier}");

//                 // Execute the action
//                 ExecuteAction(mapping.Action, mapping.Modifier, keyboard, tablet);
//             }
//         };

//         keyboard.OnKeyEvent += (sender, e) =>
//         {
//             var keyID = e.Key.ToString(); // e.g., "KEY_A", "KEY_B", etc.

//             // Find the corresponding action from the config
//             var mapping = config.Mappings.FirstOrDefault(m => m.ButtonId == keyID);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Button {keyID} should trigger action: {mapping.Action} and modifier: {mapping.Modifier}");

//                 // Execute the action
//                 ExecuteAction(mapping.Action, mapping.Modifier, keyboard, tablet);
//             }
//         };

//         tablet.StartMonitoring();
//         keyboard.StartMonitoring();
//     }

//     private static void OnExit(object sender, EventArgs e)
//     {
//         Console.WriteLine("Exiting application...");

//         // Stop monitoring devices
//         tablet?.StopMonitoring();
//         keyboard?.StopMonitoring();

//         // Perform any necessary cleanup here

//         Console.WriteLine("Application exited.");
//     }

//     private static Config LoadConfig(string filePath)
//     {
//         // Load your configuration here
//         string json = File.ReadAllText(filePath);
//         return JsonConvert.DeserializeObject<Config>(json);
        
//     }

//     private static void ExecuteAction(string action, string modifier, EvDevDevice keyboard, EvDevDevice tablet)
//     {
//         // Implement your action execution logic here
//     }
// }

// public class Config{

//     public List<ButtonMapping> Mappings { get; set; }
// }

// public class ButtonMapping
// {
//     public string ButtonId { get; set; }
//     public string Action { get; set; }
//     public string Modifier { get; set; }

//     public ButtonMapping(string buttonId, string action, string modifier)
//     {
//         ButtonId = buttonId;
//         Action = action;
//         Modifier = modifier;
//     }
// }
#endregion


#region Attempt V3

// using System;
// using System.IO;
// using System.Diagnostics;
// using System.Linq;
// using EvDevSharp;


// class Program
// {
//     private static EvDevDevice huionDevice;

//     static void Main(string[] args)
//     {
//         var devices = EvDevDevice.GetDevices().OrderBy(d => d.DevicePath).ToList();
//         foreach (var device in devices)
//         {
//             Console.WriteLine($"Device: {device.Name}, Path: {device.DevicePath}");
//         }

//         huionDevice = devices.FirstOrDefault(d => d.Name.Contains("Tablet Monitor Pad"));

//         if (huionDevice == null)
//         {
//             Console.WriteLine("Huion Kamvas 13 not found.");
//             return;
//         }

//         Console.WriteLine($"Monitoring device: {huionDevice.Name}, Path: {huionDevice.DevicePath}");

//         huionDevice.OnKeyEvent += (s, e) =>
//         {
//             Console.WriteLine($"Original Key: {e.Key}\tState: {e.Value}");
//             HandleKeyEvent(e);
//         };

//         huionDevice.StartMonitoring();
//         Console.WriteLine("Monitoring started. Press Enter to stop...");
//         Console.ReadLine();
//         huionDevice.StopMonitoring();
//     }

//     private static void HandleKeyEvent(OnKeyEventArgs e)
//     {
//         // Example rebindings
//         switch (e.Key)
//         {
//             case EvDevKeyCode.KEY_7:
//                 if (e.Value == EvDevKeyValue.KeyDown)
//                 {
//                     Console.WriteLine("Button 1 pressed");
//                     SimulateKeyPress("ctrl+z"); // Ctrl+Z
//                 }
//                 break;
//             case EvDevKeyCode.KEY_0:
//                 if (e.Value == EvDevKeyValue.KeyDown)
//                 {
//                     SimulateKeyPress("Alt"); // Escape key
//                 }
//                 break;
//             case EvDevKeyCode.KEY_1:
//                 if (e.Value == EvDevKeyValue.KeyDown)
//                 {
                    
//                     SimulateKeyPress("ctrl"); // Ctrl+S
//                 }
//                 break;
//             // Add more rebindings as needed
//         }
//     }

//     private static void SimulateKeyPress(string keys)
//     {
//         var process = new Process
//         {
//             StartInfo = new ProcessStartInfo
//             {
//                 FileName = "xdotool",
//                 Arguments = $"key {keys}",
//                 RedirectStandardOutput = true,
//                 UseShellExecute = true,
//                 CreateNoWindow = true,
//             }
//         };
//         process.Start();
//         process.WaitForExit();
//     }
// }

#endregion


#region Attempt V2


// using System;
// using System.IO;
// using EvDevSharp;
// using System.Linq;
// using System.Collections.Generic;
// using Newtonsoft.Json;
// using Microsoft.VisualBasic;
// using System.Reflection;

// #pragma warning disable CA1416 // Validate platform compatibility


// public class ButtonMapping{

//     public string ButtonId { get; set; }
//     public string Action { get; set; }

//     public string Modifier { get; set; }

//     public ButtonMapping(string buttonId, string action, string modifier)
//     {
//         ButtonId = buttonId;
//         Action = action;
//         Modifier = modifier;
//     }
// }

// public class Config{

//     public List<ButtonMapping> Mappings { get; set; }
// }




// class Program 
// {

//     static bool IsRunning { get; set; }

//     private static EvDevDevice huionDevice;
//     private static EvDevDevice keyboardDevice;

//     static void Main(string[] args)
//     {        

//         // Path to the input device
//         string devicePath = "/dev/input/event16";
//         string keyboardDevicePath = "/dev/input/event9";

        
//         if (!HasAccessToDevice(devicePath))
//         {
//             Console.WriteLine("You do not have permission to access the input device.");
//             Console.WriteLine("Would you like to run the program with elevated privileges? (y/n)");

//             string? response = Console.ReadLine()?.ToLower();

//             if (response == "y")
//             {
//                 // Here you would provide the necessary instructions for the user to run with sudo
//                 Console.WriteLine("Please run the program with 'sudo'. For example:");
//                 Console.WriteLine($"  sudo dotnet run");
//             }
//             else
//             {
//                 Console.WriteLine("You will not be able to use the input device without elevated privileges.");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Device is accessible. Proceeding with the program...");
//             // Proceed with your application logic
//         }
//         IsRunning = true;
        

//         var keyboard = GetKeyBoard();        

//         var tablet = GetTablet();
//         // while(IsRunning)
//         // {
            
//         //     InputHandler(tablet,keyboard, LoadConfig("tabletConfig.json"));


//         // }
        
//         //input check for ESC key to exit the program

//         var config = LoadConfig("tabletConfig.json");

    
//         InputHandler(tablet,keyboard, config);
//         Console.WriteLine("Key Remapper running. Press any key to exit...");
//         Console.ReadLine();
//         Console.WriteLine("Exiting the program...");
//         tablet.StopMonitoring();

//     }

//     static void InputHandler(EvDevDevice tablet, EvDevDevice keyboard, Config config)
    
//     {

//         if(tablet == null)
//         {
//             Console.WriteLine("Tablet not found");
//             return;
//         }

        

//         tablet.OnKeyEvent += (sender, e) =>
//         {
//             var buttonId = e.Key.ToString(); // e.g., "KEY_A", "KEY_B", etc.
            

//             // Find the corresponding action from the config
//             var mapping = config.Mappings.FirstOrDefault(m => m.ButtonId == buttonId);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Button {buttonId} should trigger action: {mapping.Action} and modifier: {mapping.Modifier}");

//                 //buttonId is the string name of the key being pressed
//                 //mapping.Action is the string name of the action to be performed 

//                 ExecuteAction(mapping.Action, mapping.Modifier, keyboard, tablet);
//             }
//         };

//         keyboard.OnKeyEvent += (sender, e) =>
//         {
//             var keyID = e.Key.ToString(); // e.g., "KEY_A", "KEY_B", etc.
//             // Find the corresponding action from the config
//             var mapping = config.Mappings.FirstOrDefault(m => m.ButtonId == keyID);
//             if (mapping != null)
//             {
//                 Console.WriteLine($"Button {keyID} should trigger action: {mapping.Action} and modifier: {mapping.Modifier}");
//                 ExecuteAction(mapping.Action, mapping.Modifier, keyboard, tablet);
//             }
//         };

//         // tablet.OnSynEvent += (sender, e) =>
//         // {
            
//         //     Console.WriteLine("Syn event");
//         //     Console.WriteLine(sender.GetType().ToString());
//         //     //Here I need to turn the mapping.Action into a key code and send it to the keyboard

            


//         //     sender.GetType().GetMethod("TriggerCustomKeyEvent").Invoke(sender, new object[] { EvDevKeyCode.KEY_A, EvDevKeyValue.KeyDown });
//         // };

//         // keyboard.OnKeyEvent += (sender, e) =>
//         // {
//         //     var buttonId = e.Key.ToString(); // e.g., "KEY_A", "KEY_B", etc.

//         //     // Find the corresponding action from the config
//         //     var mapping = config.Mappings.FirstOrDefault(m => m.ButtonId == buttonId);
//         //     if (mapping != null)
//         //     {
//         //         Console.WriteLine($"Button {buttonId} triggered action: {mapping.Action}");
//         //         ExecuteAction(mapping.Action, mapping.Modifier);
//         //     }
//         // };


        
//         keyboard.StartMonitoring();
//         tablet.StartMonitoring();
        
//     }

//     static void ExecuteAction(string action, string modifier, EvDevDevice keyboard, EvDevDevice tablet) 
//     {
        
//         if(keyboard == null)
//         {
//             Console.WriteLine("Keyboard not found");
//             return;
//         }
//         keyboard.StartMonitoring();
//         // tablet.OnSynEvent += (sender, e) =>
//         // {
            
//         //     Console.WriteLine("Syn event");
//         //     Console.WriteLine(sender.GetType().ToString());
//         //     //Here I need to turn the mapping.Action into a key code and send it to the keyboard

//         //     if(actionToKeyMap.ContainsKey(action))
//         //     {
//         //         var key = actionToKeyMap[action];
//         //         var modifierKey = actionToKeyMap[modifier];
//         //         keyboard.TriggerCustomKeyEvent(key, EvDevKeyValue.KeyDown);
//         //         keyboard.TriggerCustomKeyEvent(modifierKey, EvDevKeyValue.KeyDown);
//         //         keyboard.TriggerCustomKeyEvent(key, EvDevKeyValue.KeyUp);
//         //         keyboard.TriggerCustomKeyEvent(modifierKey, EvDevKeyValue.KeyUp);
//         //     }
//         //     else
//         //     {
//         //         Console.WriteLine($"Action {action} not found in the key map.");
//         //     }
//         // };

//         // keyboard.TriggerSynEvent();

//         tablet.OnSynEvent += (sender, e) =>
//         {
//             Console.WriteLine("Syn event");
//             Console.WriteLine(sender.GetType().ToString());

//             if (actionToKeyMap.ContainsKey(action))
//             {
//                 var key = actionToKeyMap[action];
//                 var modifierKey = actionToKeyMap.ContainsKey(modifier) ? actionToKeyMap[modifier] : EvDevKeyCode.KEY_RESERVED;

//                 if (modifierKey != EvDevKeyCode.KEY_RESERVED)
//                 {
//                     keyboard.TriggerCustomKeyEvent(modifierKey, EvDevKeyValue.KeyDown);
//                 }

//                 keyboard.TriggerCustomKeyEvent(key, EvDevKeyValue.KeyDown);
//                 keyboard.TriggerSynEvent();
//                 keyboard.TriggerCustomKeyEvent(key, EvDevKeyValue.KeyUp);
                

//                 if (modifierKey != EvDevKeyCode.KEY_RESERVED)
//                 {
//                     keyboard.TriggerCustomKeyEvent(modifierKey, EvDevKeyValue.KeyUp);
//                 }
//             }
//             else
//             {
//                 Console.WriteLine($"Action {action} not found in the key map.");
//             }
//         };
//     keyboard.StartMonitoring();
//     keyboard.TriggerSynEvent();

        
//     }

//     static readonly Dictionary<string, EvDevKeyCode> actionToKeyMap = new Dictionary<string, EvDevKeyCode>(StringComparer.OrdinalIgnoreCase)
//     {
//         { "KEY_LEFTCTRL", EvDevKeyCode.KEY_LEFTCTRL },
//         { "KEY_LEFTALT", EvDevKeyCode.KEY_LEFTALT },
//         { "KEY_LEFTSHIFT", EvDevKeyCode.KEY_LEFTSHIFT },
//         { "KEY_LEFTMETA", EvDevKeyCode.KEY_LEFTMETA },
//         { "KEY_F5", EvDevKeyCode.KEY_F5 },
//         { "KEY_A", EvDevKeyCode.KEY_A },
//         { "KEY_B", EvDevKeyCode.KEY_B },
//         { "KEY_C", EvDevKeyCode.KEY_C },
//         { "KEY_D", EvDevKeyCode.KEY_D },
//         { "KEY_E", EvDevKeyCode.KEY_E },
//         { "KEY_F", EvDevKeyCode.KEY_F },
//         { "KEY_G", EvDevKeyCode.KEY_G },
//         { "KEY_H", EvDevKeyCode.KEY_H },
//         { "KEY_I", EvDevKeyCode.KEY_I },
//         { "KEY_J", EvDevKeyCode.KEY_J },
//         { "KEY_K", EvDevKeyCode.KEY_K },
//         { "KEY_L", EvDevKeyCode.KEY_L },
//         { "KEY_M", EvDevKeyCode.KEY_M },
//         { "KEY_N", EvDevKeyCode.KEY_N },
//         { "KEY_O", EvDevKeyCode.KEY_O },
//         { "KEY_P", EvDevKeyCode.KEY_P },
//         { "KEY_Q", EvDevKeyCode.KEY_Q },
//         { "KEY_S", EvDevKeyCode.KEY_S },
//         { "KEY_T", EvDevKeyCode.KEY_T },
//         { "KEY_U", EvDevKeyCode.KEY_U },
//         { "KEY_V", EvDevKeyCode.KEY_V },
//         { "KEY_W", EvDevKeyCode.KEY_W },
//         { "KEY_X", EvDevKeyCode.KEY_X },
//         { "KEY_Y", EvDevKeyCode.KEY_Y },
//         { "KEY_Z", EvDevKeyCode.KEY_Z },

//         // Add more keys as needed...
//     };
    

//     static EvDevDevice GetTablet()
//     {
//         var tablet = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Tablet Monitor Pad")).FirstOrDefault();

//         if (tablet == null)
//         {
//             Console.WriteLine("Tablet not found");
//         }
//         // else
//         // {
//         //     Console.WriteLine($"Tablet found: {tablet.Name}");
//         // }

//         // foreach(var keys in tablet.Keys)
//         // {
//         //     Console.WriteLine($"Key: {keys}");
//         // }

//         return tablet;
//     }

//     static EvDevDevice GetKeyBoard(){
            
//             var keyboard = EvDevDevice.GetDevices().Where(d => d.Name.Contains("Keyboard")).FirstOrDefault();
    
//             if (keyboard == null)
//             {
//                 Console.WriteLine("Keyboard not found");
//             }
//             // else
//             // {
//             //     //Console.WriteLine($"Keyboard found: {keyboard.Name}");
//             // }

//             // foreach(var keys in keyboard.Keys)
//             // {
//             //     //Console.WriteLine($"Key: {keys}");
//             // }
    
//             return keyboard;
//     }

//     static void GetAllDevices(){
            
//             var devices = EvDevDevice.GetDevices().OrderBy(d => d.Name);
    
//             for(int i = 0; i < devices.Count(); i++)
//             {
    
//                 Console.WriteLine($"Device {i}: {devices.ElementAt(i).Name}");
//             }
//     }

//     static bool HasAccessToDevice(string devicePath)
//     {
//         try
//         {
//             // Try to open the file to check access
//             using (var file = new FileStream(devicePath, FileMode.Open, FileAccess.ReadWrite))
//             {
//                 return true;
//             }
//         }
//         catch (UnauthorizedAccessException)
//         {
//             // If we can't access the device, we return false
//             return false;
//         }
//         catch (FileNotFoundException)
//         {
//             // Device path doesn't exist
//             Console.WriteLine($"Device {devicePath} not found.");
//             return false;
//         }
//     }

//     static Config LoadConfig(string filePath){
    
//         string json = File.ReadAllText(filePath);
//         return JsonConvert.DeserializeObject<Config>(json);
//     }

// }

#endregion

#region Attempt V1

// using System;
// using System.IO;
// using System.Threading.Tasks;

// class Program
// {
//     // Private variables follow .NET standard

//     private readonly string _tabletDevicePath = "/dev/input/event16"; // Update to match your device path
//     private readonly string _keyboardDevicePath = "/dev/input/event9"; // Update to match your device path
//     private readonly int[] _buttonMappings = { 256, 257, 258, 259, 260, 261, 262, 263 }; // Update to match your mappings

//     public async Task StartListenerAsync()
//     {

        
//         Console.WriteLine($"Listening to device: {_tabletDevicePath}");

//         // Open device for reading input events
//         using var fs = new FileStream(_tabletDevicePath, FileMode.Open, FileAccess.Read, FileShare.Read);
//         var buffer = new byte[24]; // Size of input_event structure

//         while (true)
//         {
//             int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
//             if (bytesRead < buffer.Length)
//             {
//                 Console.WriteLine("Incomplete event read. Skipping...");
//                 continue;
//             }

//             // Parse the event data
//             ParseEvent(buffer);
//         }
//     }

//     private void ParseEvent(byte[] buffer)
//     {
//         // Extract event details from the buffer (adjust byte offsets as per input_event struct)
//         long timeSeconds = BitConverter.ToInt64(buffer, 0);
//         long timeMicroseconds = BitConverter.ToInt64(buffer, 8);
//         short type = BitConverter.ToInt16(buffer, 16);
//         short code = BitConverter.ToInt16(buffer, 18);
//         int value = BitConverter.ToInt32(buffer, 20);

//         // Debugging the raw event
//         //Console.WriteLine($"Raw event data: {BitConverter.ToString(buffer)}");
//         Console.WriteLine($"Parsed event - Time: {timeSeconds}.{timeMicroseconds}, Type: {type}, Code: {code}, Value: {value}");

//         // Handle the button press/release
//         if (type == 1) // EV_KEY
//         {
//             HandleKeyEvent(code, value);
//         }
//     }

//     private void HandleKeyEvent(short code, int value)
//     {
//         // Check if the code matches a known button
//         if (Array.Exists(_buttonMappings, b => b == code))
//         {
//             if (value == 1)
//             {
//                 Console.WriteLine($"Button {code} pressed.");
//                 TriggerMappedAction(code);
//             }
//             else if (value == 0)
//             {
//                 Console.WriteLine($"Button {code} released.");
//             }
//         }
//         else
//         {
//             Console.WriteLine($"Unknown button code: {code}");
//         }
//     }

//     private void TriggerMappedAction(short code)
//     {
//         switch (code)
//         {
//             case 256:
//                 Console.WriteLine("Performing action for Ctrl+Z...");
//                 break;
//             case 257:
//                 Console.WriteLine("Performing action for Ctrl+Y...");
//                 break;
//             case 258:
//                 Console.WriteLine("Performing action for Alt+Tab...");
//                 break;
//             // Add cases for other codes
//             default:
//                 Console.WriteLine($"No action mapped for code: {code}");
//                 break;
//         }
//     }

//     public static async Task Main(string[] args)
//     {
//         var handler = new Program();
//         await handler.StartListenerAsync();
//     }
// }
#endregion