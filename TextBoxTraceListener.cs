using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Tomochan154.Debugging
{
    using Tomochan154.Forms;

    /// <summary>テキストコントロールにメッセージを出力する機能を提供します。</summary>
    public class TextBoxTraceListener : TraceListener
    {
        #region Field

        /// <summary>排他ロックに使用する object。</summary>
        private readonly object _syncLock = new object();

        #endregion

        #region Constructor

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="textBox">出力先のテキストボックスを表す <see cref="TextBoxBase"/>。</param>
        public TextBoxTraceListener(TextBoxBase textBox)
            : this(textBox, null, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="textBox">出力先のテキストボックスを表す <see cref="TextBoxBase"/>。</param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        public TextBoxTraceListener(TextBoxBase textBox, string datetimeFormat)
            : this(textBox, datetimeFormat, null)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="textBox">出力先のテキストボックスを表す <see cref="TextBoxBase"/>。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        public TextBoxTraceListener(TextBoxBase textBox, TraceFilter filter)
            : this(textBox, null, filter)
        {
        }

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="textBox">出力先のテキストボックスを表す <see cref="TextBoxBase"/>。</param>
        /// <param name="datetimeFormat">日付と時刻の複合書式指定文字列を表す string。</param>
        /// <param name="filter">メッセージの出力を制御する <see cref="TraceFilter"/>。</param>
        public TextBoxTraceListener(TextBoxBase textBox, string datetimeFormat, TraceFilter filter)
            : base("TextBoxTraceListener")
        {
            this.Writer = new TextBoxWriter(textBox);
            this.Filter = filter;

            if (datetimeFormat == null)
            {
                if (this.Attributes.ContainsKey("datetimeFormat"))
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
        }

        #endregion

        #region Dispose

        /// <summary>すべてのリソースを解放します。</summary>
        /// <param name="disposing">マネージリソースを解放するかどうかを表す bool。</param>
        /// <remarks><paramref name="disposing"/> に <see langword="false"/> を指定してはいけません。</remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        #endregion

        #region Property

        /// <summary>このクラスがスレッドセーフかどうかを取得します。</summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>テキストコントロールライタを取得します。</summary>
        public TextBoxWriter Writer { get; protected set; }

        /// <summary>日付と時間の書式指定文字列を取得または設定します。</summary>
        public string DatetimeFormat { get; set; }

        /// <summary>DatetimeFormat プロパティの既定値を取得します。</summary>
        public virtual string DefaultDatetimeFormat { get { return "{0:MM/dd HH:mm:ss}"; } }

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
                    if (first)
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
                buffer.AppendLine(indent + "DateTime" + eventCache.DateTime.ToString("o", CultureInfo.CurrentCulture));
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
                return string.Format(CultureInfo.CurrentCulture, this.DatetimeFormat + "  {1}: {2} : {3}", eventCache.DateTime, eventType.ToString(), id.ToString(CultureInfo.CurrentCulture), message);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, this.DatetimeFormat + "  {1}: {2} : {3}", eventCache.DateTime, eventType.ToString(), id.ToString(CultureInfo.CurrentCulture), message) + Environment.NewLine + CreateOptionString(eventCache);
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
                return CreateMessage(eventCache, source, eventType, id, string.Format(CultureInfo.CurrentCulture, format, args));
            }
            else
            {
                return CreateMessage(eventCache, source, eventType, id, format);
            }
        }

        /// <summary>指定のメッセージをファイルに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        private void WriteLineInternal(string message)
        {
            if (this.Writer.TextBox.InvokeRequired)
            {
                this.Writer.TextBox.Invoke((Action<string>)WriteLineInternal, message);
            }
            else
            {
                this.Writer.WriteLine(message);
                this.Writer.TextBox.SelectionStart = this.Writer.TextLength;
                this.Writer.TextBox.ScrollToCaret();
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
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
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
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
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
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
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
                if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
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
                Writer.Write("");
            }
        }

        /// <summary>指定のメッセージをテキストボックスに出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        public override void Write(string message)
        {
            lock (_syncLock)
            {
                Writer.Write(message);
            }
        }

        /// <summary>指定のメッセージの先頭に現在日時を付けてテキストボックスに出力します。</summary>
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
                Writer.Close();
            }
        }

        /// <summary>サポートされるカスタム属性を取得します。</summary>
        /// <returns>カスタム属性名を表す string[]。</returns>
        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "datetimeFormat" };
        }

        #endregion
    }
}
