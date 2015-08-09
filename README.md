## Package Installer

A Visual Studio extension that makes it easy and fast to install
Bower and npm packages.

[![Build status](https://ci.appveyor.com/api/projects/status/bd4o6iumw9vwf8kh?svg=true)](https://ci.appveyor.com/project/madskristensen/packageinstaller)

Download the extension at the
[VS Gallery](https://visualstudiogallery.msdn.microsoft.com/753b9720-1638-4f9a-ad8d-2c45a410fd74)
or get the
[nightly build](http://vsixgallery.com/extension/fdd64809-376e-4542-92ce-808a8df06bcc/)

### Install a package

Simply right-click the project and select "Quick Install Package..."
to pop open the installer dialog box.

![Context menu](art/context-menu.png)

### Auto completion

You get full auto completion for all package names available
in the Bower registry.

![auto completion](art/dialog.png)

Also for version numbers for both Bower and npm:

![auto completion](art/dialog-versions.png)

### bower.json / package.json

You can install packages without having set up Bower or npm.

This extension will automatically create the JSON configuration
files, so you don't have to worry about it.

### Keyboard shortcut

The fastest way to display the dialog is to use the keyboard
shortcut `Shift+Alt+0`.