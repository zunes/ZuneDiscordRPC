# ZuneDiscordRPC
Show the world what you are currently listening to in Zune!

![Discord Screenshot](/screenshots/discord.png)

## Features
 - [x] Show Aritist, Album, Title in Discord
 - [x] Show Album Cover in Discord
 - [x] Settings File for customization 

## Settings File (settings.ini)
Click ``Reload Settings`` in the context menu to reload the settings.
Click ``Open Settings File`` in the context menu to open the settings file.

### Example
```ini
showAlbumArt=1
smallImage=modern_zune_logo
largeImage=original_zune_logo
```
### Properties
- ``showAlbumArt``: Set whether the Album Art should be serached for on Deezer and shown in discord. Can bet set to ``0`` to disable or ``1`` to enable. 
- ``smallIcon``: Set the small icon to show. Leave empty to remove the small icon. Can be set to one of the ``Predefined Logos`` or a custom one by providing a *url* to an image.
- ``largeImage``: Set the large icon to show when no cover art is available.  Can be set to one of the ``Predefined Logos`` or a custom one by providing a *url* to an image.


**NOTE:** Discord limits the length of the url, so your custom image might not work if the url is too long.

### Predefined Logos
- ``modern_zune_logo``: Thanks to Eric Mendoza (The Hat-Man)! 
- ``original_zune_logo``


## Changelog
v0.2.1:
  - Added Settings file

---

v0.2:
  - Added Tray Icon
  - Added Album Art in Discord using Deezer API
  
---

v0.1:
  - Initial Release
  - Artist, Album, Title shown in Discord