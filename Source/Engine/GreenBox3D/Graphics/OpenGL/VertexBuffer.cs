﻿// VertexBuffer.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions
#if DESKTOP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GreenBox3D.Graphics
{
    public sealed class VertexBuffer : HardwareBuffer
    {
        public VertexDeclaration VertexDeclaration { get; private set; }

        public VertexBuffer(VertexDeclaration vertexDeclaration, BufferUsage usage)
            : base(BufferTarget.ArrayBuffer, usage)
        {
            VertexDeclaration = vertexDeclaration;
        }

        public VertexBuffer(Type elementType, BufferUsage usage)
            : this(CreateVertexDeclaration(elementType), usage)
        {
        }

        private static VertexDeclaration CreateVertexDeclaration(Type elementType)
        {
            IVertexType vertexType = Activator.CreateInstance(elementType) as IVertexType;

            if (vertexType == null)
                throw new ArgumentException("elementType must implement IVertexType", "elementType");

            return vertexType.VertexDeclaration;
        }
    }
}

#endif
