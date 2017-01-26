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
	public class EditorInstance
	{
		public TextEditor txtEditor;

		public AbstractFoldingStrategy foldStrat;

		public FoldingManager foldMgr;

		public ConfigFile dataSource;

		public EditorInstance(ConfigFile source)
		{
			this.txtEditor = new TextEditor();
			this.foldMgr = FoldingManager.Install(txtEditor.TextArea);
			this.foldStrat = new IniFoldingStrategy();
			this.dataSource = source;
		}

		public void UpdateText()
		{
			this.txtEditor.Load(this.dataSource.EditorData);
		}
	}
}

