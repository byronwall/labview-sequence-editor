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
	public class SequenceFile : ConfigFile
	{
		public bool isRenderedWithLineNumbers = true;

		public SequenceFile(string path)
			: base(path)
		{
		}

		static string ProcessSectionFromLinesToLabels(List<string> currentSection)
		{
			StringBuilder sectionBuilder = new StringBuilder();
			
			List<int> commentsBeforeLine = new List<int>();
			int commentCount = 0;
			Dictionary<int, int> execLines = new Dictionary<int, int>();
			Regex lineSplitter = new Regex(@"^[^;].*?=\s*(.*)");
			List<string> newLines = new List<string>();
			for (int i = 0; i < currentSection.Count; i++) {
				var originalLine = currentSection[i];
				string newline = originalLine;
				if (lineSplitter.IsMatch(originalLine)) {
					var match = lineSplitter.Match(originalLine);
					newline = match.Groups[1].Value;
					execLines.Add(execLines.Count + 1, i);
				}
				newLines.Add(newline);
			}
			for (int i = 0; i < newLines.Count; i++) {
				var newline = newLines[i];
				Regex gotoSpotter = new Regex(@"goto (\d+)");
				if (gotoSpotter.IsMatch(newline)) {
					//get the line number
					foreach (Match match in gotoSpotter.Matches(newline)) {
						int lineNum = int.Parse(match.Groups[1].Value);
						//check if the line has a label
						int lineIndex = execLines[lineNum];
						if (!newLines[lineIndex].StartsWith(":")) {
							newLines[lineIndex] = ":" + lineNum + ": " + newLines[lineIndex];
							//in case the goto was to the current line
							newline = newLines[i];
						}
					}
					newline = gotoSpotter.Replace(newline, "goto :$1:");
					newLines[i] = newline;
				}
			}
			newLines.ForEach(c => sectionBuilder.AppendLine(c));
			return sectionBuilder.ToString();
		}
		public void ConvertFileToLabels()
		{
			//take any l1= and remove those
			//find any goto ## and replace with a label :l1:
			//take those labels and add them to teh correct line at the start
			StringBuilder newFileBuilder = new StringBuilder();
			//run through a line at a time
			
			List<string> currentSection = new List<string>();			
			using (StringReader sr = new StringReader(this.fileContents)) {
				string line;
				
				bool isFirstRun = true;
				while ((line = sr.ReadLine()) != null) {
					//need to spot a header to determine which lines are together
					if (regHeader.IsMatch(line)) {
						//process the previous section
						if (!isFirstRun) {
							//process the previous section since this is a new one							
							newFileBuilder.Append(ProcessSectionFromLinesToLabels(currentSection));							
							
						}
						//start of a new section w/ new header
						currentSection = new List<string>();
					}
					currentSection.Add(line);
					isFirstRun = false;
				}
			}
			
			newFileBuilder.Append(ProcessSectionFromLinesToLabels(currentSection));
			
			this.isRenderedWithLineNumbers = false;
			this.fileContents = newFileBuilder.ToString();
		}

		static string ProcessSectionFromLabelsToLineNum(List<string> currentSection)
		{
			StringBuilder sectionBuilder = new StringBuilder();
			
			Dictionary<string, int> labelsAndLines = new Dictionary<string, int>();
			Regex labelSpotter = new Regex(@"^(:.*?:) ");
			List<string> newLines = new List<string>();
			int commentCount = 0;
			for (int lineNum = 0; lineNum < currentSection.Count; lineNum++) {
				var originalLine = currentSection[lineNum];
				if (originalLine.StartsWith(";")) {
					commentCount++;
				}
				string newline = originalLine;
				if (labelSpotter.IsMatch(originalLine)) {
					var match = labelSpotter.Match(originalLine);
					labelsAndLines.Add(match.Groups[1].Value, lineNum - commentCount);
					newline = labelSpotter.Replace(originalLine, "");
				}
				newLines.Add(newline);
			}
			commentCount = 0;
			for (int i = 0; i < newLines.Count; i++) {
				var newline = newLines[i];
				if (newline.StartsWith(";")) {
					commentCount++;
				}
				Regex gotoLabelSpotter = new Regex(@"goto (:.*?:)");
				if (gotoLabelSpotter.IsMatch(newline)) {
					//get the line number
					while (gotoLabelSpotter.IsMatch(newline)) {
						Match match = gotoLabelSpotter.Match(newline);
						//get the line
						int lineNum = labelsAndLines[match.Groups[1].Value];
						//replace the instance with the line
						newline = gotoLabelSpotter.Replace(newline, "goto " + lineNum, 1, match.Index);
					}
				}
				if (newline != "" && i != 0 && !newline.StartsWith(";")) {
					newLines[i] = "l" + (i - commentCount) + " = " + newline;
				}
			}
			newLines.ForEach(c => sectionBuilder.AppendLine(c));
			return sectionBuilder.ToString();
		}
		public void ConvertFileToLineNumbers()
		{
			//find any :labels: and determine whichi line they are in
			//find any goto :lalbe: abnd rpelac eiwth line #
			StringBuilder newFileBuilder = new StringBuilder();
			//run through a line at a time
			List<string> currentSection = new List<string>();
			using (StringReader sr = new StringReader(this.fileContents)) {
				bool isFirstRun = true;
				string line;
				while ((line = sr.ReadLine()) != null) {
					//need to spot a header to determine which lines are together
					if (regHeader.IsMatch(line)) {
						//process the previous section
						if (!isFirstRun) {
							//go through the lines and remove the line number
							//build a list of labels->lines first
							newFileBuilder.Append(ProcessSectionFromLabelsToLineNum(currentSection));							
						}
						//start of a new section w/ new header
						currentSection = new List<string>();						
					}
					currentSection.Add(line);
					isFirstRun = false;
				}
			}
			newFileBuilder.Append(ProcessSectionFromLabelsToLineNum(currentSection));							
			
			this.isRenderedWithLineNumbers = true;
			this.fileContents = newFileBuilder.ToString();
		}
	}
}

