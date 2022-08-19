﻿using LoxSmoke.DocXml;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DotDocs.Core.Models.Language.Members
{
    public class MemberModel<T1, T2> : Model
        where T1 : MemberInfo
        where T2 : CommonComments
    {
        [JsonIgnore]
        public T1 Info { get; set; }

        public T2 Comments { get; set; }

        public override string Name => Info.Name;

        protected MemberModel(T1 info)
            => Info = info;
    }
}