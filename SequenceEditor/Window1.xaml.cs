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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.WindowsAPICodePack.Dialogs;
using Xceed.Wpf.AvalonDock.Layout;

namespace SequenceEditor
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		//files that can be created
		SequenceFile sequenceFile = null;
		ConfigFile variableFile = null;
		
		string workingFolderPath = "";
		
		public static List<ConfigViewer> viewers = new List<ConfigViewer>();
		
		public static IHighlightingDefinition highlightingDefinition;
		
		public Window1()
		{
			InitializeComponent();
			
			//create the highlighting
			using (StreamReader sr = new StreamReader("LabView.xshd")) {
				using (XmlTextReader reader = new XmlTextReader(sr)) {
					highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
				}
			}
		}

		void BtnProcessAllFiles_Click(object sender, RoutedEventArgs e)
		{
			//get a folder to look for files
			var dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;
			CommonFileDialogResult result = dialog.ShowDialog();
			
			workingFolderPath = dialog.FileName;
			
			viewers = new List<ConfigViewer>();
			
			//remove anything open before
			docPane.Children.Clear();
			
			List<ConfigFile> filesToLoad = new List<ConfigFile>();
			
			//wire up the global variables
			sequenceFile = new SequenceFile(Path.Combine(workingFolderPath, "sequences.ini"), Colors.White, Colors.Black);
			variableFile = new ConfigFile(Path.Combine(workingFolderPath, "variables.ini"));
			
			filesToLoad.Add(sequenceFile);			
			filesToLoad.Add(variableFile);
			
			string[] otherFiles = { 
				Path.Combine(workingFolderPath, "safety.ini")
					, Path.Combine(workingFolderPath, "pid.ini")
			};
			
			foreach (var otherFile in otherFiles) {
				if (File.Exists(otherFile)) {			
					filesToLoad.Add(new ConfigFile(otherFile));
				}
			}			
			
			foreach (ConfigFile file in filesToLoad) {
				
				//create a new panel for Avalon Dock
				LayoutDocumentPane ldp = new LayoutDocumentPane();
				LayoutDocument ld = new LayoutDocument();
				
				ld.Title = Path.GetFileName(file.originalPath);
				ld.CanClose = false;
				
				ConfigViewer viewer = new ConfigViewer(file);

				viewers.Add(viewer);
				
				//add text editor to the Dock
				ld.Content = viewer;
				ldp.Children.Add(ld);
				docPane.Children.Add(ldp);
				
				//create a tool pane with the labels
				LayoutAnchorable toolWindow = new LayoutAnchorable();
				toolWindow.Title = Path.GetFileNameWithoutExtension(file.originalPath);
				toolWindow.CanClose = false;
				
				//create teh ListBox to store labels
				ListBox boxOfLabels = new ListBox();
				boxOfLabels.ItemsSource = file.labels;
				
				//wire up the dbl click event
				boxOfLabels.MouseDoubleClick += (object ss, MouseButtonEventArgs eee) => {
					viewer.NavigateToLabel(boxOfLabels.SelectedValue.ToString());
				};
				
				toolWindow.Content = boxOfLabels;				
				toolPane.Children.Add(toolWindow);
				toolPane.DockWidth = new GridLength(150);
			}
				
		}
		
		void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		void BtnAbout_Click(object sender, RoutedEventArgs e)
		{
			string about = @"LabView Sequence File Manager
Initial Version Created Jan 2017

-- Contact --
Byron for questions about the program
Douwe for questions about the LabView side";
			
			MessageBox.Show(about, "LabView Sequence File Manager");
		}
	}
}