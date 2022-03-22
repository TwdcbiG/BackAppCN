using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RoboSharp.DefaultConfigurations;

namespace RoboSharp
{
    /// <summary>
    /// Setup the ErrorToken and the path to RoboCopy.exe.
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/RoboSharpConfiguration"/>
    /// </remarks>
    public class RoboSharpConfiguration : ICloneable
    {
        #region Constructors 

        /// <summary>
        /// Create new LoggingOptions with Default Settings
        /// </summary>
        public RoboSharpConfiguration() { }

        /// <summary>
        /// Clone a RoboSharpConfiguration Object
        /// </summary>
        /// <param name="options">RoboSharpConfiguration object to clone</param>
        public RoboSharpConfiguration(RoboSharpConfiguration options)
        {
            errorToken = options.errorToken;
            errorTokenRegex = options.errorTokenRegex;
            roboCopyExe = options.roboCopyExe;

            #region < File Tokens >
            newFileToken = options.newFileToken;
            olderToken = options.olderToken;
            newerToken = options.newerToken;
            sameToken = options.sameToken;
            extraToken = options.extraToken;
            mismatchToken = options.mismatchToken;
            failedToken = options.failedToken;
            #endregion

            #region < Directory Tokens >
            newerDirToken = options.newerDirToken;
            extraDirToken = options.extraDirToken;
            existingDirToken = options.existingDirToken;
            #endregion

        }

        /// <inheritdoc cref="RoboSharpConfiguration.RoboSharpConfiguration(RoboSharpConfiguration)"/>
        public RoboSharpConfiguration Clone() => new RoboSharpConfiguration(this);

        object ICloneable.Clone() => Clone();

        #endregion

        private static readonly IDictionary<string, RoboSharpConfiguration>
            defaultConfigurations = new Dictionary<string, RoboSharpConfiguration>()
        {
            {"en", new RoboSharpConfig_EN() }, //en uses Defaults for LogParsing properties
            {"de", new RoboSharpConfig_DE() },
        };

        /// <summary>
        /// Error Token Identifier -- EN = "ERROR", DE = "FEHLER", etc <br/>
        /// Leave as / Set to null to use system default.
        /// </summary>
        public string ErrorToken
        {
            get { return errorToken ?? GetDefaultConfiguration().ErrorToken; }
            set
            {
                if (value != errorToken) ErrRegexInitRequired = true;
                errorToken = value;
            }
        }
        /// <summary> field backing <see cref="ErrorToken"/> property - Protected to allow DefaultConfig derived classes to set within constructor </summary>
        protected string errorToken = null;

        /// <summary>
        /// Regex to identify Error Tokens with during LogLine parsing
        /// </summary>
        public Regex ErrorTokenRegex
        {
            get
            {
                if (ErrRegexInitRequired) goto RegenRegex;  //Regex Generation Required
                else if (errorTokenRegex != null) return errorTokenRegex; //field already assigned -> return the field
                else
                {
                    //Try get default, if default has regex defined, use that.
                    errorTokenRegex = GetDefaultConfiguration().errorTokenRegex;
                    if (errorTokenRegex != null) return errorTokenRegex;
                }
            // Generate a new Regex Statement
            RegenRegex:
                errorTokenRegex = ErrorTokenRegexGenerator(ErrorToken); //new Regex($" {this.ErrorToken} " + @"(\d{1,3}) \(0x\d{8}\) ");
                ErrRegexInitRequired = false;
                return errorTokenRegex;
            }
        }
        /// <summary> Field backing <see cref="ErrorTokenRegex"/> property - Protected to allow DefaultConfig derived classes to set within constructor </summary>
        protected Regex errorTokenRegex;
        private bool ErrRegexInitRequired = false;

