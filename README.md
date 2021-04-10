# RXHDDT
An osu replay editor to modify the mods from the command line. Only supports std mode mods.

Thanks to the osu!ftw replay editor for providing code to read and write replays.

# How it works

1. Start the program and select the replay file you want to edit.

2. Change the mods by pressing the corresponding keys on your keyboard.

3. Press enter and select a path to save the edited replay.

NOTE: The edited replay has a random score displayed on the result screen. This is necessary because osu! caches replays.
If you have two exact same scores (with or without the same mods) imported, both replays will show the same cursor movement.
If you enabled or disabled hardrock this would mean that your original replay, which you or others may have imported,
will be shown and because hardrock changes the location of the hit objects the replay would be messed up.

# Bug reports

If you found a bug, please create an issue here on GitHub.
