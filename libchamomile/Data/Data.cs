#region Using
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Newtonsoft.Json;
#endregion

namespace libchamomile.Data {
    public static class Data {
        #region 내부 함수
        /// <summary>
        /// 대상 객체를 직렬화하여 File로 저장합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 저장할 Class를 입력합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool SerializeDataToFile<T>(T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                BinaryFormatter serializer = new BinaryFormatter();
                Stream stream = File.Open(fullFilePath, FileMode.Create, FileAccess.Write);

                serializer.Serialize(stream, targetClass);
                stream.Close();
                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 직렬화된 파일로부터 클래스의 정보를 가져옵니다. 반드시 직렬화 시 사용한 Class를 입력해야 합니다.
        /// </summary>
        /// <param name="targetClass">역직렬화하여 대입할 [Class 참조(ref)]를 입력합니다. 반드시 직렬화 시 사용한 Class를 입력해야 합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool DeserializeDataFromFile<T>(ref T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                BinaryFormatter serializer = new BinaryFormatter();
                Stream stream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read);

                targetClass = (T)serializer.Deserialize(stream);
                stream.Close();
                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 암호화 된 File로 저장합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 저장할 Class를 입력합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool SerializeDataToEncryptedFile<T>(T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                // 
                byte[] key = { 0x63, 0x65, 0x6e, 0x6f, 0x6b, 0x73, 0x62, 0x6c };

                //
                byte[] iv = { 0x73, 0x6b, 0x6f, 0x6e, 0x65, 0x63, 0x6c, 0x62 };

                DESCryptoServiceProvider des = new DESCryptoServiceProvider();

                using (var fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
                using (var cryptoStream = new CryptoStream(fs, des.CreateEncryptor(key, iv), CryptoStreamMode.Write)) {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(cryptoStream, targetClass);
                }

                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 암호화 된 직렬화 파일로부터 클래스의 정보를 가져옵니다. 반드시 직렬화시 사용한 Class를 입력해야 합니다.
        /// </summary>
        /// <param name="targetClass">역직렬화하여 대입할 [Class 참조(ref)]를 입력합니다. 반드시 직렬화 시 사용한 Class를 입력해야 합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool DeserializeDataFromEncryptedFile<T>(ref T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                // 
                byte[] key = { 0x63, 0x65, 0x6e, 0x6f, 0x6b, 0x73, 0x62, 0x6c };

                // 
                byte[] iv = { 0x73, 0x6b, 0x6f, 0x6e, 0x65, 0x63, 0x6c, 0x62 };

                DESCryptoServiceProvider des = new DESCryptoServiceProvider();

                using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                using (var cryptoStream = new CryptoStream(fs, des.CreateDecryptor(key, iv), CryptoStreamMode.Read)) {
                    BinaryFormatter formatter = new BinaryFormatter();

                    targetClass = (T)formatter.Deserialize(cryptoStream);
                }

                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 XML Format File로 저장합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 XML로 저장할 Class를 입력합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.</param>
        /// <param name="fullFilePath">저장할 XML 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool SerializeDataToXML<T>(T targetClass, string fullFilePath) {
            try {
                using (var writer = new System.IO.StreamWriter(fullFilePath)) {
                    var serializer = new XmlSerializer(targetClass.GetType());
                    serializer.Serialize(writer, targetClass);
                    writer.Flush();
                }

                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 직렬화된 XML 파일로부터 클래스의 정보를 가져옵니다. 반드시 직렬화시 사용한 Class를 입력해야 합니다.
        /// </summary>
        /// <param name="targetClass">XML 파일에서 역직렬화하여 대입할 [Class 참조(ref)]를 입력합니다. 반드시 직렬화 시 사용한 Class를 입력해야 합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool DeserializeDataFromXML<T>(ref T targetClass, string fullFilePath) {
            try {
                using (var stream = System.IO.File.OpenRead(fullFilePath)) {
                    var serializer = new XmlSerializer(typeof(T));
                    targetClass = (T)serializer.Deserialize(stream);
                }

                return true;
            }

            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 Json String으로 반환합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 Json으로 출력할 Class를 입력합니다.</param>
        /// <returns>성공 시 Json String을 반환, 실패 시 exception을 throw 합니다.</returns>
        public static string SerializeDataToJson<T>(T targetClass) {
            string json = JsonConvert.SerializeObject(targetClass);
            return json;
        }
        #endregion

        #region 확장 메소드
        /// <summary>
        /// 대상 객체를 직렬화하여 File로 저장하는 확장 메소드입니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 저장할 Class를 입력합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool ToSerializeDataToFile<T>(this T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                return SerializeDataToFile(targetClass, fullFilePath);
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 암호화된 File로 저장하는 확장 메소드입니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 저장할 Class를 입력합니다.</param>
        /// <param name="fullFilePath">저장할 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool ToSerializeDataToEncryptedFile<T>(this T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                return SerializeDataToEncryptedFile(targetClass, fullFilePath);
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 XML Format File로 저장합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 XML로 저장할 Class를 입력합니다.</param>
        /// <param name="fullFilePath">저장할 XML 파일 경로를 입력합니다. 파일 이름 및 확장자까지 입력해야 합니다.</param>
        /// <returns>성공 시 true, 실패 시 exception을 throw 합니다.</returns>
        public static bool ToSerializeDataToXMLFile<T>(this T targetClass, string fullFilePath) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                return SerializeDataToXML(targetClass, fullFilePath);
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 대상 객체를 직렬화하여 Json String으로 반환합니다. targetClass는 반드시 Header로 [Serialize()]를 기입하셔야 합니다.
        /// </summary>
        /// <param name="targetClass">직렬화하여 Json으로 출력할 Class를 입력합니다.</param>
        /// <returns>성공 시 Json String을 반환, 실패 시 exception을 throw 합니다.</returns>
        public static string ToSerializeDataToJson<T>(this T targetClass) {
            if (targetClass == null) {
                throw new Exception("Class is null");
            }

            try {
                return SerializeDataToJson(targetClass);
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        #endregion

    }
}
