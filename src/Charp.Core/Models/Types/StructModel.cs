﻿using System.Reflection;

using Charp.Core.Models.Members;
using LoxSmoke.DocXml;

namespace Charp.Core.Models.Types
{
    public class StructModel : TypeMember<TypeInfo, TypeComments>, IMemberContainable
    {
        public const string STRUCT_TYPE_STRING = "struct";
        public override bool CanHaveInternalTypes => true;
        public override string Type => STRUCT_TYPE_STRING;

        public PropertyModel[] Properties { get; set; }
        public FieldModel[] Fields { get; set; }
        public MethodModel[] Methods { get; set; }
        public EventModel[] Events { get; set; }

        public StructModel(TypeInfo member, DocXmlReader reader) : base(member)
        {
            IMemberContainable.Init(this, member, reader);
        }
    }
}