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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
namespace SequenceEditor
{
	public class ConfigFile
	{
		public  BindingList<string> labels = new BindingList<string>();

		public string originalPath;

		public string _originalContents;

		public string fileContents;
		
		public Color foreGround = Colors.White;
		public Color backGround = Colors.Chocolate;
		
		HighlightingRule _HighlightingRule;

		protected static Regex regHeader = new Regex(@"^\s*\[(?<header>.*?)\]\s*$", RegexOptions.Multiline);
		protected static Regex lineSplitter = new Regex(@"^(?<key>[^;].*?)\s*=\s*(?<value>.*)");

		public void UpdateLabels()
		{		
			//get a list of the [labels]
			
			List<string> newLabels = new List<string>();
			
			foreach (Match match in regHeader.Matches(this.fileContents)) {
				var label = match.Groups["header"].Value;
				newLabels.Add(label);
			}
			
			//only update the BindingList if a change is actually needed
			if (!newLabels.ContainSameElements(this.labels)) {
				this.labels.Clear();
				newLabels.ForEach(this.labels.Add);
			}			
		}

		public ConfigFile(string path)
		{
			this.originalPath = path;
			this.ProcessFile(path);
		}
		
		public ConfigFile(string path, Color foreGround, Color backGround)
			: this(path)
		{
			this.foreGround = foreGround;
			this.backGround = backGround;
		}

		
		public void ProcessFile(string path)
		{
			//this will read in the labels			
			this._originalContents = File.ReadAllText(path);
			this.fileContents = this._originalContents;
			this.UpdateLabels();
			
			//create the highlighting rule -- will be held by reference in general rules
			_HighlightingRule = new HighlightingRule();
			_HighlightingRule.Color = new HighlightingColor {
				Foreground = new CustomizedBrush(foreGround),
				Background = new CustomizedBrush(backGround)
			};
			
			Window1.highlightingDefinition.MainRuleSet.Rules.Add(_HighlightingRule);
			UpdateHighlighter();		
		}
		
		public void UpdateHighlighter()
		{
			String[] wordList = labels.ToArray(); // Your own logic
			String regex = String.Format(@"\b({0})\w*\b", String.Join("|", wordList));
			_HighlightingRule.Regex = new Regex(regex);
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

		public virtual void SaveToFile()
		{
			//make a copy of the current file
			
			if (!Directory.Exists("backup")) {
				Directory.CreateDirectory("backup");
			}
			
			string newFileName = Path.GetFileNameWithoutExtension(this.originalPath) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".ini";
			
			File.Copy(this.originalPath, Path.Combine("backup", newFileName));
			
			//save the new file into the original one's path
			
			File.WriteAllText(this.originalPath, this.fileContents);
			
			this._originalContents = this.fileContents;
		}

		DataTable dataTable;
		public DataTable UpdateDataTable()
		{
			List<List<string>> sections = new List<List<string>>();
			
			using (StringReader sr = new StringReader(this.fileContents)) {
				bool isFirstRun = true;
				string line;
				List<string> currentSection = new List<string>();
				while ((line = sr.ReadLine()) != null) {
					//need to spot a header to determine which lines are together
					if (regHeader.IsMatch(line)) {
						//process the previous section
						if (!isFirstRun) {
							//go through the lines and remove the line number
							//build a list of labels->lines first
							sections.Add(currentSection);
						}
						//start of a new section w/ new header
						currentSection = new List<string>();						
					}
					currentSection.Add(line);
					isFirstRun = false;
				}
				
				sections.Add(currentSection);
			}
			
			//process the sections
			
			dataTable = new DataTable();			
			dataTable.Columns.Add(new DataColumn("name"));
			
			//process the first section to get the keys/columns
			foreach (var line in sections[0]) {
				foreach (Match m in lineSplitter.Matches(line)) {
					//add to the dict
					dataTable.Columns.Add(new DataColumn(m.Groups["key"].Value));
				}
			}
		
			//process the sections into a table
			foreach (var section in sections) {
				//have a list of lines, take the first line to get the header value
				DataRow dataRow = dataTable.NewRow();
				
				bool isHeader = true;				
				foreach (var line in section) {
				
					if (line == string.Empty) {
						continue;
					}
				
					if (isHeader) {
						//this is a name, add to that column
						Match hdrName = regHeader.Match(line);
						var rowName = hdrName.Groups["header"].Value;						
						dataRow["name"] = rowName;
					
					} else {
						//determine the column and add apporpriately
						Match m = lineSplitter.Match(line);
						var colName = m.Groups["key"].Value;
						var value = m.Groups["value"].Value;
						
						dataRow[colName] = value;
					}
				
					isHeader = false;
				}
				
				dataTable.Rows.Add(dataRow);
			}
			
			return dataTable;
		}
		
		public void ConvertToFlatFromTable()
		{
			//take the table and iterate each rough
			StringBuilder flatFileBuilder = new StringBuilder();
			
			foreach (DataRow row in dataTable.Rows) {
				
				//make a header from the name
				string header = (string)row["name"];
				flatFileBuilder.AppendLine(string.Format("[{0}]", header));
				
				//process each column into a line in the file
				foreach (DataColumn column in dataTable.Columns) {
					if (column.ColumnName == "name") {
						continue;
					}
					
					//the Trim is to remove trailing whitespace on empty entries
					var newLine = string.Format("{0} = {1}", column.ColumnName, row[column.ColumnName]);					
					flatFileBuilder.AppendLine(newLine.Trim());
				}

				//blank line at the end
				flatFileBuilder.AppendLine();				
			}
			
			this.fileContents = flatFileBuilder.ToString();
		}
		
		public void RevertToOriginal()
		{
			this.fileContents = this._originalContents;
		}
		
		public void UpdateDataTableFromExcel()
		{
			dataTable = new DataTable();			
			var clipboardReader = Helpers.ReadClipboardAsLines();
			//this assume the first record is filled with the column names
			var headers = clipboardReader.First().Split('\t');
			foreach (var header in headers) {
				dataTable.Columns.Add(header);
			}

			var records = clipboardReader.Skip(1);
			foreach (var record in records) {
				dataTable.Rows.Add(record.Split('\t'));
			}
			
			//deal with the new DataTable
			ConvertToFlatFromTable();
		}
		public void PutDataIntoClipboard()
		{
			DataTable table = UpdateDataTable();
			string tabletext = table.ToDelim("\t");
			Clipboard.SetText(tabletext, TextDataFormat.Text);
		}
	}
}

