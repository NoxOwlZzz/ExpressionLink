# KK_ExpressionLink

**Blendshape and Expression Controller for Koikatsu and Koikatsu Sunshine**
Author: NightOwlZzz / Owl
Current version: 0.5.1 (KKS build: runtime validation pending)

KK_ExpressionLink controls custom-eye movement, blink, iris adjustment,
visibility, and highlights. It links Koikatsu facial expressions to blendshapes
on any character `SkinnedMeshRenderer`, including head, hair, body, clothes,
accessories, and other character-owned meshes.

The plugin uses Koikatsu's existing eye and facial-expression state. It does
not calculate an independent look-at target, use vertex colors, replace
`sharedMesh`, create material instances, or move eye bones.

## Requirements

Download only the variant for your game. Both use BepInEx 5.

| Game | Plugin DLL | Framework | Game-specific dependencies |
| --- | --- | --- | --- |
| Koikatsu / Koikatu (KK) | `KK_EyeMotion.dll` | .NET Framework 3.5 | KKAPI 1.42.2+, ExtensibleSaveFormat |
| Koikatsu Sunshine (KKS) | `KKS_EyeMotion.dll` | .NET Framework 4.6 | KKSAPI 1.42.2+, KKS_ExtensibleSaveFormat |

Use dependency versions compatible with the selected game. Both API variants
use the `marco.kkapi` GUID, and both ExtendedSave variants use
`com.bepis.bepinex.extendedsave`.

- The eye-motion feature requires a compatible headmod containing its five
  movement/blink shapes. Expression Links can be used independently on other
  character-owned meshes.
- Any desired IrisY/Size, Hide, expression, hair, clothing, or accessory
  blendshapes. These additional channels are optional.
- The matching KK_ExpressionControl or KKS_ExpressionControl is optional. It is
  required only to drive the two optional IrisY/Size blendshapes.

KK targets `Koikatu.exe` (game and Maker) and `CharaStudio.exe`.
KKS targets `KoikatsuSunshine.exe` (game and Maker) and `CharaStudio.exe`.
**KKS has passed compilation and static API checks, but in-game validation is
pending.** Do not treat it as fully verified compatibility.

## Installation

1. Close the game and CharaStudio.
2. Remove old copies of the plugin DLL from `BepInEx/plugins` and its
   subfolders. In KK this includes the original EyeMotion's `KK_EyeMotion.dll`;
   in KKS the correct DLL is `KKS_EyeMotion.dll`. Remove any copy of the other
   game's variant from that installation. Keep your cards and configuration.
3. Extract the `BepInEx` folder from the matching ZIP into that game's directory:

   - KK: `KK_ExpressionLink_v0.5.1.zip`
   - KKS: `KKS_ExpressionLink_v0.5.1.zip`

4. Confirm that exactly one variant is installed, at its matching path:

   ```text
   KK:  BepInEx/plugins/KK_EyeMotion/KK_EyeMotion.dll
   KKS: BepInEx/plugins/KKS_EyeMotion/KKS_EyeMotion.dll
   ```

5. Start the game once. BepInEx creates or updates:

   ```text
   BepInEx\config\com.nightowlzzz.koikatsu.eyemotion.cfg
   ```

Do not install both variants together or keep DLL backups in `BepInEx/plugins`.
The in-game plugin name remains `KK_ExpressionLink` in both builds. The BepInEx
GUID, configuration filename, namespace, card-data identity, and `eye_motion.*`
names are shared. Matching data identifiers do not guarantee that a card,
coordinate, scene, or mesh can transfer between games.

The ZIP contains only the plugin DLL and a brief English `README.txt` at the
archive root, with requirements and installation instructions. Configuration
files, PDB files, source files, and dependency DLLs are not included.

## Required movement and blink shapes

The five required default names are:

```text
eye_motion.f00_eye_posx
eye_motion.f00_eye_negx
eye_motion.f00_eye_posy
eye_motion.f00_eye_negy
eye_motion.f00_eye_blink
```

