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
using System.Text;
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
			
			var filesToLoad = new List<string>();
			filesToLoad.Add("sequences.ini");
			filesToLoad.Add("variables.ini");
			
			foreach (string file in filesToLoad) {
				//create a new panel
				
				LayoutDocumentPane ldp = new LayoutDocumentPane();
				LayoutDocument ld = new LayoutDocument();
				
				ld.Title = file;
				
				var editor = new EditorInstance();
				editors.Add(editor);
				
				var textEditor = editor.txtEditor;
				textEditor.ShowLineNumbers = true;				
				textEditor.Load(file);
				textEditor.FontFamily = new FontFamily("Consolas");
				textEditor.FontSize = 12;
				
				ld.Content = textEditor;
				ld.CanClose = false;
				
				ldp.Children.Add(ld);				
				docPane.Children.Add(ldp);
			}
				
		}
		void BtnFoldFiles_Click(object sender, RoutedEventArgs e)
		{
			foreach (var editor in editors) {
				editor.foldStrat.UpdateFoldings(editor.foldMgr, editor.txtEditor.Document);
			}
		}
		void BtnAddHighlighting_Click(object sender, RoutedEventArgs e)
		{
			foreach (var editor in editors) {	
				using (StreamReader sr = new StreamReader("LabView.xshd")) {
					using (XmlTextReader reader = new XmlTextReader(sr)) {									
						editor.txtEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
					}
				}
			}
		}
	}
	
	public class EditorInstance
	{
		public TextEditor txtEditor;
		public AbstractFoldingStrategy foldStrat;
		public FoldingManager foldMgr;
		
		public EditorInstance()
		{
			this.txtEditor = new TextEditor();
			this.foldMgr = FoldingManager.Install(txtEditor.TextArea);
			this.foldStrat = new IniFoldingStrategy();
		}
			
	}
	
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
				newFoldings.Add(new NewFolding(startOffset.Value, document.TextLength - 1));
			}
			
			return newFoldings;
		}
	}
}