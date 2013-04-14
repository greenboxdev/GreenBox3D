﻿// CompiledPass.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenBox3D.Graphics.Detail
{
    public class CompiledPass
    {
        public CompiledPass(string glslVertexCode, string glslPixelCode, string hlslVertexCode, string hlslPixelCode)
        {
            GlslVertexCode = glslVertexCode;
            GlslPixelCode = glslPixelCode;
            HlslVertexCode = hlslVertexCode;
            HlslPixelCode = hlslPixelCode;
        }

        public string GlslVertexCode { get; private set; }
        public string GlslPixelCode { get; private set; }
        public string HlslVertexCode { get; private set; }
        public string HlslPixelCode { get; private set; }
    }
}
