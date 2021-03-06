﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Diagnostics;

namespace AnomalousMSBuild.Tasks
{
    /// <summary>
    /// This task will write the version of a specified versioned directory to a
    /// file by replacing keywords in another file. This is basicly a msbuild
    /// version of using git log --oneline to count the number of commits in a git repo.
    /// 
    /// This is not a particularly meaningful number, but will serve in a transition.
    /// </summary>
    public class WriteGitRevisionCount : Task
    {
        public override bool Execute()
        {
            bool success = true;
            int versionNumber = 0;

            try
            {
                ProcessStartInfo svnversionProcInfo = new ProcessStartInfo("git", "log --oneline");
                svnversionProcInfo.UseShellExecute = false;
                svnversionProcInfo.RedirectStandardOutput = true;
                svnversionProcInfo.CreateNoWindow = true;
                svnversionProcInfo.WorkingDirectory = Directory;

                Process svnversionProc = Process.Start(svnversionProcInfo);
                while (svnversionProc.StandardOutput.ReadLine() != null)
                {
                    ++versionNumber;
                }

                Log.LogWarning("Read lines");
            }
            catch (Exception e)
            {
                Log.LogWarningFromException(e);
                Log.LogWarning("File will be built with 0 revision");
            }

            try
            {
                String sourceFileText = null;
                using (TextReader sourceFileReader = new StreamReader(new BufferedStream(File.OpenRead(SourceFile))))
                {
                    sourceFileText = sourceFileReader.ReadToEnd();
                }

                String destFileText = sourceFileText.Replace(VersionTag, versionNumber.ToString());
                String destFileCurText = null;
                if (File.Exists(DestFile))
                {
                    using (TextReader destFileReader = new StreamReader(new BufferedStream(File.OpenRead(DestFile))))
                    {
                        destFileCurText = destFileReader.ReadToEnd();
                    }
                }

                //Only write file if the contents are not the same
                if (!destFileText.Equals(destFileCurText))
                {
                    using (TextWriter destFileWriter = new StreamWriter(new BufferedStream(File.Open(DestFile, FileMode.Create, FileAccess.Write))))
                    {
                        destFileWriter.Write(destFileText);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// The directory to read the subversion version from (under SubverisonFolder default is .svn).
        /// </summary>
        [Required]
        public String Directory { get; set; }

        /// <summary>
        /// The source file.
        /// </summary>
        [Required]
        public String SourceFile { get; set; }

        /// <summary>
        /// The file to write output to.
        /// </summary>
        [Required]
        public String DestFile { get; set; }

        String versionTag = "$WCREV$";
        /// <summary>
        /// The tag to replace with the version number.
        /// </summary>
        public String VersionTag
        {
            get
            {
                return versionTag;
            }
            set
            {
                versionTag = value;
            }
        }
    }
}
