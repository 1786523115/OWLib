﻿using System;
using System.Collections.Generic;
using System.IO;
using CASCLib;
using OWLib;
using OWLib.Types.STUD;

namespace OverTool {
    class DumpVoice : IOvertool {
        public string Help => "output [query]";
        public uint MinimumArgs => 1;
        public char Opt => 'v';
        public string FullOpt => "npc-voice";
        public string Title => "Extract NPC Voice";
        public ushort[] Track => new ushort[1] { 0x75 };
        public bool Display => true;

        public static void Save(string path, Dictionary<ulong, List<ulong>> sounds, Dictionary<ulong, Record> map, CASCHandler handler, bool quiet, Dictionary<ulong, ulong> replace = null, HashSet<ulong> done = null) {
            if (done == null) {
                done = new HashSet<ulong>();
            }
            foreach (KeyValuePair<ulong, List<ulong>> pair in sounds) {
                string rootOutput = $"{path}{GUID.LongKey(pair.Key):X12}{Path.DirectorySeparatorChar}";
                foreach (ulong key in pair.Value) {
                    if (!done.Add(key)) {
                        continue;
                    }
                    ulong typ = GUID.Type(key);
                    string ext = "wem";
                    if (typ == 0x043) {
                        ext = "bnk";
                    }
                    if (!Directory.Exists(rootOutput)) {
                        Directory.CreateDirectory(rootOutput);
                    }
                    string outputPath = $"{rootOutput}{GUID.LongKey(key):X12}.{ext}";
                    using (Stream soundStream = Util.OpenFile(map[key], handler)) {
                        if (soundStream == null) {
                            //Console.Out.WriteLine("Failed to dump {0}, probably missing key", ooutputPath);
                            continue;
                        }
                        using (Stream outputStream = File.Open(outputPath, FileMode.Create)) {
                            Util.CopyBytes(soundStream, outputStream, (int)soundStream.Length);
                            if (!quiet) {
                                Console.Out.WriteLine("Wrote file {0}", outputPath);
                            }
                        }
                    }
                }
            }
        }

        public void Parse(Dictionary<ushort, List<ulong>> track, Dictionary<ulong, Record> map, CASCHandler handler, bool quiet, OverToolFlags flags) {
            string output = flags.Positionals[2];

            List<string> heroes = new List<string>();
            if (flags.Positionals.Length > 3) {
                heroes.AddRange(flags.Positionals[3].ToLowerInvariant().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries));
            }
            bool heroAllWildcard = heroes.Count == 0 || heroes.Contains("*");

            List<ulong> masters = track[0x75];
            foreach (ulong masterKey in masters) {
                if (!map.ContainsKey(masterKey)) {
                    continue;
                }
                STUD masterStud = new STUD(Util.OpenFile(map[masterKey], handler));
                if (masterStud.Instances == null || masterStud.Instances[0] == null) {
                    continue;
                }
                HeroMaster master = (HeroMaster)masterStud.Instances[0];
                if (master == null) {
                    continue;
                }
                string heroName = Util.GetString(master.Header.name.key, map, handler);
                if (heroName == null) {
                    continue;
                }
                if (!heroes.Contains(heroName.ToLowerInvariant())) {
                    if (!heroAllWildcard) {
                        continue;
                    }
                }
                if (master.Header.itemMaster.key != 0) { // AI
                    InventoryMaster inventory = Extract.OpenInventoryMaster(master, map, handler);
                    if (inventory.ItemGroups.Length > 0 || inventory.DefaultGroups.Length > 0) {
                        continue;
                    }
                }
                Console.Out.WriteLine("Dumping voice bites for NPC {0}", heroName);
                Dictionary<ulong, List<ulong>> soundData = ExtractLogic.Sound.FindSounds(master, track, map, handler, null, masterKey);
                string path = string.Format("{0}{1}{2}{1}{3}{1}", output, Path.DirectorySeparatorChar, Util.Strip(Util.SanitizePath(heroName)), "Sound Dump");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                Save(path, soundData, map, handler, quiet);
            }
        }
    }
}
