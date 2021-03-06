﻿using System;
using System.Collections.Generic;
using System.IO;
using CASCLib;
using OWLib;
using OWLib.Types.STUD;
using OWLib.Types;
using OWLib.Types.STUD.Binding;
using OverTool.ExtractLogic;
using OWLib.Writer;

namespace OverTool.List {
    class ExtractLootbox : IOvertool {
        public string Help => "output";
        public uint MinimumArgs => 0;
        public char Opt => 'L';
        public string FullOpt => "lootbox";
        public string Title => "Extract Lootboxes";
        public ushort[] Track => new ushort[1] { 0xCF };
        public bool Display => true;

        public void Parse(Dictionary<ushort, List<ulong>> track, Dictionary<ulong, Record> map, CASCHandler handler, bool quiet, OverToolFlags flags) {
            Console.Out.WriteLine();
            foreach (ulong master in track[0xCF]) {
                if (!map.ContainsKey(master)) {
                    continue;
                }
                STUD lootbox = new STUD(Util.OpenFile(map[master], handler));
                if (lootbox.Instances == null) {
                    continue;
                }
                Lootbox box = lootbox.Instances[0] as Lootbox;
                if (box == null) {
                    continue;
                }

                Extract(box.Master.model, box, track, map, handler, quiet, flags);
                Extract(box.Master.alternate, box, track, map, handler, quiet, flags);
            }
        }

        private void Extract(ulong model, Lootbox lootbox, Dictionary<ushort, List<ulong>> track, Dictionary<ulong, Record> map, CASCHandler handler, bool quiet, OverToolFlags flags) {
            if (model == 0 || !map.ContainsKey(model)) {
                return;
            }

            string output = $"{flags.Positionals[2]}{Path.DirectorySeparatorChar}{Util.SanitizePath(lootbox.EventName)}{Path.DirectorySeparatorChar}";

            STUD stud = new STUD(Util.OpenFile(map[model], handler));

            HashSet<ulong> models = new HashSet<ulong>();
            Dictionary<ulong, ulong> animList = new Dictionary<ulong, ulong>();
            HashSet<ulong> parsed = new HashSet<ulong>();
            Dictionary<ulong, List<ImageLayer>> layers = new Dictionary<ulong, List<ImageLayer>>();
            Dictionary<ulong, ulong> replace = new Dictionary<ulong, ulong>();
            Dictionary<ulong, List<ulong>> sound = new Dictionary<ulong, List<ulong>>();

            foreach (ISTUDInstance inst in stud.Instances) {
                if (inst == null) {
                    continue;
                }
                if (inst.Name == stud.Manager.GetName(typeof(ComplexModelRecord))) {
                    ComplexModelRecord r = (ComplexModelRecord)inst;
                    ulong modelKey = r.Data.model.key;
                    models.Add(modelKey);
                    Skin.FindAnimations(r.Data.animationList.key, sound, animList, replace, parsed, map, handler, models, layers, modelKey);
                    Skin.FindAnimations(r.Data.secondaryAnimationList.key, sound, animList, replace, parsed, map, handler, models, layers, modelKey);
                    Skin.FindTextures(r.Data.material.key, layers, replace, parsed, map, handler);
                }
            }

            Skin.Save(null, output, "", "", replace, parsed, models, layers, animList, flags, track, map, handler, model, false, quiet, sound, 0);
        }
    }
}