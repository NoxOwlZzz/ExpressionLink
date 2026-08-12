# EyeMotion

**Eye Mesh and Expression Controller for Koikatsu**  
Author: NightOwlZzz / Owl  
Current version: 0.4.1

EyeMotion reuses the eye direction and effective eye opening already calculated
by Koikatsu to drive a custom head. It can also mirror KK_ExpressionControl's
real `IrisY` and `Size` values through two optional blendshapes, manages fused
eye-part and separate expression-mesh visibility, and can activate expression
meshes from the character's current brow, eye, or mouth expression.

EyeMotion does not use vertex colors. It does not replace `sharedMesh`, create
material instances, move eye bones or transforms, or calculate an independent
look-at target.

## Requirements

- Koikatsu / Koikatu with BepInEx 5.
- Modding API / KKAPI 1.42.2 or a compatible newer version.
- ExtensibleSaveFormat (`com.bepis.bepinex.extendedsave`).
- A compatible headmod containing the five required movement/blink shapes.
- Any desired IrisY/Size, Hide, and expression meshes or shapes.
  Those additional channels are optional.
- KK_ExpressionControl is optional. It is required only to drive the two
  optional IrisY/Size blendshapes.

The development installation used BetterRepack RX22, BepInEx 5.4.23.2, and
KKAPI 1.42.2. Other installations should be tested before being advertised as
supported.

## Clean ZIP installation

1. Close Koikatsu and CharaStudio.
2. Open `KK_EyeMotion-v0.4.1.zip` and merge its `BepInEx` folder into the game
   directory.
3. Confirm that the final DLL path is:

   ```text
   <Koikatsu>\BepInEx\plugins\KK_EyeMotion\KK_EyeMotion.dll
   ```

4. Start the game once. BepInEx creates or updates:

   ```text
   BepInEx\config\com.nightowlzzz.koikatsu.eyemotion.cfg
   ```

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
final `_0`; EyeMotion never adds or removes suffixes while resolving a mesh. If
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
automation, and card persistence continue to work normally; EyeMotion does not
control these two shapes and restores any values it previously owned.

## Quick Settings

Press `Left Ctrl + Left Shift + M` to open or close the runtime panel in the
game, Maker, or Studio. The panel is clamped to the current screen and its main
content scrolls so the bottom action buttons remain accessible.

Version 0.4.1 separates the panel into Motion, Iris, Expressions, and
Visibility tabs. Only the selected group is displayed.

The same panel can also be opened from:

- Maker: **Face > EyeMotion > Open Quick Settings**.
- Studio: the **EyeMotion** button in the left toolbar.

The Studio button now uses the same neutral square background and bevel as the
surrounding toolbar buttons while retaining the monochrome eye glyph.

Use the arrows at the top to select a character. Global settings such as
movement calibration, maximum weights, blendshape names, renderer targets, and
the automation threshold take effect after pressing **Apply**. Manual
visibility and per-character expression-trigger controls act on the selected
character.

Recommended X/Y gaze calibration:

1. Make the character look straight ahead and press **Center X/Y**.
2. Move the look target to a horizontal extreme and press **Use current X** if
   that range should reach maximum weight sooner.
3. Adjust positive and negative limits independently if necessary. A lower
   input limit means higher sensitivity.
4. Press **Apply**.

## Automatic ExpressionMesh triggers

Each selected character has one trigger field for `ExpressionMesh_01` through
`ExpressionMesh_04`. When a configured facial pattern exceeds the global
activation threshold, EyeMotion shows that slot; otherwise it hides it. A slot
controls its corresponding separate renderer and, when present, the matching
fused Hide destination (`hide_expression01` through `hide_expression03`).

A trigger accepts either:

- An exact runtime blendshape name from a Koikatsu brow, eyes, or mouth FBS
  Close/Open pattern, for example `happy_blendshape`.
- An explicit pattern selector: `brow:N`, `eyes:N`, or `mouth:N`, where `N` is
  a non-negative pattern index, for example `eyes:5`.

Name matching is case-insensitive but otherwise exact. If the same name maps to
more than one facial pattern, EyeMotion reports it as ambiguous; use the
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

EyeMotion exposes ten optional fused-part slots and four optional renderer
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

EyeMotion captures the original value immediately before its first write. When
returning to an uncontrolled `Original` state, reloading a character, or
unloading the plugin, it restores only values still owned by EyeMotion. This
prevents a stale restore from overwriting a later change made by another plugin.

