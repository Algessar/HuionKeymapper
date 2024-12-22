#!/bin/bash

# Path to your application
APP_PATH="/media/elric/Projects/Huion_InputHandler"

# Command to run your application
APP_COMMAND="dotnet run"

# Open a new terminal window and run the application
gnome-terminal -- bash -c "cd $APP_PATH && $APP_COMMAND; exec bash"