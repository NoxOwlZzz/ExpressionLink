# KK_ExpressionLink

**Blendshape and Expression Controller for Koikatsu**
Author: NightOwlZzz / Owl
Current version: 0.5.0

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

The development installation used BetterRepack RX22, BepInEx 5.4.23.2, and
KKAPI 1.42.2. Other installations should be verified before being advertised
as supported.

This build targets Koikatsu/Koikatu only. Its core is organized for a future
Koikatsu Sunshine adapter, but **KKS is not currently compatible or supported**.

## Clean ZIP installation

1. Close Koikatsu and CharaStudio.
2. Open `KK_ExpressionLink-v0.5.0.zip` and merge its `BepInEx` folder into
   the game directory.
3. Confirm that the final DLL path is:

   ```text
   <Koikatsu>\BepInEx\plugins\KK_EyeMotion\KK_EyeMotion.dll
   ```

4. Start the game once. BepInEx creates or updates:

   ```text
   BepInEx\config\com.nightowlzzz.koikatsu.eyemotion.cfg
   ```
The outer ZIP and visible plugin name are new. The physical
`KK_EyeMotion.dll`, internal plugin folder, BepInEx GUID, configuration file,
namespace, card-data identity, and `eye_motion.*` names deliberately remain
unchanged. An update therefore replaces the old plugin instead of loading a
duplicate.

The public ZIP contains only the plugin DLL and its English README.
It deliberately excludes configuration files, PDB files, source files, and
dependency DLLs. Installing an update therefore preserves the user's existing
configuration and does not replace shared dependencies.

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

Version 0.4.0 and later support only the two values actually exposed by
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
dependency. Without it, gaze, blink, visibility, highlights, expression
automation, and card persistence continue to work normally; KK_ExpressionLink does not
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
- **Expressions**: **ExpressionMesh slots** for the four simple mappings, and
  **Custom links** for blendshapes on any supported character mesh.
- **Visibility**: highlight synchronization, card persistence, and manual parts.

Common controls stay visible. Fine tuning, names, live values, target setup,
and troubleshooting begin collapsed and can be opened when needed.

The same panel can also be opened from:

- Maker: **Face > Expression Link > Open Expression Link Quick Settings**.
- Studio: the **KK_ExpressionLink** button in the left toolbar.

The Studio button now uses the same neutral square background and bevel as the
surrounding toolbar buttons while retaining the monochrome eye glyph.

Use the arrows at the top to select a character. Settings changed under Eyes,
ExpressionMesh slots, or Visibility are stored with **Save settings**;
**Discard changes** reloads their saved values. Manual visibility buttons take
effect immediately. ExpressionMesh mappings and Custom links use their own clearly
labelled save buttons because they belong to the selected character.

Recommended X/Y gaze calibration:

1. Make the character look straight ahead and press **Set neutral pose**.
2. Move the look target fully left and press **Set horizontal range**.
3. Move it fully right and press **Set horizontal range** again.
4. Try **Sensitive preset** or **Standard preset** for a quick starting point.
5. Open **Fine tuning** only when separate left/right or up/down adjustment is
   needed. A lower range means higher sensitivity.
6. Press **Save settings**.

## Expression Links

An Expression Link reads one live Koikatsu brow, eye, or mouth expression and
writes a blendshape on a selected character renderer. Links are stored per
character, so a VRChat-derived headmod can drive facial details as well as
hair ears, horns, accessories, clothing parts, or other authored shapes.

Use **Expressions > Custom links** for the selected character:

1. Add a link and give it a descriptive name.
2. Set its source to an exact Koikatsu FBS Close/Open blendshape name or an
   explicit `brow:N`, `eyes:N`, or `mouth:N` selector.
3. Enter the exact destination blendshape name.
4. Choose the target scope and optional slot. Leave the renderer path empty
   only when that blendshape is unique within the selected scope and slot.
5. Use an exact renderer path and component index when status reports an
   ambiguous destination.
6. Choose **On / off** or **Follow expression strength**, then save the link.

A source name is matched against Koikatsu's current brow, eyes, and mouth
Close/Open patterns. Names under `eye_motion.*` are output destinations and
are deliberately rejected as sources. If an exact source name is ambiguous,
use the explicit selector reported by the status or diagnostics.

### Multi-renderer targets

Supported target scopes are `Any`, `Head`, `Hair`, `Body`, `Clothes`,
`Accessory`, and `Other`. Hair, clothes, and accessory slots use zero-based
indices; `-1` means any slot. Renderer paths are relative to the character's
`ChaControl` transform, and component indices are zero-based when one
GameObject contains multiple `SkinnedMeshRenderer` components.

Resolution is conservative:

- With an exact path, scope, slot, and optional component index must match.
- Renderer-name and mesh-name hints can disambiguate renderers at that exact
  path, but both hints are exact and case-sensitive.
- Without a path, the destination blendshape must be unique after applying
  scope, slot, and optional component filters.
- Missing or ambiguous targets are reported instead of selecting an arbitrary
  mesh.
- The five eye-motion channels and legacy manual-visibility channels are
  reserved; an Expression Link cannot take ownership of those destinations.

### Output modes

- **On / off** (`Binary` internally): outputs `OutputMax` above `Threshold`;
  otherwise it outputs `OutputMin`.
- **Follow expression strength** (`Follow Source` internally): maps
  `InputMin..InputMax` to `OutputMin..OutputMax` after clamping.
- **Smoothing Speed:** limits movement in blendshape-weight units per second.
  Zero applies the result immediately.

