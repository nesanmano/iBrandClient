using System.Collections.Generic;
using System.IO;
using NLog;

namespace NomadLogic.iBrandDesktop.Report
{
    public class ReportWriterCsv
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private FileStream _fileStream;
        private StreamWriter _fileWriter;
        public FileInfo FilePath { get; set; }
        public string ItemSeparator = ",";
        public char QuoteCharacter = '"';

        public bool Open()
        {
            try
            {
                if (_fileWriter != null) return false;

                if (FilePath.Exists)
                {
                    FilePath.Delete();
                    FilePath.Refresh();
                }

                _fileStream = new FileStream(FilePath.FullName, FileMode.CreateNew);
                _fileWriter = new StreamWriter(_fileStream);
                return true;
            }
            catch (System.Exception e)
            {
                _fileWriter = null;
                _fileStream = null;
                log.Error(e);
                return false;
            }
        }

        public void Close()
        {
            try
            {
                _fileWriter?.Close();
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
            finally
            {
                _fileWriter = null;
                _fileStream = null;
            }
        }

        ~ReportWriterCsv()
        {
            Close();
        }

        public bool Write(IList<object> dataList)
        {
            return WriteToStream(dataList);
        }

        public bool Write(IList<string> dataList)
        {
            return WriteToStream(dataList);
        }

        private bool WriteToStream(IEnumerable<object> items)
        {
            if (_fileWriter == null) return false;

            try
            {
                var first = true;
                foreach (var item in items)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        _fileWriter.Write(ItemSeparator);
                    }

                    var str = item is string;

                    if (str) _fileWriter.Write(QuoteCharacter);

                    _fileWriter.Write(item);

                    if (str) _fileWriter.Write(QuoteCharacter);
                }

                _fileWriter.WriteLine();
                return true;
            }
            catch (System.Exception e)
            {
                log.Error(e);
                return false;
            }
        }
    }
}