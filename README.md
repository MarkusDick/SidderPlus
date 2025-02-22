# SidderPlus

SidderPlus is a fork of [Sidder](https://github.com/msfreaks/Sidder) from [msfreaks](https://github.com/msfreaks/). The tool now runs with the .net4.8 framework.

Sidder allows to quickly identify which User Profile Disk belongs to which Domain User. 

This fork also allows to close an open smb file / close an locked UPD (User Profile Disk) and to compact the UPDs.

## Why SidderPlus
Sometimes we have the problem that users do not get their own user profile disk assigned. Therefore, you must search for the locked vhdx file name in Sidder and then go to Computer Management and close the open smb file. This fork adds this feature directly to Sidder.

Since I want the project to be found on GitHub and GitHub search usually excludes forks I renamed the project. ~The original project Sidder is [no longer maintained](https://msfreaks.wordpress.com/2020/02/17/sidder-v2-6-open-sourced-and-more/).~ 

Update: It seems that the project [has been revived](https://github.com/msfreaks/Sidder/issues/3).

### Difference between Sidder and SidderPlus
| Features                                                     | Sidder | SidderPlus |
| ------------------------------------------------------------ | ------ | ---------- |
| Identifiy which UPD belongs to which Domain User             | ✅      | ✅          |
| Delete UPD                                                   | ✅      | ✅          |
| Close locked UPD                                             | ❌      | ✅          |
| Compact UPD / VHDX                                           | ❌      | ✅          |
| CSV export                                                   | ✅      | ✅          |
| Copy UPD-Name to clipboard (Ctrl + C or right-click => Copy) | ❌      | ✅          |
| Basic user search                                            | ✅      | ✅          |
| Release is available for download                            | ✅      | ✅          |
| .net Framework Version                                       | 4.8    | 4.8        |

> Update: After Sidder has been revived, it caught up with some features (Basic user search, CSV export).

For a complete comparison (Sidder v2.6 vs SidderPlus) look  [here](https://github.com/MarkusDick/SidderPlus/compare/4e749789f3094bc908d8c66b7d39447e6642be1d...main).

## Disclaimer
I am not a C# developer. I just "hacked" this version together. Before this project I had no idea how a C# project is organized. Since the state of the project serves my purposes, there is no further roadmap. If you want changes, feel free to open an issue or PR. I will definitely take a look at it.

## Download
You can download the latest version [here from the releases](https://github.com/MarkusDick/SidderPlus/releases/).

## Screenshot
### Overview
![](sidder_plus_screenshot.png)

### Close
![](sidder_plus_close_screenshot.png)

### Compact UPD / VHDX Files
![](sidder_plus_compact_screenshot.png)
![](sidder_plus_compact_results_screenshot.png)

## Usage
You need to start SidderPlus as an administrator. Then you can select an item and press the button in the middle to close the open SMB file / the locked UPD.

![](how_to_close.png)

To compact a UPD / VHDX / UVHD it is the same procedure. You have to select an or multiple items and click on the left button.

![](how_to_compact.png)

That's it.

## Thanks
Thanks to [msfreaks](https://github.com/msfreaks/) for creating and open sourcing this tool.

## License
### Sidder:
You are free to use the code to build your own Sidder or modify the existing Sidder to your liking, as long as the About box remains intact.

### SidderPlus:
My changes are under the MIT-License.
