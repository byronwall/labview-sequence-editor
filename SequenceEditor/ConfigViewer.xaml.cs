/*
 * Created by SharpDevelop.
 * User: bwall
 * Date: 01/27/2017
 * Time: 10:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;

namespace SequenceEditor
{
	/// <summary>
	/// Interaction logic for ConfigViewer.xaml
	/// </summary>
	public partial class ConfigViewer : UserControl
	{
		
		DataGrid dg;
		ConfigFile file;
		
		TextEditor textEditor;
		AbstractFoldingStrategy foldStrat;
		FoldingManager foldMgr;

		public void UpdateText()
		{
			textEditor.Load(this.file.EditorData);
		}
		
		bool isEditorVisible = true;
		
		public ConfigViewer()
		{
			InitializeComponent();
			
			dg = new DataGrid();
			dg.CanUserAddRows = false;
			dg.CanUserDeleteRows = false;
		}

		void UpdateFoldingOffsets()
		{
			foldStrat.UpdateFoldings(foldMgr, textEditor.Document);
		}
		
		public ConfigViewer(ConfigFile file)
			: this()
		{	
			
			this.file = file;
			
			//remove buttons based on type
			if (file is SequenceFile) {
				toolBtns.Items.Remove(btnToGrid);
				
			} else {
				toolBtns.Items.Remove(btnFlipSeq);
			}			
			
			//set up the text editor details and add highlighting for the loaded file
			
			textEditor = new TextEditor();
			foldMgr = FoldingManager.Install(textEditor.TextArea);
			foldStrat = new IniFoldingStrategy();				
			
			textEditor.ShowLineNumbers = true;
			textEditor.Load(file.originalPath);
			textEditor.FontFamily = new FontFamily("Consolas");
			textEditor.FontSize = 12;
			
			//set a reference to the common highlighter
			textEditor.SyntaxHighlighting = Window1.highlightingDefinition;
			
			textEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(textEditor.TextArea));
				
			//this wires the text change back to the main object
			textEditor.TextChanged += (object a, EventArgs ee) => {
				file.fileContents = textEditor.Text;
				
				UpdateFoldingOffsets();
				
				//this will change the side tool windows along with the highlighting				
				file.UpdateLabels();
			};
			
			file.labels.ListChanged += (sender, e) => {
				file.UpdateHighlighter();
				textEditor.TextArea.TextView.Redraw();
			};
			
			textEditor.TextArea.TextView.MouseDown += (object ssss, MouseButtonEventArgs eeee) => {
				if (eeee.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Control) {
					var position = textEditor.GetPositionFromPoint(eeee.GetPosition(textEditor));
					if (position == null) {
						return;
					}
					int offset = textEditor.Document.GetOffset(position.Value.Location);
        			
					//go each way from offset to find a whitespace
        			
					int origOffset = offset;
        			
					int startOffset = 0;
					int endOffset = textEditor.Document.TextLength;
        			
					while (offset > 0) {
						if (Char.IsWhiteSpace(textEditor.Document.GetCharAt(offset))) {
							startOffset = offset + 1;
							break;
						}
						offset--;
					}
					offset = origOffset;
					while (offset < textEditor.Document.TextLength) {
						if (Char.IsWhiteSpace(textEditor.Document.GetCharAt(offset))) {
							endOffset = offset;
							break;
						}
						offset++;
					}
        			
					string search = textEditor.Document.GetText(startOffset, endOffset - startOffset);
					
					foreach (var viewer in Window1.viewers) {
						viewer.NavigateToLabel(search);
					}
					eeee.Handled = true;
				}
			};
			
			UpdateFoldingOffsets();			
				
			pnlMain.Children.Add(textEditor);							
		}
		void BtnToGrid_Click(object sender, RoutedEventArgs e)
		{
			//this will convert to a data grid
			if (isEditorVisible) {			
				DataTable tbl = file.ConvertToTable();
				dg.ItemsSource = tbl.AsDataView();

				pnlMain.Children.Remove(textEditor);
				pnlMain.Children.Add(dg);	
			} else {
				file.ConvertToFlatFromTable();
				UpdateText();
				
				pnlMain.Children.Add(textEditor);
				pnlMain.Children.Remove(dg);	
			}
			
			isEditorVisible = !isEditorVisible;
		}
		
		void SetFoldStatusForAll(bool areFolded)
		{
			foreach (FoldingSection fm in foldMgr.AllFoldings) {
				fm.IsFolded = areFolded;
			}
		}
		
		void BtnCollapse_Click(object sender, RoutedEventArgs e)
		{
			SetFoldStatusForAll(true);
		}
		void BtnExpand_Click(object sender, RoutedEventArgs e)
		{
			SetFoldStatusForAll(false);
			
		}
		void BtnFlipSeq_Click(object sender, RoutedEventArgs e)
		{
			SequenceFile sequenceFile = file as SequenceFile;
				
			if (file == null) {
				return;
			}
			
			if (sequenceFile.isRenderedWithLineNumbers) {
				sequenceFile.ConvertFileToLabels();
			} else {
				sequenceFile.ConvertFileToLineNumbers();
			}
						
			UpdateText();			
		}
		void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			file.SaveToFile();			
			UpdateText();
			
		}
		void BtnRevert_Click(object sender, RoutedEventArgs e)
		{			
			file.RevertToOriginal();
			UpdateText();
		}
		
		public void NavigateToLabel(string label)
		{
			if (file.labels.Contains(label)) {				
				Regex search = new Regex(@"^\[" + label + @"\]", RegexOptions.Multiline);
				Match m = search.Match(file.fileContents);
			
				if (m.Success) {
					TextLocation loc = textEditor.Document.GetLocation(m.Index);
					var offset = (loc.Line - 1) * textEditor.TextArea.TextView.DefaultLineHeight;
					textEditor.ScrollToVerticalOffset(offset);
				}
			}
		}
	}
}