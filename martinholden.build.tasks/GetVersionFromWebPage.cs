using System;
using System.IO;
using System.Security;
using System.Collections;
using Microsoft.Build.Framework;
using HtmlAgilityPack;
using System.Linq;
using Microsoft.Build.Utilities;
using System.Collections.Generic;

namespace MartinHolden.Build.Tasks
{
    public class GetVersionFromWebPage : Task
    {
        /// <summary>
        /// Gets or sets the web page to parse
        /// </summary>
        /// <value>
        /// </value>
        /// <example>https://aeeval.skillport.com/skillportfe/login.action</example>
        [Required]
        public string WebPage
        {
            get
            {
                return _webPage;
            }

            set
            {
                _webPage = value;
            }
        }

        /// <summary>
        /// Gets or sets the XPATH used by HTMLAgility to extract the version string from page
        /// </summary>
        /// <value>
        /// </value>
        /// <example>//div[@class='s-version']</example>
        [Required]
        public string VersionXpath
        {
            get
            {
                return _versionXpath;
            }

            set
            {
                _versionXpath = value;
            }
        }

        /// <summary>
        /// Gets or sets version string
        /// </summary>
        /// <value>
        /// Major.Minor.Build
        /// </value>
        /// <example>8.0.7525</example>
        [Output]
        public string FullVersion
        {
            get
            {
                return _version;
            }

            set
            {
                _version = value;
            }
        }

        [Output]
        public int Major
        {
            get
            {
                return _major;
            }

            set
            {
                _major = value;
            }
        }

        [Output]
        public int Minor
        {
            get
            {
                return _minor;
            }

            set
            {
                _minor = value;
            }
        }

        [Output]
        public int Build
        {
            get
            {
                return _build;
            }

            set
            {
                _build = value;
            }
        }

        private string _webPage;
        private string _version = "0.0.0";
        private string _versionXpath;
        private int _major = 0;
        private int _minor = 0;
        private int _build = 0;

        /// <summary>
        /// Execute is part of the Microsoft.Build.Framework.ITask interface.
        /// When it's called, any input parameters have already been set on the task's properties.
        /// It returns true or false to indicate success or failure.
        /// </summary>
        public override bool Execute()
        {

            // From Web
            Log.LogMessage(MessageImportance.Normal, "Loading from: " + _webPage);
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc;
            Version ver = new Version();

            try
            {
                doc = web.Load(_webPage);
                Log.LogMessage(MessageImportance.Normal, "XPATH: " + _versionXpath);
                var nodes = doc.DocumentNode.SelectNodes(_versionXpath);
                string versionString = nodes[0].InnerText.Trim();

                if (nodes != null)
                {
                    Log.LogMessage(MessageImportance.Normal, "Webpage Version String: {0}", versionString);

                    //Strip any non numbers or .
                    var numericChars = "0123456789.".ToCharArray();
                    versionString = new String(versionString.Where(c => numericChars.Any(n => n == c)).ToArray());

                    Log.LogMessage(MessageImportance.Normal, "Cleaned Version String: {0}", versionString);

                    try
                    {
                        ver = Version.Parse(versionString);
                    }
                    catch (ArgumentNullException)
                    {
                        Log.LogError("Error: String to be parsed is null.");
                        throw;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Log.LogError("Error: Negative value in '{0}'.", versionString);
                        throw;
                    }
                    catch (ArgumentException)
                    {
                        Log.LogError("Error: Bad number of components in '{0}'.", versionString);
                        throw;
                    }
                    catch (FormatException)
                    {
                        Log.LogError("Error: Non-integer value in '{0}'.", versionString);
                        throw;
                    }
                    catch (OverflowException)
                    {
                        Log.LogError("Error: Number out of range in '{0}'.", versionString);
                        throw;
                    }
                }
                _major = ver.Major;
                _minor = ver.Minor;
                _build = ver.Build;
                _version = ver.Major + "." + ver.Minor + "." + ver.Build;
                Log.LogMessage(MessageImportance.Normal, "Returned Version: {0}", _version);

            }
            catch (Exception ex)
            {
                Log.LogError("Error loading source. " + ex.Message);
                throw;
            }



            return !Log.HasLoggedErrors;
        }
    }
}

