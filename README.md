# opennova

Godot reimplementation of the Novalogic engine.

This is a heavy work in progress and should not be used on assets you care about. You need to own
the Novalogic game you wish to import.

[Screenshot](https://snaps.screensnapr.io/19d0573f670cf5c749db88daa6bc1d)

Small demo of editing a mission and playing it in the original game. (Sorry recording got a little messed up)

https://github.com/user-attachments/assets/4d34ac79-4adb-47f5-a9ae-878d74826b64


## Using

- Download the repo as a zip
- Open Godot and import the project
- It will throw an error about not being able to load the plugin. Click ok.
- Click the build icon (Wrench, top right)
- Project -> Reload Project

Once the above is completed you can "import" a Novalogic game. Only Joint Operations
is supported.

OpenNova will handle importing everything it can. It will decrypt, decompress and extract the resources
it needs from the .pff files.

- Find the novalogic importer tab
- Browse to your game directory. It has only been tested on stock installs
- Click import
- Wait a few
Now it's going to import all the resources. Once it finishes reload the project. Then reload it again.

I don't know why the project has to be
reloaded. It seems some of the custom resource plugins get randomly unloaded

At this point you should be able to open the `MissionScenes/00TRg.tscn`. 

## Features

It can't really do much yet. You can technically edit missions (and still be usable in the original games), but anything more than rotatition or translation of entities is pretty awkward.

## Goals

The original goal was just to understand the various file formats, mostly for fun. It slowly got out of control from there. One of the more obtainable goals would be a MED (Novalogic's original mission editor) replacement. The groundwork is mostly there for that.
The dream would be a full implementation of the engine.

## OpenNova 3DS Max Tools

- [.bad exporter](https://github.com/taylorfinnell/onbadexporter/)
- [.3di importer](https://github.com/taylorfinnell/on3diimporter)

## NovaLogic File Formats

Visit the [wiki](https://github.com/taylorfinnell/opennova/wiki)
