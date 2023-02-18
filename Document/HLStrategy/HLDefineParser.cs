﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1262 $</version>
// </file>

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace VCodeEditor.Document
{
    internal class HLDefineParser
    {
        private HLDefineParser()
        {
            // This is a pure utility class with no instances.	
        }

        static ArrayList errors = null;

        public static DefaultHLStrategy Parse(SyntaxMode syntaxMode, XmlTextReader xmlTextReader)
        {
            return Parse(null, syntaxMode, xmlTextReader);
        }

        public static DefaultHLStrategy Parse(DefaultHLStrategy highlighter, SyntaxMode syntaxMode, XmlTextReader xmlTextReader)
        {
            if (syntaxMode == null)
                throw new ArgumentNullException("syntaxMode");
            if (xmlTextReader == null)
                throw new ArgumentNullException("xmlTextReader");
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                Stream shemaStream = typeof(HLDefineParser).Assembly.GetManifestResourceStream("VCodeEditor.Resources.Mode.xsd");
                settings.Schemas.Add("", new XmlTextReader(shemaStream));
                settings.Schemas.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);
                settings.ValidationType = ValidationType.Schema;
                XmlReader validatingReader = XmlReader.Create(xmlTextReader, settings);

                //				XmlValidatingReader validatingReader = new XmlValidatingReader(xmlTextReader);
                //				Stream shemaStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Resources.Mode.xsd");
                //				validatingReader.Schemas.Add("", new XmlTextReader(shemaStream));
                //				validatingReader.ValidationType = ValidationType.Schema;


                XmlDocument doc = new XmlDocument();
                doc.Load(validatingReader);

                if (highlighter == null)
                    highlighter = new DefaultHLStrategy(doc.DocumentElement.Attributes["name"].InnerText);

                if (doc.DocumentElement.HasAttribute("extends"))
                {
                    KeyValuePair<SyntaxMode, ISyntaxModeFileProvider> entry = HLManager.Manager.FindHighlighterEntry(doc.DocumentElement.GetAttribute("extends"));
                    if (entry.Key == null)
                    {
                        MessageBox.Show("Cannot find referenced highlighting source " + doc.DocumentElement.GetAttribute("extends"));
                    }
                    else
                    {
                        highlighter = Parse(highlighter, entry.Key, entry.Value.GetSyntaxModeFile(entry.Key));
                        if (highlighter == null)
                            return null;
                    }
                }
                if (doc.DocumentElement.HasAttribute("extensions"))
                {
                    highlighter.Extensions = doc.DocumentElement.GetAttribute("extensions").Split(new char[] { ';', '|' });
                }

                XmlElement col = doc.DocumentElement["Colors"];
                if (col != null)
                {
                    foreach (XmlElement c in col.ChildNodes)
                    {
                        string n = c.Attributes["name"].InnerText;
                        highlighter.Colors.Add(n,
                            new HighlightColor(c));
                    }
                }

                XmlElement environment = doc.DocumentElement["Environment"];
                if (environment != null)
                {
                    foreach (XmlNode node in environment.ChildNodes)
                    {
                        if (node is XmlElement)
                        {
                            XmlElement el = (XmlElement)node;
                            highlighter.SetColorFor(el.Name, el.HasAttribute("bgcolor") ? new HLBackground(el) : new HighlightColor(el));
                        }
                    }
                }

                // parse properties
                if (doc.DocumentElement["Properties"] != null)
                {
                    foreach (XmlElement propertyElement in doc.DocumentElement["Properties"].ChildNodes)
                    {
                        highlighter.Properties[propertyElement.Attributes["name"].InnerText] = propertyElement.Attributes["value"].InnerText;
                    }
                }

                if (doc.DocumentElement["Digits"] != null)
                {
                    highlighter.DigitColor = new HighlightColor(doc.DocumentElement["Digits"]);
                }

                XmlNodeList nodes = doc.DocumentElement.GetElementsByTagName("RuleSet");
                foreach (XmlElement element in nodes)
                {
                    highlighter.AddRuleSet(new HLRuleSet(element));
                }

                xmlTextReader.Close();

                if (errors != null)
                {
                    ReportErrors(syntaxMode.FileName);
                    errors = null;
                    return null;
                }
                else
                {
                    return highlighter;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not load mode definition file '" + syntaxMode.FileName + "'.\n" + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return null;
            }
        }

        private static void ValidationHandler(object sender, ValidationEventArgs args)
        {
            if (errors == null)
            {
                errors = new ArrayList();
            }
            errors.Add(args);
        }

        private static void ReportErrors(string fileName)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Could not load mode definition file. Reason:\n\n");
            foreach (ValidationEventArgs args in errors)
            {
                msg.Append(args.Message);
                msg.Append(Console.Out.NewLine);
            }
            MessageBox.Show(msg.ToString(), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
        }

    }
}