Names must match the runtime mesh exactly. Some import pipelines preserve a
final `_0`; KK_ExpressionLink never adds or removes suffixes while resolving a mesh. If
the game shows `eye_motion.f00_eye_posx_0`, enter that exact name in Quick
Settings or BepInEx Configuration Manager.

All five shapes must be on the same `SkinnedMeshRenderer`. The four direction
shapes represent positive X, negative X, positive Y, and negative Y. The blink
shape represents both eyes fully closed at weight 100.

## Optional ExpressionControl eye-adjustment shapes

The optional integration supports the two values exposed by
the matching ExpressionControl plugin's `IrisY` and `Size` sliders:

```text
eye_motion.f00_iris_y
eye_motion.f00_iris_size
```

`IrisY` has a source range of 0 to 0.5. Zero leaves
`eye_motion.f00_iris_y` at weight 0; 0.5 reaches its configured maximum weight.

`Size` has a source range of 0 to 1 and represents iris shrink in
KK_ExpressionControl. Zero leaves `eye_motion.f00_iris_size` at weight 0; 1
reaches its configured maximum. Author this shape so increasing its weight
makes the iris smaller.

Both shapes are optional and must share the selected `SkinnedMeshRenderer`
with the five required gaze/blink shapes when present. They do not replace or
bias the four gaze-direction shapes. A missing shape does not disable any
other feature.

KK_ExpressionControl.dll (KK) or KKS_ExpressionControl.dll (KKS) is an optional
integration, not a hard plugin dependency. Without it, gaze, blink, visibility,
highlights, Expression Links, and card persistence remain available; the plugin does not
control these two shapes and restores any values it previously owned.

## Quick Settings

Press `Left Ctrl + Left Shift + M` to open or close the runtime panel in the
game, Maker, or Studio. The compact panel uses internal scrolling so its bottom
actions remain accessible. Drag the title bar to move it: the body may leave the
screen, but the complete header stays visible vertically and at least 120 pixels
remain available horizontally to recover it.

The panel is organized into three categories:

- **Eyes**: **Tracking** for camera tracking and calibration, and **Size
  controls** for the game's Iris/Size sliders.
- **Expressions**: the per-character workflow for linking a live
  Koikatsu expression to a blendshape on any supported character mesh.
- **Visibility**: highlight synchronization and manual eye-part visibility.

Common controls stay visible. Fine tuning, names, live values, target setup,
and troubleshooting begin collapsed and can be opened when needed.

The same panel can also be opened from:

- Maker: **Face > Expression Link > Open Expression Link Quick Settings**.
- Studio: the **KK_ExpressionLink** button in the left toolbar.

Use the arrows at the top to select a character. Tracking, size, and visibility
configuration use **Apply global settings**; **Discard global changes** reloads
them. Manual visibility buttons take effect immediately. Expression links are
saved to the selected character with **Save expression link**.

Recommended X/Y gaze calibration:

1. Make the character look straight ahead and press **Set neutral pose**.
2. Move the look target fully left and press **Set horizontal range**.
3. Move it fully right and press **Set horizontal range** again.
4. Try **Sensitive preset** or **Standard preset** for a quick starting point.
5. Open **Fine tuning** only when separate left/right or up/down adjustment is
   needed. A lower range means higher sensitivity.
6. Press **Apply global settings**.

## Expression Links

An Expression Link reads one live Koikatsu brow, eye, or mouth expression and
writes a blendshape on a selected character renderer. Links are stored per
character, so a VRChat-derived headmod can drive facial details as well as
hair ears, horns, accessories, clothing parts, or other authored shapes.

Use **Expressions** for the selected character:

1. Select **Create link**.
2. Pose the character and select **Use current eyes**, **Use current brows**,
   or **Use current mouth**.
3. Choose the character area and enter the exact destination blendshape name.
4. Choose **Switch fully** or **Follow expression** and set its maximum strength.
5. Select **Save expression link**.

