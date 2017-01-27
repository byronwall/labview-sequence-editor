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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace SequenceEditor
{
	public static class Helpers
	{
		public static bool ContainSameElements<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
		{
			var diffCounter = new Dictionary<T, int>();
			foreach (T s in list1) {
				if (diffCounter.ContainsKey(s)) {
					diffCounter[s]++;
				} else {
					diffCounter.Add(s, 1);
				}
			}
			foreach (T s in list2) {
				if (diffCounter.ContainsKey(s)) {
					diffCounter[s]--;
				} else {
					return false;
				}
			}
			return diffCounter.Values.All(c => c == 0);
		}
		
		public static IEnumerable<string> ReadClipboardAsLines()
		{
			string clipboardData = Clipboard.GetText();
			
			string line;
			using (var reader = new StringReader(clipboardData)) {
				while ((line = reader.ReadLine()) != null) {
					yield return line;				       		
				}
			}
		}
		
		public static string ToDelim(this DataTable table, string delim)
		{
			var result = new StringBuilder();
			
			for (int i = 0; i < table.Columns.Count; i++) {
				result.Append(table.Columns[i].ColumnName);
				result.Append(i == table.Columns.Count - 1 ? "\n" : delim);
			}

			foreach (DataRow row in table.Rows) {
				for (int i = 0; i < table.Columns.Count; i++) {
					result.Append(row[i].ToString());
					result.Append(i == table.Columns.Count - 1 ? "\n" : delim);
				}
			}

			return result.ToString();
		}
	}
}



