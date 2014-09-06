using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.IO.Compression;

namespace Tomochan154.Configuration
{
    /// <summary>設定情報を XML で出力する機能を提供します。</summary>
    /// <typeparam name="T">シリアライズするクラスの型。</typeparam>
    public interface IPortableSettingsSerializer<T>
        where T : class
    {
        /// <summary>指定のインスタンスを保存します。</summary>
        /// <param name="path">シリアライズした内容を保存するパス。</param>
        /// <param name="instance">シリアライズする対象のインスタンス。</param>
        void Serialize(string path, T instance);

        /// <summary>指定のパスからインスタンスを取得します。</summary>
        /// <param name="path">デシリアライズする内容を読み込むパス。</param>
        /// <returns>デシリアライズしたインスタンス。</returns>
        T Desilialize(string path);
    }

    /// <summary>設定情報を XML で出力する機能を提供します。</summary>
    /// <typeparam name="T">シリアライズするクラスの型。</typeparam>
    public class PortableSettingsXmlSerializer<T> : IPortableSettingsSerializer<T>
        where T : class
    {
        #region Members

        /// <summary>PortableSettingsXmlSerializer クラスの新しいインスタンスを作成します。</summary>
        public PortableSettingsXmlSerializer()
        {
            XmlWriterSettings = new XmlWriterSettings { Indent = true };
            KnownTypes = new List<Type>();
            MaxItemsInObjectGraph = Int32.MaxValue;
            IgnoreExtensionDataObject = true;
        }

        /// <summary>XML の出力形式を制御するために使用するコンポーネントを取得または設定します。</summary>
        public XmlWriterSettings XmlWriterSettings { get; set; }

        /// <summary>既知のコントラクト型に xsi:type 宣言を動的にマップするのに使用するコンポーネントを取得または設定します。</summary>
        public DataContractResolver DataContractResolver { get; set; }

        /// <summary>シリアル化または逆シリアル化プロセスを拡張できるサロゲート型を取得または設定します。</summary>
        public IDataContractSurrogate DataContractSurrogate { get; set; }

        /// <summary>クラスがシリアル化または逆シリアル化されるときに、そのクラスの拡張により提供されるデータを無視するかどうかを指定する値を取得または設定します。</summary>
        public bool IgnoreExtensionDataObject { get; set; }

        /// <summary>DataContractSerializer のこのインスタンスを使用してシリアル化されるオブジェクト グラフ内に存在可能な型のコレクションを取得します。</summary>
        public IList<Type> KnownTypes { get; private set; }

        /// <summary>シリアル化または逆シリアル化するオブジェクト グラフ内の項目の最大数を取得または設定します。</summary>
        public int MaxItemsInObjectGraph { get; set; }

        /// <summary>オブジェクトの参照データを保持するために非標準の XML コンストラクトを使用するかどうかを示す値を取得または設定します。</summary>
        public bool PreserveObjectReferences { get; set; }

        /// <summary>指定のインスタンスを保存します。</summary>
        /// <param name="path">シリアライズした内容を保存するパス。</param>
        /// <param name="instance">シリアライズする対象のインスタンス。</param>
        public void Serialize(string path, T instance)
        {
            var serializer = GetSerializer();
            using (MemoryStream stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
            {
                serializer.WriteObject(writer, instance);
                writer.Flush();
                string json = Encoding.UTF8.GetString(stream.ToArray());
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
            }
        }

        /// <summary>指定のパスからインスタンスを取得します。</summary>
        /// <param name="path">デシリアライズする内容を読み込むパス。</param>
        /// <returns>デシリアライズしたインスタンス。</returns>
        public T Desilialize(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            var serializer = GetSerializer();
            byte[] bytes = Encoding.UTF8.GetBytes(File.ReadAllText(path, Encoding.UTF8));
            using (var stream = new MemoryStream(bytes))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        /// <summary>シリアライザのインスタンスを取得します。</summary>
        /// <returns>取得したシリアライザのインスタンス。</returns>
        private DataContractSerializer GetSerializer()
        {
            return new DataContractSerializer(typeof(T), KnownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, PreserveObjectReferences, DataContractSurrogate, DataContractResolver);
        }

        #endregion
    }

    /// <summary>設定情報を JSON で出力する機能を提供します。</summary>
    /// <typeparam name="T">シリアライズするクラスの型。</typeparam>
    public class PortableSettingsJsonSerializer<T> : IPortableSettingsSerializer<T>
        where T : class
    {
        #region Members

        /// <summary>PortableSettingsJsonSerializer クラスの新しいインスタンスを作成します。</summary>
        public PortableSettingsJsonSerializer()
        {
            KnownTypes = new List<Type>();
            MaxItemsInObjectGraph = Int32.MaxValue;
        }

        /// <summary>指定した IDataContractSurrogate インスタンスで現在アクティブなサロゲート型を取得または設定します。サロゲートは、シリアル化または逆シリアル化プロセスを拡張できます。</summary>
        public IDataContractSurrogate DataContractSurrogate { get; set; }

        /// <summary>逆シリアル化時に未知のデータを無視するかどうか、およびシリアル化時に IExtensibleDataObject インターフェイスを無視するかどうかを指定する値を取得または設定します。</summary>
        public bool IgnoreExtensionDataObject { get; set; }

        /// <summary>DataContractJsonSerializer のこのインスタンスを使用してシリアル化されるオブジェクト グラフ内に存在可能な型のコレクションを取得します。</summary>
        public IList<Type> KnownTypes { get; private set; }

        /// <summary>シリアライザーが 1 回の読み取りまたは書き込みの呼び出しでシリアル化または逆シリアル化するオブジェクト グラフ内の項目の最大数を取得または設定します。</summary>
        public int MaxItemsInObjectGraph { get; set; }

        /// <summary>型情報を出力するかどうかを指定する値を取得または設定します。</summary>
        public bool AlwaysEmitTypeInformation { get; set; }

        /// <summary>指定のインスタンスを保存します。</summary>
        /// <param name="path">シリアライズした内容を保存するパス。</param>
        /// <param name="instance">シリアライズする対象のインスタンス。</param>
        public void Serialize(string path, T instance)
        {
            var serializer = GetSerializer();
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, instance);
                string json = Encoding.UTF8.GetString(ms.ToArray());
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
            }
        }

        /// <summary>指定のパスからインスタンスを取得します。</summary>
        /// <param name="path">デシリアライズする内容を読み込むパス。</param>
        /// <returns>デシリアライズしたインスタンス。</returns>
        public T Desilialize(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            var serializer = GetSerializer();
            byte[] bytes = Encoding.UTF8.GetBytes(File.ReadAllText(path, Encoding.UTF8));
            using (var stream = new MemoryStream(bytes))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        /// <summary>シリアライザのインスタンスを取得します。</summary>
        /// <returns>取得したシリアライザのインスタンス。</returns>
        private DataContractJsonSerializer GetSerializer()
        {
            return new DataContractJsonSerializer(typeof(T), KnownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject, DataContractSurrogate, AlwaysEmitTypeInformation);
        }

        #endregion
    }

    /// <summary>設定情報をバイナリで出力する機能を提供します。</summary>
    /// <typeparam name="T">シリアライズするクラスの型。</typeparam>
    public class PortableSettingsBinarySerializer<T> : IPortableSettingsSerializer<T>
        where T : class
    {
        #region Members

        /// <summary>PortableSettingsBinarySerializer クラスの新しいインスタンスを作成します。</summary>
        public PortableSettingsBinarySerializer()
        {
            _binaryFormatter = new BinaryFormatter();
        }

        /// <summary>指定されたパラメーターに基づいて、PortableSettingsValue クラスの新しいインスタンスを作成します。</summary>
        /// <param name="selector">サロゲート セレクター。</param>
        /// <param name="context">シリアル化されたデータの転送元と転送先。</param>
        public PortableSettingsBinarySerializer(ISurrogateSelector selector, StreamingContext context)
        {
            _binaryFormatter = new BinaryFormatter(selector, context);
        }

        /// <summary>バイナリシリアライズ機能を提供するクラス。</summary>
        private readonly BinaryFormatter _binaryFormatter;

        public bool IsCompression { get; set; }

        /// <summary>アセンブリの検索と読み込みに関するデシリアライザの動作を取得または設定します。</summary>
        public FormatterAssemblyStyle AssemblyFormat
        {
            get { return _binaryFormatter.AssemblyFormat; }
            set { _binaryFormatter.AssemblyFormat = value; }
        }

        /// <summary>シリアル化されたオブジェクトから型へのバインディングを制御する、SerializationBinder 型のオブジェクトを取得または設定します。</summary>
        public SerializationBinder Binder
        {
            get { return _binaryFormatter.Binder; }
            set { _binaryFormatter.Binder = value; }
        }


        /// <summary>対象のフォーマッタで使用する StreamingContext を取得または設定します。</summary>
        public StreamingContext Context
        {
            get { return _binaryFormatter.Context; }
            set { _binaryFormatter.Context = value; }
        }


        /// <summary>BinaryFormatter が実行する自動逆シリアル化の TypeFilterLevel を取得または設定します。</summary>
        public TypeFilterLevel FilterLevel
        {
            get { return _binaryFormatter.FilterLevel; }
            set { _binaryFormatter.FilterLevel = value; }
        }


        /// <summary>シリアル化中および逆シリアル化中に行われる型の置換を制御する ISurrogateSelector を取得または設定します。</summary>
        public ISurrogateSelector SurrogateSelector
        {
            get { return _binaryFormatter.SurrogateSelector; }
            set { _binaryFormatter.SurrogateSelector = value; }
        }


        /// <summary>シリアル化されたストリームにおける型の記述のレイアウト形式を取得または設定します。</summary>
        public FormatterTypeStyle TypeFormat
        {
            get { return _binaryFormatter.TypeFormat; }
            set { _binaryFormatter.TypeFormat = value; }
        }

        /// <summary>指定のインスタンスを保存します。</summary>
        /// <param name="path">シリアライズした内容を保存するパス。</param>
        /// <param name="instance">シリアライズする対象のインスタンス。</param>
        public void Serialize(string path, T instance)
        {
            using (var stream = GetStream(path, FileMode.Create, CompressionMode.Compress))
            {
                _binaryFormatter.Serialize(stream, instance);
            }
        }

        /// <summary>指定のパスからインスタンスを取得します。</summary>
        /// <param name="path">デシリアライズする内容を読み込むパス。</param>
        /// <returns>デシリアライズしたインスタンス。</returns>
        public T Desilialize(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            using (var stream = GetStream(path, FileMode.Open, CompressionMode.Decompress))
            {
                return _binaryFormatter.Deserialize(stream) as T;
            }
        }

        /// <summary>ストリームを取得します。</summary>
        /// <param name="path">リアライズまたはデシリアライズするファイルのパス。</param>
        /// <param name="fileMode">ファイルを開く方法。</param>
        /// <param name="compression">ストリームを圧縮するか圧縮解除するかどうか。</param>
        /// <returns>作成したストリーム。</returns>
        private Stream GetStream(string path, FileMode fileMode, CompressionMode compression)
        {
            Stream stream = File.Open(path, fileMode);
            if (IsCompression)
            {
                stream = new DeflateStream(stream, compression);
            }
            return stream;
        }

        #endregion
    }
}
