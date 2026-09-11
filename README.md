# ExpressionLink

Blendshape-based eye movement and facial-expression links for **Koikatsu /
Koikatsu Party (KK)** and **Koikatsu Sunshine (KKS)**.

## Features

- Custom eye movement and blinking.
- Expression links for head, hair, body, clothes, accessories, and other
  character-owned meshes.
- Per-character expression links saved with character cards and Studio characters.
- Custom highlight visibility linked to the game's **Erase Highlight** setting.
- Optional IrisY and Size controls through ExpressionControl.
- Quick Settings in Maker, Studio, and the main game.

## Requirements

Both variants require **BepInEx 5** and the dependencies for the selected game:

| Game | Plugin DLL | Required plugins |
| --- | --- | --- |
| Koikatsu / Koikatsu Party | `KK_EyeMotion.dll` | KKAPI 1.42.2+, ExtensibleSaveFormat |
| Koikatsu Sunshine | `KKS_EyeMotion.dll` | KKSAPI 1.42.2+, KKS_ExtensibleSaveFormat |

Use a character or headmod with the blendshapes needed by the selected features.
Expression Links need only their destination shape; they do not require the
eye-movement shapes.

The matching **KK_ExpressionControl** or **KKS_ExpressionControl** is optional
and supplies only the IrisY and Size inputs. Other features work without it.

## Installation

Download the matching archive from [Releases](../../releases):

- `KK_ExpressionLink_v0.5.3.zip`: Koikatsu / Koikatsu Party.
- `KKS_ExpressionLink_v0.5.3.zip`: Koikatsu Sunshine.

1. Close the game and CharaStudio.
2. Remove previous copies of the plugin DLL from `BepInEx/plugins` and its
   subfolders. This includes the original `KK_EyeMotion.dll` when upgrading
   from EyeMotion. Keep your character cards and configuration files.
3. Extract the archive's `BepInEx` folder into the matching game folder.
4. Keep only one copy of that game's DLL:

   ```text
   KK:  BepInEx/plugins/KK_EyeMotion/KK_EyeMotion.dll
   KKS: BepInEx/plugins/KKS_EyeMotion/KKS_EyeMotion.dll
   ```

Do not install both variants in the same game or keep DLL backups in its
plugins folder. PDB files and dependency DLLs are not included in the downloads.

## Quick Settings

Press **Ctrl + Shift + M** to open or close the panel. It is also available
from **Face > Expression Link** in Maker and the plugin's left-toolbar button
in Studio. Drag the title bar to move the panel.

Select a character with the arrows at the top, then choose:

| Page | Purpose |
| --- | --- |
| **Eyes** | Tracking, neutral pose, movement range, and optional size controls. |
| **Expressions** | Link a facial expression to a blendshape on the selected character. |
| **Visibility** | Highlight synchronization and manual eye-part visibility. |

Eye and visibility configuration uses **Apply global settings**.
**Discard global changes** reloads those settings. Manual visibility buttons
take effect immediately. Expression Links use **Save expression link** and
belong to the selected character.

### Eye tracking

1. Make the character look straight ahead and select **Set neutral pose**.
2. Move the look target fully left and select **Set horizontal range**.
3. Move it fully right and select **Set horizontal range** again.
4. Use **Standard preset**, **Sensitive preset**, or **Fine tuning** to adjust
   the response. A lower input range gives higher sensitivity.
5. Select **Apply global settings**.

The plugin follows the game's eye state; it does not create its own look-at
target or move eye bones.

### Link an expression to a shape

For example, to show heart-shaped eyes when the character's eye expression is
Happy:

1. Select the character and open **Expressions > Create link**.
2. Set the character's eye expression to Happy using the game's controls.
3. Select **Use current eyes**. For other sources, use **Use current brows**
   or **Use current mouth**.
4. Choose the character area and enter the destination blendshape's exact name.
5. Choose **Switch fully** or **Follow expression**, then set the maximum strength.
6. Select **Save expression link**, then save the character card or Studio scene.

For ears on a hair mesh, choose **Hair** as the destination area. Set the
desired facial expression before capturing it, then enter the ear blendshape's
name. The source expression and destination mesh do not need to be on the
same object.

**Advanced targeting and tuning** provides exact renderer paths, slot filters,
transition speed, and priority. Use it when several meshes contain the same
shape or when a link needs a custom response.

### Reusable link sets

Save and load link sets from the **Reusable link sets** panel. Loading a set
copies its links to the selected character; it does not modify other characters.

