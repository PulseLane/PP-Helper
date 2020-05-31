# PP Helper
Beat Saber Mod that adds various things related to pp

Requires:
  * BSML
  * BS Utils
  * SongDataCore
  * A scoresaber profile (Should theoretically work without, but will be missing a lot of features)

Optional (Additional features):
  * SongBrowser
  * Counters+

**Please note that all pp values are approximate**

# Overview
When you first launch this mod, it will attempt to download your profile data from scoresaber. It then saves this information, so it will only refetch this data if you manually choose to do so (by pressing the in-game button under the mods tab). As you complete songs this data will be automatically updated, so there should be no need to refresh your data unless things get out of sync.

There are three main features of PP Helper:
  * PP info on song view info panel
  * Sorting by "possible" pp and pp gain (Requires SongBrowser)
  * Live PP counter that shows how much PP your current accuracy is worth. This counter accepts all modifiers (even if the map doesn't!), but may optionally ignore the NoFail modifier

The PP info is composed of 5 parts

![PP Info](/PP%20Helper/Assets/info.png)

* (1) The pp a pass with the accuracy in (3) is worth on this map.

* (2) How much pp you will earn from passing with the accuracy in (3)

* (3) The currently set accuracy

* (4) A save button, to save the current accuracy in (3) for auto-loading in the future

* (5) A load button, to load the star accuracy into (3) and delete any saved accuracies

This info live updates as you enable/disable modifiers (meaning any song that is not the ranked Overkills will show 0pp if any positive modifiers are selected) and adjust the accuracy.

The initially accuracy loaded in on selecting a song is determined as follows:

First, it looks for a saved accuracy, and sets it if one is found. If there was no saved accuracy, it then loads in the better of your highscore and the star accuracy. Lastly, if neither of those are found (or are 0), it loads in the default accuracy you have set in the settings.

# Star Accuracy

Star accuracy uses the given calculation method from the settings to compute a default accuracy for maps depending on their star rating. The "Star Acc. Range" option in the settings controls this width, where e.g. a value of 0.5 means that accuracy is computed for 0-0.5, 0.5-1.0, ... 7.5-8.0, etc.

There is an option in the settings to disable the default behavior of overriding lower star accuracies with a better higher one. For example, if your average accuracy on 7-7.5* is 82%, but is 85% on 8-8.5*. then 7-7.5* maps will instead default to 85% if using the default behavior.

The three calculation methods are:

* Average of Top N

  * This method takes the top N accuracies in the given star range, where N can be defined in the settings, and averages your accuracy to determine the star accuracy

* Average of All

  * This method averages your accuracy across all finished maps in the given star range to determine the star accuracy

* Max

  * This method takes your highest accuracy in the given star range and uses that to determine the star accuracy

# UserData
There are 3 json files used by this mod, located in UserData\PP Helper. There is also a "PP Helper.ini" file in \UserData\ that stores your settings

* AverageStarAcc.json

  * This file is automatically formatted, and is set up with the star range as defined above. Feel free to edit this file to customize your star accuracies manually, though be aware that things may break if you don't follow the format correctly. I may add an in-game way to edit this in the future.

* ProfileData.json

  * This file stores your scoresaber profile data (and updates automatically as you complete songs with the mod installed!). You should never have a reason to edit this file. If your profile is out-of-sync, use the in-game button under the "Mods" tab to refresh your data.

* SongSpecificAcc.json

  * This file stores your song-specific accuracy overrides. All editing can be done in-game so there should be no reason to edit this file.
