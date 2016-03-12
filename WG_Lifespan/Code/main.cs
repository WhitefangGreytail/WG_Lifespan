using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using System.Diagnostics;
using Boformer.Redirection;
using ColossalFramework.Math;

namespace WG_Lifespan
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_Lifespan.xml";

        private readonly Dictionary<MethodInfo, Redirector> redirectsOnLoaded = new Dictionary<MethodInfo, Redirector>();
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();


        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                isModEnabled = true;
                readFromXML();
                Redirect();
            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionOne();
                    xml.writeXML(currentFileLocation);
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }

                RevertRedirect();
            }
        }


        public override void OnLevelUnloading()
        {
            if (isLevelLoaded)
            {
                isLevelLoaded = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                }
            }
        }

        private void Redirect()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    var r = RedirectionUtil.RedirectType(type);
                    if (r != null)
                    {
                        foreach (var pair in r)
                        {
                            redirectsOnCreated.Add(pair.Key, pair.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugging.writeDebugToFile($"An error occured while applying {type.Name} redirects!");
                    Debugging.writeDebugToFile(e.StackTrace);
                }
            }
        }

        private void RevertRedirect()
        {
            foreach (var kvp in redirectsOnCreated)
            {
                try
                {
                    kvp.Value.Revert();
                }
                catch (Exception e)
                {
                    Debugging.writeDebugToFile($"An error occured while reverting {kvp.Key.Name} redirect!");
                    Debugging.writeDebugToFile(e.StackTrace);
                }
            }
            redirectsOnCreated.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        private void readFromXML()
        {
            // Check the exe directory first
            currentFileLocation = ColossalFramework.IO.DataLocation.executableDirectory + Path.DirectorySeparatorChar + XML_FILE;
            bool fileAvailable = File.Exists(currentFileLocation);

            if (!fileAvailable)
            {
                // Switch to default which is the cities skylines in the application data area.
                currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;
                fileAvailable = File.Exists(currentFileLocation);
            }

            if (fileAvailable)
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionOne();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    try
                    {
                        reader = new XML_VersionOne();
                    }
                    catch
                    {
                        // Default to new XML structure
                    }

                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.panelMessage(e.Message);
                }
            }
            else
            {
                Debugging.panelMessage("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
