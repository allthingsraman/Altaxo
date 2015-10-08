﻿// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Altaxo.Gui.Graph3D.Common
{
	using SharpDX.Direct3D10;
	using System;

	/// <summary>
	/// Designates a location, where a scene can be rendered to.
	/// </summary>
	public interface ISceneHost
	{
		Device Device { get; }
		Altaxo.Graph.PointD2D HostSize { get; }
	}

	/// <summary>
	/// Scene.
	/// </summary>
	public interface IScene
	{
		/// <summary>
		/// Attaches the scene to the specified scene host.
		/// </summary>
		/// <param name="host">The scene host.</param>
		void Attach(ISceneHost host);

		/// <summary>
		/// Detaches this scene from the scene host.
		/// </summary>
		void Detach();

		/// <summary>
		/// Updates the scene, taking into account the specified time.
		/// </summary>
		/// <param name="timeSpan">The current scene time.</param>
		void Update(TimeSpan timeSpan);

		/// <summary>
		/// Renders this scene to the scene host.
		/// </summary>
		void Render();
	}
}