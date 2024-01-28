# [Fork - HIGHLY OPTIMIZED] HDLethalCompany V1.5.6 - Sligili | CLIENT SIDE 🎈

## INSTALLATION 🛠
### Extract in BepinEx plugins folder (where ```Lethal Company.exe``` is located).

## CONFIGURATION (```BepInEx/config/HDLethalCompany.cfg```) ⚙
📃*This mod maintains the vanilla rendering aspect ratio ***to avoid breaking any HUD elements***.
Calculate the value equivalent to your desired resolution -> ```DesiredResolutionWidth / 860```*

- **RESOLUTION FIX:** Set to ```true``` or ```false``` - if false, disables my custom resolution method so you can use another resolution mod with a different solution like [this one](https://www.nexusmods.com/lethalcompany/mods/8) or use any widescreen fix

- **RESOLUTION SCALE:** Set to any number between ```1``` (*or lower*) - ```4.5```
  - ***0.923*** -> ***800x480*** (*Increases performance*)
  - ***1.000*** -> ***860x520*** (*Vanilla resolution*)
  - ***1.488*** -> ***1280x720***
  - ***2.233*** -> ***1920x1080*** (*Default mod resolution*)
  - ***2.977*** -> ***2560x1440***
  - ***4.465*** -> ***3840x2060***

- **TEXTURE QUALITY:** Set to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies texture resolution
  - ***0*** -> ***VERY LOW*** (*Eighth resolution*)
  - ***1*** -> ***LOW*** (*Quarter resolution*)
  - ***2*** -> ***MEDIUM*** (*Half resolution*)
  - ***3*** -> ***HIGH*** (*Full resolution - Vanilla and default mod value*)

- **VOLUMETRIC FOG QUALITY:** Set to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies the volumetric fog budget
  - ***0*** -> ***VERY LOW*** (*Lower than the Vanilla preset, increases performance*)
  - ***1*** -> ***LOW*** (*Vanilla preset and the default mod value*)
  - ***2*** -> ***MEDIUM***
  - ***3*** -> ***HIGH*** (*Can significantly impact performance at high resolutions*)

- **LEVEL OF DETAIL:** Set to any of the following values ```0``` ```1``` ```2``` - modifies the distance at which models decrease their poly count/disappear
  - ***0*** -> ***LOW*** (*half distance*)
  - ***1*** -> ***MEDIUM*** (*Vanilla distance - Default mod value*)
  - ***2*** -> ***HIGH*** (*Twice the distance*)

- **SHADOW QUALITY:** Set to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies the maximum resolution they can reach
  - ***0*** -> ***VERY LOW*** (*disables shadows*)
  - ***1*** -> ***LOW*** (*256 max resolution*)
  - ***2*** -> ***MEDIUM*** (*1024 max resolution*)
  - ***3*** -> ***HIGH*** (*2048 max resolution - Vanilla and default mod value*)

- **POST-PROCESSING:** Set to ```true``` or ```false``` - if false, disables the HDRP Custom Pass, therefore disabling color grading

- **VOLUMETRIC FOG:** Set to ```true``` or ```false``` - if false, disables the HDRP Volumetric Fog. **Use as a last resort if lowering the fog quality is not enough to get decent performance**

- **ANTI-ALIASING:** Set to ```true``` or ```false``` - if true, enables built-in FXAA

- **FOLIAGE TOGGLE:** Set to ```true``` or ```false``` - if false, disables the "Foliage" layer (trees won't be affected, only most bushes and grass)

## CHANGELOG 🕗

- ***Fork:***
     - Highly optimized performance when scanning
     - Fix incorrect offsets when scanning
     - Organize project hierarchy
     - Asset bundle -> Embedded resource (simplify file hierarchy)