The capture buttons store the current Koikatsu selector automatically, so the
raw `brow:N`, `eyes:N`, or `mouth:N` value is not required for normal setup.

Open **Advanced targeting and tuning** only when the target is ambiguous or
needs custom ranges. It exposes the raw expression selector, exact renderer
path, component and slot indices, renderer/mesh hints, smoothing, and priority.

### Multi-renderer targets

Supported target scopes are `Any`, `Head`, `Hair`, `Body`, `Clothes`,
`Accessory`, and `Other`. The `Any`, `Hair`, `Clothes`, and `Accessory` scopes
accept zero-based character slot filters; `-1` means any slot. Renderer paths
are relative to the character's `ChaControl` transform, and component indices
are zero-based when one GameObject contains multiple `SkinnedMeshRenderer`
components.

Resolution is conservative:

- With an exact path, scope, slot, and optional component index must match.
- Renderer-name and mesh-name hints can disambiguate renderers at that exact
  path, but both hints are exact and case-sensitive.
- Without a path, the destination blendshape must be unique after applying
  scope, slot, and optional component filters.
- Missing or ambiguous targets are reported instead of selecting an arbitrary
  mesh.
- The five eye-motion channels and the seven eye-part channels exposed by
  **Visibility** are reserved. Expression-specific shapes are configured as
  ordinary per-character Expression Link destinations.

### Output modes

- **Switch fully** (`Binary` internally): outputs `OutputMax` above
  `Threshold`; otherwise it outputs `OutputMin`.
- **Follow expression** (`Follow Source` internally): maps
  `InputMin..InputMax` to `OutputMin..OutputMax` after clamping.
- **Transition speed:** limits movement in blendshape-weight units per second.
  Zero applies the result immediately.

Source ranges are 0 to 1 and destination weights are 0 to 100. Up to 128
links can be stored for one character.

If multiple enabled links resolve to the same renderer, mesh, and blendshape,
the highest priority wins. At equal priority, the greatest evaluated weight
wins. KK_ExpressionLink captures the original target weight before its first
write and restores it only if the channel still contains the last value it
wrote. A later external writer is therefore not overwritten by a stale
restore.

### Reusable link sets

The Reusable link sets panel saves and loads validated JSON files under:

```text
BepInEx\config\KK_ExpressionLink\Profiles
```

Loading a set copies its links to the selected character with fresh IDs.
Reusable sets are separate from character-card data and are intended for reuse
across compatible headmods or characters. A newly saved set declares only the
current game's ID in `supportedGames`: `KK` or `KKS`. Loading requires that
the list include the current game. Existing KK-only sets are not automatically
accepted by KKS. The profile format remains version 1; a shared format does not
verify matching expression IDs, renderer paths, or meshes between games.

## Manual visibility

KK_ExpressionLink exposes seven optional eye-part blendshape slots. Each row has
three modes:

- `Original`: relinquish manual control and restore the captured runtime value.
- `Visible`: force the configured Hide blendshape to 0.
- `Hidden`: use the configured hide weight.

Default eye-part names:

```text
eye_motion.f00_hide_highlight01
eye_motion.f00_hide_highlight02
eye_motion.f00_hide_irisinner
eye_motion.f00_hide_irisouter
eye_motion.f00_hide_pupil
eye_motion.f00_hide_sclera
eye_motion.f00_hide_normaleyes
```

Every visibility slot is optional. Use **Expressions** when a facial expression
should activate a different authored blendshape, including shapes on hair,
clothes, accessories, or other character-owned meshes.

KK_ExpressionLink captures the original value immediately before its first write. When
returning to an uncontrolled `Original` state, reloading a character, or
unloading the plugin, it restores only values still owned by KK_ExpressionLink. This
prevents a stale restore from overwriting a later change made by another plugin.

## Erase Highlight synchronization

With **Follow Koikatsu highlight visibility** enabled, Koikatsu's public
`hideEyesHighlight` state adds a temporary Hidden override to Highlight 01 and
Highlight 02. Compatible Erase Highlight controls, including
KK_ExpressionControl, therefore also hide the configured custom-head highlight
blendshapes.

