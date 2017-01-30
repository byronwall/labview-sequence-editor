/*
 * Created by SharpDevelop.
 * User: bwall
 * Date: 1/25/2017
 * Time: 3:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using Xceed.Wpf.AvalonDock.Layout;
namespace SequenceEditor
{
	public class IniFoldingStrategy : AbstractFoldingStrategy
	{
		/// <summary>
		/// Creates a new BraceFoldingStrategy.
		/// </summary>
		public IniFoldingStrategy()
		{
		}

		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}

		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			List<NewFolding> newFoldings = new List<NewFolding>();
			char openingBrace = ']';
			char closingBrace = '[';
			int? startOffset = null;
			for (int i = 0; i < document.TextLength; i++) {
				char c = document.GetCharAt(i);
				if (c == openingBrace) {
					startOffset = i + 1;
				} else if (c == closingBrace && startOffset.HasValue) {
					//look backward from here to find the last new line
					int j = i - 1;
					while (j > 0) {
						if (document.GetCharAt(j) == '\n') {
							//not allowed to fold between \r and \n so catch that
							if (document.GetCharAt(j - 1) == '\r') {
								j -= 1;
							}
							newFoldings.Add(new NewFolding(startOffset.Value, j));
							startOffset = null;
							break;
						}
					}
				}
			}
			//get the last one
			if (startOffset.HasValue) {
				int endOffset = document.TextLength - 2;
				if (endOffset > startOffset.Value) {
					newFoldings.Add(new NewFolding(startOffset.Value, endOffset));
				}
			}
			return newFoldings;
		}
	}
}

