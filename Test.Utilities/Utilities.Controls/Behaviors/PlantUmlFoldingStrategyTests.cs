using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using Utilities.Controls.Behaviors;
using Xunit;
using Xunit.Extensions;

namespace Unit.Tests.Utilities.Controls.Behaviors
{
	public class PlantUmlFoldingStrategyTests
	{
		[Theory]
		[PropertyData("FoldingTestData")]
		public void Test_CreateNewFoldings(string input, IList<NewFolding> expected)
		{
			// Arrange.
			var document = new TextDocument(input);

			// Act.
			int errorOffset;
			var actual = foldingStrategy.CreateNewFoldings(document, out errorOffset).ToList();

			// Assert.
			Assert.Equal(expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				var expectedFolding = expected[i];
				var actualFolding = actual[i];

				Assert.Equal(expectedFolding.StartOffset, actualFolding.StartOffset);
				Assert.Equal(expectedFolding.EndOffset, actualFolding.EndOffset);
			}
		}

		public static IEnumerable<object[]> FoldingTestData
		{
			get
			{
				yield return new object[] { 
@"note left
	jufcjsffj
	note right
		svjvjf
			note over A
				ikjhkjhfj
			end note
	end note
end note", new [] { new NewFolding(0, 109), new NewFolding(23, 99), new NewFolding(46, 88) } };

				yield return new object[] { 
@"activate A
	jufcjsffj
deactivate A", new [] { new NewFolding(0, 36) } };

				yield return new object[] { 
@"activate  A
	jufcjsffj
deactivate       A", new [] { new NewFolding(0, 43) } };

				yield return new object[] { 
@"activate A
	jufcjsffj
deactivate B", new List<NewFolding>() };

				yield return new object[] { 
@"activate A
	jufcjsffj
deactivate Adsds", new List<NewFolding>() };

				yield return new object[] { 
@"activate A jufcjsffj deactivate B", new List<NewFolding>() };

				yield return new object[] { 
@"if ihusjvcs
	jufcjsffj
endif", new List<NewFolding>() };

				yield return new object[] { 
@"if ihusjvcs then
	jufcjsffj
endif asas", new [] { new NewFolding(0, 40) } };	// This tests that we are not TOO strict with folding.

				yield return new object[] { 
@"if ihusjvcs then
	jufcjsffj
endif", new [] { new NewFolding(0, 35) } };

				yield return new object[] { 
@"--> [jhchjbc] if ihusjvcs then
	jufcjsffj
endif", new [] { new NewFolding(13, 49) } };

				yield return new object[] { 
@"partition icugucue {
	jufcjsffj
}", new [] { new NewFolding(0, 35) } };

				yield return new object[] { 
@"class C {
	jufcjsffj
}", new [] { new NewFolding(0, 24) } };

				yield return new object[] { 
@"enum E {
	jufcjsffj
}", new [] { new NewFolding(0, 23) } };

				yield return new object[] { 
@"package P {
	jufcjsffj
}", new [] { new NewFolding(0, 26) } };

				yield return new object[] { 
@"package P
	jufcjsffj
end package", new [] { new NewFolding(0, 34) } };

				yield return new object[] { 
@"namespace Name.space {
	jufcjsffj
}", new [] { new NewFolding(0, 37) } };

				yield return new object[] { 
@"title
	jufcjsffj
end title", new [] { new NewFolding(0, 28) } };

				yield return new object[] { 
@"title
	jufcjsffj
end titledfdfdf", new List<NewFolding>() };

				yield return new object[] { 
@"box ""Box""
	jufcjsffj
end box", new [] { new NewFolding(0, 30) } };

				yield return new object[] { 
@"box ""Box""
	jufcjsffj
end boxghghg", new List<NewFolding>() };

			}
		}

		private readonly PlantUmlFoldingStrategy foldingStrategy = new PlantUmlFoldingStrategy();
	}
}