When the base highlight state becomes visible again, KK_ExpressionLink returns each
highlight slot to its previous `Original`, `Visible`, or `Hidden` mode.

## Character-card persistence

`CardPersistenceEnabled` is enabled by default. Expression Links are always
stored per character. Schema 3 stores:

- The seven current eye-part manual modes.
- The validated Expression Link definitions.
- Legacy visibility and expression fields retained for older cards.

Schema 1 and schema 2 cards remain compatible. Schema 1 loads its 14 manual
modes with empty legacy trigger fields and no links. Schema 2 also loads its
four trigger strings and starts with no links. Saving new links upgrades that
character payload to schema 3. The four legacy trigger slots are preserved
and are not silently converted into Expression Links. These legacy fields keep
their fixed 10-blendshape, 4-renderer layout for compatibility, but are not
shown as a second expression workflow in Quick Settings.

Invalid or unsupported newer payloads are preserved conservatively instead of
being silently overwritten. Live source activity, smoothing state, and the
Erase Highlight override are runtime state and are not saved. Global eye
calibration, default channel names, and global feature switches remain in the
BepInEx configuration.

KKAPI keeps per-character data attached to characters stored in Studio scenes.
KK_ExpressionLink does not create a separate global scene payload. JSON
profiles are separate reusable files, not additional Studio scene data. Test
compatibility on copies of valuable cards and keep the originals unchanged.

## Renderer selection

The main renderer is resolved on character reload and retried once per frame up
to the configured retry limit:

1. Exact `TargetRendererPath`, when configured.
2. Exact `TargetRendererName`, when configured.
3. Presence of all four configured directional blendshapes.
4. Presence of blink when blink is enabled and required.
5. Membership in the same character's head hierarchy.

The two KK_ExpressionControl eye-adjustment shapes are optional and do not
participate in the required compatibility check. If multiple candidates remain,
binding is reported as ambiguous until an exact path is configured. Vertex colors are not
used as a tie-breaker.

## Performance behavior

- KK_ExpressionControl `IrisY` and `Size` values are cached; weights are
  recalculated only when a source or relevant configuration changes.
- Managed eye-adjustment weights are written only when needed and verified on a
  staggered 64-frame cadence to recover from external writers without a
  constant write war.
- The character renderer catalog and Expression Link destinations are scanned
  only during bind/rebuild, never by traversing the hierarchy every frame.
- All configured expression sources share one live FBS sample per character and
  frame.
- Expression FBS controllers and trigger selectors are resolved once per bind.
- The three live FBS dictionaries are sampled without reflection or managed
  allocations in the frame loop.
- Visibility writes occur only when trigger/manual state changes.
- Closed Quick Settings avoids automatic GUILayout work.

## Diagnostics

Press the configured diagnostics shortcut (default `Left Shift + F10`) or open
**Help** and select **Create debug report**. Reports are written under:

```text
BepInEx\config\KK_ExpressionLink\diagnostics
```

Reports include sampled gaze/IrisY/Size values, applied weights, renderer
binding details, optional-channel status, manual/automatic visibility state,
Expression Link source and target resolution, conflict status, resolved
expression selectors and weights, highlight synchronization, and card
persistence status.

## Building and packaging

Use Visual Studio Build Tools with MSBuild 17 or newer and a C# 7.3-capable
compiler. KK targets .NET Framework 3.5; KKS targets .NET Framework 4.6.
Use reference assemblies from the corresponding game and mod installation.

Place the following files in the ignored reference directories:

