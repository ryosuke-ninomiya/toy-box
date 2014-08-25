using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Tomochan154.Forms
{
    /// <summary>テキストコントロールに文字列を書き込む機能を提供します。</summary>
    public class TextBoxWriter : TextWriter
    {
        #region InteropServices

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        #endregion

        #region Constructor

        /// <summary>インスタンスを初期化します。</summary>
        /// <param name="textBox">入出力の対象となるテキストコントロールを表す <see cref="TextBoxBase"/>。</param>
        public TextBoxWriter(TextBoxBase textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(new StackFrame().GetMethod().GetParameters()[0].Name);
            }

            this.TextBox = textBox;
        }

        #endregion

        #region Delegate

        /// <summary>UI スレッドにマーシャリングしてメッセージを出力します。</summary>
        /// <param name="message">出力するテキストを表す string。</param>
        protected delegate void WriteStringEventHandler(string message);

        #endregion

        #region Property

        /// <summary>テキストコントロールを取得します。</summary>
        public TextBoxBase TextBox { get; protected set; }

        /// <summary>テキストコントロールで扱うエンコーディングを取得します。</summary>
        public override Encoding Encoding { get { return Encoding.Unicode; } }

        /// <summary>コントロールの文字列の長さを取得します。</summary>
        public virtual int TextLength { get { return this.TextBox.TextLength; } }

        /// <summary>インデントのレベルを取得・設定します。</summary>
        public int IndentLevel { get; set; }

        /// <summary>1 つのインデントに使用する空白文字の数を取得・設定します。</summary>
        public int IndentSize { get; set; }

        /// <summary>インデントに使用する空白文字を取得・設定します。</summary>
        public char IndentChar { get; set; }

        /// <summary>インデントを追加するかどうかを取得・設定します。</summary>
        protected bool NeedIndent { get; set; }

        /// <summary>IndentLevel プロパティの既定値を取得します。</summary>
        public virtual int DefaultIndentLevel { get { return 0; } }

        /// <summary>IndentSize プロパティの既定値を取得します。</summary>
        public virtual int DefaultIndentSize { get { return 4; } }

        /// <summary>IndentChar プロパティの既定値を取得します。</summary>
        public virtual char DefaultIndentChar { get { return ' '; } }

        #endregion

        #region Method

        /// <summary>インスタンスに関連付けられたすべてのリソースを開放します。</summary>
        public override void Close()
        {
            this.TextBox = null;
        }

        /// <summary>指定の文字列をテキストボックスに追加します。</summary>
        /// <param name="value">追加する文字列を表す string。</param>
        public override void Write(string value)
        {
            if (this.NeedIndent && this.IndentLevel > 0)
            {
                this.NeedIndent = false;
                value = new string(this.IndentChar, this.IndentSize * this.IndentLevel) + value;
            }

            WriteStringInvoke(value);
        }

        /// <summary>指定の文字列と改行をテキストボックスに追加します。</summary>
        /// <param name="value">追加する文字列を表す string。</param>
        public override void WriteLine(string value)
        {
            WriteStringInvoke(value + Environment.NewLine);
            this.NeedIndent = true;
        }

        /// <summary>UI スレッドにマーシャリングしてメッセージを出力します。</summary>
        /// <param name="message">出力するメッセージを表す string。</param>
        protected void WriteStringInvoke(string message)
        {
            if (this.TextBox.InvokeRequired)
            {
                this.TextBox.Invoke(new WriteStringEventHandler(WriteStringInvoke), message);
            }
            else
            {
                WriteStringCallback(message);
            }
        }

        /// <summary>テキストボックスにメッセージを出力します。</summary>
        /// <param name="message">出力するテキストを表す string。</param>
        protected virtual void WriteStringCallback(string message)
        {
            if (message.Length > this.TextBox.MaxLength)
            {
                message = message.Substring(message.Length - this.TextBox.MaxLength);
            }

            SendMessage(this.TextBox.Handle, WM_SETREDRAW, 0, 0);

            int removeLength = this.TextBox.TextLength + message.Length - this.TextBox.MaxLength;
            if (removeLength > 0)
            {
                string buf = this.TextBox.Text;
                int pos = 0;
                while (removeLength > pos && pos < buf.Length)
                {
                    pos = buf.IndexOf("\n", pos) + 1;
                    if (pos == -1)
                    {
                        break;
                    }
                }

                removeLength = (pos == -1) ? buf.Length : pos;

                int selectionStart = this.TextBox.SelectionStart - removeLength;
                int selectionLength = this.TextBox.SelectionLength + Math.Max(selectionStart, 0);

                this.TextBox.SelectionStart = 0;
                this.TextBox.SelectionLength = removeLength;
                this.TextBox.SelectedText = "";

                this.TextBox.SelectionStart = selectionStart;
                this.TextBox.SelectionLength = selectionLength;
            }

            this.TextBox.AppendText(message);
            SendMessage(this.TextBox.Handle, WM_SETREDRAW, 1, 0);
            this.TextBox.Invalidate();
        }

        #endregion
    }
}
