# opennova

Godot reimplementation of the Novalogic engine.


This is a heavy work and progress and should not be used on assets you care about.

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

## OpenNova Tools

- [.bad exporter](https://github.com/taylorfinnell/onbadexporter/)

## NovaLogic File Formats

Visit the [wiki](https://github.com/taylorfinnell/opennova/wiki)
