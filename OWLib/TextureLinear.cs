﻿using System;
using System.IO;
using OWLib.Types;
using System.Drawing;
using System.Drawing.Imaging;

namespace OWLib {
  public class TextureLinear {
    private TextureHeader header;
    private DXGI_PIXEL_FORMAT format;
    private uint size;
    private bool loaded = false;
    private byte[] data;

    public TextureHeader Header => header;
    public DXGI_PIXEL_FORMAT Format => format;
    public uint Size => size;
    public byte[] Data => data;
    public bool Loaded => loaded;

    public void Save(Stream output) {
      using(BinaryWriter ddsWriter = new BinaryWriter(output)) {
        DDSHeader dds = header.ToDDSHeader();
        ddsWriter.Write(dds);
        if(dds.format.fourCC == 808540228) {
          DDS_HEADER_DXT10 d10 = new DDS_HEADER_DXT10 {
            format = (uint)header.format,
            dimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D,
            misc = 0,
            size = 1,
            misc2 = 0
          };
          ddsWriter.Write(d10);
        }
        ddsWriter.Write(data, 0, (int)header.dataSize);
      }
    }

    public TextureLinear(Stream imageStream) {
      using(BinaryReader imageReader = new BinaryReader(imageStream)) {
        header = imageReader.Read<TextureHeader>();
        if(header.dataSize == 0) {
          return;
        }
        size = header.dataSize;
        format = header.format;
        
        imageStream.Seek(128, SeekOrigin.Begin);
        data = new byte[header.dataSize];
        imageStream.Read(data, 0, (int)header.dataSize);
      }
      loaded = true;
    }
  }
}