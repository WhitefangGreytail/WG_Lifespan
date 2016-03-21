using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;


namespace WG_Lifespan
{
    public class XML_VersionOne : WG_XMLBaseVersion
    {


        /// <param name="doc"></param>
        public override void readXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals("lifeSpan_Multiplier"))
                {
                    try
                    {
                        DataStore.lifeSpanMultiplier = Convert.ToInt32(node.InnerText);
                    }
                    catch (Exception e)
                    {
                        Debugging.bufferWarning("lifeSpan_Multiplier: " + e.Message + ". Setting to 2");
                        DataStore.lifeSpanMultiplier = 2;
                    }

                    if (DataStore.lifeSpanMultiplier <= 0)
                    {
                        Debugging.bufferWarning("Detecting a lifeSpan multiplier less than or equal to 0 . Setting to 2");
                        DataStore.lifeSpanMultiplier = 2;
                    }
                }
            }
        } // end readXML


        /// <param name="fullPathFileName"></param>
        /// <returns></returns>
        public override bool writeXML(string fullPathFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_LifeSpan");
            xmlDoc.AppendChild(rootNode);

            XmlNode node = xmlDoc.CreateElement("lifeSpan_Multiplier");
            node.InnerXml = DataStore.lifeSpanMultiplier.ToString();
            rootNode.AppendChild(node);

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
                return false;  // Only time when we say there's an error
            }

            return true;
        } // end writeXML

    }
}