## Erase Highlight synchronization

With **Follow Koikatsu highlight visibility** enabled, Koikatsu's public
`hideEyesHighlight` state adds a temporary Hidden override to Highlight 01 and
Highlight 02. Compatible Erase Highlight controls, including
KK_ExpressionControl, therefore also hide the configured custom-head highlight
blendshapes.

When the base highlight state becomes visible again, EyeMotion returns each
highlight slot to its previous `Original`, `Visible`, or `Hidden` mode.

## Character-card persistence

`CardPersistenceEnabled` is enabled by default. Schema 2 stores, per character:

- The ten fused-part manual modes.
- The four renderer manual modes.
- The four ExpressionMesh trigger strings.

Schema 2 remains backward-compatible with schema 1 cards. A schema 1 card loads
its 14 manual modes and receives empty expression triggers; it can then be saved
as schema 2. Unsupported newer schemas are preserved conservatively instead of
being silently overwritten.

The automatic active/inactive state and Erase Highlight override are runtime
state and are not saved. Global movement calibration, channel names, renderer
targets, enable switches, maximum weights, and activation threshold remain in
the BepInEx configuration.

KKAPI keeps per-character data attached to characters stored in Studio scenes.
EyeMotion does not create a separate global scene payload. If all 14 modes are
`Original` and all four triggers are empty, EyeMotion removes the unnecessary
card payload. Back up valuable cards before compatibility testing.

## Renderer selection

The main renderer is resolved on character reload and retried once per frame up
to the configured retry limit:

1. Exact `TargetRendererPath`, when configured.
2. Exact `TargetRendererName`, when configured.
3. Presence of all four configured directional blendshapes.
4. Presence of blink when blink is enabled and required.
5. Membership in the same character's head hierarchy.

The two KK_ExpressionControl eye-adjustment shapes are optional and do not
participate in the required compatibility test. If multiple candidates remain, binding is
reported as ambiguous until an exact path is configured. Vertex colors are not
used as a tie-breaker.

## Performance behavior

The current runtime retains the optimized gaze/blink pipeline and extends the
features conservatively:

- KK_ExpressionControl `IrisY` and `Size` values are cached; weights are
  recalculated only when a source or relevant configuration changes.
- Managed eye-adjustment weights are written only when needed and verified on a
  staggered 64-frame cadence to recover from external writers without a
  constant write war.
- Expression FBS controllers and trigger selectors are resolved once per bind.
- The three live FBS dictionaries are sampled without reflection or managed
  allocations in the frame loop.
- Visibility writes occur only when trigger/manual state changes.
- Closed Quick Settings avoids automatic GUILayout work.

## Diagnostics

Press the configured diagnostics shortcut (default `Left Shift + F10`) or use
the **Diagnostics** button. Reports are written under:

```text
BepInEx\config\KK_EyeMotion\diagnostics
```

Reports include sampled gaze/IrisY/Size values, applied weights, renderer
binding details, optional-channel status, manual/automatic visibility state,
resolved expression selectors and weights, highlight synchronization, and
card-persistence status.

## Building, testing, and packaging

```bat
build-release.bat
run-tests.bat
```

After a successful Release build, create the clean public archive:

```bat
package-release.bat
```

The packager validates the assembly version and every ZIP entry, excludes
configuration/dependency/PDB files, and creates:

```text
dist\KK_EyeMotion-v0.4.1.zip
dist\KK_EyeMotion-v0.4.1.zip.sha256
```

For local deployment, `deploy-release.bat` backs up the previous DLL before
copying the new one. The PDB is copied only with its explicit development
option.

## Current limitations

- Headmods must author the five required movement/blink shapes and whichever
  optional IrisY/Size or visibility shapes they want to support.
- Blink is joint; asymmetric wink channels are not implemented.
- IrisY/Size shapes must be authored to combine acceptably with gaze
  and blink; EyeMotion cannot repair incompatible vertex deltas.
- Expression triggers follow Koikatsu FBS pattern weights. An arbitrary shape
  that is not referenced by a brow/eyes/mouth Close/Open pattern cannot be a
  source trigger.
- `Renderer.enabled = true` cannot reveal an inactive GameObject or inactive
  parent. EyeMotion deliberately does not call `GameObject.SetActive`.
- There is no global Studio scene payload or headmod-profile system.
- KoikatuVR is outside the declared compatibility scope.

See [MANUAL_TESTS_0.4.0.md](MANUAL_TESTS_0.4.0.md) for the release test plan.
