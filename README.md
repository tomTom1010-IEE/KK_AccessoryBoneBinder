# KKS_AccessoryBoneBinder

KKS_AccessoryBoneBinder binds accessory bones marked with ModBoneImplantor's `BoneImplantProcess` directly to matching body bones at runtime, without requiring AccessoryClothes.

In Unity, add `BoneImplantProcess` to the accessory asset, set `trfSrc` to the accessory dynamic-bone root, and set `trfDst` to a placeholder body bone with the same name as the KKS body bone to bind to.
