﻿// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
//
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions
//

in vec2 TexCoord0;

uniform vec4 Tint;
uniform sampler2D Texture;

out vec4 OutFragColor;

void main()
{
	OutFragColor = texture(Texture, TexCoord0) * Tint;
}
