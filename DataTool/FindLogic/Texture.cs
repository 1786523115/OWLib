﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using OWLib;
using STULib.Types;
using STULib.Types.Generic;
using static DataTool.Helper.STUHelper;
using OWLib.Types;
using static DataTool.Helper.IO;
using static DataTool.Program;

namespace DataTool.FindLogic {
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class TextureInfo : IEquatable<TextureInfo> {
        public Common.STUGUID GUID;
        public Common.STUGUID DataGUID;
        public string Name;
        public ulong MaterialID;
        internal string DebuggerDisplay => $"{GUID.ToString()}{(DataGUID != null ? $" - {DataGUID.ToString()}" : "")}";

        public bool Equals(TextureInfo other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(GUID, other.GUID) && Equals(DataGUID, other.DataGUID) && MaterialID == other.MaterialID;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextureInfo) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (GUID != null ? GUID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DataGUID != null ? DataGUID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MaterialID.GetHashCode();
                return hashCode;
            }
        }
    }

    public class FoundTextures {
        public Dictionary<ulong, List<TextureInfo>> Textures;
        public List<ulong> Done;
    }
    
    public static class Texture {
        public static void AddGUID(Dictionary<ulong, List<TextureInfo>> textures, Common.STUGUID mainKey, Common.STUGUID dataKey, ulong parentKey, string name=null, bool forceZero=false, ulong materialId = 0) {
            if (mainKey == null) return;
            if (forceZero) parentKey = 0;
            if (!textures.ContainsKey(parentKey)) {
                textures[parentKey] = new List<TextureInfo>();
            }

            TextureInfo newTexture = new TextureInfo {
                GUID = mainKey,
                DataGUID = dataKey,
                Name = name,
                MaterialID = materialId
            };

            if (!textures[parentKey].Contains(newTexture)) {
                textures[parentKey].Add(newTexture);
            }
        }

        public static Dictionary<ulong, List<TextureInfo>> FindTextures(Dictionary<ulong, List<TextureInfo>> existingTextures, STUDecalReference decal, string name = null, bool forceZero = false) {
            return FindTextures(existingTextures, decal.DecalResource, name, forceZero);
        }
        
        public static Dictionary<ulong, List<TextureInfo>> FindTextures(Dictionary<ulong, List<TextureInfo>> existingTextures, Common.STUGUID textureGUID, string name=null, bool forceZero=false, Dictionary<ulong, ulong> replacements = null, ulong materialId = 0) {
            if (existingTextures == null) {
                existingTextures = new Dictionary<ulong, List<TextureInfo>>();
            }

            if (textureGUID == null) return existingTextures;
            if (replacements == null) replacements = new Dictionary<ulong, ulong>();
            if (replacements.ContainsKey(textureGUID)) textureGUID = new Common.STUGUID(replacements[textureGUID]);

            switch (GUID.Type(textureGUID)) {
                case 0xB3:
                    ImageDefinition def = new ImageDefinition(OpenFile(textureGUID));
                    foreach (ImageLayer layer in def.Layers) {
                        FindTextures(existingTextures, new Common.STUGUID(layer.key), name, forceZero, replacements, materialId);
                    }
                    break;
                case 0xA8:
                    STUDecal decal = GetInstance<STUDecal>(textureGUID);
                    if (decal == null) break;
                    foreach (Common.STUGUID decalMaterial in decal.Materials) {
                        if (!Files.ContainsKey(decalMaterial)) continue;
                        existingTextures = FindTextures(existingTextures, decalMaterial, name, forceZero, replacements);
                    }
                    break;
                case 0x04:
                    ulong dataKey = (textureGUID & 0xFFFFFFFFUL) | 0x100000000UL | 0x0320000000000000UL;
                    AddGUID(existingTextures, textureGUID,
                        Files.ContainsKey(dataKey) ? new Common.STUGUID(dataKey) : null, 0, name, forceZero, materialId);
                    break;
                case 0x08:
                    Material material = new Material(OpenFile(textureGUID), 0);
                    existingTextures = FindTextures(existingTextures, new Common.STUGUID(material.Header.definitionKey), null, forceZero, replacements, materialId);
                    break;
                case 0x1A:
                    STUModelLook modelLook = GetInstance<STUModelLook>(textureGUID);
                    foreach (STUModelLook.Material modelLookMaterial in modelLook.Materials) {
                        existingTextures = FindTextures(existingTextures, modelLookMaterial.MaterialReference, null,
                            forceZero, replacements, modelLookMaterial.Id);
                    }
                    // foreach (STUModelLook.MaterialReferenceWrapper modelLookWrapper in modelLook.Wrappers) {
                    //     foreach (STUModelLook.Material refMaterial in modelLookWrapper.Materials) {
                    //         existingTextures = FindTextures(existingTextures, refMaterial.MaterialReference, null,
                    //             forceZero, replacements);
                    //     }
                    //     
                    // }
                    break;
                case 0x03:
                    // STUStatescriptComponentMaster container = GetInstance<STUStatescriptComponentMaster>(textureGUID);
                    //  foreach (KeyValuePair<ulong, STUStatescriptComponent> statescriptComponent in container.Components) {
                    //      STUStatescriptComponent component = statescriptComponent.Value;
                    //      if (component == null) continue;
                    //      if (component is STUModelComponent) {
                    //          STUModelComponent modelComponent = component as STUModelComponent;
                    //          existingTextures = FindTextures(existingTextures, modelComponent.Material);
                    //      }
                    // }
                    break;
                default:
                    Debugger.Log(0, "DataTool.FindLogic.Texture", $"[DataTool.FindLogic.Texture] Unhandled type: {GUID.Type(textureGUID):X3}\n");
                    break;
            }

            return existingTextures;
        }
    }
}