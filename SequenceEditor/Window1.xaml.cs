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
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public List<EditorInstance> editors = new List<EditorInstance>();
		public FoldingManager foldingManaer;
		
		//files that can be created
		SequenceFile sequenceFile = null;
		ConfigFile variableFile = null;
		
		public Window1()
		{
			InitializeComponent();
		}
		void button1_Click(object sender, RoutedEventArgs e)
		{
			//this will open a file and stick some code into the editor
			//txtEditor.Load("sequences.ini");
		}
		void BtnProcessAllFiles_Click(object sender, RoutedEventArgs e)
		{
			//create some stuff
			Debug.Print("creating the panes");
			
			//remove anything open before
			docPane.Children.Clear();
			
			List<ConfigFile> filesToLoad = new List<ConfigFile>();
			
			//wire up the global variables
			sequenceFile = new SequenceFile("sequences.ini");
			
			filesToLoad.Add(sequenceFile);
			filesToLoad.Add(new ConfigFile("variables.ini"));
			//filesToLoad.Add(new ConfigFile("safety.ini"));
			//filesToLoad.Add(new ConfigFile("pid.ini"));
			
			//create the highlighting
			using (StreamReader sr = new StreamReader("LabView.xshd")) {
				using (XmlTextReader reader = new XmlTextReader(sr)) {
					highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
				}
			}
			
			foreach (ConfigFile file in filesToLoad) {
				//create a new panel for Avalon Dock
				LayoutDocumentPane ldp = new LayoutDocumentPane();
				LayoutDocument ld = new LayoutDocument();
				
				ld.Title = Path.GetFileName(file.originalPath);
				ld.CanClose = false;
				
				//set up the text editor details and add highlighting for the loaded file
				var editor = new EditorInstance(file);
				editors.Add(editor);
				
				var textEditor = editor.txtEditor;
				textEditor.ShowLineNumbers = true;
				textEditor.Load(file.originalPath);
				textEditor.FontFamily = new FontFamily("Consolas");
				textEditor.FontSize = 12;
				
				//this wires the text change back to the main object
				textEditor.TextChanged += (object a, EventArgs ee) => {file.fileContents = textEditor.Text;};
				
				//TODO make these colors file type dependent
				addHighlightingRule(file.labels, Colors.White, Colors.Brown);
				
				//add text editor to the Dock
				ld.Content = textEditor;
				ldp.Children.Add(ld);
				docPane.Children.Add(ldp);
			}
				
		}
		
		private void addHighlightingRule(IEnumerable<string> labels, Color colorFore, Color colorBack)
		{
			
			var _HighlightingRule = new HighlightingRule();
			_HighlightingRule.Color = new HighlightingColor {
				Foreground = new CustomizedBrush(colorFore),
				Background = new CustomizedBrush(colorBack)
			};

			String[] wordList = labels.ToArray(); // Your own logic
			String regex = String.Format(@"\b({0})\w*\b", String.Join("|", wordList));
			_HighlightingRule.Regex = new Regex(regex);
					
			highlightingDefinition.MainRuleSet.Rules.Add(_HighlightingRule);
		}
		
		void BtnFoldFiles_Click(object sender, RoutedEventArgs e)
		{
			foreach (var editor in editors) {
				editor.foldStrat.UpdateFoldings(editor.foldMgr, editor.txtEditor.Document);
			}
		}

		IHighlightingDefinition highlightingDefinition;

		void BtnAddHighlighting_Click(object sender, RoutedEventArgs e)
		{			
			foreach (var editor in editors) {
						
				editor.txtEditor.SyntaxHighlighting = highlightingDefinition;
			}
		}
		void BtnSequenceSwap_Click(object sender, RoutedEventArgs e)
		{
			if (sequenceFile.isRenderedWithLineNumbers) {
				sequenceFile.ConvertFileToLabels();
			} else {
				sequenceFile.ConvertFileToLineNumbers();
			}
			
			foreach (var editor in editors) {
				editor.UpdateText();
			}
			
		}
		void BtnSaveSeqFile_Click(object sender, RoutedEventArgs e)
		{
			sequenceFile.SaveToFile("newSeq.ini");
		}
	}
}