Sets are JSON files under `BepInEx/config/KK_ExpressionLink/Profiles`.
A set must declare the current game as supported. Shared file formats do not
convert meshes, expression IDs, or cards between games.

## Blendshape names

Names must match the runtime mesh exactly, including any final `_0` suffix.
Default names can be configured in Quick Settings or BepInEx Configuration Manager.

### Eye movement and blink

These five shapes belong on the same `SkinnedMeshRenderer`:

| Function | Default name |
| --- | --- |
| Positive X | `eye_motion.f00_eye_posx` |
| Negative X | `eye_motion.f00_eye_negx` |
| Positive Y | `eye_motion.f00_eye_posy` |
| Negative Y | `eye_motion.f00_eye_negy` |
| Blink | `eye_motion.f00_eye_blink` |

Blink weight 100 represents both eyes fully closed. Separate wink channels
are not supported.

### Optional size controls

These shapes share the eye-movement renderer and use ExpressionControl inputs:

| Input | Default name | Source range | Shape at maximum weight |
| --- | --- | --- | --- |
| IrisY | `eye_motion.f00_iris_y` | 0–0.5 | Authored IrisY adjustment |
| Size | `eye_motion.f00_iris_size` | 0–1 | Smaller iris |

Zero input produces zero weight. These shapes are independent of gaze;
a missing optional shape does not disable the other channels.

### Optional visibility controls

| Part | Default name |
| --- | --- |
| Highlight 01 | `eye_motion.f00_hide_highlight01` |
| Highlight 02 | `eye_motion.f00_hide_highlight02` |
| Inner iris | `eye_motion.f00_hide_irisinner` |
| Outer iris | `eye_motion.f00_hide_irisouter` |
| Pupil | `eye_motion.f00_hide_pupil` |
| Sclera | `eye_motion.f00_hide_sclera` |
| Normal eyes | `eye_motion.f00_hide_normaleyes` |

**Original** releases manual control, **Visible** sets the Hide shape to zero,
and **Hidden** applies its configured hide weight.

With **Hide custom highlights with Erase Highlight** enabled, **Erase Highlight**
also hides Highlight 01 and 02. When highlights return, each slot resumes its
previous manual mode.

Other expression shapes have no required naming convention. Configure them
as per-character Expression Link destinations.

## Saved data and compatibility

- Expression Links and manual visibility modes are saved per character through
  ExtendedSave when `CardPersistenceEnabled` is enabled.
- Global calibration, default shape names, and feature switches use
  `BepInEx/config/com.nightowlzzz.koikatsu.eyemotion.cfg`.
- Runtime expression weights and the temporary Erase Highlight override are
  not saved.
- KK supports `Koikatu.exe`, `Koikatsu Party.exe`, and `CharaStudio.exe`.
  KKS supports `KoikatsuSunshine.exe` and its `CharaStudio.exe`. VR is not supported.
- Cards, coordinates, scenes, and meshes are not converted between games.
- Expression sources come from the game's brow, eye, or mouth expression
  patterns, not arbitrary unrelated blendshapes.
- Missing or ambiguous destinations remain inactive. Select an exact renderer
  in advanced settings if needed. If a target appears after initial loading,
  reload the character or save the link again.

## Troubleshooting

Open **Help > Create debug report** or press **Shift + F10**. Reports are
written to `BepInEx/config/KK_ExpressionLink/diagnostics` and include source
values, target paths, weights, and link status.
BepInEx loading errors are recorded in `BepInEx/LogOutput.log`.

## Development

<details>
<summary>Build, packaging, and maintenance contracts</summary>

### Building

Use Visual Studio Build Tools with MSBuild 17+ and a C# 7.3-capable compiler.
KK targets .NET Framework 3.5; KKS targets .NET Framework 4.6.

Place references from the matching game installation in these ignored folders:

