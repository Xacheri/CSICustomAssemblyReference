using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using System.Data;
using System.Linq;
using System.IO;
using System;
namespace CNHDevelopmentTaskExample
{
    [IDOExtensionClass("CNH_DevelopmentTasks")]
    public class CNH_DevelopmentTaskAssembly : IDOExtensionClass
    {

        /// <summary>
        /// SyteLine uses this method to hook into the class
        /// </summary>
        /// <returns>A result set of tasks</returns>
        [IDOMethod(MethodFlags.CustomLoad, "Infobar")]
        public DataTable CNH_GetAllTasks()
        {
            // even though we call it "context" elsewhere, it is actually Context.Commands
            return CNH_GetAllTasksProcess(base.Context.Commands);
        }

        [IDOMethod(MethodFlags.None, "Infobar")]
        public int CNH_CreateNewTask(       
            string NewTaskName,
            string NewTaskDesc,
            string NewTaskAssignee,
            string NewTaskDue
        )
        {
            return CNH_CreateNewTaskProcess(base.Context.Commands, NewTaskName, NewTaskDesc, NewTaskAssignee, NewTaskDue);
        }

        [IDOMethod(MethodFlags.CustomLoad, "Infobar")]
        public DataTable CNH_GetOverdueTaskDevelopers()
        {
            return CNH_GetUsersWithOverdueTasks(base.Context.Commands);
        }

        /// <summary>
        /// Returns a DataTable representing results for a Custom Load Method
        /// </summary>
        /// <param name="context">IIDOCommands to the Syteline Environment</param>
        /// <returns>A DataTable of tasks</returns>
        public static DataTable CNH_GetAllTasksProcess(IIDOCommands context) {
            string columns = "CNH_TaskName, CNH_TaskDesc, CNH_Assignee, CNH_TaskDue";
            DataTable returnData = new DataTable();
            foreach( string col in columns.Split(','))
            {
                returnData.Columns.Add(col.Trim());
            }
            string filter = "CNH_TaskDue > ";

            LoadCollectionRequestData request = new LoadCollectionRequestData()
            {
                IDOName = "CNH_DevelopmentTasks",
                PropertyList = new PropertyList(columns),
                RecordCap = 200,
                OrderBy = "CNH_TaskDue"
            };

            LoadCollectionResponseData response = context.LoadCollection(request);

            response.Fill(returnData);

            // if we hit the record cap, enter the pagination loop
            while (response.Items.Count > 199 && response.Items.ElementAt(-1).PropertyValues.ElementAt(3).Value != null)
            {
                // the last property of the last row is the last date
                string lastDate = response.Items.ElementAt(-1).PropertyValues.ElementAt(-1).Value;
                string thisFilter = filter + lastDate;
                
                LoadCollectionRequestData pagRequest = new LoadCollectionRequestData()
                {
                    IDOName = "CNH_DevelopmentTasks",
                    PropertyList = new PropertyList(columns),
                    RecordCap = 200,
                    OrderBy = "CNH_TaskDue",
                    Filter = thisFilter
                };

                response = context.LoadCollection(pagRequest);
                response.Fill(returnData);
            }
            
            return returnData;
        }


        

        public static int CNH_CreateNewTaskProcess(
            IIDOCommands context,
            string NewTaskName,
            string NewTaskDesc,
            string NewTaskAssignee,
            string NewTaskDue
        )
        {
            // create a new item to insert
            IDOUpdateItem newItem = new IDOUpdateItem( UpdateAction.Insert );
            newItem.Properties.Add("CNH_TaskName", NewTaskName);
            newItem.Properties.Add("CNH_TaskDesc", NewTaskDesc);
            newItem.Properties.Add("CNH_Assignee", NewTaskAssignee);
            newItem.Properties.Add("CNH_TaskDue", NewTaskDue);


            UpdateCollectionRequestData updateCollectionRequestData = new UpdateCollectionRequestData()
            {
                IDOName = "CNH_DevelopmentTasks",
                Items = new IDOUpdateItems() { newItem },
                RefreshAfterUpdate = true
            };

            UpdateCollectionResponseData response = context.UpdateCollection(updateCollectionRequestData);

            return 0;
        }

        /// <summary>
        /// Returns Username, User Code, and User Email of User with an overdue task
        /// </summary>
        /// <param name="context">IIDOCommands from base.Context.Commands</param>
        /// <returns>A DataTable of the information</returns>
        public static DataTable CNH_GetUsersWithOverdueTasks(
            IIDOCommands context
        )
        {
            // set up the data table for returning
            string columns = "Username, EmailAddress, CNH_TaskName, CNH_TaskDesc, CNH_Assignee, CNH_TaskDue";
            DataTable returnData = new DataTable();
            foreach( string col in columns.Split(','))
            {
                returnData.Columns.Add(col.Trim());
            }

            // set up the request object to the CNH_DevelopmentTasks IDO
            string taskColumns = "CNH_TaskName, CNH_TaskDue, CNH_Assignee";
            LoadCollectionRequestData baseRequest = new LoadCollectionRequestData() { 
                IDOName = "CNH_DevelopmentTasks",
                PropertyList = new PropertyList(taskColumns),
                RecordCap = 200,
                Filter = "CNH_TaskDue < GETDATE()"
            };
     
            
            // set up the request object to the UserNames IDO
            string userNamesColumns = "UserId, Username, UserAlias";
            LoadCollectionRequestData request = new LoadCollectionRequestData() { 
                IDOName = "UserNames",
                PropertyList = new PropertyList(userNamesColumns),
                RecordCap = 200
            };
            request.SetLinkBy("CNH_Assignee", "Username"); // set the relationship for if this request is nested

            // set up the request object to the UserEmails IDO
            string userEmailColumns = "EmailAddress";
            LoadCollectionRequestData childRequest = new LoadCollectionRequestData()
            {
                IDOName = "UserEmails",
                PropertyList = new PropertyList(userEmailColumns),
                RecordCap = 200
            };
            childRequest.SetLinkBy("UserId", "UserId"); // set the relationship for if this request is nested

            request.AddNestedRequest(childRequest); // link/join our requests
            baseRequest.AddNestedRequest(request);

            LoadCollectionResponseData loadCollectionResponseData = context.LoadCollection(baseRequest);

            loadCollectionResponseData.Fill(returnData);

            return returnData;
        }

        public static void CheckInforExample(IIDOCommands context)
        {
          LoadCollectionRequestData emailsRequest = new LoadCollectionRequestData
           {
              IDOName = "UserEmails",
              PropertyList = new PropertyList( "EmailAddress, EmailType" ),
              RecordCap = -1
           };
           // Set the relationship data of the child and parent IDOs
           emailsRequest.SetLinkBy( "UserId", "UserId" );
         
           LoadCollectionRequestData usersRequest = new LoadCollectionRequestData
           {
              IDOName = "UserNames",
              PropertyList = new PropertyList( "UserId, Username, UserDesc" ),
              RecordCap = -1
           };
           // Nest the user emails LoadCollection request inside the user LoadCollection request
           usersRequest.AddNestedRequest( emailsRequest );
         
           LoadCollectionResponseData response = context.LoadCollection( usersRequest );
         
           // Property info can be enumerated as follows
           foreach ( IDOItem item in response.Items )
           {
              // Do something...
              foreach ( IDOPropertyValue property in item.PropertyValues )
              {
                 Console.WriteLine( property.Value );
              }
           }
        }
    }
}
