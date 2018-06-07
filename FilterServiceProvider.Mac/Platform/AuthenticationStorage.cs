using System;
using System.IO;
using System.Collections.Generic;
using FilterServiceProvider.Common.Platform;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Services;

namespace FilterServiceProvider.Mac.Platform
{
    public class AuthenticationStorage : IAuthenticationStorage
    {
        public AuthenticationStorage()
        {
            
        }

        private Dictionary<string, string> m_authDict;

        private void initializeDictionary()
        {
            if (m_authDict == null)
            {
                m_authDict = new Dictionary<string, string>();


            }
        }

        private void loadDictionary()
        {
            string fileName = FilterProvider.Platform.Path.GetAppDataFile("authentication.settings");

            if (!File.Exists(fileName))
            {
                return;
            }

            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineParts = line.Split('=', 1);

                    m_authDict[lineParts[0]] = lineParts[1];
                }
            }
        }

        private void saveDictionary()
        {
            string fileName = FilterProvider.Platform.Path.GetAppDataFile("authentication.settings");

            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (var pair in m_authDict)
                {
                    writer.WriteLine($"{pair.Key}={pair.Value}");
                }
            }
        }

        public string AuthToken
        {
            get
            {
                initializeDictionary();

                string value = null;
                m_authDict.TryGetValue("AuthToken", out value);

                return value;
            }

            set
            {
                initializeDictionary();

                m_authDict["AuthToken"] = value;
            }
        }

        public string UserEmail
        {
            get
            {
                initializeDictionary();

                string value = null;
                m_authDict.TryGetValue("UserEmail", out value);

                return value;
            }

            set
            {
                initializeDictionary();

                m_authDict["UserEmail"] = value;
            }
        }
    }
}
