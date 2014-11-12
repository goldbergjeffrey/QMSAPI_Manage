using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using QMSAPI_Manage.ServiceSupport;
using QMSAPI_Manage.QMSAPIService;

namespace QMSAPI_Manage.ServiceSupport
{
    public class TaskManagement
    {

        private QMSClient qmsClientInfo;
        public QMSClient qmsClient
        {
            get
            {
                return qmsClientInfo;
            }
            set
            {
                qmsClientInfo = value;
            }
        }

        private Boolean saveTask(DocumentTask task)
        {
            try
            {
                qmsClient.SaveDocumentTask(task);
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Boolean generateTask(System.Guid qdsID, DocumentNode srcDoc, String taskName,
            String taskDesc)
        {
            Boolean taskCreated = false;
            DocumentTask newTask = createNewTask(qdsID, srcDoc, taskName, taskDesc);

            if (saveTask(newTask))
            {
                taskCreated = true;
            }

            return taskCreated;
        }

        
        private DocumentTask createNewTask(System.Guid qdsID, DocumentNode srcDoc, String taskName,
            String taskDesc)
        {
            DocumentTask newTask = new DocumentTask();
            newTask.QDSID = qdsID;
            newTask.ID = System.Guid.NewGuid();
            newTask.Document = srcDoc;
            qmsClient.SaveDocumentTask(newTask);
            newTask = qmsClient.GetDocumentTask(newTask.ID, DocumentTaskScope.All);
          
            DocumentTask.TaskGeneral generalInfo = new DocumentTask.TaskGeneral();
           
            generalInfo.Enabled = true;
            generalInfo.TaskName = taskName;
            generalInfo.TaskDescription = taskDesc;
            newTask.General = generalInfo;
            DocumentTask.TaskReload reloadInfo = new DocumentTask.TaskReload();
            reloadInfo.Mode = TaskReloadMode.Full;
            newTask.Reload = reloadInfo;

            List<Trigger> TriggerList = new List<Trigger>();
            RecurrenceTrigger timeTrigger = new RecurrenceTrigger();

            DateTime triggerDate = new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,DateTime.UtcNow.Day,
                DateTime.UtcNow.Hour,DateTime.UtcNow.Minute,DateTime.UtcNow.Second);

            RecurrenceTrigger.RecurrenceTriggerDaily recurDays = new RecurrenceTrigger.RecurrenceTriggerDaily();
            recurDays.RecurEvery = 1;

            timeTrigger.Enabled = true;
            timeTrigger.ID = System.Guid.NewGuid();
            timeTrigger.Type = TaskTriggerType.DailyTrigger;
            timeTrigger.Daily = recurDays;
            timeTrigger.StartAt = triggerDate;
            timeTrigger.ExpireAt = triggerDate.Add(new TimeSpan(180, 0, 0, 0));

            TriggerList.Add(timeTrigger);

            newTask.Triggering = new DocumentTask.TaskTriggering();
            newTask.Triggering.ExecutionAttempts = 2;
            newTask.Triggering.ExecutionTimeout = 1440;
            
            newTask.Triggering.Triggers = TriggerList;

            List<TaskInfo> taskDependencies = new List<TaskInfo>();
            newTask.Triggering.TaskDependencies = taskDependencies;
//            qmsClient.SaveDocumentTask(newTask);
            return newTask;
        }

        public List<DocumentNode> getSrcDocuments(System.Guid qdsID)
        {
            List<DocumentNode> sourceDocuments = new List<DocumentNode>();
            sourceDocuments = qmsClient.GetSourceDocuments(qdsID);

            return sourceDocuments;
        }

        public DocumentNode getSrcDocument(String docName, List<DocumentNode> sourceDocuments)
        {
           DocumentNode selectedDoc = new DocumentNode();
            foreach(DocumentNode sourceDocument in sourceDocuments)
            {
                if(docName == sourceDocument.Name)
                {
                    selectedDoc = sourceDocument;
                    break;
                }
            }

            return selectedDoc;
        }

        

        private String executeEDXTask(System.Guid qdsID, System.Guid taskID, 
                String taskName, String taskPassword, String taskVariable, List<String> taskValueList)
        {
            try
            {
                int counter;
                Boolean Abort = false;
                counter = 0;

                TriggerEDXTaskResult result = new TriggerEDXTaskResult();
                result = qmsClient.TriggerEDXTask(qdsID, taskName, taskPassword, taskVariable, taskValueList);

                EDXStatus executionStatus = null;
                TaskStatus taskStatus = new TaskStatus();
                Thread.Sleep(1000);
                taskStatus.Extended = new TaskStatus.TaskStatusExtended();
                taskStatus.Extended.FinishedTime = "Never";
                do
                {
                    if (Abort)
                    {
                        if (taskID.ToString() != null)
                        {
                            qmsClient.AbortTask(taskID);
                            Abort = false;
                        }
                        else
                        {
                            Abort = false;
                        }
                    }
                    Thread.Sleep(2000);

                    taskStatus = qmsClient.GetTaskStatus(taskID, TaskStatusScope.Extended);
                    if (counter < 5)
                    {
                        counter += 1;
                    }
                    else
                    {
                        counter = 1;
                    }

                    if (counter == 1)
                    {
                        try
                        {
                            executionStatus = qmsClient.GetEDXTaskStatus(taskID, result.ExecId);
                        }
                        catch (System.Exception ex)
                        {
                            return "Execute Task: " + ex.Message;
                        }
                    }
                } while ((executionStatus == null && (executionStatus.TaskStatus != TaskStatusValue.Completed 
                        || taskStatus.Extended.FinishedTime.ToString() == "Never")) || (Abort));
                return "Task Complete";
            }
            catch (System.Exception ex)
            {
                return "Execute Task: " + ex.Message;
            }
        }

        

    }
}