| Directory | Required references |
| --- | --- |
| `lib/` (KK) | `mscorlib.dll`, `System.dll`, `System.Core.dll`, `BepInEx.dll`, `KKAPI.dll`, `ExtensibleSaveFormat.dll`, `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.UI.dll` |
| `lib/KKS/` (KKS) | `mscorlib.dll`, `System.dll`, `System.Core.dll`, `BepInEx.dll`, `KKSAPI.dll`, `KKS_ExtensibleSaveFormat.dll`, `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.UI.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `UnityEngine.InputLegacyModule.dll`, `UnityEngine.JSONSerializeModule.dll`, `UnityEngine.TextRenderingModule.dll`, `UnityEngine.UIModule.dll` |

The system and Unity DLLs must come from that game's Managed directory.
BepInEx comes from its core directory; the API and ExtendedSave come from its
plugins directory. Do not mix KK references into the KKS directory or distribute
any of these dependencies. Every project reference uses `Private=False`.
KKS needs both the Unity facade (used by BepInEx) and the concrete Unity modules.

From a Visual Studio Developer Command Prompt or Developer PowerShell:

```shell
msbuild ExpressionLink.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /m /v:minimal
```

`KK_EyeMotion.csproj` and `KKS_EyeMotion.csproj` can also be built individually.
The DLLs are written to `bin/Release/KK_EyeMotion.dll` and
`bin/KKS/Release/KKS_EyeMotion.dll`. Debug uses the corresponding `Debug`
directories. Intermediate files are isolated by game.

The same `src/` files are listed once in `ExpressionLink.Shared.projitems`.
`ExpressionLink.Build.props` owns shared compiler settings and references.
Game projects own their framework, references and `KK`/`KKS` constant.
`GameCompatibility` contains only the executable and profile-game identifiers;
the runtime controllers, UI, eye sampling and card codec remain shared.

Build and run both test targets after building the Release plugins:

```shell
msbuild tests/KK_EyeMotion.Tests.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
.\tests\bin\Release\KK_EyeMotion.Tests.exe
msbuild tests/KKS_EyeMotion.Tests.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
.\tests\bin\KKS\Release\KKS_EyeMotion.Tests.exe
```

The tests exercise shared managed logic and inspect the selected game's DLL and
references. They do not launch Unity or replace the in-game checks below.
Release builds treat warnings as errors. If Windows blocks trusted DLLs copied
from a downloaded archive, unblock only those local reference copies before
running the tests.

`ExpressionLink.proj` provides these operations without command wrappers:

| Operation | Command |
| --- | --- |
| Build KK | `msbuild ExpressionLink.proj /t:BuildKK` |
| Build KKS | `msbuild ExpressionLink.proj /t:BuildKKS` |
| Build both | `msbuild ExpressionLink.proj /t:BuildBoth` |
| Clean both build outputs | `msbuild ExpressionLink.proj /t:Clean` |
| Package KK | `msbuild ExpressionLink.proj /t:PackageKK` |
| Package KKS | `msbuild ExpressionLink.proj /t:PackageKKS` |
| Package both | `msbuild ExpressionLink.proj /t:PackageBoth` |

The default configuration is Release. Build/Clean accept
`/p:Configuration=Debug`; packaging requires Release. The generic targets
`Build`, `Clean`, and `Package` also accept `/p:Game=KK`, `KKS`, or `Both`.

Packages are written to `dist/<Game>_ExpressionLink_v<version>.zip` with a
SHA-256 sidecar. Each contains only its matching plugin DLL under
`BepInEx/plugins/<Game>_EyeMotion/` and a brief English `README.txt`.
Packaging reads the assembly version, excludes dependencies and PDBs, and does
not install the plugin or publish anything.

## Current limitations

- Eye-motion headmods must author the five movement/blink shapes and whichever
  optional IrisY/Size or visibility shapes they want to support. General
  Expression Links require only their configured destination blendshape.
- Blink is joint; asymmetric wink channels are not implemented.
- IrisY/Size shapes must be authored to combine acceptably with gaze
  and blink; KK_ExpressionLink cannot repair incompatible vertex deltas.
- Expression triggers follow Koikatsu FBS pattern weights. An arbitrary shape
  that is not referenced by a brow/eyes/mouth Close/Open pattern cannot be a
  source trigger.
- `Renderer.enabled = true` cannot reveal an inactive GameObject or inactive
  parent. KK_ExpressionLink deliberately does not call `GameObject.SetActive`.
- JSON profiles store Expression Links only; they do not package meshes or
  convert incompatible VRChat blendshape deltas.
- A destination that remains absent throughout the initial resolution window
  is not polled indefinitely. Reload the character or save the link again after
  its renderer is present.
- KoikatuVR is not supported. KKS in-game compatibility remains pending the
  checklist below.

## Compatibility validation

The source compiles against both games' installed dependencies. Static API
comparison covers the used eye/FBS fields, character hierarchy, KKAPI callbacks,
Maker controls, Studio toolbar and ExpressionControl bridge. No Harmony patches
or game AssetBundles are used. Static agreement does not verify Unity lifecycle,
expression semantics, timing, or cross-game content conversion.

| Function | KK | KKS | Implementation |
| --- | --- | --- | --- |
| Gaze, blink, highlights, manual visibility | Pending runtime test (regression) | Pending runtime test | Shared controllers and sampling |
| IrisY / Size | Pending runtime test (regression) | Pending runtime test | Shared optional ExpressionControl bridge |
| Multi-renderer Expression Links | Pending runtime test (regression) | Pending runtime test | Shared resolution, evaluation and ownership |
| Card / Studio character persistence | Pending runtime test (regression) | Pending runtime test | Shared schemas 1–3 and ExtendedSave identity |
| Reusable link sets | Pending runtime test (regression) | Pending runtime test | Shared codec with current-game compatibility gate |
| Maker / Studio / game Quick Settings | Pending runtime test (regression) | Pending runtime test | Shared UI, game-specific process filter |

### KK regression checklist

- Load a compatible character in Maker and the main game: gaze calibration,
  X/Y movement, blink, highlight hiding and optional IrisY/Size must behave as before.
- Open Quick Settings from its shortcut, Maker control and Studio toolbar:
  dragging, scrolling and applying settings must work without duplicate controls.
- Create expression links on head and hair/accessory meshes: only the resolved
  targets respond; disabling/removing a link restores only plugin-owned weights.
- Save/reload a card and a Studio scene, then import/duplicate/remove characters:
  per-character links persist, and removed characters release their targets.
- Change clothes/accessories and reload the character: targets rebind or report
  missing/ambiguous status. Save/load a KK reusable set without changing its game ID.

### KKS compatibility checklist

- Install only the KKS DLL and start Maker, the main game and Studio:
  BepInEx must load one plugin instance without missing dependencies or exceptions.
- Verify neutral/left/right/up/down gaze, blink and Erase Highlight on a compatible
  KKS headmod. With KKS_ExpressionControl installed, IrisY/Size must use the same
  authored shape ranges; without it, the other channels must remain usable.
- Open, move and resize the game window with Quick Settings visible. All controls
  and the header must remain accessible; close/reopen must not duplicate listeners.
- Link eye/brow/mouth expressions to head and hair/accessory shapes. Change
  clothes/accessories, reload, disable links and remove characters: bindings and
  ownership restoration must remain correct.
- Save/load cards and Studio scenes, import/duplicate characters, and save/load a
  KKS reusable set. Links must remain per character; a KK-only set must be rejected.

For either checklist, inspect `BepInEx/LogOutput.log` for dependency, lifecycle or
UI errors. For incorrect weights/targets, use **Help > Create debug report** and
compare source values, resolved paths and channel ownership. Also observe idle
scenes with the panel closed: no repeating errors or continuous renderer scans
should occur. Cross-game card, coordinate and scene transfer is not validated.

## Credits

KK_ExpressionLink uses BepInEx, the matching KKAPI/KKSAPI, and ExtensibleSaveFormat.
It can optionally integrate with the matching ExpressionControl plugin.
Those projects are not bundled in the release archive.

## License

Copyright (c) 2026 NightOwlZzz / Owl.

No open-source license is currently included in this repository. Contact the
author before redistributing the plugin or reusing or modifying its source
code.