Source ranges are 0 to 1 and destination weights are 0 to 100. Up to 128
links can be stored for one character.

If multiple enabled links resolve to the same renderer, mesh, and blendshape,
the highest priority wins. At equal priority, the greatest evaluated weight
wins. KK_ExpressionLink captures the original target weight before its first
write and restores it only if the channel still contains the last value it
wrote. A later external writer is therefore not overwritten by a stale
restore.

### Reusable profiles

The Profiles panel saves and loads validated JSON link sets under:

```text
BepInEx\config\KK_ExpressionLink\Profiles
```

Loading a profile copies its links to the selected character with fresh IDs.
Profiles are separate from character-card data and are intended for reuse
across compatible headmods or characters. Version 0.5.0 profiles declare
Koikatsu support only; a future KKS build will require its own validated adapter.

## Automatic ExpressionMesh triggers

Each selected character has one trigger field for `ExpressionMesh_01` through
`ExpressionMesh_04`. When a configured facial pattern exceeds the global
activation threshold, KK_ExpressionLink shows that slot; otherwise it hides it. A slot
controls its corresponding separate renderer and, when present, the matching
fused Hide destination (`hide_expression01` through `hide_expression03`).

A trigger accepts either:

- An exact runtime blendshape name from a Koikatsu brow, eyes, or mouth FBS
  Close/Open pattern, for example `happy_blendshape`.
- An explicit pattern selector: `brow:N`, `eyes:N`, or `mouth:N`, where `N` is
  a non-negative pattern index, for example `eyes:5`.

Name matching is case-insensitive but otherwise exact. If the same name maps to
more than one facial pattern, KK_ExpressionLink reports it as ambiguous; use the
explicit selector shown by diagnostics/status. `eye_motion.*` names are
destinations and are deliberately rejected as triggers.

For easier setup, pose the character with the desired facial expression, then
use the row labeled **Use current:** and press **Brow**, **Eyes**, or **Mouth**
on the desired ExpressionMesh row. The panel records the strongest current
pattern as an explicit selector. Press **Apply expression triggers to selected
character** to resolve and use the four edited fields.

Automatic visibility has lower priority than manual intent:

1. Koikatsu's Erase Highlight override remains highest for highlight slots.
2. Manual `Visible` or `Hidden` overrides automatic expression state.
3. Manual `Original` lets a configured expression trigger show/hide the slot.
4. With no valid trigger, `Original` restores the captured runtime value.

Disable **Enable expression-trigger automation** to return trigger-controlled
slots to their normal manual/original behavior without deleting saved trigger
assignments.

## Manual visibility

KK_ExpressionLink exposes ten optional fused-part slots and four optional renderer
slots. Each row has three modes:

- `Original`: relinquish manual control and restore the captured runtime value,
  unless a configured automatic expression trigger currently controls it.
- `Visible`: force a fused Hide blendshape to 0, or enable a renderer.
- `Hidden`: use the configured hide weight, or disable a renderer.

Default fused-part names:

```text
eye_motion.f00_hide_highlight01
eye_motion.f00_hide_highlight02
eye_motion.f00_hide_irisinner
eye_motion.f00_hide_irisouter
eye_motion.f00_hide_pupil
eye_motion.f00_hide_sclera
eye_motion.f00_hide_normaleyes
eye_motion.f00_hide_expression01
eye_motion.f00_hide_expression02
eye_motion.f00_hide_expression03
```

Default separate renderer targets:

```text
ExpressionMesh_01
ExpressionMesh_02
ExpressionMesh_03
ExpressionMesh_04
```

Every visibility slot is optional. Renderer targets can be exact GameObject
names or paths relative to `ChaControl`. Ambiguous names are rejected instead
of selecting an arbitrary renderer.

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

`CardPersistenceEnabled` is enabled by default. Schema 3 stores, per character:

- The ten fused-part manual modes.
- The four renderer manual modes.
- The four legacy ExpressionMesh trigger strings.
- The validated Expression Link definitions.

Schema 1 and schema 2 cards remain compatible. Schema 1 loads its 14 manual
modes with empty legacy trigger fields and no links. Schema 2 also loads its
four trigger strings and starts with no links. Saving new links upgrades that
character payload to schema 3. The four legacy trigger slots are preserved
and are not silently converted into Expression Links.

Invalid or unsupported newer payloads are preserved conservatively instead of
being silently overwritten. Live source activity, smoothing state, and the
Erase Highlight override are runtime state and are not saved. Global eye
calibration, default channel names, and global feature switches remain in the
BepInEx configuration.

KKAPI keeps per-character data attached to characters stored in Studio scenes.
KK_ExpressionLink does not create a separate global scene payload. JSON
profiles are separate reusable files, not additional Studio scene data. Back
up valuable cards before checking compatibility.

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

```bat
build-release.bat
```

After a successful Release build, create the clean public archive:

```bat
package-release.bat
```

The packager validates the assembly version and every ZIP entry, excludes
configuration/dependency/PDB files, and creates:

```text
dist\KK_ExpressionLink-v0.5.0.zip
dist\KK_ExpressionLink-v0.5.0.zip.sha256
```

For local deployment, `deploy-release.bat` backs up the previous DLL outside
the scanned `BepInEx\plugins` tree before copying the new one. The PDB is copied
only with its explicit development option.

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
- KoikatuVR and Koikatsu Sunshine are outside the declared compatibility
  scope. KKS support must not be assumed from the shared-core architecture.
