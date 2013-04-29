﻿//  PlantUML Studio
//  Copyright 2013 Matthew Hamilton - matthamilton@live.com
//  Copyright 2010 Omar Al Zabir - http://omaralzabir.com/ (original author)
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using ICSharpCode.AvalonEdit.Snippets;

namespace PlantUmlStudio.Model.Snippets
{
	/// <summary>
	/// Represents a diagram code snippet.
	/// </summary>
	public class CodeSnippet
	{
		/// <summary>
		/// Creates a new snippet.
		/// </summary>
		/// <param name="name">The name of the snippet</param>
		/// <param name="category">The snippet category</param>
		/// <param name="code">The snippet code structure</param>
		public CodeSnippet(string name, string category, Snippet code)
		{
			Name = name;
			Category = category;
			Code = code;
		}

		/// <summary>
		/// The name of the category.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The snippet category.
		/// </summary>
		public string Category { get; private set; }

		/// <summary>
		/// The snippet code.
		/// </summary>
		public Snippet Code { get; private set; }
	}
}