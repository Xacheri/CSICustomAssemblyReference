using CNHDevelopmentTaskExample;
using Mongoose.Core.DataAccess;
using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CNH_DevelopmentTaskAssemblyTEST
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string slserver = "http://yoursever/IDORequestService/RequestService.aspx";

            try
            {
                using (Client context = new Client(slserver, IDOProtocol.Http))
                {
                    OpenSessionResponseData opnSesRes = context.OpenSession("user", "pw!", "siteconfig");

                    if (opnSesRes.Result.ToString() != "Success") { throw new Exception(opnSesRes.Result.ToString()); }

                    Console.WriteLine("Welcome to the Extension Class Testing Framework. Which method would you like to test?");

                    Type type = typeof(CNH_DevelopmentTaskAssembly);
                    try
                    {
                        TestingFrameworkMethods.PromptAndExecuteExtensionMethod(type, context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadLine();
                    }
                }

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }

        }
    }
}
