Huion Kamvas 13 drivers are broken on Linux Mint. Screen Workspace gets locked to main computer screen and cannot be changed. While this app has no workspace selection, the default Mint drivers correctly identifies the tablet screen, so I focused on keymapping functionality.


Notes:
- Currently, pen buttons are limited to be mapped to mouse buttons.
- Simultaneous input from tablet and pen does not work. Which means that ctrl-middle mouse zoom, if mapped on tablet and pen, does not work as intended. (A function exists at the bottom of the Program class where I will attempt to adress this. Eventually.)


# Huion Keymapper

A small utility to remap Huion tablet and pen buttons on Linux.  
It uses [EvDevSharp](https://github.com/) to listen for events and xdotool for simulating key presses.

## Features
- Custom key or mouse click mappings for tablet buttons.
- Separate mappings for pen buttons.
- Simple JSON-based configuration.
- Basic GUI built with Gtk# for quick edits.

## Installation
1. Clone this repository.  
2. Install [xdotool](https://github.com/jordansissel/xdotool) and Gtk dependencies.  
3. Build the solution with your preferred .NET build tools.

## Usage
1. Edit `deviceConfig.json` to define your button mappings.  
2. Run the program.  
3. The GUI lets you modify mappings on the fly.  

## Contributing
1. Fork the repository.  
2. Submit pull requests focusing on bug fixes or feature improvements.  

## License
MIT License.  