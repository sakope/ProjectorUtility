﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;


namespace XmlStorage.Components {
    /// <summary>SerializeしてXMLデータとしてファイル保存する時に利用する形式</summary>
    [Serializable]
    public sealed class DataElement {
        /// <summary>データを取り出す時に使うキー</summary>
        public string Key { get; private set; }
        /// <summary>保存するデータ</summary>
        public object Value { get; private set; }
        /// <summary>データの型のフルネーム</summary>
        public string TypeName { get; private set; }
        /// <summary>データの型(RO)</summary>
        public Type ValueType { get { return this.GetType(this.TypeName); } }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataElement() : this(Guid.NewGuid().ToString(), new object(), typeof(object).FullName) {; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">データを取り出す時に使うキー</param>
        /// <param name="value">保存するデータ</param>
        /// <param name="type">データの型</param>
        public DataElement(string key, object value, Type type) : this(key, value, type.FullName) {; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="key">データを取り出す時に使うキー</param>
        /// <param name="value">保存するデータ</param>
        /// <param name="type">データの型のフルネーム</param>
        public DataElement(string key, object value, string type) { this.Set(key, value, type); }

        /// <summary>
        /// メンバ変数の値を更新する
        /// </summary>
        /// <param name="key">データを取り出す時に使うキー</param>
        /// <param name="value">保存するデータ</param>
        /// <param name="type">データの型</param>
        public void Set(string key, object value, Type type) { this.Set(key, value, type.FullName); }

        /// <summary>
        /// メンバ変数の値を更新する
        /// </summary>
        /// <param name="key">データを取り出す時に使うキー</param>
        /// <param name="value">保存するデータ</param>
        /// <param name="type">データの型のフルネーム</param>
        public void Set(string key, object value, string type) {
            if(key == null) { throw new ArgumentNullException("key", "Key cannot be null."); }
            if(key == "") { throw new ArgumentException("key", "Key cannot be empty."); }

            if(value == null) { throw new ArgumentNullException("value", "Value cannot be null."); }

            if(type == null) { throw new ArgumentNullException("type", "Type cannnot be null."); }
            if(type == "") { throw new ArgumentException("type", "Type cannot be empty."); }

            this.Key = key;
            this.Value = value;
            this.TypeName = type;
        }

        // http://ja.stackoverflow.com/questions/1552/type-gettypestring%E3%81%AE%E5%B8%B0%E3%82%8A%E5%80%A4%E3%81%8Cnull%E3%81%AB%E3%81%AA%E3%82%8B
        private Type GetType(string typeName) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(typeName);

            // If it worked, then we're done here
            if(type != null) {
                return type;
            }

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if(typeName.Contains(".")) {
                // Get the name of the assembly (Assumption is that we are using 
                // fully-qualified type names)
                var assemblyName = typeName.Substring(0, typeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                var assembly = Assembly.Load(assemblyName);
                if(assembly == null) {
                    return null;
                }

                // Ask that assembly to return the proper Type
                type = assembly.GetType(typeName);
                if(type != null) {
                    return type;
                }
            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            foreach(var assemblyName in referencedAssemblies) {
                // Load the referenced assembly
                var assembly = Assembly.Load(assemblyName);

                if(assembly != null) {
                    // See if that assembly defines the named type
                    type = assembly.GetType(typeName);

                    if(type != null) {
                        return type;
                    }
                }
            }

            // The type just couldn't be found...
            return null;
        }
    }
}
