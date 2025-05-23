﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Tractor.Com.QuantAsylum.Hardware;

namespace Com.QuantAsylum.Hardware
{
    class QA450 : IInstrument, Tractor.TestManagers.IProgrammableLoad, ICurrentMeter, IPowerSupply
    {
        static HttpClient Client = new HttpClient();

        static string RootUrl;

        public QA450()
        {
            SetRootUrl("http://localhost:9450");
        }

        void SetRootUrl(string rootUrl)
        {
            RootUrl = rootUrl;
            Client = new HttpClient
            {
                BaseAddress = new Uri(RootUrl)
            };
        }

        public bool SetToDefaults(string title)
        {
            return true;
        }

        public bool ConnectToDevice(out string result)
        {
            // Nothing special to do for REST device
            result = "";
            return true;
        }

        public void CloseConnection()
        {
            
        }

        public double GetVersion()
        {
            string result = GetSync(RootUrl + "/Status/Version", "Value");
            return Convert.ToDouble(result);
        }

        public bool IsConnected()
        {
            // Do a version read and see if the correct version comes back
            try
            {
                double current = GetVersion();
                return true;
            }
            catch
            {

            }

            return false;
        }

        public bool IsRunning()
        {
            return IsConnected();
        }

        public void LaunchApplication()
        {
        }

        public void SetToDefaults()
        {
        }

        public int[] GetSupportedImpedances()
        {
            return new int[] { 0, 4, 8 };
        }

        public void SetImpedance(int impedance)
        {
            if (impedance == 4)
            {
                PutSync("/Settings/Impedance/4");
            }
            else if (impedance == 8)
            {
                PutSync("/Settings/Impedance/8");
            }
            else if (impedance == 0)
            {
                PutSync("/Settings/Impedance/0");
            }
            else
                throw new NotImplementedException("Bad value in SetImpedance()");
        }

        public int GetImpedance()
        {
            string result = GetSync(RootUrl + "/Settings/Impedance", "Value");
            return Convert.ToInt32(result);
        }

        public float GetLoadTemperature()
        {
            throw new NotImplementedException();
        }

        public bool GetSupplyState()
        {
            string result = GetSync(RootUrl + "/Settings/DutPower", "Value");
            return Convert.ToBoolean(result);
        }

        public void SetSupplyState(bool powerEnable)
        {
            if (powerEnable)
                PutSync("/Settings/DutPower/1");
            else
                PutSync("/Settings/DutPower/0");
        }

        float Voltage;
        public void SetSupplyVoltage(float voltage)
        {
            // Doesn't do anything on QA450. Fake it.
            Voltage = voltage;
        }

        public float GetSupplyVoltage()
        {
            return Voltage;
        }

        public float GetDutCurrent(int averages = 1)
        {
            float sum = 0;
            for (int i = 0; i < averages; i++)
            {
                string result = GetSync(RootUrl + "/Current", "Value");
                sum += Convert.ToSingle(result);
                Thread.Sleep(1);
            }

            return sum / averages;
        }

        /*******************************************************************/
        /*********************** HELPERS for REST **************************/
        /*******************************************************************/

        private void PutSync(string url)
        {
            PutSync(url, "", 0);
        }

        /// <summary>
        /// Synchronous PUT. This will throw an exception of the PUT fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="value"></param>
        private void PutSync(string url, string token, int value)
        {
            string json;

            if (token != "")
                json = string.Format("{{\"{0}\":{1}}}", token, value);
            else
                json = "{{}}";

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // We make the PutAsync synchronous via the .Result
            var response = Client.PutAsync(url, content).Result;

            // Throw an exception if not successful
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Synchronous GET. This will throw an exception if the GET fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetSync(string url, string token)
        {
            string content;

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            content = response.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(content);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict = (Dictionary<string, object>)result;

            return dict[token].ToString();
        }
    }
}
