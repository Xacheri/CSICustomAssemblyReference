using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CNH_DevelopmentTaskAssemblyTEST
{
    internal class TestingFrameworkMethods
    {

        public static void PromptAndExecuteExtensionMethod(Type extensionClassType, IIDOCommands context)
        {
            // Get all public static methods of the specified type
            MethodInfo[] methods = extensionClassType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            // Display methods
            for (int i = 0; i < methods.Length; i++)
            {
                Console.WriteLine($"{i + 1} : {methods[i].Name}");
            }

            // Take user input to select a method
            Console.WriteLine("Enter the number of the method you want to execute:");
            if (int.TryParse(Console.ReadLine(), out int methodIndex) && methodIndex > 0 && methodIndex <= methods.Length)
            {
                MethodInfo selectedMethod = methods[methodIndex - 1];
                ParameterInfo[] parameters = selectedMethod.GetParameters();
                object[] parameterValues = new object[parameters.Length];

                // Prompt user for parameters
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].Name == "context")
                    {
                        parameterValues[i] = context;
                    }
                    else
                    {
                        Console.WriteLine($"Enter value for parameter '{parameters[i].Name}' of type '{parameters[i].ParameterType}':");
                        string input = Console.ReadLine();

                        // Convert input to the appropriate type
                        try
                        {
                            Type parameterType = parameters[i].ParameterType;
                            if (parameterType.IsByRef)
                            {
                                parameterType = parameterType.GetElementType();
                            }
                            parameterValues[i] = Convert.ChangeType(input, parameterType);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error converting '{input}' to '{parameters[i].ParameterType}': {ex.Message}");
                            return;
                        }
                    }
                }

                // Invoke the method
                try
                {
                    object result = selectedMethod.Invoke(null, parameterValues);
                    if (selectedMethod.ReturnType == typeof(DataTable)) // display the results if DataTable/CLM
                    {
                        Console.WriteLine($"Method result: ");
                        WriteDataTableToConsole((DataTable)result);
                    }
                    else {
                        Console.Write($"Method {selectedMethod.Name} executed with parameters: ");
                        for(int i = 1; i < parameterValues.Length; i++) // first parameter is the session context, we want to ignore it
                        {
                            string p = parameterValues[i].ToString();
                     
                            Console.Write($"{p}, ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking method: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        public static void WriteDataTableToCSV(DataTable data, string path)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataColumn col in data.Columns)
            {
                sb.Append(col.ColumnName + ',');
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(Environment.NewLine);

            foreach (DataRow row in data.Rows)
            {
                foreach (DataColumn col in data.Columns)
                {
                    sb.Append(row[col].ToString().Replace(",", "|") + ",");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(Environment.NewLine);
            }

            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);

            sw.WriteLine(sb);
            sw.Close();
        }



        public static void WriteDataTableToConsole(DataTable data)
        {
            // Determine the maximum length of each column
            int[] maxLengths = new int[data.Columns.Count];
            for (int i = 0; i < data.Columns.Count; i++)
            {
                maxLengths[i] = data.Columns[i].ColumnName.Length;
            }

            foreach (DataRow row in data.Rows)
            {
                // See if any big strings require length adjustments
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    int length = row[i].ToString().Length;
                    if (length > maxLengths[i])
                    {
                        maxLengths[i] = length;
                    }
                }
            }

            // Calculate total table width
            int totalWidth = maxLengths.Sum() + (maxLengths.Length * 5) + 1; // 5 spaces for padding and separator
            int consoleWidth = Console.WindowWidth;

            // Adjust column widths if total width exceeds console width
            if (totalWidth > consoleWidth)
            {
                double scale = (double)consoleWidth / totalWidth;
                for (int i = 0; i < maxLengths.Length; i++)
                {
                    maxLengths[i] = (int)(maxLengths[i] * scale);
                }
            }

            StringBuilder sb = new StringBuilder();

            // print top border
            sb.Append(new string('_', totalWidth) + "\n");

            // Print column headers
            for (int i = 0; i < data.Columns.Count; i++)
            {
                sb.Append(data.Columns[i].ColumnName.PadRight(maxLengths[i] + 3) + "| ");
            }

            sb.Append(Environment.NewLine);

            // Print rows
            foreach (DataRow row in data.Rows)
            {
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    string value = row[i].ToString();
                    if (value.Length > maxLengths[i])
                    {
                        value = value.Substring(0, maxLengths[i] - 3) + "...";
                    }
                    sb.Append(value.PadRight(maxLengths[i] + 3) + "| ");
                }
                sb.Append(Environment.NewLine);
            }
            // print bottom border
            sb.Append(new string('_', totalWidth) + "\n");

            Console.Write(sb.ToString());
        }

    }
}
