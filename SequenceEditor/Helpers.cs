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
using System.Linq;
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
				}
				else {
					diffCounter.Add(s, 1);
				}
			}
			foreach (T s in list2) {
				if (diffCounter.ContainsKey(s)) {
					diffCounter[s]--;
				}
				else {
					return false;
				}
			}
			return diffCounter.Values.All(c => c == 0);
		}
	}
}



