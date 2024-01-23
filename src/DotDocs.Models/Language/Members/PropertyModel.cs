﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotDocs.Models.Language.Members
{
    public class PropertyModel : MemberModel
    {
        public string Name { get; set; }

        public ITypeable PropertyType { get; set; }

        public PropertyModel(
            PropertyInfo info,
            ImmutableDictionary<string, AssemblyModel> assemblies,
            Dictionary<string, ITypeable> types
            ) {
            Name = info.Name;
            PropertyType = ITypeable.GetOrCreateTypeFrom(info.PropertyType, assemblies, types);
        }
    }
}
