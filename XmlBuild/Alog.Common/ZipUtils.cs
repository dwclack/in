using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace Alog.Common
{
    public class ZipUtils
    {
        /// <summary>
        /// 将源文件压缩后并生成新的zip文件
        /// </summary>
        /// <param name="filePath">源文件所在路径</param>
        /// <param name="fileName">源文件名</param>
        public static void CreateZipFile(string filePath, string fileName)
        {
            if (!Directory.Exists(filePath))
            {
                Console.WriteLine("'{0}'文件路径不存在", filePath);
                return;
            }

            try
            {
                string zipFilePath = filePath + fileName + ".zip";

                if (File.Exists(zipFilePath))
                {
                    Console.WriteLine("'{0}'文件已存在", zipFilePath);
                    return;
                }

                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
                {
                    s.SetLevel(9);

                    byte[] buffer = new byte[4096]; //缓冲区大小

                    ZipEntry entry = new ZipEntry(Path.GetFileName(filePath + fileName));

                    entry.DateTime = DateTime.Now;

                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(filePath + fileName))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);

                            s.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }

                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("文件处理异常：{0}", ex);
            }
        }
    }
}
