using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using QMSAPI_Manage.QMSAPIService;
using QMSAPI_Manage.ServiceSupport;
using System.Configuration;

namespace QMSAPI_Manage
{
    public partial class UserManagement : System.Web.UI.Page
    {
        protected QMSServiceHandler serviceProps()
        {
            QMSServiceHandler serviceProps = new QMSServiceHandler();
            serviceProps = QMSServiceHandler.setQmsProps("http://qv112sr6:4799/qms/service", "qmsservice",
                    System.Configuration.ConfigurationManager.AppSettings["ServicePassword"]);

            return serviceProps;
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //set the properties for connection, userid and password
                QMSServiceHandler myProps = serviceProps();
           
                GetDocuments(myProps);
//                GetListBox1();
//                FinalizeListBoxes();
            }
        }

        
        protected void GetDocuments(QMSServiceHandler serviceProps)
        {

            QMSClient qmsClient = serviceProps.qmsApi(serviceProps.qmsUrl, serviceProps.qmsUserId, serviceProps.qmsUserPassword);

            //now let's clear the qvs document cache.
            qmsClient.ClearQVSCache(QVSCacheObjects.UserDocumentList);

            List<ServiceInfo> qvServer = qmsClient.GetServices(ServiceTypes.QlikViewServer);
            List<DocumentNode> userDocuments = qmsClient.GetUserDocuments(qvServer[0].ID);

            DropDownList1.Items.Add("Please select from the list...");

            foreach (DocumentNode userDocument in userDocuments)
            {
                DropDownList1.Items.Add(userDocument.Name);
            }


        }

        protected void setUsers(Array arrayUserList)
        {
            QMSServiceHandler myProps = serviceProps();
            QMSClient qmsClient = myProps.qmsApi(myProps.qmsUrl, myProps.qmsUserId, myProps.qmsUserPassword);
            qmsClient.ClearQVSCache(QVSCacheObjects.UserDocumentList);
            List<ServiceInfo> qvServer = qmsClient.GetServices(ServiceTypes.QlikViewServer);
            List<DocumentNode> userDocuments = qmsClient.GetUserDocuments(qvServer[0].ID);

            foreach (DocumentNode selectedDoc in userDocuments)
            {

                if (selectedDoc.Name == DropDownList1.SelectedItem.Text)
                {
                    Label2.Text = selectedDoc.Name;
                    DocumentMetaData docMetaData = new DocumentMetaData();
                    List<DocumentAccessEntry> docAccessList = new List<DocumentAccessEntry>();
                    docMetaData = qmsClient.GetDocumentMetaData(selectedDoc, DocumentMetaDataScope.Authorization);
                    docMetaData.Authorization.Access = docAccessList;
                    qmsClient.SaveDocumentMetaData(docMetaData);

                    DayOfWeek[] input = { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, 
                                DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
                    List<DayOfWeek> dayRange = new List<DayOfWeek>(input);

                    if (arrayUserList.Length == 0)
                    {
                        //do nothing
                    }
                    else
                    {
                        for (int i = 0; i < arrayUserList.Length - 1; i++)
                        {
                            DocumentAccessEntry docAccess = new DocumentAccessEntry();
                            docAccess.AccessMode = DocumentAccessEntryMode.Always;
                            docAccess.IsAnonymous = false;
                            docAccess.DayOfWeekConstraints = dayRange;
                            docAccess.ExtensionData = docMetaData.ExtensionData;
                            docAccess.UserName = arrayUserList.GetValue(i).ToString();
                            docAccessList.Add(docAccess);
                        }
                        List<DocumentAccessEntry> userList = new List<DocumentAccessEntry>(docAccessList);
                        docMetaData.Authorization.Access = userList;
                        qmsClient.SaveDocumentMetaData(docMetaData);
                    }
                }
            }
        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox1.Items.Clear();
            Label1.Text = DropDownList1.SelectedItem.Text;
            QMSServiceHandler myProps = serviceProps();
            QMSClient qmsClient = myProps.qmsApi(myProps.qmsUrl, myProps.qmsUserId, myProps.qmsUserPassword);
            qmsClient.ClearQVSCache(QVSCacheObjects.UserDocumentList);
            List<ServiceInfo> qvServer = qmsClient.GetServices(ServiceTypes.QlikViewServer);
            List<DocumentNode> userDocuments = qmsClient.GetUserDocuments(qvServer[0].ID);

            foreach (DocumentNode selectedDoc in userDocuments)
            {
                
                if (selectedDoc.Name == DropDownList1.SelectedItem.Text)
                {
                    Label2.Text = selectedDoc.Name;
                    DocumentMetaData docMetaData = new DocumentMetaData();
                    docMetaData = qmsClient.GetDocumentMetaData(selectedDoc, DocumentMetaDataScope.Authorization);
                    List<DocumentAccessEntry> docAccessList = docMetaData.Authorization.Access;
                    foreach (DocumentAccessEntry docAccess in docAccessList)
                    {
                        ListBox1.Items.Add(docAccess.UserName);
                    }


                }
            }

        }

        

        

    }
}