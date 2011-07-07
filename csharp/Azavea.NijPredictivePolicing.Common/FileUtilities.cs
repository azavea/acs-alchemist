using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Ionic.Zip;

namespace Azavea.NijPredictivePolicing.Common
{
    public static class FileUtilities
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Delete without exceptions.  Returns true on success, false on failure.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool TryDelete(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Try changing the file's creation time without exceptions.  Returns true on success, false on failure.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool TryChangeCreationTime(string filename, DateTime newTime)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.SetCreationTime(filename, newTime);
                }
                    
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Try changing the file's last write time without exceptions.  Returns true on success, false on failure.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool TryChangeLastWriteTime(string filename, DateTime newTime)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.SetLastWriteTime(filename, newTime);
                }

                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Given a basepath and a list of names of subdirectories, creates the subdirectories if necessary.  Throws exceptions on error.
        /// </summary>
        /// <param name="basepath">A path to a directory that already exists</param>
        /// <param name="chunks">Subdirectories to create (chunks[i + 1] is a subdirectory of chunks[i]</param>
        /// <returns>The path to the deepest subdirectory</returns>
        public static string PathEnsure(string basepath, params string[] chunks)
        {
            if (string.IsNullOrEmpty(basepath))
                return basepath;

            if ((chunks != null) && (chunks.Length > 0))
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    basepath = Path.Combine(basepath, chunks[i]);
                }
            }

            //CreateDirectory creates all necessary directories
            if (!Directory.Exists(basepath))
                Directory.CreateDirectory(basepath);

            return basepath;
        }

        /// <summary>
        /// Wrapper around PathEnsure() that doesn't throw exceptions.  Returns "" on error and prints an error message to _log
        /// </summary>
        /// <param name="basepath"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>
        public static string SafePathEnsure(string basepath, params string[] chunks)
        {
            try
            {
                //I'd rather do this here, than directly in the constructor or class definition.
                //The file system loves to throw exceptions, and I'd rather see em than a app 'exit'!
                //In Soviet Russia, computer throws things at YOU!

                return FileUtilities.PathEnsure(basepath, chunks);
            }
            catch (Exception ex)
            {
                _log.Error("Error constructing path", ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// Like Path.Combine(), only works for an arbitrary number of arguments
        /// </summary>
        /// <param name="chunks"></param>
        /// <returns></returns>
        public static string PathCombine(params string[] chunks)
        {
            if ((chunks == null) || (chunks.Length == 0))
            {
                return string.Empty;
            }

            string result = chunks[0];
            for (int i = 1; i < chunks.Length; i++)
            {
                result = Path.Combine(result, chunks[i]);
            }

            return result;
        }

        //public static bool UnzipFileTo(string basePath, string zipFileName)
        //{
        //    try
        //    {
        //        FileUtilities.SafePathEnsure(basePath);

        //        //using (System.IO.Packaging.Package pack = ZipPackage.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        using (var pack = Package.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            var parts = pack.GetParts();
        //            foreach (var part in parts)
        //            {
        //                string newFilePath = FileUtilities.PathCombine(newPath, part.Uri.ToString());
        //                var zipStream = part.GetStream(FileMode.Open, FileAccess.Read);
        //                var outputStream = new FileStream(newFilePath, FileMode.Create);
        //                Utilities.CopyTo(zipStream, outputStream);

        //                outputStream.Close();
        //                zipStream.Close();
        //            }

        //            pack.Close();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error("Error while unzipping file", ex);
        //    }
        //    return false;
        //}

        /// <summary>
        /// Uses Ionic.Zip library to expand a file (without overwriting) to a given location
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="zipFileName"></param>
        /// <returns></returns>
        public static bool UnzipFileTo(string basePath, string zipFileName)
        {
            try
            {
                _log.DebugFormat("Unzipping {0}", Path.GetFileName(zipFileName));
                FileUtilities.SafePathEnsure(basePath);

                var zipFile = new ZipFile(zipFileName);                
                zipFile.ExtractAll(basePath, ExtractExistingFileAction.DoNotOverwrite);

                _log.Debug("Unzipping... Done!");

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while unzipping file", ex);
            }
            return false;
        }

        /// <summary>
        /// Uses Ionic.Zip library to search a compressed file for a given pattern
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<string> FindFileNameInZipLike(string zipFileName, string pattern)
        {
            var zipFile = new ZipFile(zipFileName);
            var coll = zipFile.SelectEntries(pattern);

            var results = new List<string>();
            foreach (ZipEntry entry in coll)
            {
                results.Add(entry.FileName);
            }
            return results;
        }


    }
}
