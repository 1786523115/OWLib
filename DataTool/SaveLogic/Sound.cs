﻿using System.Collections.Generic;
using System.IO;
using DataTool.FindLogic;
using DataTool.Flag;
using DataTool.ToolLogic.Extract;
using OWLib;
using static DataTool.Helper.IO;

namespace DataTool.SaveLogic {
    public class Sound {
        public static void Save(ICLIFlags flags, string path, Dictionary<ulong, List<SoundInfo>> sounds) {
            bool convertWem = false;
            bool convertBnk = false;
            if (flags is ExtractFlags extractFlags) {
                convertWem = extractFlags.ConvertWem;
                // convertBnk = extractFlags.ConvertBnk;
                convertBnk = false;
            }

            foreach (KeyValuePair<ulong,List<SoundInfo>> pair in sounds) {
                string rootOutput = Path.Combine(path, GUID.LongKey(pair.Key).ToString("X12")) + Path.DirectorySeparatorChar;
                foreach (SoundInfo sound in pair.Value) {
                    ulong typ = GUID.Type(sound.GUID);
                    string ext = "wem";
                    if (typ == 0x043) {
                        ext = "bnk";
                    }
                    
                    string outputPath = $"{rootOutput}{GUID.LongKey(sound.GUID):X12}.{ext}";
                    string outputPathOgg = $"{rootOutput}{GUID.LongKey(sound.GUID):X12}.ogg";
                    CreateDirectoryFromFile(outputPath);
                    if (ext == "wem") {
                        using (Stream soundStream = OpenFile(sound.GUID)) {
                            if (soundStream == null) continue;
                            using (Stream outputStream = File.OpenWrite(outputPath)) {
                                soundStream.CopyTo(outputStream);
                            }
                        }
                        if (convertWem) {
                            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                            pProcess.StartInfo.FileName = "Third Party\\ww2ogg.exe";
                            pProcess.StartInfo.Arguments = $"\"{outputPath}\" --pcb \"Third Party\\packed_codebooks_aoTuV_603.bin\" -o \"{outputPathOgg}\"";
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.Start();
                            pProcess.WaitForExit();                            
                            System.Diagnostics.Process pProcess2 = new System.Diagnostics.Process();
                            pProcess2.StartInfo.FileName = "Third Party\\revorb.exe";
                            pProcess2.StartInfo.Arguments = $"\"{outputPathOgg}\"";
                            pProcess2.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess2.Start();
                            pProcess.WaitForExit();
                            File.Delete(outputPath);
                        }
                    }

                    if (ext == "bnk") {
                        using (Stream soundStream = OpenFile(sound.GUID)) {
                            if (soundStream == null) continue;
                            using (Stream outputStream = File.OpenWrite(outputPath)) {
                                soundStream.CopyTo(outputStream);
                            }
                        }
                        // todo: I'm not going to add conversion here because no OW bnks have wems (yet)
                        // if (convertBnk) { }
                    }


                    // CreateDirectoryFromFile(outputPath);
                    // using (Stream soundStream = OpenFile(sound)) {
                    //     // ConvertLogic.Sound.WwiseRIFFVorbis vorbis = new ConvertLogic.Sound.WwiseRIFFVorbis(soundStream, "");
                    //     if (soundStream == null) continue;
                    //     using (Stream outputStream = File.OpenWrite(outputPath)) {
                    //         // Stream vorbisStream = vorbis.ConvertToOgg();
                    //         // vorbisStream.Position = 0;
                    //         // vorbisStream.CopyTo(outputStream);
                    //         // vorbisStream.Dispose();
                    //     }
                    // }
                }
            }
        }
    }
}