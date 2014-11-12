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
    public partial class CreateTask : System.Web.UI.Page
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

                QMSClient qmsClient = myProps.qmsApi(myProps.qmsUrl,myProps.qmsUserId,myProps.qmsUserPassword);
                TaskManagement taskClass = new TaskManagement();
                
                taskClass.qmsClient = qmsClient;

                GetSourceDocuments(taskClass);

            }
        }

        protected void GetSourceDocuments(TaskManagement taskClass)
        {
            List<ServiceInfo> qdsServer = taskClass.qmsClient.GetServices(ServiceTypes.QlikViewDistributionService);
            List<DocumentNode> srcDocs = taskClass.getSrcDocuments(qdsServer[0].ID);

            DropDownList1.Items.Add("Please select from the list...");

            foreach (DocumentNode srcDocument in srcDocs)
            {
                DropDownList1.Items.Add(srcDocument.Name);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            QMSServiceHandler myProps = serviceProps();

            QMSClient qmsClient = myProps.qmsApi(myProps.qmsUrl, myProps.qmsUserId, myProps.qmsUserPassword);
            TaskManagement taskClass = new TaskManagement();

            taskClass.qmsClient = qmsClient;

            List<ServiceInfo> qdsServer = taskClass.qmsClient.GetServices(ServiceTypes.QlikViewDistributionService);
            List<DocumentNode> srcDocs = taskClass.getSrcDocuments(qdsServer[0].ID);

            foreach (DocumentNode srcDoc in srcDocs)
            {
                if (srcDoc.Name == DropDownList1.SelectedItem.Text)
                {
                    taskClass.generateTask(qdsServer[0].ID, srcDoc, "Hello World", "Testing QMSAPI Task Creation");
                    //taskClass.qmsClient.ClearQVSCache(QVSCacheObjects.All);
                }
            }

        }


    }
}