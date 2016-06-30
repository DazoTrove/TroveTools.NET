<div id="main_content_wrap" class="outer"><section id="main_content" class="inner">

## Version 1.0.0.6 Beta (TBD)
- Clicking on the tray icon balloon notification now restores the window in addition to clicking or double-clicking on the tray icon itself
- Trovesaurus tab: sort event calendar by end date, and moved refresh button
- Trovesaurus tab: refresh button now also refreshes mail and server status immediately
- Updated Trovesaurus mail URL
- Mod Detail Pane: updated download date display to show full date and time
- Get More Mods: added log message to display refresh timer (refreshes are allowed only every 30 seconds)
- All tabs with toolbars: hide overflow button (window size has a minimum so overflow is never needed)
- Get More Mods: updated default sort to Likes

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