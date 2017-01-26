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
	internal sealed class CustomizedBrush : HighlightingBrush
	{
		private readonly SolidColorBrush brush;

		public CustomizedBrush(Color color)
		{
			brush = CreateFrozenBrush(color);
		}

		public override Brush GetBrush(ITextRunConstructionContext context)
		{
			return brush;
		}

		public override string ToString()
		{
			return brush.ToString();
		}

		private static SolidColorBrush CreateFrozenBrush(Color color)
		{
			SolidColorBrush brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}
	}
}

