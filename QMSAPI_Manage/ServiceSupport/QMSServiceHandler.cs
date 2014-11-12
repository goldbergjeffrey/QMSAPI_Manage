using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QMSAPI_Manage.QMSAPIService;
using QMSAPI_Manage.ServiceSupport;

namespace QMSAPI_Manage.ServiceSupport
{
    public class QMSServiceHandler
    {
        private string qmsAddress;
        public string qmsUrl
        {
            get
            {
                return qmsAddress;
            }
            set
            {
                qmsAddress = value;
            }
        }

        private string userId;
        public string qmsUserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        private string userPassword;
        public string qmsUserPassword
        {
            get
            {
                return userPassword;
            }
            set
            {
                userPassword = value;
            }
        }

        public static QMSServiceHandler setQmsProps(string qmsAddress, string userId, string password)
        {
            QMSServiceHandler serviceProps = new QMSServiceHandler();
            serviceProps.qmsUserId = userId;
            serviceProps.qmsUserPassword = Base64Decode(password);
            serviceProps.qmsUrl = qmsAddress;
            return serviceProps;
        }

        public QMSClient qmsApi(string qmsAddress, string userId, string password)
        {
            //Make the call to QMS and get the service key.  We will need this later on to perform actions.
            QMSClient qmsClient;
            string QMSServiceAddress = qmsAddress;
            qmsClient = new QMSClient("BasicHttpBinding_IQMS", QMSServiceAddress);
            qmsClient.ClientCredentials.Windows.ClientCredential.UserName = userId;
            qmsClient.ClientCredentials.Windows.ClientCredential.Password = password;
            string key = qmsClient.GetTimeLimitedServiceKey();
            ServiceKeyClientMessageInspector.ServiceKey = key;
            return qmsClient;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}