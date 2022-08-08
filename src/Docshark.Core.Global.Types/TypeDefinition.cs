﻿using LoxSmoke.DocXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docshark.Core.Global.Types
{
    public class TypeDefinition
    {
        public string Id { get; set; }
        public string? Parent { get; set; }
        public List<string> TypeArguments { get; set; } = new();
        public string Namespace { get; set; }
        public string AssemblyPath { get; set; }
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public CommonComments Comments { get; set; }
        
        public static TypeDefinition From(Type info)
            => new()
            {
                Id = info.ToString(),
                Namespace = info.Namespace,
                TypeName = info.Name,
                Parent = info.BaseType?.ToString(),
                AssemblyPath = info.Module.FullyQualifiedName,
                AssemblyName = info.Module.Name
            };
    }
}