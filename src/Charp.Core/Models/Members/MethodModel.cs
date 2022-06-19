﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using LoxSmoke.DocXml;

namespace Charp.Core.Models.Members
{
    public class MethodModel : Model<MethodInfo, CommonComments>, IFunctional, IMemberable
    {
        public override string Type => "method";
        public string ReturnType => Meta.ReturnType.ToString();
        public Parameter[] Parameters { get; set; }
        public bool IsVirtual => Meta.IsVirtual && !IsAbstract;        
        public bool IsAbstract => Meta.IsAbstract;
        public bool IsStatic => Meta.IsStatic;

        #region IMemberable
        public bool IsPublic => Meta.IsPublic;
        public bool IsProtected => Meta.IsFamily || Meta.IsFamilyOrAssembly;
        public bool IsInternal => Meta.IsAssembly || Meta.IsFamilyOrAssembly;
        #endregion

        public MethodModel(MethodInfo member) : base(member)
            => Parameters = ((IFunctional)this).GetParameters(member);        
    }
}