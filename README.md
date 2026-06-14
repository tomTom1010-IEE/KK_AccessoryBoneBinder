# KK_AccessoryBoneBinder

KK_AccessoryBoneBinder binds accessory bones marked with ModBoneImplantor's `BoneImplantProcess` directly to matching body bones at runtime, without requiring AccessoryClothes.

In Unity, add `BoneImplantProcess` to the accessory asset, set `trfSrc` to the accessory dynamic-bone root, and set `trfDst` to a placeholder body bone with the same name as the KK body bone to bind to.
