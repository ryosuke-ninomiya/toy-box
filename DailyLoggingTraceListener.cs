using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Tomochan154.Debugging
{
    /// <summary>日付単位でファイルを分割したメッセージを出力する機能を提供します。</summary>
    public class DailyLoggingTraceListener : TraceListener
    {
        #region Field

        /// <summary>排他ロックに使用する object。</summary>
        private readonly object _syncLock = new object();

        #endregion

        #region Constructor

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        public DailyLoggingTraceListener()
            : this(-1, null, null, null, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        public DailyLoggingTraceListener(long maxSize)
            : this(maxSize, null, null, null, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(long maxSize, string appName)
            : this(maxSize, null, null, null, null, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        public DailyLoggingTraceListener(string outputDirectory)
            : this(-1, outputDirectory, null, null, null, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(string outputDirectory, string appName)
            : this(-1, outputDirectory, null, null, null, null, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        public DailyLoggingTraceListener(string outputDirectory, TraceFilter filter)
            : this(-1, outputDirectory, null, null, null, filter, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(string outputDirectory, TraceFilter filter, string appName)
            : this(-1, outputDirectory, null, null, null, filter, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        public DailyLoggingTraceListener(long maxSize, string outputDirectory, string fileNameFormat, string datetimeFormat)
            : this(maxSize, outputDirectory, fileNameFormat, datetimeFormat, null, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(long maxSize, string outputDirectory, string fileNameFormat, string datetimeFormat, string appName)
            : this(maxSize, outputDirectory, fileNameFormat, datetimeFormat, null, null, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="encoding">メッセージのエンコーディングを表す <see cref="Encoding"/>。</param>
        public DailyLoggingTraceListener(string outputDirectory, string fileNameFormat, string datetimeFormat, Encoding encoding)
            : this(-1, outputDirectory, fileNameFormat, datetimeFormat, encoding, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="encoding">メッセージのエンコーディングを表す <see cref="Encoding"/>。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(string outputDirectory, string fileNameFormat, string datetimeFormat, Encoding encoding, string appName)
            : this(-1, outputDirectory, fileNameFormat, datetimeFormat, encoding, null, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        public DailyLoggingTraceListener(string outputDirectory, string fileNameFormat, string datetimeFormat, TraceFilter filter)
            : this(-1, outputDirectory, fileNameFormat, datetimeFormat, null, filter, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(string outputDirectory, string fileNameFormat, string datetimeFormat, TraceFilter filter, string appName)
            : this(-1, outputDirectory, fileNameFormat, datetimeFormat, null, filter, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="encoding">メッセージのエンコーディングを表す <see cref="Encoding"/>。</param>
        public DailyLoggingTraceListener(long maxSize, string outputDirectory, string fileNameFormat, string datetimeFormat, Encoding encoding)
            : this(maxSize, outputDirectory, fileNameFormat, datetimeFormat, encoding, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="encoding">メッセージのエンコーディングを表す <see cref="Encoding"/>。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(long maxSize, string outputDirectory, string fileNameFormat, string datetimeFormat, Encoding encoding, string appName)
            : this(maxSize, outputDirectory, fileNameFormat, datetimeFormat, encoding, null, appName)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="maxSize">
        /// ファイルの最大サイズを表す long。<br/>
        /// ゼロを指定した場合はサイズによるファイルの分割を行いません。<br/>
        /// 負の値を指定した場合は <see cref="DefaultMaxSize"/> プロパティの値が使用されます。
        /// </param>
        /// <param name="outputDirectory">出力先のディレクトリパスを表す string。</param>
        /// <param name="fileNameFormat">
        /// 出力ファイル名の複合書式指定文字列を表す string。<br/>
        /// 各要素のインデックスは以下の通りです。<br/>
        /// 0 = 書き込み日時、1 = ファイル番号。
        /// </param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="encoding">メッセージのエンコーディングを表す <see cref="Encoding"/>。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        /// <param name="appName">メッセージを出力するアプリケーション名を表す string。</param>
        public DailyLoggingTraceListener(long maxSize, string outputDirectory, string fileNameFormat, string datetimeFormat, Encoding encoding, TraceFilter filter, string appName)
            : base(appName)
        {
            this.FileNumber = 0;
            this.LastUpdate = DateTime.MinValue;
            this.Filter = filter;

            if (maxSize < 0)
            {
                if (this.Attributes.ContainsKey("maxSize") == true)
                {
                    this.MaxSize = long.Parse(this.Attributes["maxSize"]);
                }
                else
                {
                    this.MaxSize = this.DefaultMaxSize;
                }
            }
            else
            {
                this.MaxSize = maxSize;
            }

            if (outputDirectory == null)
            {
                if (this.Attributes.ContainsKey("outputDirectory") == true)
                {
                    this.OutputDirectory = this.Attributes["outputDirectory"];
                }
                else
                {
                    this.OutputDirectory = Application.StartupPath + Path.DirectorySeparatorChar;
                }
            }
            else
            {
                this.OutputDirectory = outputDirectory;
            }

            if (fileNameFormat == null)
            {
                if (this.Attributes.ContainsKey("fileNameFormat") == true)
                {
                    this.FileNameFormat = this.Attributes["fileNameFormat"];
                }
                else
                {
                    this.FileNameFormat = this.DefaultFileNameFormat;
                }
            }
            else
            {
                this.FileNameFormat = fileNameFormat;
            }

            if (datetimeFormat == null)
            {
                if (this.Attributes.ContainsKey("datetimeFormat") == true)
                {
                    this.DatetimeFormat = this.Attributes["datetimeFormat"];
                }
                else
                {
                    this.DatetimeFormat = this.DefaultDatetimeFormat;
                }
            }
            else
            {
                this.DatetimeFormat = datetimeFormat;
            }

            if (encoding == null)
            {
                if (this.Attributes.ContainsKey("encoding") == true)
                {
                    this.Encoding = Encoding.GetEncoding(this.Attributes["encoding"]);
                }
                else
                {
                    this.Encoding = this.DefaultEncoding;
                }
            }
            else
            {
                this.Encoding = encoding;
            }
        }

        #endregion

        #region Dispose

        /// <summary>すべてのリソースを解放します。</summary>
        /// <param name="disposing">マネージリソースを解放するかどうかを表す bool。</param>
        /// <remarks><paramref name="disposing"/> に <see langword="false"/> を指定してはいけません。</remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this.Close();
            }
        }

        #endregion

        #region Property

        /// <summary>このクラスがスレッドセーフかどうかを取得します。</summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>テキストライタを取得または設定します。</summary>
        public TextWriter Writer { get; set; }

        /// <summary>メッセージのエンコーディングを取得または設定します。</summary>
        public Encoding Encoding { get; set; }

        /// <summary>ファイルの最大サイズを取得または設定します。</summary>
        public long MaxSize { get; set; }

        /// <summary>出力ファイル名の書式指定文字列を取得または設定します。</summary>
        public string FileNameFormat { get; set; }

        /// <summary>日付と時刻の書式指定文字列を取得または設定します。</summary>
        public string DatetimeFormat { get; set; }

        /// <summary>現在のファイル番号を取得します。</summary>
        public int FileNumber { get; protected set; }

        /// <summary>メッセージを書き込んだ最終日時を取得します。</summary>
        public DateTime LastUpdate { get; protected set; }

        /// <summary>出力先のディレクトリパスを取得または設定します。</summary>
        public string OutputDirectory { get; set; }

        /// <summary>現在開いているファイル情報を取得します。</summary>
        public FileInfo CurrentFile { get; protected set; }

        /// <summary>MaxSize プロパティの既定値を取得します。</summary>
        public virtual long DefaultMaxSize { get { return 1024 * 1024; } }

        /// <summary>FileNameFormat プロパティの既定値を取得します。</summary>
        public virtual string DefaultFileNameFormat { get { return "{0:yyyyMMdd}_{1}.txt"; } }

        /// <summary>DatetimeFormat プロパティの既定値を取得します。</summary>
        public virtual string DefaultDatetimeFormat { get { return "{0:MM/dd HH:mm:ss}"; } }

        /// <summary>Encoding プロパティの既定値を取得します。</summary>
        public virtual Encoding DefaultEncoding { get { return Encoding.Default; } }

        #endregion

        #region Method

        /// <summary>オプションのメッセージを作成します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <returns>作成したオプションのメッセージを表す string。</returns>
        protected virtual string CreateOptionString(TraceEventCache eventCache)
        {
            StringBuilder buffer = new StringBuilder();
            string indent = new string(' ', this.IndentSize * (this.IndentLevel + 1));

            if ((this.TraceOutputOptions & TraceOptions.ProcessId) != 0)
            {
                buffer.AppendLine(indent + "ProcessId=" + eventCache.ProcessId);
            }

            if ((this.TraceOutputOptions & TraceOptions.LogicalOperationStack) != 0)
            {
                bool first = true;

                foreach (Object obj in eventCache.LogicalOperationStack)
                {
                    if (first == true)
                    {
                        buffer.Append(indent + "LogicalOperationStack=" + obj.ToString());
                    }
                    else
                    {
                        buffer.Append(", " + obj.ToString());
                    }

                    first = false;
                }

                buffer.AppendLine();
            }

            if ((this.TraceOutputOptions & TraceOptions.ThreadId) != 0)
            {
                buffer.AppendLine(indent + "ThreadId=" + eventCache.ThreadId);
            }

            if ((this.TraceOutputOptions & TraceOptions.Timestamp) != 0)
            {
                buffer.AppendLine(indent + "Timestamp=" + eventCache.Timestamp);
            }

            if ((this.TraceOutputOptions & TraceOptions.DateTime) != 0)
            {
                buffer.AppendLine(indent + "DateTime" + eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }

            if ((this.TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                buffer.AppendLine(indent + "Callstack=" + eventCache.Callstack);
            }

            return buffer.ToString();
        }

        /// <summary>トレース情報に対応するトレースメッセージを作成します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="message">メッセージとして出力する文字列を表す string。</param>
        /// <returns>作成したトレースメッセージを表す string。</returns>
        protected virtual string CreateMessage(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message)
        {
            if (this.TraceOutputOptions == TraceOptions.None)
            {
                return string.Format(CultureInfo.InvariantCulture, this.DatetimeFormat + "  {1}: {2} : {3}", eventCache.DateTime, eventType.ToString(), id.ToString(CultureInfo.InvariantCulture), message);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, this.DatetimeFormat + "  {1}: {2} : {3}", eventCache.DateTime, eventType.ToString(), id.ToString(CultureInfo.InvariantCulture), message) + Environment.NewLine + CreateOptionString(eventCache);
            }
        }

        /// <summary>トレース情報に対応するトレースメッセージを作成します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="data">メッセージとして出力するデータを表す object[]。</param>
        /// <returns>作成したトレースメッセージを表す string。</returns>
        protected virtual string CreateMessage(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data)
        {
            if (data != null && data.Length > 0)
            {
                StringBuilder buffer = new StringBuilder(data[0].ToString());
                for (int i = 1; i < data.Length; i++)
                {
                    if (data[i] != null)
                    {
                        buffer.Append(", " + data[i].ToString());
                    }
                }
                return CreateMessage(eventCache, source, eventType, id, buffer.ToString());
            }
            else
            {
                return CreateMessage(eventCache, source, eventType, id, null as string);
            }
        }

        /// <summary>トレース情報に対応するトレースメッセージを作成します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="format">メッセージの複合書式指定文字列を表す string。</param>
        /// <param name="args"><paramref name="format"/> に対応する出力データを表す object[]。</param>
        /// <returns>作成したトレースメッセージを表す string。</returns>
        protected virtual string CreateMessage(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (args != null)
            {
                return CreateMessage(eventCache, source, eventType, id, string.Format(CultureInfo.InvariantCulture, format, args));
            }
            else
            {
                return CreateMessage(eventCache, source, eventType, id, format);
            }
        }

        /// <summary>指定のメッセージが現在のファイルに出力できるか計算し、必要に応じて新規ファイルを作成して割り当てます。</summary>
        /// <param name="appendLength">出力するメッセージのバイト数を表す int。</param>
        private void CalculateFileSize(int appendLength)
        {
            DateTime now = DateTime.Now;

            if (this.Writer == null || now.Date != this.LastUpdate)
            {
                CloseStream();
                this.FileNumber = 0;
                this.LastUpdate = now.Date;
                this.CurrentFile = new FileInfo(string.Format(CultureInfo.InvariantCulture, this.OutputDirectory + this.FileNameFormat, this.LastUpdate, this.FileNumber));

                if (this.Writer == null)
                {
                    if (this.CurrentFile.Directory.Exists == false)
                    {
                        Directory.CreateDirectory(this.OutputDirectory);
                        this.CurrentFile.Directory.Create();
                    }
                }
            }

            this.CurrentFile.Refresh();
            while (this.CurrentFile.Exists == true && this.MaxSize > 0 && this.CurrentFile.Length + appendLength > this.MaxSize)
            {
                CloseStream();
                this.FileNumber += 1;
                this.CurrentFile = new FileInfo(string.Format(CultureInfo.InvariantCulture, this.OutputDirectory + this.FileNameFormat, this.LastUpdate, this.FileNumber));
            }

            if (this.Writer == null)
            {
                this.Writer = new StreamWriter(new FileStream(this.CurrentFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read));
            }
        }

        /// <summary>指定のメッセージをファイルに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        private void WriteLineInternal(string message)
        {
            WriteInternal(message + Environment.NewLine);
            this.Writer.Flush();
            this.NeedIndent = true;
        }

        /// <summary>指定のメッセージをファイルに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        private void WriteInternal(string message)
        {
            if (this.NeedIndent == true && this.IndentLevel > 0)
            {
                this.NeedIndent = false;
                message = new string(' ', this.IndentSize * this.IndentLevel) + message;
            }

            byte[] binary = this.Encoding.GetBytes(message);
            CalculateFileSize(binary.Length);
            this.Writer.Write(message);
            this.Writer.Flush();
        }

        /// <summary>ストリームを閉じます。</summary>
        private void CloseStream()
        {
            if (this.Writer != null)
            {
                this.Writer.Flush();
                this.Writer.Close();
                this.Writer = null;
            }
        }

        #endregion

        #region Override Method

        /// <summary>トレース情報に対応するトレースメッセージを出力します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="data">メッセージとして出力するデータを表す object[]。</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            lock (_syncLock)
            {
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data) == true)
                {
                    WriteLineInternal(CreateMessage(eventCache, source, eventType, id, data));
                }
            }
        }

        /// <summary>トレース情報に対応するトレースメッセージを出力します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="data">メッセージとして出力するデータを表す object。</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            lock (_syncLock)
            {
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null) == true)
                {
                    WriteLineInternal(CreateMessage(eventCache, source, eventType, id, data));
                }
            }
        }

        /// <summary>トレース情報に対応するトレースメッセージを出力します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="message">メッセージとして出力する文字列を表す string。</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            lock (_syncLock)
            {
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null) == true)
                {
                    WriteLineInternal(CreateMessage(eventCache, source, eventType, id, message));
                }
            }
        }

        /// <summary>トレース情報に対応するトレースメッセージを出力します。</summary>
        /// <param name="eventCache">固有のトレースイベント情報を表す <see cref="TraceEventCache"/>。</param>
        /// <param name="source">メッセージを出力するアプリケーション名を表す string。</param>
        /// <param name="eventType">トレースイベントの種類を表す <see cref="TraceEventType"/>。</param>
        /// <param name="id">トレースイベントの識別子を表す int。</param>
        /// <param name="format">メッセージの複合書式指定文字列を表す string。</param>
        /// <param name="args"><paramref name="format"/> に対応する出力データを表す object[]。</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            lock (_syncLock)
            {
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null) == true)
                {
                    WriteLineInternal(CreateMessage(eventCache, source, eventType, id, format, args));
                }
            }
        }

        /// <summary>インデントを出力します。</summary>
        protected override void WriteIndent()
        {
            lock (_syncLock)
            {
                WriteInternal("");
            }
        }

        /// <summary>指定のメッセージをファイルに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        public override void Write(string message)
        {
            lock (_syncLock)
            {
                WriteInternal(message);
            }
        }

        /// <summary>指定のメッセージの先頭に現在日時を付けてファイルに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        public override void WriteLine(string message)
        {
            lock (_syncLock)
            {
                WriteLineInternal(String.Format(DatetimeFormat + "  {1}", DateTime.Now, message));
            }
        }

        /// <summary>ストリームを閉じます。</summary>
        public override void Close()
        {
            lock (_syncLock)
            {
                CloseStream();
            }
        }

        /// <summary>ストリームをフラッシュします。</summary>
        public override void Flush()
        {
            lock (_syncLock)
            {
                this.Writer.Flush();
            }
        }

        /// <summary>サポートされるカスタム属性を取得します。</summary>
        /// <returns>カスタム属性名を表す string[]。</returns>
        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "maxSize", "outputDirectory", "fileNameFormat", "datetimeFormat", "encoding" };
        }

        #endregion
    }
}
