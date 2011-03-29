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

        public static string PathEnsure(string basepath, params string[] chunks)
        {
            if ((chunks == null) || (chunks.Length == 0))
            {
                return basepath;
            }
            for (int i = 0; i < chunks.Length; i++)
            {
                basepath = Path.Combine(basepath, chunks[i]);

                if (!Directory.Exists(basepath))
                {
                    Directory.CreateDirectory(basepath);
                }
            }
            return basepath;
        }

        public static string SafePathEnsure(string basepath, params string[] chunks)
        {
            try
            {
                //I'd rather do this here, than directly in the constructor or class definition.
                //The file system loves to throw exceptions, and I'd rather see em than a app 'exit'!

                return FileUtilities.PathEnsure(basepath, chunks);
            }
            catch (Exception ex)
            {
                _log.Error("Error constructing path", ex);
            }
            return string.Empty;
        }

        public static string PathCombine(string basepath, params string[] chunks)
        {
            if ((chunks == null) || (chunks.Length == 0))
            {
                return string.Empty;
            }
            for (int i = 0; i < chunks.Length; i++)
            {
                basepath = Path.Combine(basepath, chunks[i]);
            }
            return basepath;
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

        public static bool UnzipFileTo(string basePath, string zipFileName)
        {
            try
            {
                _log.DebugFormat("Unzipping {0}", zipFileName);
                FileUtilities.SafePathEnsure(basePath);

                var zipFile = new ZipFile(zipFileName);
                zipFile.ExtractAll(basePath, ExtractExistingFileAction.OverwriteSilently);

                _log.Debug("Unzipping... Done!");

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while unzipping file", ex);
            }
            return false;
        }


    }
}