        /// <summary>
        /// Generate a new ErrorTokenRegex object from by insterting the <see cref="ErrorToken"/> into a standardized pattern.
        /// </summary>
        /// <param name="errorToken">Language Specific <see cref="ErrorToken"/></param>
        /// <returns></returns>
        internal static Regex ErrorTokenRegexGenerator(string errorToken)
        {
            Regex BaseErrTokenRegex = new Regex("(?<Date>.*?)\\s+IDENTIFIER\\s+(?<ErrCode>[0-9]+)\\s+(?<SignedErrCode>\\([0-9Xx]+\\))\\s+(?<Descrip>[\\w\\s]+(?!:))(?<Path>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            string pattern = BaseErrTokenRegex.ToString().Replace("IDENTIFIER", errorToken);
            return new Regex(pattern, BaseErrTokenRegex.Options);
        }
        

        #region < Tokens for Log Parsing >

        #region < File Tokens >

        /// <summary>
        /// Log Lines starting with this string indicate : New File -> Source FILE Exists, Destination does not
        /// </summary>
        public string LogParsing_NewFile
        {
            get { return newFileToken ?? GetDefaultConfiguration().newFileToken ?? "New File"; }
            set { newFileToken = value; }
        }
        private string newFileToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Destination File newer than Source
        /// </summary>
        public string LogParsing_OlderFile
        {
            get { return olderToken ?? GetDefaultConfiguration().olderToken ?? "Older"; }
            set { olderToken = value; }
        }
        private string olderToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Source File newer than Destination
        /// </summary>
        public string LogParsing_NewerFile
        {
            get { return newerToken ?? GetDefaultConfiguration().newerToken ?? "Newer"; }
            set { newerToken = value; }
        }
        private string newerToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Source FILE is identical to Destination File
        /// </summary>
        public string LogParsing_SameFile
        {
            get { return sameToken ?? GetDefaultConfiguration().sameToken ?? "same"; }
            set { sameToken = value; }
        }
        private string sameToken;

        /// <summary>
        /// Log Lines starting with this string indicate : EXTRA FILE -> Destination Exists, but Source does not
        /// </summary>
        public string LogParsing_ExtraFile
        {
            get { return extraToken ?? GetDefaultConfiguration().extraToken ?? "*EXTRA File"; }
            set { extraToken = value; }
        }
        private string extraToken;

        /// <summary>
        /// Log Lines starting with this string indicate : MISMATCH FILE
        /// </summary>
        public string LogParsing_MismatchFile
        {
            get { return mismatchToken ?? GetDefaultConfiguration().mismatchToken ?? "*Mismatch"; } // TODO: Needs Verification
            set { mismatchToken = value; }
        }
        private string mismatchToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File Failed to Copy
        /// </summary>
        public string LogParsing_FailedFile
        {
            get { return failedToken ?? GetDefaultConfiguration().failedToken ?? "*Failed"; } // TODO: Needs Verification
            set { failedToken = value; }
        }
        private string failedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.ExcludedFiles"/> filters
        /// </summary>
        public string LogParsing_FileExclusion
        {
            get { return fileExcludedToken ?? GetDefaultConfiguration().fileExcludedToken ?? "named"; } // TODO: Needs Verification
            set { fileExcludedToken = value; }
        }
        private string fileExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.ExcludeAttributes"/> or <see cref="SelectionOptions.IncludeAttributes"/> filters
        /// </summary>
        public string LogParsing_AttribExclusion
        {
            get { return attribExcludedToken ?? GetDefaultConfiguration().attribExcludedToken ?? "attrib"; } 
            set { attribExcludedToken = value; }
        }
        private string attribExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.MaxFileSize"/> filters
        /// </summary>
        public string LogParsing_MaxFileSizeExclusion
        {
            get { return maxfilesizeExcludedToken ?? GetDefaultConfiguration().maxfilesizeExcludedToken ?? "large"; } 
            set { maxfilesizeExcludedToken = value; }
        }
        private string maxfilesizeExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.MinFileSize"/> filters
        /// </summary>
        public string LogParsing_MinFileSizeExclusion
        {
            get { return minfilesizeExcludedToken ?? GetDefaultConfiguration().minfilesizeExcludedToken ?? "small"; } 
            set { minfilesizeExcludedToken = value; }
        }
        private string minfilesizeExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.MaxFileAge"/> or <see cref="SelectionOptions.MaxLastAccessDate"/>filters
        /// </summary>
        public string LogParsing_MaxAgeOrAccessExclusion
        {
            get { return maxageoraccessExcludedToken ?? GetDefaultConfiguration().maxageoraccessExcludedToken ?? "too old"; }
            set { maxageoraccessExcludedToken = value; }
        }
        private string maxageoraccessExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.MinFileAge"/> or <see cref="SelectionOptions.MinLastAccessDate"/>filters
        /// </summary>
        public string LogParsing_MinAgeOrAccessExclusion
        {
            get { return minageoraccessExcludedToken ?? GetDefaultConfiguration().minageoraccessExcludedToken ?? "too new"; }
            set { minageoraccessExcludedToken = value; }
        }
        private string minageoraccessExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was excluded by <see cref="SelectionOptions.ExcludeChanged"/> filters
        /// </summary>
        public string LogParsing_ChangedExclusion
        {
            get { return changedExcludedToken ?? GetDefaultConfiguration().changedExcludedToken ?? "changed"; }
            set { changedExcludedToken = value; }
        }
        private string changedExcludedToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File was included by <see cref="SelectionOptions.IncludeTweaked"/> filters
        /// </summary>
        public string LogParsing_TweakedInclusion
        {
            get { return tweakedIncludedToken ?? GetDefaultConfiguration().tweakedIncludedToken ?? "tweaked"; }
            set { tweakedIncludedToken = value; }
        }
        private string tweakedIncludedToken;

        #endregion

        #region < Directory Tokens >

        /// <summary>
        /// Log Lines starting with this string indicate : New Dir -> Directory will be copied to Destination
        /// </summary>
        public string LogParsing_NewDir
        {
            get { return newerDirToken ?? GetDefaultConfiguration().newerDirToken ?? "New Dir"; }
            set { newerDirToken = value; }
        }
        private string newerDirToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Extra Dir -> Does not exist in source
        /// </summary>
        public string LogParsing_ExtraDir
        {
            get { return extraDirToken ?? GetDefaultConfiguration().extraDirToken ?? "*EXTRA Dir"; }
            set { extraDirToken = value; }
        }
        private string extraDirToken;

        /// <summary>
        /// Existing Dirs do not have an identifier on the line. Instead, this string will be used when creating the <see cref="ProcessedFileInfo"/> object to indicate an Existing Directory.
        /// </summary>
        public string LogParsing_ExistingDir
        {
            get { return existingDirToken ?? GetDefaultConfiguration().existingDirToken ?? "Existing Dir"; }
            set { existingDirToken = value; }
        }
        private string existingDirToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Folder was excluded by <see cref="SelectionOptions.ExcludedDirectories"/> filters
        /// </summary>
        public string LogParsing_DirectoryExclusion
        {
            get { return dirExcludedToken ?? GetDefaultConfiguration().dirExcludedToken ?? "named"; } // TODO: Needs Verification
            set { dirExcludedToken = value; }
        }
        private string dirExcludedToken;

        #endregion

        #endregion </ Tokens for Log Parsing >

        /// <summary>
        /// Specify the path to RoboCopy.exe here. If not set, use the default copy.
        /// </summary>
        public string RoboCopyExe
        {
            get { return roboCopyExe ?? "Robocopy.exe"; }
            set { roboCopyExe = value; }
        }
        private string roboCopyExe = null;

        /// <Remarks>Default is retrieved from the OEMCodePage</Remarks>
        /// <inheritdoc cref="System.Diagnostics.ProcessStartInfo.StandardOutputEncoding" path="/summary"/>
        public System.Text.Encoding StandardOutputEncoding { get; set; } = System.Text.Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

        /// <Remarks>Default is retrieved from the OEMCodePage</Remarks>
        /// <inheritdoc cref="System.Diagnostics.ProcessStartInfo.StandardErrorEncoding" path="/summary"/>
        public System.Text.Encoding StandardErrorEncoding { get; set; } = System.Text.Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);


        private RoboSharpConfiguration defaultConfig = null;
        private RoboSharpConfiguration GetDefaultConfiguration()
        {
            if (defaultConfig != null) return defaultConfig;

            // check for default with language Tag xx-YY (e.g. en-US)
            var currentLanguageTag = System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag;
            if (defaultConfigurations.ContainsKey(currentLanguageTag))
            {
                defaultConfig = defaultConfigurations[currentLanguageTag];
            }
            else
            {
                // check for default with language Tag xx (e.g. en)
                var match = Regex.Match(currentLanguageTag, @"^\w+", RegexOptions.Compiled);
                if (match.Success)
                {
                    var currentMainLanguageTag = match.Value;
                    if (defaultConfigurations.ContainsKey(currentMainLanguageTag))
                    {
                        defaultConfig = defaultConfigurations[currentMainLanguageTag];
                    }
                }
            }

            // no match, fallback to en
            return defaultConfig ?? defaultConfigurations["en"];
        }
    }
}
