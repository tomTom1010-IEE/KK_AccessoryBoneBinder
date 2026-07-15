# KK_AccessoryBoneBinder

KK_AccessoryBoneBinder is a BepInEx plugin for Koikatsu that attaches selected accessory bone chains directly to matching character body bones at runtime. It uses `BoneImplantProcess` markers authored with ModBoneImplantor and does not require AccessoryClothes.

This is useful for tails, ribbons, hair-like accessories, hanging parts, and other accessory bone chains that must follow a body bone instead of remaining under the accessory `N_move` hierarchy.

## Requirements

- Koikatsu
- BepInEx
- KKAPI / KoikatuAPI (required, no minimum version is declared)
- ModBoneImplantor (required, no minimum version is declared)
- KKABMX (optional, recommended for editable and saved bone offsets)
- Coordinate Load Option (optional)

## Installation

Place `KK_AccessoryBoneBinder.dll` in:

```text
BepInEx/plugins/
```

Install KKAPI and ModBoneImplantor as well. ABMX and Coordinate Load Option are soft dependencies: the basic bone attachment still works when they are absent.

## Unity Setup

The plugin has no in-game setup UI. Each binding is authored in the accessory asset before export.

1. Add ModBoneImplantor's `BoneImplantProcess` component to the accessory prefab.
2. Assign `trfSrc` to the root of the accessory bone chain that should follow the character body.
3. Create or select a placeholder Transform and assign it to `trfDst`.
4. Rename the placeholder to exactly match the target KK body bone, including letter case.
5. Export and register the accessory normally.

Example:

```text
trfSrc: tail_root
trfDst: cf_j_waist02
```

At runtime, `tail_root` is re-parented to the real `cf_j_waist02` of the current character. Multiple `BoneImplantProcess` components may be used when one accessory needs several independent bindings.

Do not assign `trfSrc` and `trfDst` to the same Transform. `trfSrc` should be the custom chain root, not the accessory's `N_move` control object.

## In-Game Behavior

The plugin automatically scans and rebinds accessories after accessory changes, character reloads, and coordinate changes. Removing the accessory lets the game destroy its original objects normally.

The binding itself is derived from the accessory asset and is not saved as separate card data. With ABMX installed, the plugin creates a stable slot-aware alias such as `ABB_S01_tail_root` and keeps its modifier per-coordinate. This allows ABMX to save, restore, copy, and continue editing the bound bone after the accessory is reloaded.

### Editing with ABMX

1. Load the prepared accessory in Maker.
2. Open ABMX and find the `ABB_Sxx_...` bone entry.
3. Edit its position, rotation, length, or scale.
4. Save the character card or coordinate card normally.
5. Reload the card and verify the modifier is still applied.

Accessory slot copy and transfer operations copy the matching ABMX modifier to the destination slot. Coordinate copy operations copy the selected slot modifiers to the destination coordinate. Older modifiers stored under the original unaliased bone name are migrated when possible.

When Coordinate Load Option extracts ABMX data from a coordinate card, the plugin performs a synchronous rebind first so the accessory modifiers can be transferred correctly in Replace Mode.

In Studio, KKPE does not read this plugin's data directly. ABMX first applies the saved modifier to the bound Transform; KKPE then observes the resulting bone transform normally.

## Limitations

- This plugin re-parents Transform bone chains. It does not redirect `SkinnedMeshRenderer.bones` or transfer skin weights.
- Target body bone names must match exactly.
- ABMX is required if runtime bone offsets must be edited and persisted. Without ABMX, only the asset-defined attachment is performed.
- Compatibility is intended for ordinary accessories and does not depend on AccessoryClothes.

## Troubleshooting

Check `output_log.txt` or the BepInEx log when a binding does not appear.

- `trfSrc` or `trfDst` is missing: recheck the exported `BoneImplantProcess` component.
- Target body bone not found: verify the placeholder name and capitalization.
- The bone attaches but saved offsets do not apply: verify KKABMX is loaded and edit the `ABB_Sxx_...` entry.
- Coordinate card data is missing: use Coordinate Load Option Replace Mode and include accessories/ABMX in the selection.

## Building

The project targets .NET Framework 3.5.

```powershell
dotnet build KK_AccessoryBoneBinder.csproj -c Release
```

Local game assemblies and plugin dependencies are expected under `dlls/` and are not committed.

## License

Licensed under the GNU GPL-3.0 license.