| Folder | References |
| --- | --- |
| `lib/` | `mscorlib.dll`, `System.dll`, `System.Core.dll`, `BepInEx.dll`, `KKAPI.dll`, `ExtensibleSaveFormat.dll`, `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.UI.dll` |
| `lib/KKS/` | `mscorlib.dll`, `System.dll`, `System.Core.dll`, `BepInEx.dll`, `KKSAPI.dll`, `KKS_ExtensibleSaveFormat.dll`, `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.UI.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `UnityEngine.InputLegacyModule.dll`, `UnityEngine.JSONSerializeModule.dll`, `UnityEngine.TextRenderingModule.dll`, `UnityEngine.UIModule.dll` |

System and Unity assemblies come from the game's Managed directory, BepInEx
from its core directory, and the API/ExtendedSave from its plugins directory.
Do not mix game references or redistribute them. References use `Private=False`.

From a Visual Studio Developer Command Prompt or Developer PowerShell:

```shell
msbuild ExpressionLink.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /m /v:minimal
msbuild tests/KK_EyeMotion.Tests.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
.\tests\bin\Release\KK_EyeMotion.Tests.exe
msbuild tests/KKS_EyeMotion.Tests.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
.\tests\bin\KKS\Release\KKS_EyeMotion.Tests.exe
msbuild ExpressionLink.proj /t:PackageBoth
```

Individual targets are `BuildKK`, `BuildKKS`, `PackageKK`, and `PackageKKS`.
Builds also accept `/p:Configuration=Debug`; packaging requires Release.

Outputs are `bin/Release/KK_EyeMotion.dll` and
`bin/KKS/Release/KKS_EyeMotion.dll`. Packages go to
`dist/<Game>_ExpressionLink_v<version>.zip`, with SHA-256 sidecars.
Each archive contains only its plugin DLL and English `README.txt`.
Packaging does not install or publish the plugin.

### Shared code and runtime ownership

- `ExpressionLink.Shared.projitems` lists the shared source;
  `ExpressionLink.Build.props` owns compiler settings and common references.
  Each game project owns its framework, additional references, and game constant.
- `GameCompatibility` owns executable names and profile-game IDs.
  Controllers, UI, sampling, and serialization are shared.
- Renderer catalogs and destinations are resolved during bind/rebuild, not by
  traversing the hierarchy each frame. Configured expression sources share one
  FBS sample per character and frame.
- Original weights are captured before the first write. Restoration occurs
  only while a channel still contains the plugin's last value, so another
  writer's changes are not overwritten.
- Optional ExpressionControl values are cached. Stable owned adjustment
  channels are checked on a staggered 64-frame cadence.
- SliderHighlight selection overlays are excluded from automatic eye-target
  selection through ownership. Explicit renderer selection remains available;
  unavailable optional ownership metadata leaves the normal resolver in charge.

### Target and output contracts

Paths are relative to `ChaControl`; component and slot indices are zero-based,
with slot `-1` meaning any. Scope, slot, and component filters must match.
Renderer and mesh hints are exact and case-sensitive. Without a path, the
destination shape must be unique within the filters.

The five eye-motion and seven manual eye-part channels are reserved.
Up to 128 Expression Links can be stored per character. For competing links,
higher priority wins, then the greatest evaluated weight.

`Binary` outputs `OutputMax` above `Threshold` and `OutputMin` otherwise.
`Follow Source` clamps and maps `InputMin..InputMax` to `OutputMin..OutputMax`.
Source weights range from 0 to 1; destination weights from 0 to 100.
Transition speed is weight units per second; zero applies immediately.

### Persistence contracts

The BepInEx GUID and ExtendedSave identity are
`com.nightowlzzz.koikatsu.eyemotion`. DLL names, namespace,
`eye_motion.*` defaults, configuration keys, and the stored
`KK_ExpressionLink` folder names are compatibility identifiers.
BepInEx currently displays `KK_ExpressionLink` for both variants.

Card schema 3 stores the seven manual eye-part modes, Expression Links, and
legacy fields. Schemas 1 and 2 remain readable: schema 1 has 14 manual modes;
schema 2 also has four trigger strings. Both load without Expression Links.
Their fixed 10-blendshape/4-renderer layout and trigger slots are preserved,
not silently converted into links or exposed as a separate editor.

Invalid or unsupported newer payloads remain untouched unless Expression Links
are explicitly edited; saving those edits writes the current schema.
KKAPI attaches per-character data to Studio characters; there is no separate
global scene payload.

Reusable JSON sets use format version 1 and declare `supportedGames`.
Loading requires the current game ID (`KK` or `KKS`) and creates fresh link IDs.
Public serialized fields and enum values are data contracts.

</details>

## Credits and license

Author: **NightOwlZzz / Owl**. Uses BepInEx, KKAPI/KKSAPI, and
ExtensibleSaveFormat, with optional ExpressionControl integration.

Copyright (c) 2026 NightOwlZzz / Owl.

No open-source license is currently included in this repository. Contact the
author before redistributing the plugin or reusing or modifying its source code.
