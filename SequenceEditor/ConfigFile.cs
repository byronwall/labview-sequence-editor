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
	public class ConfigFile
	{
		public List<string> labels;

		public string originalPath;

		public string _originalContents;

		public string fileContents;

		protected static Regex regHeader = new Regex(@"^\s*\[(.*?)\]\s*$", RegexOptions.Multiline);

		protected void UpdateLabels(string contents)
		{
			//get a list of the [labels]
			this.labels = new List<string>();
			foreach (Match match in regHeader.Matches(contents)) {
				var label = match.Groups[1].Value;
				this.labels.Add(label);
			}
		}

		public ConfigFile(string path)
		{
			this.originalPath = path;
			this.ProcessFile(path);
		}

		public void ProcessFile(string path)
		{
			//this will read in the labels			
			this._originalContents = File.ReadAllText(path);
			this.fileContents = this._originalContents;
			this.UpdateLabels(this._originalContents);
		}

		public Stream EditorData {
			get {
				MemoryStream stream = new MemoryStream();
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(this.fileContents);
				writer.Flush();
				stream.Position = 0;
				return stream;
			}
		}

		public void SaveToFile(string path)
		{
			File.WriteAllText(path, this.fileContents);
		}
	}
}

