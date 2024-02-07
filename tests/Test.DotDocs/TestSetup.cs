﻿using DotDocs.IO.Routing;
using DotDocs.IO;
using DotDocs.Markdown;
using DotDocs;
using DotDocs.Models;
using System.Text;
using Test.Source.One;

namespace Test.DotDocs
{
    /// <summary>
    /// Provides state used by tests.
    /// </summary>
    [TestClass]
    public static class TestSetup
    {
        public static Builder? Builder { get; private set; }

        // public static MarkdownRenderer Renderer { get; private set; }       

        public static Builder GetBuilder()
        {
            var builder = Builder.FromPath(
                "../../../../data/TestData",
                new MarkdownRenderer(
                    new TextFileOutput(
                        "docs",
                        new FlatRouter(),
                        ".md")));
            builder.Prepare();
            builder.Build();
            return builder;
        }        

        [AssemblyInitialize]
        public static void Init(TestContext _)
        {
            Builder = GetBuilder();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Builder?.Dispose();
        }
    }
}