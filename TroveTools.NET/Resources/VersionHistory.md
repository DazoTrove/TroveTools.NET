<div id="main_content_wrap" class="outer"><section id="main_content" class="inner">
  
If you are a new player lower than Mastery Rank 20, you can earn a free class coin by **[clicking this link: http://bit.ly/DazoFriend](http://bit.ly/DazoFriend)**, and signing in with your Glyph account to apply my Refer a Friend code
  
### [Check out Dazo's Trove Resource Compilation Spreadsheet: http://bit.ly/DazoTrove](http://bit.ly/DazoTrove)
- Links to other guides, tools, and Trove resources
- **Forging &amp; Deconstruction Costs / Profit Calculator**
- **Item Rarity Levels**
- **Dragons:** contains a list of all dragons, current crafting costs, total mastery granted, and stat buffs
- **Adventure &amp; Shadow Tower:** includes required power ranks, gear drops, level ranges, and rewards for all the various types of worlds and realms
- **Shadowy Market:** a list of items that can be purchased from the Shadow Tower market with number of mastery points granted for each
- **Pirrot Merchants:** a list of items that can be purchased from the pirate merchants in Treasure Isles including identifying features of the ships
- **Mastery:** a list of various things that grant mastery points

## Version 1.1.5 Release (TBD)
- Style Update
  - Trovesaurus tab: aligned caledar event images

## Version 1.1.4 Release (7/15/2016)
- Trovesaurus tab: added images to Calendar events
- Trovesaurus tab: updated formatting of server status text
- Background tasks (updating trove game status, auto updates, checking Trovesaurus mail, and server status updates) now startup upon launching the application. Previously when the start minimized option was enabled these tasks did not start until the window was restored and fully loaded

## Version 1.1.3 Release (7/13/2016)
- Trovesaurus tab: added Live server status in addition to the Live launcher status - thanks to Etaew for adding this info to the API that is used and for all his hard work getting Trovesaurus restored.
More detail on Trove server downtime whether planned or unexpected can always be found by clicking on the [server status link](https://www.trovesaurus.com/status.php) to view the Trovesaurus server status page.

## Version 1.1.2 Release (7/12/2016)
- Removed beta version tag from title bar and About tab
- Get More Mods: a cached copy of the Trovesaurus mod list is now saved
- Get More Mods: updated default sorting to sort by total downloads after likes
- Save window size, position, and state to user settings
- Restore window size, position, and state on startup
- Save mod details grid splitter position to user settings
- Restore mod details grid splitter position on startup
- About tab: added some Trove resources also created by Dazo

## Version 1.1.1 Release (7/11/2016)
- Trovesaurus tab: server status link is now always shown, and only the server details are hidden if no data is returned and instead a message stating that server status is not available is displayed
- Settings: new Check Server Status every minute option
- My Mods: fixed installation of mods from zip files while Trovesaurus is temporarily offline
- Improved error handling for Trovesaurus API calls

## Version 1.1.0 Release (7/10/2016)
- My Mods: new mod pack management features
  - Loading mod packs from Trovesaurus and local mod packs. The dropdown populates automatically and includes a link to the Trovesaurus mod packs page where you can compile your own packs to share with others
  - Removing local mod packs
  - Saving all enabled standalone mods as a local mod pack
  - Copying Trove:// URI protocol links for Trovesaurus and local mod packs. New URI format is **trove://modpack=PackId** for Trovesaurus mod packs, and **trove://Pack+Name?ModId1&amp;ModId2&amp;...&amp;ModIdN** for local mod packs
  - Mod listing is now grouped by mod pack and includes an option to quickly enable or disable all the mods in a mod pack
- Trovesaurus API requests now include details to identify that requests came from the TroveTools.NET mod loader to help Etaew track usage metrics
- Trovesaurus server status is now hidden if no data was returned from the Trovesaurus API
- My Mods: added delete key shortcut to remove the selected mod
- Bug Fix
  - Trovesaurus server status web calls are now limited to once every 30 seconds, and should no longer cause a stack overflow exception crash

## Version 1.0.0.8 Beta (7/5/2016)
- About: added [link to forum post](http://forums.trovegame.com/showthread.php?103168-Mod-Loader-TroveTools.NET-created-by-Dazo-(for-Windows-PCs)) for feedback
- Mod Detail Pane: hide many fields when they contain no data for custom mods installed from zip files
- Trove:// URI schema links now work when the program is already running (the new instance passes the data in the background to the already running instance and closes)
- Bug Fix
  - My Mods: fixed exception when loading custom mods installed from zip files

## Version 1.0.0.7 Beta (7/4/2016)
- My Mods: check for valid mod zip file format before installing. The folder(s) where the zip file will be extracted must contain an index.tfi file before an override folder is automatically created
- My Mods: disable updates per mod with right click context menu option
- My Mods and Get More Mods: copy Trove:// URI schema links with right click context menu command
- Style Update
  - My Mods: changed status to use text trimming with an elipsis and a tooltip for long error messages
- Bug Fixes
  - Trovesaurus tab: fixed server status not displaying date and time properly when offline
  - My Mods: the current version of a mod is uninstalled before updating to a new version of a mod to properly remove files that were in an old version but not included in the latest version
  - Get More Mods: updated right click context menu for downloads to remove extra spacing

## Version 1.0.0.6 Beta (6/30/2016)
- Clicking on the tray icon balloon notification now restores the window in addition to clicking or double-clicking on the tray icon itself
- Trovesaurus tab: sort event calendar by end date, and moved refresh button
- Trovesaurus tab: refresh button now also refreshes mail and server status immediately
- Updated Trovesaurus mail URL
- Settings: added links to open Trove game folders
- Mod Detail Pane: updated download date display to show full date and time
- Get More Mods: added log message to display refresh timer (refreshes are allowed only every 30 seconds)
- Get More Mods: updated default sort to Likes
- Design/Style Updates
  - All tabs with toolbars: hide overflow button (window size has a minimum so overflow is never needed)
  - Mod Detail Pane: improved spacing between text fields
  - All HTML data fields (news, mod descriptions, replaces, download changes, etc.) now use a simple stylesheet to reduce large spaces from paragraph and other tags
- Bug Fixes and error prevention
  - Updated all binding value converters to include error logging
  - Mod Detail Pane: fixed date/time conversion issue for manually loaded mods
  - Mod Detail Pane: downloads table is hidden for manually loaded mods that have no downloads
  - My Mods: fixed update button to only display when a valid Trovesaurus ID and downloads are found for the mod

## Version 1.0.0.5 Beta (6/29/2016)
- Trovesaurus tab with Trove news, online streams, mail message count, server status, and events/contests calendar
- Trovesaurus tab: added full date values as tooltips on friendly values and URLs as tooltips on links
- Settings: removed auto update every 1 minute option to prevent too many requests to Trovesaurus API (every 5 minutes is sufficient for auto updates)
- Settings: added check Trovesaurus mail every minute option
- My Mods: added mod path details
- My Mods: open mods folder button
- Added logging to text file in addition to the log messages shown within the application
- Mod Detail Pane: new downloads table with HTML changes (thanks to Etaew for updating the API to include the download changes data)
- Hide mod detail pane contents when no item is selected in list
- Style Updates
  - Increased margins around main tab control
  - Increased size of tab controls
  - Added padding to the mod detail frame
  - Added drag handle style to mod detail pane divider so that it is more noticeable

## Version 1.0.0.4 Beta (6/26/2016)
- Settings: auto update mods now refreshes the Trovesaurus API mod list on every update (rather than only the first)
- Settings: new start minimized option
- About: updated format of version history to be Markdown to match GitHub release details
- My Mods and Get More Mods: mod detail pane for selected mod - shows HTML description and replaces text including images and links
- Get More Mods: Added Likes column (thanks to Etaew for updating the API to include this data)

## Version 1.0.0.3 Beta (6/23/2016)
- Settings: run on windows startup option
- Settings: minimize to tray option
- Settings: auto update mods on application startup and at a configurable interval
- Settings: updates to Trove:// URI Schema Handler to automatically re-register after ClickOnce application updates
- Settings: changed Trove:// URI Schema Handler to be handled per user rather than all users since the executable is located in an individual user's application data folder
- About: Updated Version History to use HTML formatted text

## Version 1.0.0.2 Beta (6/19/2016)
- Double-click on a list view item on the Get More Mods tab to install the latest version
- About tab with version history
- Get More Mods: removed replaces display in Type/SubType field, plan to implement lower detail pane bound to selected item to display description and other details later
- Settings: Trovesaurus account link key
- Settings: Trove game status tracking to Trovesaurus

## Version 1.0.0.1 Beta (6/14/2016)
- Initial public release
- Settings: manage Trove installation locations
- Settings: Trove:// URI Schema Handler for mod installation links on Trovesaurus.com
- My Mods: add mods from local zip files, remove installed mods, check for updates on Trovesaurus, update all with one click, uninstall all mods, and enable/disable individual mods
- My Mods: customized list view display with mod images (and hover tooltip to see larger image), names, authors, links to mod on Trovesaurus, current status, update button, and last updated date
- Get More Mods: search Trovesaurus mod listing (auto-filters as you type), mod types dropdown, mod subtypes dropdown, and refresh button to re-download list from Trovesaurus
- Get More Mods: customized list view display with mod images (and hover tooltip to see larger image), names, authors, link to mod on Trovesaurus, install button, mod types &amp; subtypes, Trovesaurus status, and last updated date
- Get More Mods: right click to download alternate or previous versions of mods
- Logging: most commands have logging that is displayed in a status bar at the bottom of the window. This can be clicked to bring up a detailed log of operations completed
- Application is built with C# in WPF (Windows Presentation Foundation) using the MVVM (Model, View, View Model) design pattern
- Published application using ClickOnce to allow for easy automatic updates in the future
- Published to GitHub with full application source code
- Thanks to Etaew at [Trovesaurus.com](http://www.trovesaurus.com/) for providing the JSON web service API for listing mods, hosting files to download, and for providing the Trove pack reward for creating a mod loader application!
- Thanks to Dusty_Mustard for [organizing the creation of mod loaders for the community](http://forums.trovegame.com/showthread.php?99562-A-request-from-the-community-to-the-community-Build-a-new-Mod-Loader!) and providing information about the mod formats!
- Thanks to Milambit for providing the reward of 101 Megaflux Tanks in Trove!

### TroveTools.NET allows easy installation of Trove mods by searching and downloading from the Trovesaurus mod website.
#### Copyright (C) 2016 David Trousdale (aka Dazo: [dazo@wizdave.com](mailto:dazo@wizdave.com))
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the [GNU General Public License](https://raw.githubusercontent.com/DazoTrove/TroveTools.NET/master/LICENSE)
along with this program. If not, see [http://www.gnu.org/licenses/](http://www.gnu.org/licenses/).

</section></div>