﻿using Docsharp.Test.Data.Enumerations;
using Docsharp.Test.Data.Interfaces;

namespace Docsharp.Test.Data.Classes
{
    /// <summary>
    /// The most common type of boat used in medium to large size bodies of water.
    /// </summary>
    public class Runabout : Boat, IPowerable
    {
        public EngineSize Engine { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int EngineCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
