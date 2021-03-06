// File auto generated by STUHashTool

using System.Collections.Generic;
using System.Linq;
using static STULib.Types.Generic.Common;

namespace STULib.Types {
    [STU(0x47C34433)]
    public class STU_47C34433 : STUInstance {
        [STUField(0x2D124BE2)]
        public STUGUID m_2D124BE2;  // STU_871BD3D0

        [STUField(0x2F08CD5E)]
        public STU_D9E3E761[] m_2F08CD5E;
    }
    
    [STU(0xD9E3E761)]
    public class STU_D9E3E761 : STUInstance {
        [STUField(0x0CC61A13)]
        public STUGUID m_0CC61A13;  // STU_CBD8CDF3

        [STUField(0xE101F943)]
        public byte m_E101F943;
    }
    
    [STU(0xB7EEA3BE)]
    public class STUDataOverride : STUOverrideBase {
        [STUField(0xAA8E1BB0)]
        public STUGUID[] m_AA8E1BB0;  // STU_0A29DB0D

        [STUField(0xBCC55571)]
        public STU_47C34433[] m_BCC55571;

        [STUField(0x258A7D5C)]
        public STUHashMap<STUOverrideFileChange> Replacements;  // key is before

        public Dictionary<STUGUID, STUGUID> ProperReplacements => Replacements?.ToDictionary(x => new STUGUID(x.Key), x => x.Value.New);
    }
}
