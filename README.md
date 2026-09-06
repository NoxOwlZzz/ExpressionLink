# KK_ExpressionLink

**Blendshape and Expression Controller for Koikatsu**
Author: NightOwlZzz / Owl
Current version: 0.5.1

KK_ExpressionLink preserves the optimized custom-eye movement, blink, iris
adjustment, visibility, and highlight features introduced as EyeMotion. It now
also links Koikatsu facial expressions to blendshapes on any character
`SkinnedMeshRenderer`, including head, hair, body, clothes, accessories, and
other character-owned meshes.

The plugin uses Koikatsu's existing eye and facial-expression state. It does
not calculate an independent look-at target, use vertex colors, replace
`sharedMesh`, create material instances, or move eye bones.

## Requirements

- Koikatsu / Koikatu with BepInEx 5.
- Modding API / KKAPI 1.42.2 or a compatible newer version.
- ExtensibleSaveFormat (`com.bepis.bepinex.extendedsave`).
- The eye-motion feature requires a compatible headmod containing its five
  movement/blink shapes. Expression Links can be used independently on other
  character-owned meshes.
- Any desired IrisY/Size, Hide, expression, hair, clothing, or accessory
  blendshapes. These additional channels are optional.
- KK_ExpressionControl is optional. It is required only to drive the two
  optional IrisY/Size blendshapes.

This build targets Koikatsu/Koikatu only. **Koikatsu Sunshine is not compatible
or supported**; separation of the shared core is not a KKS compatibility claim.

## Clean ZIP installation

1. Close Koikatsu and CharaStudio.
2. If upgrading from EyeMotion or an older KK_ExpressionLink version, remove
   all old copies of `KK_EyeMotion.dll` from `BepInEx/plugins` and its subfolders,
   including any old EyeMotion or KK_EyeMotion installation folder. Keep your
   character cards and configuration files.
3. Open `KK_ExpressionLink-v0.5.1.zip` and merge its `BepInEx` folder into
   the game directory.
4. Confirm that only one `KK_EyeMotion.dll` remains, at:

   ```text
   <Koikatsu>\BepInEx\plugins\KK_EyeMotion\KK_EyeMotion.dll
   ```

5. Start the game once. BepInEx creates or updates:

   ```text
   BepInEx\config\com.nightowlzzz.koikatsu.eyemotion.cfg
   ```

The new DLL is still named `KK_EyeMotion.dll`. Do not keep both versions or DLL
backups inside `BepInEx/plugins`. The internal plugin folder, BepInEx GUID,
configuration file, namespace, card-data identity, and `eye_motion.*` names
remain unchanged for compatibility.

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

## Optional KK_ExpressionControl eye-adjustment shapes

The optional integration supports the two values exposed by
KK_ExpressionControl's `IrisY` and `Size` sliders:

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

KK_ExpressionControl.dll is an optional integration, not a hard plugin
dependency. Without it, gaze, blink, visibility, highlights, Expression Links,
and card persistence continue to work normally; KK_ExpressionLink does not
control these two shapes and restores any values it previously owned.

## Quick Settings

Press `Left Ctrl + Left Shift + M` to open or close the runtime panel in the
game, Maker, or Studio. The compact panel uses internal scrolling so its bottom
actions remain accessible. Drag the title bar to move it: the body may leave the
screen, but the complete header stays visible vertically and at least 120 pixels
remain available horizontally to recover it.

The panel is organized into three clear categories:

- **Eyes**: **Tracking** for camera tracking and calibration, and **Size
  controls** for the game's Iris/Size sliders.
- **Expressions**: the single guided, per-character workflow for linking a live
  Koikatsu expression to a blendshape on any supported character mesh.
- **Visibility**: highlight synchronization and manual eye-part visibility.

Common controls stay visible. Fine tuning, names, live values, target setup,
and troubleshooting begin collapsed and can be opened when needed.

The same panel can also be opened from:

- Maker: **Face > Expression Link > Open Expression Link Quick Settings**.
- Studio: the **KK_ExpressionLink** button in the left toolbar.

The Studio button uses a neutral square background and bevel consistent with
the surrounding toolbar buttons while retaining its monochrome eye glyph.

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
across compatible headmods or characters. Reusable sets declare Koikatsu
support only and must not be treated as KKS-compatible data.

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

The current runtime retains the optimized gaze/blink pipeline and extends the
features conservatively:

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

Use Visual Studio Build Tools with a C# 7.3-capable compiler. The plugin and
tests target .NET Framework 3.5 using the game's reference assemblies.

Place these references from the compatible Koikatsu/BepInEx installation in
the ignored `lib` directory:

```text
mscorlib.dll
System.dll
System.Core.dll
BepInEx.dll
KKAPI.dll
ExtensibleSaveFormat.dll
Assembly-CSharp.dll
UnityEngine.dll
UnityEngine.UI.dll
```

Do not commit or distribute these reference assemblies. All runtime project
references use `Private=False`.

Open a Visual Studio Developer Command Prompt or Developer PowerShell, change
to the repository root, and build the plugin:

```shell
msbuild KK_EyeMotion.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /m /v:minimal
```

The DLL is written to `bin/Release/KK_EyeMotion.dll`. Use
`/p:Configuration=Debug` for a Debug plugin build.

Build and run the automated checks after building Release:

```shell
msbuild tests/KK_EyeMotion.Tests.csproj /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /m /v:minimal
.\tests\bin\Release\KK_EyeMotion.Tests.exe
```

The checks use the local reference assemblies and Release DLL; they do not
launch the game. Release builds treat warnings as errors.

For local deployment, close Koikatsu and CharaStudio and copy
`bin/Release/KK_EyeMotion.dll` into the game's
`BepInEx/plugins/KK_EyeMotion` folder. Follow the upgrade instructions above
to leave only one DLL.

To prepare a download, create a ZIP with this layout:

```text
BepInEx/plugins/KK_EyeMotion/KK_EyeMotion.dll
README.txt
```

Use a brief English README with the requirements and upgrade instructions
above, including removal of old DLL copies while retaining cards and config.
Do not include dependency DLLs, configuration files, PDBs, source, or tests.
Calculate SHA-256 from the finished ZIP when providing a checksum.

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
- KoikatuVR and Koikatsu Sunshine are outside the declared compatibility
  scope. KKS support must not be assumed from the shared-core architecture.

## Credits

KK_ExpressionLink uses BepInEx, KKAPI, and ExtensibleSaveFormat. It can
optionally integrate with KK_ExpressionControl when that plugin is installed.
Those projects are not bundled in the release archive.

## License

Copyright (c) 2026 NightOwlZzz / Owl.

No open-source license is currently included in this repository. Contact the
author before redistributing the plugin or reusing or modifying its source
code.
