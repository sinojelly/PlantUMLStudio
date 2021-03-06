//  PlantUML Studio
//  Copyright 2016 Matthew Hamilton - matthamilton@live.com
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using SharpEssentials.Collections;

namespace PlantUmlStudio.Controls.Behaviors.AvalonEdit.Folding
{
	/// <summary>
	/// Creates folding regions based on start and end pattern definitions.
	/// </summary>
	public class PatternBasedFoldingStrategy : IFoldingStrategy
	{
		/// <summary>
		/// Initializes a new folding strategy.
		/// </summary>
		/// <param name="foldRegions">The patterns that define fold regions</param>
		public PatternBasedFoldingStrategy(IEnumerable<FoldedRegionDefinition> foldRegions)
		{
			int counter = 1;
			_tokens = foldRegions.ToDictionary(region => counter++.ToString(), region => region);

		    _startTokens = new Regex(_tokens.Select(t => $"(?<{t.Key}>{t.Value.StartPattern})").ToDelimitedString("|"),
		                             RegexOptions.ExplicitCapture);
		}

		#region Implementation of IFoldingStrategy

		/// <see cref="IFoldingStrategy.CreateNewFoldings"/>
		public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			var foldings = new List<NewFolding>();

			var openRegions = new Stack<PotentialFoldRegion>();
			foreach (var line in document.Lines.Where(l => l.Length > 0))	// Filter out empty lines.
			{
				var lineText = document.GetText(line.Offset, line.Length);	// Get the line's text content.

				// Determine if there is a match for a start token.
				var startMatch = TryMatchStartToken(lineText);
				if (startMatch != null)
				{
					var foldDefinition = startMatch.Item1;
					var match = startMatch.Item2;
					if (match.Success && !Regex.IsMatch(lineText, foldDefinition.EndPattern))
					{
						int startOffset = line.Offset + match.Index;
						string foldedDisplay = lineText.Substring(match.Index, Math.Min(lineText.Length, 15));

						// If the start pattern specifies an identifier that must be included in the end pattern,
						// construct the end pattern using this identifier.
						string identifier = match.Groups["id"].Success ? match.Groups["id"].Value + @"($|\s+)" : string.Empty;

						openRegions.Push(new PotentialFoldRegion(startOffset, foldedDisplay)
						{
							StartToken = foldDefinition.StartPattern,
							EndToken = foldDefinition.EndPattern + identifier
						});
					}
				}

				if (openRegions.Count > 0)
				{
					if (Regex.IsMatch(lineText, openRegions.Peek().EndToken))
					{
						var region = openRegions.Pop();
						foldings.Add(new NewFolding(region.StartOffset, line.Offset + line.Length) { Name = $"{region.StartLine}..." });
					}
				}
			}
			return foldings.OrderBy(f => f.StartOffset);
		}

		#endregion

		private Tuple<FoldedRegionDefinition, Match> TryMatchStartToken(string input)
		{
			var matches = _startTokens.Matches(input);
			return (matches.Count > 0 && matches[0].Success)
                        ? _tokens.Keys
			                     .Where(groupName => matches[0].Groups[groupName].Success)
			                     .Select(groupName => Tuple.Create(_tokens.TryGetValue(groupName).Value, matches[0]))
			                     .FirstOrDefault()
                        : null;
		}

		private readonly IDictionary<string, FoldedRegionDefinition> _tokens;

		/// <summary>
		/// A pattern that can match any of the start tokens.
		/// </summary>
		private readonly Regex _startTokens;

		private sealed class PotentialFoldRegion
		{
			public PotentialFoldRegion(int startOffset, string startLine)
			{
				StartOffset = startOffset;
				StartLine = startLine;
			}

			public string StartToken { get; set; }
			public string EndToken { get; set; }

			public int StartOffset { get; }
			public string StartLine { get; }
		}
	}
}