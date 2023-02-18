// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

namespace VCodeEditor.Document
{
	public class HLColorNotFoundException : Exception
	{
		public HLColorNotFoundException(string name) : base(name)
		{
		}
	}
}
