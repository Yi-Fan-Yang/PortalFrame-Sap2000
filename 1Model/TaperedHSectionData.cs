using System;
using System.Collections.Generic;
using System.Linq;
namespace PortalFrame._1Model
{
    // 变截面定义（小端+大端+插值方式）
    public class TaperedSectionData : SectionBase
    {
        public HSectionData Start=null!;
        public HSectionData End = null!;
        public int EI33Type, EI22Type;
    }
}
