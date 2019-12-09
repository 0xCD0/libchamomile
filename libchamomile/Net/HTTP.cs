#region Using
using System;
using System.IO;
using System.Net;
using System.Text;
#endregion

namespace libchamomile.Net {
    /// <summary>
    /// HTTP Request/Response를 담당하는 Web 요청 클래스입니다.
    /// </summary>
    public static class HTTP {
        #region 내부 함수
        /// <summary>
        /// HTTP Post를 요청합니다.
        /// </summary>
        /// <param name="url">Post할 주소를 입력합니다. ex)http://192.168.0.1/restapi</param>
        /// <param name="PostPrameter">Post 데이터를 입력합니다. ex)id=testAccount&password=testPassword</param>
        /// <returns>성공시 Resonse Data를 리턴, 실패시 null을 리턴합니다.</returns>
        public static string PostRequest(string url, string PostPrameter = "") {
            try {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                byte[] sendData = UTF8Encoding.UTF8.GetBytes(PostPrameter);

                httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = sendData.Length;

                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(sendData, 0, sendData.Length);
                requestStream.Close();

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                string result = streamReader.ReadToEnd().Trim();

                streamReader.Close();
                httpWebResponse.Close();

                return result;
            }
            catch (ArgumentException ex) {
                throw ex;
            }
            catch (WebException ex) {
                throw ex;
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// HTTP Get을 요청합니다.
        /// </summary>
        /// <param name="url">Get을 요청할 주소를 입력합니다. ex)http://192.168.0.1/restapi</param>
        /// <param name="GetParameter">Parameter 데이터를 입력합니다. ex)uid=testuid</param>
        /// <returns>성공 시 Resonse Data를 리턴, 실패 시 null을 리턴합니다.</returns>
        public static string GetRequest(string url, string GetParameter = "") {
            try {
                url = string.Format($"{url}?{GetParameter}");

                WebRequest webRequest = WebRequest.Create(url);

                WebResponse resp = webRequest.GetResponse();
                StreamReader streamReader = new StreamReader(resp.GetResponseStream());

                return streamReader.ReadToEnd().Trim();
            }
            catch (ArgumentException ex) {
                throw ex;
            }
            catch (WebException ex) {
                throw ex;
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        #endregion
    }
}